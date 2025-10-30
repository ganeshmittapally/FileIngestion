namespace FileIngestion.Application.Ports;

public interface IFileRepository
{
    /// <summary>
    /// Uploads a file stream to storage and returns a storage path or URL.
    /// </summary>
    Task<string> UploadAsync(Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file by its storage path or id.
    /// </summary>
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
