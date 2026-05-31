namespace GameKeysStore.API.Models.DTOs;

public class CreateRatingDto
{
    public int OrderId { get; set; }
    public int SellerProfileId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
}

public class RatingDto
{
    public int Id { get; set; }
    public string BuyerUsername { get; set; } = string.Empty;
    public string? BuyerAvatarUrl { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SellerRatingsDto
{
    public string DisplayName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public List<RatingDto> Ratings { get; set; } = new();
}
