namespace GameKeysStore.API.Models.DTOs;

public class GameKeyDto
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public bool IsCheapest { get; set; }
    public SellerSummaryDto Seller { get; set; } = null!;
}

public class SellerSummaryDto
{
    public string DisplayName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalSales { get; set; }
    public int MonthsOnPlatform { get; set; }
}