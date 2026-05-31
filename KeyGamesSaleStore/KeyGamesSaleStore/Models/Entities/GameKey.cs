namespace GameKeysStore.API.Models.Entities;

public class GameKey
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int SellerProfileId { get; set; }
    public string KeyCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsSold { get; set; } = false;
    public bool IsActive { get; set; } = true; // el vendedor puede desactivarla
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SoldAt { get; set; }

    public Game Game { get; set; } = null!;
    public SellerProfile Seller { get; set; } = null!;
    public OrderItem? OrderItem { get; set; }
}