using GameKeysStore.API.Models.DTOs;

namespace GameKeysStore.API.Services.Interfaces;

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(int userId);
    Task<ProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<string> UploadAvatarAsync(int userId, IFormFile file);
    Task DeleteAccountAsync(int userId);
    Task<List<PurchaseHistoryDto>> GetPurchaseHistoryAsync(int userId);
}