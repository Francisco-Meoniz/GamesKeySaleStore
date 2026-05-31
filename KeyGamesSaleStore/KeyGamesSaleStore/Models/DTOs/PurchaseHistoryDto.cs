namespace GameKeysStore.API.Models.DTOs;

public class PurchaseHistoryDto
{
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<PurchaseItemDto> Items { get; set; } = new();
    public int SellerProfileId { get; set; }
}

public class PurchaseItemDto
{
    public string GameTitle { get; set; } = string.Empty;
    public string GameImageUrl { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string KeyCode { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int SellerProfileId { get; set; }
}