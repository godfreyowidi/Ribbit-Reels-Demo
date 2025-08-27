namespace RibbitReels.Data.DTOs;
public class SearchAllResponse
{
    public List<YouTubeVideo> YouTube { get; set; } = new();
    public UdemyCourseListResponse Udemy { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
