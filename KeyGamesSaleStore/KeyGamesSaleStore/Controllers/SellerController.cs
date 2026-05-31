using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameKeysStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Seller,Admin")]
public class SellerController : ControllerBase
{
    private readonly ISellerService _sellerService;

    public SellerController(ISellerService sellerService)
    {
        _sellerService = sellerService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var stats = await _sellerService.GetStatsAsync(GetUserId());
            return Ok(stats);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("keys")]
    public async Task<IActionResult> GetMyKeys()
    {
        try
        {
            var keys = await _sellerService.GetMyKeysAsync(GetUserId());
            return Ok(keys);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("keys")]
    public async Task<IActionResult> PublishKey([FromBody] PublishKeyDto dto)
    {
        try
        {
            var key = await _sellerService.PublishKeyAsync(GetUserId(), dto);
            return Ok(key);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("keys/{keyId}/toggle")]
    public async Task<IActionResult> ToggleKey(int keyId)
    {
        try
        {
            await _sellerService.ToggleKeyActiveAsync(GetUserId(), keyId);
            return Ok(new { message = "Estado actualizado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("keys/{keyId}/price")]
    public async Task<IActionResult> UpdatePrice(int keyId, [FromBody] UpdateKeyPriceDto dto)
    {
        try
        {
            await _sellerService.UpdateKeyPriceAsync(GetUserId(), keyId, dto);
            return Ok(new { message = "Precio actualizado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("keys/{keyId}")]
    public async Task<IActionResult> DeleteKey(int keyId)
    {
        try
        {
            await _sellerService.DeleteKeyAsync(GetUserId(), keyId);
            return Ok(new { message = "Key eliminada." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}