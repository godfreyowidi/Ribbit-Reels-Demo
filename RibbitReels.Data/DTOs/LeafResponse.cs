namespace RibbitReels.Data.DTOs;

public class LeafResponse
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }
    public string Source { get; set; } = null!;
    public string Status { get; set; } = "Published";

    // manual
    public string? VideoUrl { get; set; }

    // YT
    public string? YouTubeVideoId { get; set; }
    public string? ThumbnailUrl { get; set; }

    public DateTime CreatedAt { get; set; }
}

