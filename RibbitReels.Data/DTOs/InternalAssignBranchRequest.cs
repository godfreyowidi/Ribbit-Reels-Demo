namespace RibbitReels.Data.DTOs;

public class InternalAssignBranchRequest
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public Guid AssignedByManagerId { get; set; }
}
