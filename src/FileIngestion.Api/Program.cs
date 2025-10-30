using Azure.Cosmos;
using Azure.Storage.Blobs;
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
var blobConnection = builder.Configuration["Azure:Blob:ConnectionString"];
var blobContainer = builder.Configuration["Azure:Blob:Container"] ?? "file-ingestion";
if (!string.IsNullOrEmpty(blobConnection))
{
    var blobService = new BlobServiceClient(blobConnection);
    var containerClient = blobService.GetBlobContainerClient(blobContainer);
    containerClient.CreateIfNotExists();
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
    var db = await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosDatabase);
    var container = await db.Database.CreateContainerIfNotExistsAsync(cosmosContainer, "/id");
    builder.Services.AddSingleton(container.Container);
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
