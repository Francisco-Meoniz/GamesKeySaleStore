using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db) => _db = db;

    public async Task<CartSummaryDto> GetCartAsync(int userId)
    {
        var items = await _db.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.GameKey)
                .ThenInclude(k => k.Game)
            .Include(c => c.GameKey)
                .ThenInclude(k => k.Seller)
            .OrderBy(c => c.AddedAt)
            .ToListAsync();

        return MapToSummary(items);
    }

    public async Task<CartSummaryDto> AddToCartAsync(int userId, int gameKeyId)
    {
        // Verificar límite de 3 keys
        var count = await _db.CartItems.CountAsync(c => c.UserId == userId);
        if (count >= 3)
            throw new InvalidOperationException("El carrito no puede tener más de 3 keys.");

        // Verificar que la key existe, no está vendida y está activa
        var key = await _db.GameKeys
            .Include(k => k.Game)
            .Include(k => k.Seller)
            .FirstOrDefaultAsync(k => k.Id == gameKeyId && !k.IsSold && k.IsActive)
            ?? throw new InvalidOperationException("La key no está disponible.");

        // Verificar que no esté ya en el carrito
        var exists = await _db.CartItems.AnyAsync(c =>
            c.UserId == userId && c.GameKeyId == gameKeyId);
        if (exists)
            throw new InvalidOperationException("Esta key ya está en el carrito.");

        var cartItem = new CartItem
        {
            UserId = userId,
            GameKeyId = gameKeyId,
            AddedAt = DateTime.UtcNow
        };

        _db.CartItems.Add(cartItem);
        await _db.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<CartSummaryDto> RemoveFromCartAsync(int userId, int cartItemId)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(c =>
            c.Id == cartItemId && c.UserId == userId)
            ?? throw new InvalidOperationException("Item no encontrado.");

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task ClearCartAsync(int userId)
    {
        var items = await _db.CartItems.Where(c => c.UserId == userId).ToListAsync()
;
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    private static CartSummaryDto MapToSummary(List<CartItem> items) => new()
    {
        Items = items.Select(c => new CartItemDto
        {
            Id = c.Id,
            GameId = c.GameKey.GameId,
            GameKeyId = c.GameKeyId,
            GameTitle = c.GameKey.Game.Title,
            GameImageUrl = c.GameKey.Game.ImageUrl,
            Platform = c.GameKey.Game.Platform,
            Price = c.GameKey.Price,
            SellerName = c.GameKey.Seller.DisplayName,
            AddedAt = c.AddedAt
        }).ToList(),
        Total = items.Sum(c => c.GameKey.Price),
        Count = items.Count
    };
}