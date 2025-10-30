using FileIngestion.Shared.Dtos;

namespace FileIngestion.Application.Services;

public interface IFileService
{
    Task<(string Url, string? MetadataId)> UploadAsync(System.IO.Stream content, string fileName, string contentType, long size, CancellationToken cancellationToken = default);

    Task<IEnumerable<FileMetadata>> ListMetadataAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    Task<FileMetadata?> GetMetadataAsync(string id, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
