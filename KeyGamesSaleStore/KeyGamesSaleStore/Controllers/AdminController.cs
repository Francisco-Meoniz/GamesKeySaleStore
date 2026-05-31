using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameKeysStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _adminService.GetUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{userId}/role")]
    public async Task<IActionResult> UpdateRole(int userId, [FromBody] UpdateUserRoleDto dto)
    {
        try
        {
            await _adminService.UpdateUserRoleAsync(userId, dto.Role);
            return Ok(new { message = "Rol actualizado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("users/{userId}/ban")]
    public async Task<IActionResult> ToggleBan(int userId)
    {
        try
        {
            await _adminService.ToggleBanUserAsync(userId);
            return Ok(new { message = "Estado actualizado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("games")]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameDto dto)
    {
        try
        {
            var game = await _adminService.CreateGameAsync(dto);
            return Ok(game);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("games/{gameId}")]
    public async Task<IActionResult> UpdateGame(int gameId, [FromBody] UpdateGameDto dto)
    {
        try
        {
            var game = await _adminService.UpdateGameAsync(gameId, dto);
            return Ok(game);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("games/{gameId}")]
    public async Task<IActionResult> DeleteGame(int gameId)
    {
        try
        {
            await _adminService.DeleteGameAsync(gameId);
            return Ok(new { message = "Juego eliminado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _adminService.GetAllOrdersAsync();
        return Ok(orders);
    }
}