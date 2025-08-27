using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RibbitReels.Data.DTOs;
using RibbitReels.Services.Interfaces;

namespace RibbitReels.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserBranchAssignmentController : ControllerBase
{
    private readonly IUserBranchAssignmentService _assignmentService;

    public UserBranchAssignmentController(IUserBranchAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    // POST: api/UserBranchAssignment/assign
    [Authorize(Roles = "Admin")]
    [HttpPost("assign")]
    public async Task<IActionResult> AssignBranch([FromBody] AssignBranchRequest request)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (adminId == null)
            return Forbid();

        var result = await _assignmentService.AssignBranchAsync(
            new InternalAssignBranchRequest
            {
                UserId = request.UserId,
                BranchId = request.BranchId,
                AssignedByManagerId = Guid.Parse(adminId)
            });

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, result.FailureMessage);

        return Ok(result.Value);
    }


    // GET: api/UserBranchAssignment/user/{userId}
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetAssignmentsByUser(Guid userId)
    {
        var result = await _assignmentService.GetAssignmentsByUserAsync(userId);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, result.FailureMessage);

        return Ok(result.Value);
    }

     // GET: api/UserBranchAssignment/all
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAssignments()
    {
        var result = await _assignmentService.GetAllAssignmentsAsync();

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return Ok(result.Value);
    }

    // GET: api/UserBranchAssignment/manager/{managerId}
    [HttpGet("manager/{managerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAssignmentsByManager(Guid managerId)
    {
        var result = await _assignmentService.GetAssignmentsByManagerAsync(managerId);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, result.FailureMessage);

        return Ok(result.Value);
    }

    // DELETE: api/UserBranchAssignment/{userId}/{branchId}
    [HttpDelete("{userId}/{branchId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnassignBranch(Guid userId, Guid branchId)
    {
        var result = await _assignmentService.UnassignBranchAsync(userId, branchId);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, result.FailureMessage);

        return Ok(result.Value);
    }
}
