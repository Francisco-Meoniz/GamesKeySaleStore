using GameKeysStore.API.Models.DTOs;

namespace GameKeysStore.API.Services.Interfaces;

public interface ISellerService
{
    Task<SellerStatsDto> GetStatsAsync(int userId);
    Task<List<SellerKeyDto>> GetMyKeysAsync(int userId);
    Task<SellerKeyDto> PublishKeyAsync(int userId, PublishKeyDto dto);
    Task ToggleKeyActiveAsync(int userId, int keyId);
    Task UpdateKeyPriceAsync(int userId, int keyId, UpdateKeyPriceDto dto);
    Task DeleteKeyAsync(int userId, int keyId);
}