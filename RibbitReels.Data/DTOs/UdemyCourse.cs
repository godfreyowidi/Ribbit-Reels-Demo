namespace RibbitReels.Data.DTOs;

public class UdemyCourseListResponse
{
    public int Count { get; set; }
    public List<UdemyCourse> Results { get; set; } = new();
}

public class UdemyCourse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;          // course title
    public string Url { get; set; } = string.Empty;            // relative url
    public string Image_480x270 { get; set; } = string.Empty;  // thumbnail
    public string Headline { get; set; } = string.Empty;       // short description 😂
}