using GameKeysStore.API.Models.Entities;

namespace GameKeysStore.API.Services.Interfaces;

public interface IEmailService
{
    Task SendPurchaseConfirmationAsync(Order order);
}