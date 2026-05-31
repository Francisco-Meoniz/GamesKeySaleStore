namespace GameKeysStore.API.Models.DTOs;

public class CartItemDto
{
    public int Id { get; set; }
    public int GameKeyId { get; set; }
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string GameImageUrl { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}

public class CartSummaryDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class AddToCartDto
{
    public int GameKeyId { get; set; }
}