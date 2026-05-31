using GameKeysStore.API.Data;
using GameKeysStore.API.Services.Interfaces;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public PaymentService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
        MercadoPagoConfig.AccessToken = _config["MercadoPago:AccessToken"]!;
    }

    public async Task<string> CreatePreferenceAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Game)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new InvalidOperationException("Orden no encontrada.");

        var frontendUrl = _config["Frontend:BaseUrl"]!;

        var items = order.Items.Select(i => new PreferenceItemRequest
        {
            Title = i.Game.Title,
            Quantity = 1,
            UnitPrice = i.UnitPrice,
            CurrencyId = "ARS"
        }).ToList();

        var request = new PreferenceRequest
        {
            Items = items,
            Payer = new PreferencePayerRequest
            {
                Email = order.BuyerEmail,
                Name = order.BuyerName
            },
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = $"{_config["Frontend:BaseUrl"]}/payment/success?orderId={order.Id}",
                Failure = $"{_config["Frontend:BaseUrl"]}/payment/failure?orderId={order.Id}",
                Pending = $"{_config["Frontend:BaseUrl"]}/payment/pending?orderId={order.Id}"
            },
            ExternalReference = order.Id.ToString(),
            NotificationUrl = $"{_config["Backend:BaseUrl"]}/api/payments/webhook"
        };

        var client = new PreferenceClient();
        Preference preference = await client.CreateAsync(request);

        // Guardar el preference ID en la orden
        order.MercadoPagoPreferenceId = preference.Id;
        await _db.SaveChangesAsync();

        return preference.InitPoint; // URL de pago
    }
}