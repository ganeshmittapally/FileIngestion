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
        var doc = new { id, payload = metadata, createdAt = DateTime.UtcNow };
        var response = await _container.CreateItemAsync(doc, new PartitionKey(id), cancellationToken: cancellationToken);
        return response.Resource.id;
    }

    public async Task<T?> GetMetadataAsync<T>(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<dynamic>(id, new PartitionKey(id), cancellationToken: cancellationToken);
            var payload = response.Resource.payload;
            if (payload is null) return default;
            return System.Text.Json.JsonSerializer.Deserialize<T>(payload.ToString());
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public async Task<IEnumerable<T>> ListMetadataAsync<T>(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC OFFSET @offset LIMIT @limit")
            .WithParameter("@offset", (page - 1) * pageSize)
            .WithParameter("@limit", pageSize);

        var iterator = _container.GetItemQueryIterator<dynamic>(query);
        var results = new List<T>();
        while (iterator.HasMoreResults)
        {
            var feed = await iterator.ReadNextAsync(cancellationToken);
            foreach (var item in feed)
            {
                if (item.payload is not null)
                {
                    var json = item.payload.ToString();
                    var obj = System.Text.Json.JsonSerializer.Deserialize<T>(json);
                    if (obj != null) results.Add(obj);
                }
            }
        }
        return results;
    }

    public async Task DeleteMetadataAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.DeleteItemAsync<dynamic>(id, new PartitionKey(id), cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // noop
        }
    }
}
