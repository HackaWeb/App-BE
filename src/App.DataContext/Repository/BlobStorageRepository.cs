using App.Domain;
using App.Infrastructure.Repositories;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace App.DataContext.Repository;

public class BlobStorageRepository : IBlobStorageRepository
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageRepository(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable(AppConstants.AZURE_STORAGE_CONNECTION_STRING);
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<bool> DeleteAsync(string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        return await blobClient.DeleteIfExistsAsync();
    }

    public async Task<Stream?> DownloadAsync(string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fileName);
        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

        return blobClient.Uri.ToString();
    }
}
