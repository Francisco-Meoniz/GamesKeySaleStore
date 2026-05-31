using GameKeysStore.API.Models.Entities;

namespace GameKeysStore.API.Services.Interfaces;

public interface IPdfService
{
    byte[] GenerateReceipt(Order order);
}