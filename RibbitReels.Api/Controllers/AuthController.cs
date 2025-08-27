using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RibbitReels.Data.DTOs;
using RibbitReels.Services.Interfaces;

namespace RibbitReels.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public  IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (Guid.TryParse(userId, out var id))
        {
            return Ok(new { id, email, role });
        }

        return Unauthorized();
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var result = await _userService.RegisterUserAsync(request);

        if (!result.IsSuccessful)
            return BadRequest(new { error = result.FailureMessage });

        return Ok(new { userId = result.Value.Id, email = result.Value.Email });
    }

    // POST: api/auth/register-admin
    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequest request)
    {
        var result = await _userService.RegisterAdminAsync(request);

        if (!result.IsSuccessful)
            return BadRequest(new { error = result.FailureMessage });

        return Ok(new { adminId = result.Value.Id, email = result.Value.Email });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);

        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return Ok(result.Value);
    }

    // POST: api/auth/google-login
    [HttpPost("google-login")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest(new { error = "Google ID token not provided or malformed request." });

        var result = await _userService.LoginWithGoogleAsync(request);
        if (!result.IsSuccessful)
            return StatusCode(result.StatusCode, new { error = result.FailureMessage });

        return Ok(result.Value);
    }
}
