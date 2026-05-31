using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db) => _db = db;

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        var recentOrders = await _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                Id = o.Id,
                BuyerEmail = o.BuyerEmail,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            }).ToListAsync();

        return new AdminStatsDto
        {
            TotalUsers = await _db.Users.CountAsync(),
            TotalSellers = await _db.Users.CountAsync(u => u.Role == UserRole.Seller),
            TotalGames = await _db.Games.CountAsync(),
            TotalOrders = await _db.Orders.CountAsync(),
            PaidOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Paid),
            TotalRevenue = await _db.Orders
                .Where(o => o.Status == OrderStatus.Paid)
                .SumAsync(o => o.TotalAmount),
            RecentOrders = recentOrders
        };
    }

    public async Task<List<UserListDto>> GetUsersAsync()
    {
        return await _db.Users
            .Include(u => u.SellerProfile)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt,
                SellerDisplayName = u.SellerProfile != null ? u.SellerProfile.DisplayName : null,
                TotalSales = u.SellerProfile != null ? u.SellerProfile.TotalSales : null
            }).ToListAsync();
    }

    public async Task UpdateUserRoleAsync(int userId, string role)
    {
        var user = await _db.Users.Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (!Enum.TryParse<UserRole>(role, true, out var newRole))
            throw new InvalidOperationException("Rol inválido.");

        user.Role = newRole;

        // Si se convierte en vendedor y no tiene perfil, crearlo
        if (newRole == UserRole.Seller && user.SellerProfile == null)
        {
            _db.SellerProfiles.Add(new SellerProfile
            {
                UserId = user.Id,
                DisplayName = user.Username,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task ToggleBanUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        user.IsBanned = !user.IsBanned;
        await _db.SaveChangesAsync();
    }

    public async Task<GameDto> CreateGameAsync(CreateGameDto dto)
    {
        var game = new Game
        {
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            Platform = dto.Platform,
            CreatedAt = DateTime.UtcNow
        };

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        return new GameDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            ImageUrl = game.ImageUrl,
            Platform = game.Platform,
            AvailableStock = 0
        };
    }

    public async Task<GameDto> UpdateGameAsync(int gameId, UpdateGameDto dto)
    {
        var game = await _db.Games.FindAsync(gameId)
            ?? throw new InvalidOperationException("Juego no encontrado.");

        game.Title = dto.Title;
        game.Description = dto.Description;
        game.ImageUrl = dto.ImageUrl;
        game.Platform = dto.Platform;
        await _db.SaveChangesAsync();

        return new GameDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            ImageUrl = game.ImageUrl,
            Platform = game.Platform,
            AvailableStock = await _db.GameKeys.CountAsync(k => k.GameId == game.Id && !k.IsSold && k.IsActive)
        };
    }

    public async Task DeleteGameAsync(int gameId)
    {
        var game = await _db.Games.FindAsync(gameId)
            ?? throw new InvalidOperationException("Juego no encontrado.");

        _db.Games.Remove(game);
        await _db.SaveChangesAsync();
    }

    public async Task<List<RecentOrderDto>> GetAllOrdersAsync()
    {
        return await _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new RecentOrderDto
            {
                Id = o.Id,
                BuyerEmail = o.BuyerEmail,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            }).ToListAsync();
    }
}