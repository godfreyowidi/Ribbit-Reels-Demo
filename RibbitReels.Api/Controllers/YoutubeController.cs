using Microsoft.AspNetCore.Mvc;
using RibbitReels.Services.Implementations;

namespace RibbitReels.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YoutubeController : ControllerBase
    {
        private readonly YouTubeRepository _youTubeRepository;
        private readonly ILogger<YoutubeController> _logger;

        public YoutubeController(YouTubeRepository youTubeRepository, ILogger<YoutubeController> logger)
        {
            _youTubeRepository = youTubeRepository;
            _logger = logger;
        }

        // GET /api/youtube/search?query=rainforest&maxResults=5&pageToken=...
        [HttpGet("search")]
        public async Task<IActionResult> SearchVideos([FromQuery] string query, [FromQuery] int maxResults = 5, [FromQuery] string? pageToken = null)
        {
            var result = await _youTubeRepository.SearchVideosAsync(query, maxResults, pageToken);

            if (!result.IsSuccessful)
            {
                _logger.LogWarning("YT search failed: {Message}", result.FailureMessage);
                return StatusCode(result.StatusCode, result.FailureMessage);
            }

            return Ok(result.Value);
        }

        // GET /api/youtube/{videoId}
        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetVideoDetails(string videoId)
        {
            var result = await _youTubeRepository.GetVideoDetailsAsync(videoId);

            if (!result.IsSuccessful)
            {
                _logger.LogWarning("YT video details fetch failed: {Message}", result.FailureMessage);
                return StatusCode(result.StatusCode, result.FailureMessage);
            }

            return Ok(result.Value);
        }
    }
}
