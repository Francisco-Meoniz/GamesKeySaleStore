using GameKeysStore.API.Models.DTOs;

namespace GameKeysStore.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto dto);
    Task<AuthResponseDto> VerifyEmailAsync(string token);
    Task ResendVerificationAsync(string email);
}