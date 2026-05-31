using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Services;

public class GamesService : IGamesService
{
    private readonly AppDbContext _db;

    public GamesService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<GameDto>> GetAllGamesAsync()
    {
        return await _db.Games.Select(g => new GameDto
        {
            Id = g.Id,
            Title = g.Title,
            Description = g.Description,
            ImageUrl = g.ImageUrl,
            Platform = g.Platform,
            AvailableStock = g.Keys.Count(k => !k.IsSold)
        }).ToListAsync();
    }

    public async Task<GameDto?> GetGameByIdAsync(int id)
    {
        return await _db.Games
            .Where(g => g.Id == id)
            .Select(g => new GameDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                ImageUrl = g.ImageUrl,
                Platform = g.Platform,
                AvailableStock = g.Keys.Count(k => !k.IsSold)
            }).FirstOrDefaultAsync();
    }

    public async Task<int> GetAvailableStockAsync(int gameId)
    {
        return await _db.GameKeys
            .CountAsync(k => k.GameId == gameId && !k.IsSold);
    }

    public async Task<List<GameKeyDto>> GetAvailableKeysAsync(int gameId)
    {
        var keys = await _db.GameKeys
            .Where(k => k.GameId == gameId && !k.IsSold && k.IsActive)
            .Include(k => k.Seller)
            .OrderBy(k => k.Price)
            .ToListAsync();

        if (!keys.Any()) return new List<GameKeyDto>();

        decimal cheapestPrice = keys.First().Price;

        return keys.Select(k => new GameKeyDto
        {
            Id = k.Id,
            Price = k.Price,
            IsCheapest = k.Price == cheapestPrice,
            Seller = new SellerSummaryDto
            {
                DisplayName = k.Seller.DisplayName,
                AverageRating = k.Seller.AverageRating,
                TotalSales = k.Seller.TotalSales,
                MonthsOnPlatform = (int)((DateTime.UtcNow - k.Seller.CreatedAt).TotalDays / 30)
            }
        }).ToList();
    }
}