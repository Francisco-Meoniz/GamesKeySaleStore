namespace GameKeysStore.API.Models.DTOs;

public class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsSeller { get; set; } = false;
    public string? SellerDisplayName { get; set; }

    // Obligatorios solo si IsSeller = true (se valida en el service)
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DNI { get; set; }
    public DateTime? BirthDate { get; set; }

    // Siempre opcional
    public string? MercadoPagoAccount { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int UserId { get; set; }
    public bool IsEmailVerified { get; set; }
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SellerDisplayName { get; set; } // solo si es Seller
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DNI { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? MercadoPagoAccount { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? SellerDisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DNI { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? MercadoPagoAccount { get; set; }
}

public class ResendVerificationDto
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyEmailDto
{
    public string Token { get; set; } = string.Empty;
}