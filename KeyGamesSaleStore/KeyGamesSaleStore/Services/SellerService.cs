using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Services;

public class SellerService : ISellerService
{
    private readonly AppDbContext _db;

    public SellerService(AppDbContext db) => _db = db;

    private async Task<SellerProfile> GetProfileAsync(int userId)
    {
        return await _db.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == userId)
            ?? throw new InvalidOperationException("Perfil de vendedor no encontrado.");
    }

    public async Task<SellerStatsDto> GetStatsAsync(int userId)
    {
        var profile = await GetProfileAsync(userId);

        var keys = await _db.GameKeys
            .Where(k => k.SellerProfileId == profile.Id)
            .ToListAsync();

        var earnings = await _db.OrderItems
            .Where(oi => oi.SellerProfileId == profile.Id)
            .SumAsync(oi => (decimal?)oi.UnitPrice) ?? 0;

        return new SellerStatsDto
        {
            DisplayName = profile.DisplayName,
            TotalSales = profile.TotalSales,
            AverageRating = profile.AverageRating,
            ActiveKeys = keys.Count(k => !k.IsSold && k.IsActive),
            SoldKeys = keys.Count(k => k.IsSold),
            TotalEarnings = earnings
        };
    }

    public async Task<List<SellerKeyDto>> GetMyKeysAsync(int userId)
    {
        var profile = await GetProfileAsync(userId);

        return await _db.GameKeys
            .Where(k => k.SellerProfileId == profile.Id)
            .Include(k => k.Game)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new SellerKeyDto
            {
                Id = k.Id,
                GameTitle = k.Game.Title,
                GameImageUrl = k.Game.ImageUrl,
                Platform = k.Game.Platform,
                Price = k.Price,
                IsActive = k.IsActive,
                IsSold = k.IsSold,
                CreatedAt = k.CreatedAt,
                SoldAt = k.SoldAt
            }).ToListAsync();
    }

    public async Task<SellerKeyDto> PublishKeyAsync(int userId, PublishKeyDto dto)
    {
        var profile = await GetProfileAsync(userId);

        var game = await _db.Games.FindAsync(dto.GameId)
            ?? throw new InvalidOperationException("Juego no encontrado.");

        if (dto.Price <= 0)
            throw new InvalidOperationException("El precio debe ser mayor a 0.");

        // Verificar que no haya una key igual ya publicada
        var exists = await _db.GameKeys.AnyAsync(k =>
            k.SellerProfileId == profile.Id &&
            k.KeyCode == dto.KeyCode &&
            !k.IsSold);

        if (exists)
            throw new InvalidOperationException("Ya tenés esa key publicada.");

        var key = new GameKey
        {
            GameId = dto.GameId,
            SellerProfileId = profile.Id,
            KeyCode = dto.KeyCode,
            Price = dto.Price,
            IsActive = true,
            IsSold = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.GameKeys.Add(key);
        await _db.SaveChangesAsync();

        return new SellerKeyDto
        {
            Id = key.Id,
            GameTitle = game.Title,
            GameImageUrl = game.ImageUrl,
            Platform = game.Platform,
            Price = key.Price,
            IsActive = key.IsActive,
            IsSold = key.IsSold,
            CreatedAt = key.CreatedAt
        };
    }

    public async Task ToggleKeyActiveAsync(int userId, int keyId)
    {
        var profile = await GetProfileAsync(userId);

        var key = await _db.GameKeys.FirstOrDefaultAsync(k =>
            k.Id == keyId && k.SellerProfileId == profile.Id)
            ?? throw new InvalidOperationException("Key no encontrada.");

        if (key.IsSold)
            throw new InvalidOperationException("No podés modificar una key ya vendida.");

        key.IsActive = !key.IsActive;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateKeyPriceAsync(int userId, int keyId, UpdateKeyPriceDto dto)
    {
        var profile = await GetProfileAsync(userId);

        var key = await _db.GameKeys.FirstOrDefaultAsync(k =>
            k.Id == keyId && k.SellerProfileId == profile.Id)
            ?? throw new InvalidOperationException("Key no encontrada.");

        if (key.IsSold)
            throw new InvalidOperationException("No podés modificar una key ya vendida.");

        if (dto.Price <= 0)
            throw new InvalidOperationException("El precio debe ser mayor a 0.");

        key.Price = dto.Price;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteKeyAsync(int userId, int keyId)
    {
        var profile = await GetProfileAsync(userId);

        var key = await _db.GameKeys.FirstOrDefaultAsync(k =>
            k.Id == keyId && k.SellerProfileId == profile.Id)
            ?? throw new InvalidOperationException("Key no encontrada.");

        if (key.IsSold)
            throw new InvalidOperationException("No podés eliminar una key ya vendida.");

        _db.GameKeys.Remove(key);
        await _db.SaveChangesAsync();
    }
}