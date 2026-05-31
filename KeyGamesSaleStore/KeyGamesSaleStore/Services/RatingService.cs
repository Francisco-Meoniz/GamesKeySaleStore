using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Services;

public class RatingService : IRatingService
{
    private readonly AppDbContext _db;

    public RatingService(AppDbContext db) => _db = db;

    public async Task<bool> CanRateAsync(int buyerId, int orderId, int sellerProfileId)
    {
        // Verificar que la orden existe, está pagada y es del comprador
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o =>
                o.Id == orderId &&
                o.BuyerId == buyerId &&
                o.Status == OrderStatus.Paid);

        if (order == null) return false;

        // Verificar que compró al vendedor en esa orden
        var boughtFromSeller = order.Items.Any(i => i.SellerProfileId == sellerProfileId);
        if (!boughtFromSeller) return false;

        // Verificar que no calificó ya esa orden+vendedor
        var alreadyRated = await _db.SellerRatings.AnyAsync(r =>
            r.BuyerId == buyerId &&
            r.OrderId == orderId &&
            r.SellerProfileId == sellerProfileId);

        return !alreadyRated;
    }

    public async Task<RatingDto> CreateRatingAsync(int buyerId, CreateRatingDto dto)
    {
        if (dto.Stars < 1 || dto.Stars > 5)
            throw new InvalidOperationException("Las estrellas deben ser entre 1 y 5.");

        var canRate = await CanRateAsync(buyerId, dto.OrderId, dto.SellerProfileId);
        if (!canRate)
            throw new InvalidOperationException("No podés calificar este vendedor.");

        var buyer = await _db.Users.FindAsync(buyerId)!;

        var rating = new SellerRating
        {
            SellerProfileId = dto.SellerProfileId,
            BuyerId = buyerId,
            OrderId = dto.OrderId,
            Stars = dto.Stars,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _db.SellerRatings.Add(rating);

        // Actualizar promedio del vendedor
        var seller = await _db.SellerProfiles.FindAsync(dto.SellerProfileId)!;
        var allRatings = await _db.SellerRatings
            .Where(r => r.SellerProfileId == dto.SellerProfileId)
            .ToListAsync();

        seller!.TotalRatings = allRatings.Count + 1;
        seller.AverageRating = (allRatings.Sum(r => r.Stars) + dto.Stars) / (double)seller.TotalRatings;

        await _db.SaveChangesAsync();

        return new RatingDto
        {
            Id = rating.Id,
            BuyerUsername = buyer!.Username,
            BuyerAvatarUrl = buyer.AvatarUrl,
            Stars = rating.Stars,
            Comment = rating.Comment,
            CreatedAt = rating.CreatedAt
        };
    }

    public async Task<SellerRatingsDto> GetSellerRatingsAsync(int sellerProfileId)
    {
        var seller = await _db.SellerProfiles
            .Include(s => s.Ratings)
                .ThenInclude(r => r.Buyer)
            .FirstOrDefaultAsync(s => s.Id == sellerProfileId)
            ?? throw new InvalidOperationException("Vendedor no encontrado.");

        return new SellerRatingsDto
        {
            DisplayName = seller.DisplayName,
            AverageRating = seller.AverageRating,
            TotalRatings = seller.TotalRatings,
            Ratings = seller.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RatingDto
                {
                    Id = r.Id,
                    BuyerUsername = r.Buyer.Username,
                    BuyerAvatarUrl = r.Buyer.AvatarUrl,
                    Stars = r.Stars,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
        };
    }
}
