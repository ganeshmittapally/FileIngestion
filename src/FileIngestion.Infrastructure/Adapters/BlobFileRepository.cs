using Azure.Storage.Blobs;
using FileIngestion.Application.Ports;

namespace FileIngestion.Infrastructure.Adapters;

public class BlobFileRepository : IFileRepository
{
    private readonly BlobContainerClient _container;

    public BlobFileRepository(BlobContainerClient container)
    {
        _container = container;
    }

    public async Task<string> UploadAsync(Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var name = Guid.NewGuid().ToString();
        var blob = _container.GetBlobClient(name);
        await blob.UploadAsync(content, overwrite: false, cancellationToken);
        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        // path assumed to be full uri or blob name; try to derive name
        var uri = new Uri(path);
        var blobName = uri.Segments.Last();
        var blob = _container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
