using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RibbitReels.Data.DTOs;
using RibbitReels.Data.Models;
using RibbitReels.Services.Interfaces;

namespace RibbitReels.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    // POST: api/branch
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        if (request == null || request.Title == null)
            return BadRequest(new { error = "Title is required." });

        var branch = new Branch
        {
            Title = request.Title,
            Description = request.Description ?? string.Empty
        };

        var result = await _branchService.CreateBranchAsync(branch);

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return StatusCode(result.StatusCode, result.Value);
    }

    // GET: api/branch
    [HttpGet]
    public async Task<IActionResult> GetAllBranches()
    {
        var result = await _branchService.GetAllBranchesAsync();

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        var response = result.Value.Select(branch => new BranchResponse
        {
            Id = branch.Id,
            Title = branch.Title,
            Description = branch.Description,
            Leafs = branch.Leafs?.Select(leaf => new LeafResponse
            {
                Id = leaf.Id,
                Title = leaf.Title,
                Order = leaf.Order
            }).ToList() ?? new List<LeafResponse>()
        });

        return Ok(response);

    }

    // GET: api/branch/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBranchById(Guid id)
    {
        var result = await _branchService.GetBranchByIdAsync(id);

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        var branch = result.Value;
        return Ok(new BranchResponse
        {
            Id = branch.Id,
            Title = branch.Title,
            Description = branch.Description,
            Leafs = branch.Leafs?.Select(leaf => new LeafResponse
            {
                Id = leaf.Id,
                Title = leaf.Title,
                Order = leaf.Order
            }).ToList() ?? new List<LeafResponse>()

        });

    }

    // GET: api/branch/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] Branch updatedBranch)
    {
        var result = await _branchService.UpdateBranchAsync(id, updatedBranch);

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return StatusCode(result.StatusCode, result.Value);
    }

    // DELETE: api/branch/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        var result = await _branchService.DeleteBranchAsync(id);

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return StatusCode(result.StatusCode, new { success = result.Value });
    }
}