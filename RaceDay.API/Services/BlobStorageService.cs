using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration config)
        {
            var connectionString = config["AzureBlob:ConnectionString"]
                ?? throw new InvalidOperationException("AzureBlob:ConnectionString is missing.");

            var containerName = config["AzureBlob:ContainerName"] ?? "raceday";

            var serviceClient = new BlobServiceClient(connectionString);
            _container = serviceClient.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file supplied.");

            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };

            if (!allowed.Contains(file.ContentType))
                throw new ArgumentException("Only JPG, PNG, WEBP or GIF images are allowed.");

            var extension = Path.GetExtension(file.FileName);
            var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
            var blobClient = _container.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blobClient.Uri.ToString();
        }
    }
}
