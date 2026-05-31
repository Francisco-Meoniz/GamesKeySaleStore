using GameKeysStore.API.Models.DTOs;

namespace GameKeysStore.API.Services.Interfaces;

public interface IRatingService
{
    Task<RatingDto> CreateRatingAsync(int buyerId, CreateRatingDto dto);
    Task<SellerRatingsDto> GetSellerRatingsAsync(int sellerProfileId);
    Task<bool> CanRateAsync(int buyerId, int orderId, int sellerProfileId);
}