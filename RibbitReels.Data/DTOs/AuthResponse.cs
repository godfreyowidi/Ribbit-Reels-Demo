namespace RibbitReels.Data.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
}
