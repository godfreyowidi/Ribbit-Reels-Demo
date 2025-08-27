using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RibbitReels.Data.Configs;
using RibbitReels.Data.DTOs;
using RibbitReels.Services.Shared;

namespace RibbitReels.Services.Implementations;

public class YouTubeRepository
{
    private readonly YouTubeConfiguration _youTubeConfig;
    private readonly HttpClient _httpClient;

    public YouTubeRepository(
        IOptions<YouTubeConfiguration> options,
        HttpClient httpClient)
    {
        _youTubeConfig = options.Value;
        _httpClient = httpClient;
    }

    public async Task<OperationResult<YouTubeSearchResult>> SearchVideosAsync(
        string subject,
        int maxResults = 5,
        string? pageToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subject))
                return OperationResult<YouTubeSearchResult>.Fail("Subject cannot be empty.", HttpStatusCode.BadRequest);

            var url = $"{_youTubeConfig.BaseUrl}/search" +
                      $"?q={Uri.EscapeDataString(subject)}" +
                      $"&part=snippet" +
                      $"&type=video" +
                      $"&videoDuration=short" +
                      $"&maxResults={maxResults}" +
                      $"&key={_youTubeConfig.ApiKey}";

            if (!string.IsNullOrEmpty(pageToken))
                url += $"&pageToken={pageToken}";

            var response = await _httpClient.GetAsync(url);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return OperationResult<YouTubeSearchResult>.Fail(
                    $"YT API error: {response.StatusCode}, Body: {responseString}",
                    HttpStatusCode.BadGateway);
            }

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseString));
            var data = await JsonSerializer.DeserializeAsync<YouTubeSearchResponse>(stream);

            if (data?.Items == null || data.Items.Length == 0)
                return OperationResult<YouTubeSearchResult>.Fail("No videos found.", HttpStatusCode.NotFound);

            var results = data.Items.Select(item => new YouTubeVideo
            {
                VideoId = item.Id?.VideoId ?? string.Empty,
                Title = item.Snippet?.Title ?? "Untitled",
                Description = item.Snippet?.Description ?? string.Empty,
                ThumbnailUrl = GetBestThumbnail(item.Snippet?.Thumbnails),
            }).ToList();

            var searchResult = new YouTubeSearchResult(
                results,
                data.NextPageToken,
                data.PrevPageToken);

            return OperationResult<YouTubeSearchResult>.Success(searchResult);
        }
        catch (Exception ex)
        {
            return OperationResult<YouTubeSearchResult>.Fail(ex, "Failed to fetch tutorials from YouTube.");
        }
    }

    public async Task<OperationResult<YouTubeVideo>> GetVideoDetailsAsync(string videoId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(videoId))
                return OperationResult<YouTubeVideo>.Fail("VideoId cannot be empty.", HttpStatusCode.BadRequest);

            var url = $"{_youTubeConfig.BaseUrl}/videos" +
                      $"?id={Uri.EscapeDataString(videoId)}" +
                      $"&part=snippet" +
                      $"&key={_youTubeConfig.ApiKey}";

            var response = await _httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return OperationResult<YouTubeVideo>.Fail(
                    $"YT API error: {response.StatusCode}, Body: {responseString}",
                    HttpStatusCode.BadGateway);
            }

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseString));
            var data = await JsonSerializer.DeserializeAsync<YouTubeVideoDetailsResponse>(stream);

            if (data?.Items == null || data.Items.Length == 0)
                return OperationResult<YouTubeVideo>.Fail("Video not found.", HttpStatusCode.NotFound);

            var snippet = data.Items[0].Snippet;
            var video = new YouTubeVideo
            {
                VideoId = videoId,
                Title = snippet?.Title ?? "Untitled",
                Description = snippet?.Description ?? string.Empty,
                ThumbnailUrl = GetBestThumbnail(snippet?.Thumbnails),
            };

            return OperationResult<YouTubeVideo>.Success(video);
        }
        catch (Exception ex)
        {
            return OperationResult<YouTubeVideo>.Fail(ex, "Failed to fetch video details from YouTube.");
        }
    }

    private static string GetBestThumbnail(YouTubeThumbnails? thumbnails)
    {
        return thumbnails?.High?.Url
            ?? thumbnails?.Medium?.Url
            ?? thumbnails?.Default?.Url
            ?? string.Empty;
    }
}
