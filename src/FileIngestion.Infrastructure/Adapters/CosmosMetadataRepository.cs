using FileIngestion.Application.Ports;

namespace FileIngestion.Infrastructure.Adapters;

// Lightweight in-memory placeholder implementation of IMetadataRepository.
// This avoids compiling against an Azure Cosmos SDK when running locally
// in environments where the SDK package/version is unstable. Replace
// with a real Cosmos adapter (or restore the previous implementation)
// when a supported Azure.Cosmos package is available.
public class CosmosMetadataRepository : IMetadataRepository
{
    private readonly Dictionary<string, string> _store = new();

    public Task<string> CreateMetadataAsync(object metadata, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var json = System.Text.Json.JsonSerializer.Serialize(metadata);
        _store[id] = json;
        return Task.FromResult(id);
    }

    public Task<T?> GetMetadataAsync<T>(string id, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(id, out var json)) return Task.FromResult<T?>(default);
        var obj = System.Text.Json.JsonSerializer.Deserialize<T>(json);
        return Task.FromResult(obj);
    }

    public Task<IEnumerable<T>> ListMetadataAsync<T>(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var items = _store.Values
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => System.Text.Json.JsonSerializer.Deserialize<T>(s))
            .Where(x => x is not null)
            .Cast<T>()
            .ToList();
        return Task.FromResult<IEnumerable<T>>(items);
    }

    public Task DeleteMetadataAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }
}
