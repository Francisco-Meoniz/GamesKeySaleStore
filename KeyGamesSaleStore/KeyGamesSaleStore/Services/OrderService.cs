using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ICartService _cartService;

    public OrderService(AppDbContext db, IEmailService emailService, ICartService cartService)
    {
        _db = db;
        _emailService = emailService;
        _cartService = cartService;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var buyer = await _db.Users.FindAsync(dto.BuyerId);
        if (buyer != null && buyer.Role == UserRole.Seller)
            throw new InvalidOperationException("Los vendedores no pueden comprar keys.");

        // Validar máximo 3 keys en total
        int totalKeys = dto.Items.Sum(i => i.Quantity);
        if (totalKeys > 3)
            throw new InvalidOperationException("No podés comprar más de 3 keys por orden.");
        if (totalKeys < 1)
            throw new InvalidOperationException("Debés seleccionar al menos 1 key.");

        var order = new Order
        {
            BuyerId = dto.BuyerId,
            BuyerEmail = dto.BuyerEmail,
            BuyerName = dto.BuyerName,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0;

        foreach (var item in dto.Items)
        {
            var game = await _db.Games.FindAsync(item.GameId)
                ?? throw new InvalidOperationException($"Juego {item.GameId} no encontrado.");

            // Reservar keys disponibles
            var availableKeys = await _db.GameKeys
                .Where(k => k.GameId == item.GameId && !k.IsSold)
                .Take(item.Quantity)
                .ToListAsync();

            if (availableKeys.Count < item.Quantity)
                throw new InvalidOperationException($"Stock insuficiente para {game.Title}.");

            foreach (var key in availableKeys)
            {
                key.IsSold = true;
                key.SoldAt = DateTime.UtcNow;

                order.Items.Add(new OrderItem
                {
                    GameKeyId = key.Id,
                    GameId = game.Id,
                    SellerProfileId = key.SellerProfileId, // ← asegurate que esté esta línea
                    UnitPrice = key.Price
                });

                total += key.Price;
            }
        }

        order.TotalAmount = total;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return MapToDto(order);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Game)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapToDto(order);
    }

    public async Task ConfirmPaymentAsync(string paymentId, string externalReference)
    {
        Console.WriteLine($"ConfirmPayment - paymentId: '{paymentId}' externalReference: '{externalReference}'");
        Order? order = null;

        if (!string.IsNullOrEmpty(externalReference) && int.TryParse(externalReference, out int orderId))
        {
            order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            Console.WriteLine($"Orden encontrada por externalReference: {order?.Id}");
        }

        if (order == null && !string.IsNullOrEmpty(paymentId))
        {
            order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.MercadoPagoPaymentId == paymentId);
        }

        if (order != null && order.Status != OrderStatus.Paid)
        {
            order.Status = OrderStatus.Paid;
            order.MercadoPagoPaymentId = string.IsNullOrEmpty(paymentId) ? "manual_confirm" : paymentId;
            order.PaidAt = DateTime.UtcNow;

            // Acreditar balance a cada vendedor
            var sellerAmounts = order.Items
                .GroupBy(i => i.SellerProfileId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.UnitPrice));

            foreach (var (sellerProfileId, amount) in sellerAmounts)
            {
                var sellerProfile = await _db.SellerProfiles.FindAsync(sellerProfileId);
                if (sellerProfile != null)
                {
                    sellerProfile.Balance += amount;
                    sellerProfile.TotalSales++;
                }
            }

            await _db.SaveChangesAsync();

            try
            {
                var fullOrder = await _db.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Game)
                    .FirstAsync(o => o.Id == order.Id);

                Console.WriteLine($"Mandando email a: {fullOrder.BuyerEmail}");
                await _emailService.SendPurchaseConfirmationAsync(fullOrder);
                Console.WriteLine($"Email enviado correctamente!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando email: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
            }
        }

        // Vaciar carrito del comprador
        if (order?.BuyerId.HasValue == true)
            await _cartService.ClearCartAsync(order.BuyerId.Value);
    }

    private static OrderResponseDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        BuyerEmail = order.BuyerEmail,
        BuyerName = order.BuyerName,
        TotalAmount = order.TotalAmount,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(i => new OrderItemResponseDto
        {
            GameTitle = i.Game?.Title ?? "",
            Platform = i.Game?.Platform ?? "",
            UnitPrice = i.UnitPrice
        }).ToList()
    };

    // Retiros
    public class WithdrawalRequestDto
    {
        public decimal Amount { get; set; }
    }

    public class WithdrawalResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Alias { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? AdminNote { get; set; }
    }

    public class SellerBalanceDto
    {
        public decimal Balance { get; set; }
        public List<WithdrawalResponseDto> Withdrawals { get; set; } = new();
    }

    // Para el admin
    public class ResolveWithdrawalDto
    {
        public bool Approve { get; set; }
        public string? AdminNote { get; set; }
    }
}