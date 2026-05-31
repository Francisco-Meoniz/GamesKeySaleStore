using GameKeysStore.API.Models.DTOs;
namespace GameKeysStore.API.Services.Interfaces;

public interface IGamesService
{
    Task<List<GameDto>> GetAllGamesAsync();
    Task<GameDto?> GetGameByIdAsync(int id);
    Task<int> GetAvailableStockAsync(int gameId);
    Task<List<GameKeyDto>> GetAvailableKeysAsync(int gameId);
}