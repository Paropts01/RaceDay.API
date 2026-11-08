
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services;

public class AzureBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;

    private const long MaxFileSize = 5 * 1024 * 1024;

    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _configuration = configuration;
    }

    public Task<string> UploadImageAsync(IFormFile file)
    {
        return UploadImageAsync(file, "");
    }

    public async Task<string> UploadImageAsync(
        IFormFile file,
        string folderName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Please select an image.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("The image cannot exceed 5 MB.");

        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        var allowedExtensions = new[]
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        var contentType = file.ContentType?.ToLowerInvariant() ?? "";

        if (!allowedExtensions.Contains(extension) ||
            !allowedTypes.Contains(contentType))
        {
            throw new ArgumentException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        var containerName =
            _configuration["AzureBlob:ContainerName"];

        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new InvalidOperationException(
                "Azure Blob container name is not configured.");
        }

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync();

        var fileName = $"{Guid.NewGuid()}{extension}";

        // Store the file inside its specified virtual folder.
        var blobName = string.IsNullOrWhiteSpace(folderName)
            ? fileName
            : $"{folderName.Trim('/')}/{fileName}";

        var blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(
            stream,
            new BlobHttpHeaders
            {
                ContentType = contentType
            });

        return blobClient.Uri.ToString();
    }

    public Task<string> UploadAsync(IFormFile file)
    {
        return UploadImageAsync(file);
    }

    public Task<string> UploadAsync(
        IFormFile file,
        string folderName)
    {
        return UploadImageAsync(file, folderName);
    }
}


