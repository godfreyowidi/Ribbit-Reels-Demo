using System.ComponentModel.DataAnnotations;

namespace RibbitReels.Data.Models;
public enum UserRole
{
    User,
    Admin
}

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string DisplayName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    [Required]
    public string AuthProvider { get; set; } = "local";

    public string? ProviderUserId { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public string? PasswordHash { get; set; }

    public ICollection<LearningProgress> Progress { get; set; } = new List<LearningProgress>();
    public ICollection<UserBranchAssignment> AssignedBranches { get; set; } = new List<UserBranchAssignment>();
}
