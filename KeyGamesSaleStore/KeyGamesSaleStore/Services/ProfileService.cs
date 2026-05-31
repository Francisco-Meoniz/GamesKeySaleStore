using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<ProfileDto> GetProfileAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        return MapToDto(user);
    }

    public async Task<ProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _db.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        // Verificar que el username no esté en uso por otro
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username && u.Id != userId))
            throw new InvalidOperationException("El nombre de usuario ya está en uso.");

        // Verificar que el email no esté en uso por otro
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId))
            throw new InvalidOperationException("El email ya está en uso.");

        user.Username = dto.Username;
        user.Email = dto.Email;

        // Actualizar perfil de vendedor si corresponde
        if (user.Role == UserRole.Seller && user.SellerProfile != null)
        {
            if (!string.IsNullOrEmpty(dto.SellerDisplayName))
                user.SellerProfile.DisplayName = dto.SellerDisplayName;

            // 👇 NUEVOS CAMPOS
            if (!string.IsNullOrEmpty(dto.FirstName))
                user.SellerProfile.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                user.SellerProfile.LastName = dto.LastName;

            if (!string.IsNullOrEmpty(dto.DNI))
            {
                // Verificar que el DNI no esté en uso por otro vendedor
                var dniExists = await _db.SellerProfiles.AnyAsync(s => s.DNI == dto.DNI && s.UserId != userId);
                if (dniExists)
                    throw new InvalidOperationException("El DNI ya está registrado por otro vendedor.");
                user.SellerProfile.DNI = dto.DNI;
            }

            if (dto.BirthDate.HasValue)
                user.SellerProfile.BirthDate = dto.BirthDate;

            if (!string.IsNullOrEmpty(dto.MercadoPagoAccount))
                user.SellerProfile.MercadoPagoAccount = dto.MercadoPagoAccount;
        }

        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("La contraseña actual es incorrecta.");

        if (dto.NewPassword.Length < 6)
            throw new InvalidOperationException("La nueva contraseña debe tener al menos 6 caracteres.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
    }

    public async Task<string> UploadAvatarAsync(int userId, IFormFile file)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        // Validar archivo
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            throw new InvalidOperationException("Solo se permiten imágenes JPG, PNG o WebP.");

        if (file.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("La imagen no puede superar los 2MB.");

        // Crear carpeta si no existe
        var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "avatars");
        Directory.CreateDirectory(uploadsFolder);

        // Borrar avatar anterior si existe
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var oldPath = Path.Combine(_env.WebRootPath ?? "wwwroot", user.AvatarUrl.TrimStart('/'));
            if (File.Exists(oldPath)) File.Delete(oldPath);
        }

        // Guardar nuevo avatar
        var ext = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"avatar_{userId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        user.AvatarUrl = $"/avatars/{fileName}";
        await _db.SaveChangesAsync();

        return user.AvatarUrl;
    }

    public async Task DeleteAccountAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.SellerProfile)
            .ThenInclude(s => s.Keys)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        // Borrar keys del vendedor si tiene
        if (user.SellerProfile != null)
        {
            var keys = await _db.GameKeys
                .Where(k => k.SellerProfileId == user.SellerProfile.Id && !k.IsSold)
                .ToListAsync();
            _db.GameKeys.RemoveRange(keys);
        }

        // Desvincular órdenes
        var orders = await _db.Orders.Where(o => o.BuyerId == userId).ToListAsync();
        foreach (var order in orders)
        {
            order.BuyerId = null;
        }

        await _db.SaveChangesAsync();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task<List<PurchaseHistoryDto>> GetPurchaseHistoryAsync(int userId)
    {
        return await _db.Orders
            .Where(o => o.BuyerId == userId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Game)
            .Include(o => o.Items)
            .ThenInclude(i => i.GameKey)
            .Include(o => o.Items)
            .ThenInclude(i => i.Seller)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new PurchaseHistoryDto
            {
                OrderId = o.Id,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(i => new PurchaseItemDto
                {
                    GameTitle = i.Game.Title,
                    GameImageUrl = i.Game.ImageUrl,
                    Platform = i.Game.Platform,
                    KeyCode = o.Status == OrderStatus.Paid ? i.GameKey.KeyCode : "••••-••••-••••",
                    SellerName = i.Seller.DisplayName,
                    SellerProfileId = i.SellerProfileId,
                    UnitPrice = i.UnitPrice
                }).ToList()
            }).ToListAsync();
    }

    // 👇 MÉTODO MAPTODTO ACTUALIZADO
    private static ProfileDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role.ToString(),
        AvatarUrl = user.AvatarUrl,
        SellerDisplayName = user.SellerProfile?.DisplayName,
        CreatedAt = user.CreatedAt,
        // 👇 NUEVOS CAMPOS
        FirstName = user.SellerProfile?.FirstName,
        LastName = user.SellerProfile?.LastName,
        DNI = user.SellerProfile?.DNI,
        BirthDate = user.SellerProfile?.BirthDate,
        MercadoPagoAccount = user.SellerProfile?.MercadoPagoAccount
    };
}