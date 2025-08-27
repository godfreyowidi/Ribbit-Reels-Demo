using System.Text.Json.Serialization;

namespace RibbitReels.Data.DTOs;

public class YouTubeVideo
{
    public string VideoId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? YouTubeVideoId { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class YouTubeSearchResult
{
    public List<YouTubeVideo> Videos { get; }
    public string? NextPageToken { get; }
    public string? PrevPageToken { get; }

    public YouTubeSearchResult(List<YouTubeVideo> videos, string? nextPageToken, string? prevPageToken)
    {
        Videos = videos;
        NextPageToken = nextPageToken;
        PrevPageToken = prevPageToken;
    }
}

public class YouTubeSearchResponse
{
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    [JsonPropertyName("prevPageToken")]
    public string? PrevPageToken { get; set; }

    [JsonPropertyName("items")]
    public YouTubeSearchItem[]? Items { get; set; }

    [JsonPropertyName("pageInfo")]
    public YouTubePageInfo? PageInfo { get; set; }
}

public class YouTubeSearchItem
{
    [JsonPropertyName("id")]
    public YouTubeId? Id { get; set; }

    [JsonPropertyName("snippet")]
    public YouTubeSnippet? Snippet { get; set; }
}

public class YouTubeVideoDetailsResponse
{
    [JsonPropertyName("items")]
    public YouTubeVideoDetailItem[]? Items { get; set; }
}

public class YouTubeVideoDetailItem
{
    [JsonPropertyName("snippet")]
    public YouTubeSnippet? Snippet { get; set; }
}

public class YouTubeId
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("videoId")]
    public string? VideoId { get; set; }
}

public class YouTubeSnippet
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("thumbnails")]
    public YouTubeThumbnails? Thumbnails { get; set; }

    [JsonPropertyName("channelTitle")]
    public string? ChannelTitle { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTime? PublishedAt { get; set; }
}

public class YouTubeThumbnails
{
    [JsonPropertyName("high")]
    public YouTubeThumbnail? High { get; set; }

    [JsonPropertyName("medium")]
    public YouTubeThumbnail? Medium { get; set; }

    [JsonPropertyName("default")]
    public YouTubeThumbnail? Default { get; set; }
}

public class YouTubeThumbnail
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class YouTubePageInfo
{
    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }
}
