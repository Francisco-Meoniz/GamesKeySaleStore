using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameKeysStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("seller/{sellerProfileId}")]
    public async Task<IActionResult> GetSellerRatings(int sellerProfileId)
    {
        try
        {
            var ratings = await _ratingService.GetSellerRatingsAsync(sellerProfileId);
            return Ok(ratings);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("can-rate")]
    [Authorize]
    public async Task<IActionResult> CanRate(
        [FromQuery] int orderId,
        [FromQuery] int sellerProfileId)
    {
        var canRate = await _ratingService.CanRateAsync(GetUserId(), orderId, sellerProfileId);
        return Ok(new { canRate });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRating([FromBody] CreateRatingDto dto)
    {
        try
        {
            var rating = await _ratingService.CreateRatingAsync(GetUserId(), dto);
            return Ok(rating);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
