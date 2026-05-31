namespace GameKeysStore.API.Models.DTOs;

public class OrderResponseDto
{
    public int Id { get; set; }
    public string BuyerEmail { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MercadoPagoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public string GameTitle { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

public class PaymentWebhookDto
{
    public string Action { get; set; } = string.Empty;
    public WebhookDataDto Data { get; set; } = null!;
}

public class WebhookDataDto
{
    public string Id { get; set; } = string.Empty;
}