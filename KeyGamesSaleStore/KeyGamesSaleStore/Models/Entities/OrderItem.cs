namespace GameKeysStore.API.Models.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int GameKeyId { get; set; }
    public int GameId { get; set; }
    public int SellerProfileId { get; set; }
    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;
    public GameKey GameKey { get; set; } = null!;
    public Game Game { get; set; } = null!;
    public SellerProfile Seller { get; set; } = null!;
}