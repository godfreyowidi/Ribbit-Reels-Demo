using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RibbitReels.Data.Configs;
using RibbitReels.Services.Interfaces;
using RibbitReels.Services.Shared;
using System.Net;

namespace RibbitReels.Services.Implementations
{
    public class AzureBlobRepository : IAzureBlobRepository
    {
        private readonly string _containerName;
        private readonly BlobContainerClient _blobContainerClient;

        public AzureBlobRepository(IOptions<AzureBlobConfiguration> options)
        {
            var config = options.Value;
            _containerName = config.ContainerName;

            _blobContainerClient = new BlobContainerClient(config.ConnectionString, _containerName);
        }

        public async Task<OperationResult<string>> UploadVideoAsync(IFormFile file, string leafId)
        {
            if (file == null || file.Length == 0)
                return OperationResult<string>.Fail("Invalid file.", HttpStatusCode.BadRequest);

            try
            {
                // so we changes to only ensuring container exist only when uploading to cover yt
                await _blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
                
                // relative blob path
                var blobName = $"leaf_{leafId}/{Guid.NewGuid()}_{file.FileName}";

                var blobClient = _blobContainerClient.GetBlobClient(blobName);

                await using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                // we return only relative path - not full url
                return OperationResult<string>.Success(blobName, HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Fail(ex, $"Failed to upload video: {ex.Message}");
            }
        }

        public async Task<OperationResult<string>> GetVideoUrlAsync(string blobPath, int validMinutes = 60)
        {
            try
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobPath);

                if (!await blobClient.ExistsAsync())
                    return OperationResult<string>.Fail("Video not found.", HttpStatusCode.NotFound);

                var sasUri = blobClient.GenerateSasUri(
                    Azure.Storage.Sas.BlobSasPermissions.Read,
                    DateTimeOffset.UtcNow.AddMinutes(validMinutes)
                );

                return OperationResult<string>.Success(sasUri.ToString());
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Fail(ex, $"Failed to get video: {ex.Message}");
            }
        }
    }
}
