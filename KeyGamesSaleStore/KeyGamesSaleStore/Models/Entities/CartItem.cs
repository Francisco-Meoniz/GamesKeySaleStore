namespace GameKeysStore.API.Models.Entities;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GameKeyId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public GameKey GameKey { get; set; } = null!;
}