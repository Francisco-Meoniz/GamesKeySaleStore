namespace GameKeysStore.API.Models.Entities;

public class WithdrawalRequest
{
    public int Id { get; set; }
    public int SellerProfileId { get; set; }
    public decimal Amount { get; set; }
    public string Alias { get; set; } = string.Empty; // alias o CBU al momento de pedir
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? AdminNote { get; set; } // nota del admin al aprobar/rechazar

    public SellerProfile SellerProfile { get; set; } = null!;
}

public enum WithdrawalStatus
{
    Pending,
    Approved,
    Rejected
}