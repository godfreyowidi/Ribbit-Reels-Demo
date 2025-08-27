using Microsoft.AspNetCore.Mvc;
using RibbitReels.Services.Interfaces;
using RibbitReels.Data.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace RibbitReels.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/users
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllUsersAsync();
        return result.IsSuccessful ? Ok(new { data = result.Value }) : BadRequest(result.FailureMessage);
    }

    // GET: api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.IsSuccessful ? Ok(new { data = result.Value }) : NotFound(result.FailureMessage);
    }

    // PUT: api/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.UpdateUserAsync(id, request);
        return result.IsSuccessful ? Ok(new { data = result.Value }) : NotFound(result.FailureMessage);
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.IsSuccessful ? NoContent() : NotFound(result.FailureMessage);
    }
}
