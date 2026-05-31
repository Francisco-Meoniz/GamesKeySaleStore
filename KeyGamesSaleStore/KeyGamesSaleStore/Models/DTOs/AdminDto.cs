namespace GameKeysStore.API.Models.DTOs;

public class UpdateUserRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class CreateGameDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}

public class UpdateGameDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}

public class UserListDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsBanned { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SellerDisplayName { get; set; }
    public int? TotalSales { get; set; }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalSellers { get; set; }
    public int TotalGames { get; set; }
    public int TotalOrders { get; set; }
    public int PaidOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

public class RecentOrderDto
{
    public int Id { get; set; }
    public string BuyerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}