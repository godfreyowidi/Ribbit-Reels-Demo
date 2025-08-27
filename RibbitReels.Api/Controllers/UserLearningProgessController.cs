using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RibbitReels.Data.DTOs;
using RibbitReels.Services.Interfaces;
using System.Security.Claims;

namespace RibbitReels.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserLearningProgressController : ControllerBase
{
    private readonly ILearningProgressService _learningProgressService;

    public UserLearningProgressController(ILearningProgressService learningProgressService)
    {
        _learningProgressService = learningProgressService;
    }

    // GET: api/UserLearningProgress?userId={userId}&branchId={branchId}
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress([FromQuery] Guid branchId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { error = "Invalid user ID." });

        var result = await _learningProgressService.GetProgressAsync(userId, branchId);

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return Ok(result.Value);
    }


    // PUT: api/UserLearningProgress
    [HttpPut("progress")]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
    {
        try
        {
            if (request == null || request.BranchId == Guid.Empty)
                return BadRequest(new { error = "Invalid request payload." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { error = "Invalid user ID." });

            request.UserId = userId;

            var result = await _learningProgressService.UpdateProgressAsync(request);

            if (!result.IsSuccessful)
                return StatusCode(result.StatusCode, new { error = result.FailureMessage });

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedProgress()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { error = "Invalid user ID" });
            
        var result = await _learningProgressService.GetCompletedBranchesAsync(userId);

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { message = result.FailureMessage });

        return Ok(result.Value);
    }

}
