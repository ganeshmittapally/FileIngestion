using Azure.Cosmos;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FileIngestion.Application.Ports;
using FileIngestion.Infrastructure.Adapters;
using FileIngestion.Api.Middleware;
using FileIngestion.Application.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        var rb = ResourceBuilder.CreateDefault().AddService("FileIngestion");
        tracerProviderBuilder
            .SetResourceBuilder(rb)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        // If OTLP endpoint is configured, use OTLP exporter (Datadog/collector)
        var otlpEndpoint = builder.Configuration["Observability:Datadog:OtLPEndpoint"];
        var datadogApiKey = builder.Configuration["Observability:Datadog:ApiKey"];
        if (!string.IsNullOrEmpty(otlpEndpoint))
        {
            tracerProviderBuilder.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
                if (!string.IsNullOrEmpty(datadogApiKey))
                {
                    o.Headers = $"DD-API-KEY={datadogApiKey}";
                }
            });
        }
        else
        {
            tracerProviderBuilder.AddConsoleExporter();
        }
    })
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        var otlpEndpoint = builder.Configuration["Observability:Datadog:OtLPEndpoint"];
        var datadogApiKey = builder.Configuration["Observability:Datadog:ApiKey"];
        if (!string.IsNullOrEmpty(otlpEndpoint))
        {
            metricsBuilder.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
                if (!string.IsNullOrEmpty(datadogApiKey)) o.Headers = $"DD-API-KEY={datadogApiKey}";
            });
        }
        else
        {
            metricsBuilder.AddConsoleExporter();
        }
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Azure SDK clients from configuration (expecting connection strings or endpoints in configuration)
// Azure configuration placeholders. We intentionally do not create resources at startup.
var blobConnection = builder.Configuration["Azure:Blob:ConnectionString"];
var blobContainer = builder.Configuration["Azure:Blob:Container"] ?? "file-ingestion";
if (!string.IsNullOrEmpty(blobConnection))
{
    // Register clients; resource creation is handled by Terraform or admin outside the app.
    var blobService = new BlobServiceClient(blobConnection);
    var containerClient = blobService.GetBlobContainerClient(blobContainer);
    builder.Services.AddSingleton(containerClient);
    builder.Services.AddScoped<IFileRepository, BlobFileRepository>();
}

var cosmosEndpoint = builder.Configuration["Azure:Cosmos:Endpoint"];
var cosmosKey = builder.Configuration["Azure:Cosmos:Key"];
var cosmosDatabase = builder.Configuration["Azure:Cosmos:Database"] ?? "FileIngestionDb";
var cosmosContainer = builder.Configuration["Azure:Cosmos:Container"] ?? "Metadata";
if (!string.IsNullOrEmpty(cosmosEndpoint) && !string.IsNullOrEmpty(cosmosKey))
{
    var cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
    // Get container reference; do not create resources from the application in production.
    var container = cosmosClient.GetContainer(cosmosDatabase, cosmosContainer);
    builder.Services.AddSingleton(container);
    builder.Services.AddScoped<IMetadataRepository, CosmosMetadataRepository>();
}

// Application services
builder.Services.AddScoped<IFileService, FileService>();

// Register default (no-op) event publisher and optionally replace it at runtime
builder.Services.AddSingleton<FileIngestion.Application.Ports.IEventPublisher, FileIngestion.Infrastructure.Adapters.NoOpEventPublisher>();

// Attempt to call optional runtime registration in the Infrastructure assembly (compiled conditionally)
var regType = Type.GetType("FileIngestion.Infrastructure.ServiceBusRegistration, FileIngestion.Infrastructure");
if (regType is not null)
{
    var method = regType.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
    method?.Invoke(null, new object[] { builder.Services, builder.Configuration });
}

var app = builder.Build();

// Use API key middleware (validates X-API-KEY against configuration ApiKey)
app.UseMiddleware<FileIngestion.Api.Middleware.ApiKeyMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// File upload endpoint (delegates orchestration to IFileService)
app.MapPost("/files", async (HttpRequest request, IFileService fileService, CancellationToken ct) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Expected multipart/form-data");
    }

    var form = await request.ReadFormAsync(ct);
    var file = form.Files.FirstOrDefault();
    if (file == null)
    {
        return Results.BadRequest("No file provided");
    }

    await using var stream = file.OpenReadStream();
    var (url, metadataId) = await fileService.UploadAsync(stream, file.FileName, file.ContentType ?? "application/octet-stream", file.Length, ct);

// Register default (no-op) event publisher and optionally replace it
builder.Services.AddSingleton<FileIngestion.Application.Ports.IEventPublisher, FileIngestion.Infrastructure.Adapters.NoOpEventPublisher>();

// Attempt to call optional runtime registration in the Infrastructure assembly
var regType = Type.GetType("FileIngestion.Infrastructure.ServiceBusRegistration, FileIngestion.Infrastructure");
if (regType is not null)
{
    var method = regType.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
    method?.Invoke(null, new object[] { builder.Services, builder.Configuration });
}

    return Results.Ok(new { url, metadataId });
}).WithOpenApi();

// List metadata (paged)
app.MapGet("/files", async (int page, int pageSize, IFileService fileService, CancellationToken ct) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 ? 50 : pageSize;
    var items = await fileService.ListMetadataAsync(page, pageSize, ct);
    return Results.Ok(items);
}).WithOpenApi();

// Get metadata by id
app.MapGet("/files/{id}", async (string id, IFileService fileService, CancellationToken ct) =>
{
    var item = await fileService.GetMetadataAsync(id, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
}).WithOpenApi();

// Delete file and metadata
app.MapDelete("/files/{id}", async (string id, IFileService fileService, CancellationToken ct) =>
{
    var item = await fileService.GetMetadataAsync(id, ct);
    if (item is null) return Results.NotFound();

    await fileService.DeleteAsync(id, ct);
    return Results.NoContent();
}).WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
