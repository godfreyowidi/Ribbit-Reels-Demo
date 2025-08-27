namespace RibbitReels.Data.DTOs;

public class LearningProgressResponse
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public List<Guid> CompletedLeafIds { get; set; } = new();
    public double PercentageCompleted { get; set; } 
    public DateTime? CompletedAt { get; set; }
}
