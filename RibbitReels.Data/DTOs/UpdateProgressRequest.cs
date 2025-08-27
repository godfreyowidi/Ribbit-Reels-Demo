namespace RibbitReels.Data.DTOs;

public class UpdateProgressRequest
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public List<Guid> CompletedLeafIds { get; set; } = new();
    public DateTime? CompletedAt { get; set; }
}
