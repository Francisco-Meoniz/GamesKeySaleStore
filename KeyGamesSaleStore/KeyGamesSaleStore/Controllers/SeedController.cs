using GameKeysStore.API.Data;
using GameKeysStore.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GameKeysStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _db;

    public SeedController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Seed()
    {
        if (_db.Games.Any())
            return BadRequest(new { error = "Ya hay datos cargados." });

        // Vendedor de prueba
        var user = new User
        {
            Username = "pepe",
            Email = "pepe@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Seller,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var seller = new SellerProfile
        {
            UserId = user.Id,
            DisplayName = "Pepe Store",
            TotalSales = 142,
            AverageRating = 4.7,
            TotalRatings = 98,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
        _db.SellerProfiles.Add(seller);

        // Segundo vendedor
        var user2 = new User
        {
            Username = "marcos",
            Email = "marcos@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Seller,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };
        _db.Users.Add(user2);
        await _db.SaveChangesAsync();

        var seller2 = new SellerProfile
        {
            UserId = user2.Id,
            DisplayName = "Marcos Keys",
            TotalSales = 38,
            AverageRating = 4.2,
            TotalRatings = 31,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };
        _db.SellerProfiles.Add(seller2);
        await _db.SaveChangesAsync();

        // Juegos
        var games = new List<Game>
        {
            new() {
                Title = "Elden Ring",
                Description = "Un RPG de acción de mundo abierto desarrollado por FromSoftware.",
                Platform = "Steam",
                ImageUrl = "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1245620/header.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Title = "Cyberpunk 2077",
                Description = "RPG de acción y aventura de mundo abierto ambientado en Night City.",
                Platform = "Steam",
                ImageUrl = "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1091500/header.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Title = "Red Dead Redemption 2",
                Description = "Una épica historia sobre el ocaso de la era de los forajidos.",
                Platform = "Steam",
                ImageUrl = "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1174180/header.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Title = "Hogwarts Legacy",
                Description = "Viví la experiencia de ser estudiante en el mundo mágico de Hogwarts.",
                Platform = "Steam",
                ImageUrl = "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/990080/header.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Title = "God of War",
                Description = "Kratos y su hijo Atreus se embarcan en un viaje por los reinos nórdicos.",
                Platform = "Steam",
                ImageUrl = "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1593500/header.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Title = "Hollow Knight",
                Description = "Un juego de acción y aventura ambientado en un reino de insectos.",
                Platform = "Steam",
                ImageUrl = "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/367520/header.jpg",
                CreatedAt = DateTime.UtcNow
            },
        };

        _db.Games.AddRange(games);
        await _db.SaveChangesAsync();

        // Keys para cada juego
        var prices1 = new[] { 12999m, 13499m, 11999m };
        var prices2 = new[] { 8999m, 9499m, 8499m };
        var prices3 = new[] { 10999m, 11499m };
        var prices4 = new[] { 9999m, 10499m };
        var prices5 = new[] { 7999m, 8299m, 7499m };
        var prices6 = new[] { 2999m, 3199m };

        var allPrices = new[] { prices1, prices2, prices3, prices4, prices5, prices6 };

        for (int i = 0; i < games.Count; i++)
        {
            var gamePrices = allPrices[i];
            for (int j = 0; j < gamePrices.Length; j++)
            {
                _db.GameKeys.Add(new GameKey
                {
                    GameId = games[i].Id,
                    SellerProfileId = j % 2 == 0 ? seller.Id : seller2.Id,
                    KeyCode = $"XXXX-YYYY-ZZZZ-{i}{j}",
                    Price = gamePrices[j],
                    IsActive = true,
                    IsSold = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Datos de prueba cargados correctamente." });
    }
}