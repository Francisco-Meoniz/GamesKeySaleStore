namespace GameKeysStore.API.Models.Entities;

public class SellerProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DNI { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? MercadoPagoAccount { get; set; }
    public int TotalSales { get; set; } = 0;
    public double AverageRating { get; set; } = 0;
    public int TotalRatings { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<GameKey> Keys { get; set; } = new List<GameKey>();
    public ICollection<SellerRating> Ratings { get; set; } = new List<SellerRating>();
    public decimal Balance { get; set; } = 0;
    public ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
}