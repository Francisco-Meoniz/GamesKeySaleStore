namespace GameKeysStore.API.Models.Entities;

public class SellerRating
{
    public int Id { get; set; }
    public int SellerProfileId { get; set; }
    public int BuyerId { get; set; }
    public int OrderId { get; set; }
    public int Stars { get; set; } // 1-5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SellerProfile Seller { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public Order Order { get; set; } = null!;
}