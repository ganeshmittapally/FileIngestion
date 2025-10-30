namespace FileIngestion.Shared.Dtos;

public record FileMetadata(string Id, string FileName, string ContentType, long Size, string Path, DateTime CreatedAt);
