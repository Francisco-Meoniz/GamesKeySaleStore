namespace GameKeysStore.API.Models.DTOs;

public class GameDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Platform { get; set; } = string.Empty;
    public int AvailableStock { get; set; }
}