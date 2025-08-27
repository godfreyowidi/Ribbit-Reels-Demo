namespace RibbitReels.Data.DTOs;

public class RegisterUserRequest
{
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
