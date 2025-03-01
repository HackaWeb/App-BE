namespace App.Infrastructure.Repositories;

public interface IBlobStorageRepository
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string containerName);

    Task<Stream?> DownloadAsync(string fileName, string containerName);

    Task<bool> DeleteAsync(string fileName, string containerName);
}
