namespace RibbitReels.Data.DTOs;

public class UserBranchAssignmentResponse
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public Guid AssignedByManagerId { get; set; }
    public BranchResponse Branch { get; set; } = null!;
}
