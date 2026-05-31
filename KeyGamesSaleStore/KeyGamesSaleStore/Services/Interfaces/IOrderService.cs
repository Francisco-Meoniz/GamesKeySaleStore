using GameKeysStore.API.Models.DTOs;

namespace GameKeysStore.API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderResponseDto?> GetOrderByIdAsync(int id);
    Task ConfirmPaymentAsync(string paymentId, string preferenceId);
}