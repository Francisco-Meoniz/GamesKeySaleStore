namespace GameKeysStore.API.Models.DTOs;

public class CreateOrderDto
{
    public int BuyerId { get; set; }
    public string BuyerEmail { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

public class OrderItemRequestDto
{
    public int GameId { get; set; }
    public int Quantity { get; set; } // máx 3 en total
}