namespace GameKeysStore.API.Models.DTOs;

public class SellerStatsDto
{
    public string DisplayName { get; set; } = string.Empty;
    public int TotalSales { get; set; }
    public double AverageRating { get; set; }
    public int ActiveKeys { get; set; }
    public int SoldKeys { get; set; }
    public decimal TotalEarnings { get; set; }
}

public class SellerKeyDto
{
    public int Id { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string GameImageUrl { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public bool IsSold { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SoldAt { get; set; }
}

public class PublishKeyDto
{
    public int GameId { get; set; }
    public string KeyCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdateKeyPriceDto
{
    public decimal Price { get; set; }
}