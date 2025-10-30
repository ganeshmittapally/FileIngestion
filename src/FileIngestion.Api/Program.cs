using Azure.Cosmos;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FileIngestion.Application.Ports;
using FileIngestion.Infrastructure.Adapters;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("FileIngestion"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    })
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
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

var app = builder.Build();

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

// File upload endpoint (simple implementation)
app.MapPost("/files", async (HttpRequest request, IFileRepository fileRepo, IMetadataRepository? metadataRepo, CancellationToken ct) =>
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
    var url = await fileRepo.UploadAsync(stream, file.ContentType ?? "application/octet-stream", ct);

    string? metadataId = null;
    if (metadataRepo != null)
    {
        var metadata = new { fileName = file.FileName, contentType = file.ContentType, size = file.Length, path = url };
        metadataId = await metadataRepo.CreateMetadataAsync(metadata, ct);
    }

    return Results.Ok(new { url, metadataId });
}).WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
