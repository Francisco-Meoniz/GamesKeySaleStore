using GameKeysStore.API.Models.DTOs;

namespace GameKeysStore.API.Services.Interfaces;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync();
    Task<List<UserListDto>> GetUsersAsync();
    Task UpdateUserRoleAsync(int userId, string role);
    Task ToggleBanUserAsync(int userId);
    Task<GameDto> CreateGameAsync(CreateGameDto dto);
    Task<GameDto> UpdateGameAsync(int gameId, UpdateGameDto dto);
    Task DeleteGameAsync(int gameId);
    Task<List<RecentOrderDto>> GetAllOrdersAsync();
}