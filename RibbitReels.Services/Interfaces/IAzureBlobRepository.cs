using Microsoft.AspNetCore.Http;
using RibbitReels.Services.Shared;

namespace RibbitReels.Services.Interfaces;

public interface IAzureBlobRepository
{
    Task<OperationResult<string>> UploadVideoAsync(IFormFile file, string leafId);
    Task<OperationResult<string>> GetVideoUrlAsync(string blobPath, int validMinutes = 60);
}
