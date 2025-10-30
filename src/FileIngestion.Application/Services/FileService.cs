using FileIngestion.Application.Ports;
using FileIngestion.Shared.Dtos;

namespace FileIngestion.Application.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepo;
    private readonly IMetadataRepository? _metadataRepo;

    public FileService(IFileRepository fileRepo, IMetadataRepository? metadataRepo = null)
    {
        _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
        _metadataRepo = metadataRepo;
    }

    public async Task<(string Url, string? MetadataId)> UploadAsync(System.IO.Stream content, string fileName, string contentType, long size, CancellationToken cancellationToken = default)
    {
        var url = await _fileRepo.UploadAsync(content, contentType, cancellationToken);

        if (_metadataRepo == null)
            return (url, null);

        var metadata = new FileMetadata(Guid.NewGuid().ToString(), fileName, contentType, size, url, DateTime.UtcNow);
        var id = await _metadataRepo.CreateMetadataAsync(metadata, cancellationToken);
        return (url, id);
    }

    public async Task<IEnumerable<FileMetadata>> ListMetadataAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (_metadataRepo == null) return Enumerable.Empty<FileMetadata>();
        return await _metadataRepo.ListMetadataAsync<FileMetadata>(page, pageSize, cancellationToken);
    }

    public async Task<FileMetadata?> GetMetadataAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_metadataRepo == null) return null;
        return await _metadataRepo.GetMetadataAsync<FileMetadata>(id, cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_metadataRepo == null) return;

        var item = await _metadataRepo.GetMetadataAsync<FileMetadata>(id, cancellationToken);
        if (item is null) return;

        if (!string.IsNullOrEmpty(item.Path))
        {
            try
            {
                await _fileRepo.DeleteAsync(item.Path, cancellationToken);
            }
            catch
            {
                // swallow - repository implementation should log
            }
        }

        await _metadataRepo.DeleteMetadataAsync(id, cancellationToken);
    }
}
