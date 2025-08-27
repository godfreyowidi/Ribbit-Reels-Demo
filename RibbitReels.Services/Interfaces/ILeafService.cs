using RibbitReels.Api.DTOs;
using RibbitReels.Data.DTOs;
using RibbitReels.Data.Models;
using RibbitReels.Services.Shared;

namespace RibbitReels.Services.Interfaces;

public interface ILeafService
{
    Task<OperationResult<LeafResponse>> CreateManualLeafAsync(Guid branchId, CreateManualLeafRequest request);

    Task<OperationResult<LeafResponse>> CreateYouTubeLeafAsync(Guid branchId, YouTubeVideo video);

    Task<OperationResult<LeafResponse>> GetLeafByIdAsync(Guid id);

    Task<OperationResult<List<LeafResponse>>> GetLeafsAsync();

    Task<OperationResult<List<LeafResponse>>> GetLeafsByBranchIdAsync(Guid branchId);

    Task<OperationResult<LeafResponse>> UpdateLeafAsync(Guid id, Leaf updatedLeaf);

    Task<OperationResult<bool>> DeleteLeafAsync(Guid id);
}
