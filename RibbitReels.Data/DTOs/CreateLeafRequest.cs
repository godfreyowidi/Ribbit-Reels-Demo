using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RibbitReels.Api.DTOs;

public abstract class CreateLeafRequestBase
{
    [MaxLength(255)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int Order { get; set; }
}

public class CreateManualLeafRequest : CreateLeafRequestBase
{
    public IFormFile? VideoFile { get; set; }
}

public class CreateYouTubeLeafRequest : CreateLeafRequestBase
{
    [Required]
    [MaxLength(50)]
    public string VideoId { get; set; } = null!;

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }
}
