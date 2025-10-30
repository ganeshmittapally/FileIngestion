using Azure.Cosmos;
using FileIngestion.Application.Ports;

namespace FileIngestion.Infrastructure.Adapters;

public class CosmosMetadataRepository : IMetadataRepository
{
    private readonly Container _container;

    public CosmosMetadataRepository(Container container)
    {
        _container = container;
    }

    public async Task<string> CreateMetadataAsync(object metadata, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var doc = new { id, payload = metadata };
        var response = await _container.CreateItemAsync(doc, new PartitionKey(id), cancellationToken: cancellationToken);
        return response.Resource.id;
    }

    public async Task<T?> GetMetadataAsync<T>(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<dynamic>(id, new PartitionKey(id), cancellationToken: cancellationToken);
            return ((object)response.Resource.payload) is T t ? t : System.Text.Json.JsonSerializer.Deserialize<T>(response.Resource.payload.ToString());
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }
}
