using RibbitReels.Data.DTOs;
using RibbitReels.Services.Shared;

namespace RibbitReels.Services.Interfaces;

public interface IUserBranchAssignmentService
{
    Task<OperationResult<UserBranchAssignmentResponse>> AssignBranchAsync(InternalAssignBranchRequest request);
    Task<OperationResult<List<UserBranchAssignmentResponse>>> GetAssignmentsByUserAsync(Guid userId);
    Task<OperationResult<List<UserBranchAssignmentResponse>>> GetAllAssignmentsAsync();

    Task<OperationResult<bool>> UnassignBranchAsync(Guid userId, Guid branchId);
    Task<OperationResult<List<UserBranchAssignmentResponse>>> GetAssignmentsByManagerAsync(Guid managerId);
}
