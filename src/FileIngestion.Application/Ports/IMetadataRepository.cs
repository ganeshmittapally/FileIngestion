namespace FileIngestion.Application.Ports;

public interface IMetadataRepository
{
    /// <summary>
    /// Persists metadata and returns its id.
    /// </summary>
    Task<string> CreateMetadataAsync(object metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves metadata by id.
    /// </summary>
    Task<T?> GetMetadataAsync<T>(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists metadata items (paged) matching optional filter.
    /// </summary>
    Task<IEnumerable<T>> ListMetadataAsync<T>(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes metadata by id.
    /// </summary>
    Task DeleteMetadataAsync(string id, CancellationToken cancellationToken = default);
}
