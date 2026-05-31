namespace GameKeysStore.API.Services.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePreferenceAsync(int orderId);
}