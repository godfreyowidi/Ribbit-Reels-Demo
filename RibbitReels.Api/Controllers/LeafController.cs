using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RibbitReels.Api.DTOs;
using RibbitReels.Data.DTOs;
using RibbitReels.Data.Models;
using RibbitReels.Services.Implementations;
using RibbitReels.Services.Interfaces;

namespace RibbitReels.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeafController : ControllerBase
{
    private readonly ILeafService _leafService;
    private readonly YouTubeRepository _youTubeRepository;

    public LeafController(ILeafService leafService, YouTubeRepository youTubeRepository)
    {
        _leafService = leafService;
        _youTubeRepository = youTubeRepository;
    }

    // POST: api/leaf/{branchId}/manual
    [HttpPost("{branchId}/manual")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateManualLeaf(Guid branchId, [FromForm] CreateManualLeafRequest request)
    {
        if (request.VideoFile == null || request.VideoFile.Length == 0)
            return BadRequest(new { error = "Video file is required." });

        var result = await _leafService.CreateManualLeafAsync(branchId, request);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return CreatedAtAction(nameof(GetLeafById), new { id = result.Value.Id }, result.Value);
    }

    // POST: api/leaf/{branchId}/youtube
    [HttpPost("{branchId}/youtube")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddYouTubeLeaf(Guid branchId, [FromBody] CreateYouTubeLeafRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VideoId))
            return BadRequest(new { error = "YT VideoId is required." });

        var videoResult = await _youTubeRepository.GetVideoDetailsAsync(request.VideoId);
        if (!videoResult.IsSuccessful)
            return StatusCode(videoResult.StatusCode, new { error = videoResult.FailureMessage });

        var result = await _leafService.CreateYouTubeLeafAsync(branchId, videoResult.Value);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return CreatedAtAction(nameof(GetLeafById), new { id = result.Value.Id }, result.Value);
    }

    // GET: api/leaf/{id}
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeafById(Guid id)
    {
        var result = await _leafService.GetLeafByIdAsync(id);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return Ok(result.Value);
    }

    // GET: api/leaf
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeafs()
    {
        var result = await _leafService.GetLeafsAsync();
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return Ok(result.Value);
    }

    // GET: api/leaf/branch/{branchId}
    [HttpGet("branch/{branchId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeafsByBranchId(Guid branchId)
    {
        var result = await _leafService.GetLeafsByBranchIdAsync(branchId);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return Ok(result.Value);
    }

    // PUT: api/leaf/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateLeaf(Guid id, [FromBody] UpdateLeafRequest request)
    {
        var updatedLeaf = new Leaf
        {
            Title = request.Title,
            Order = request.Order,
        };

        var result = await _leafService.UpdateLeafAsync(id, updatedLeaf);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return Ok(result.Value);
    }

    // DELETE: api/leaf/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLeaf(Guid id)
    {
        var result = await _leafService.DeleteLeafAsync(id);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return NoContent();
    }
}
