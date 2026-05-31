namespace GameKeysStore.API.Models.Entities;

public enum UserRole { User, Seller, Admin }

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public DateTime? LastVerificationEmailSent { get; set; }
    // Solo relevante si es Seller
    public SellerProfile? SellerProfile { get; set; }
    public bool IsBanned { get; set; } = false;
    public string? AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
}