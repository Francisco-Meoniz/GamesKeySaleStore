using GameKeysStore.API.Models.DTOs;

namespace GameKeysStore.API.Services.Interfaces;

public interface ICartService
{
    Task<CartSummaryDto> GetCartAsync(int userId);
    Task<CartSummaryDto> AddToCartAsync(int userId, int gameKeyId);
    Task<CartSummaryDto> RemoveFromCartAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
}