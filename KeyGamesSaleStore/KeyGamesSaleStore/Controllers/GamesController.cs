using GameKeysStore.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameKeysStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGamesService _gamesService;

    public GamesController(IGamesService gamesService)
    {
        _gamesService = gamesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var games = await _gamesService.GetAllGamesAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var game = await _gamesService.GetGameByIdAsync(id);
        return game == null ? NotFound() : Ok(game);
    }

    [HttpGet("{id}/keys")]
    public async Task<IActionResult> GetAvailableKeys(int id)
    {
        var keys = await _gamesService.GetAvailableKeysAsync(id);
        return Ok(keys);
    }
}