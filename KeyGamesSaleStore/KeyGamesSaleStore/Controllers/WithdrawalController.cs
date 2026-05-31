using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static GameKeysStore.API.Services.OrderService;

namespace GameKeysStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WithdrawalController : ControllerBase
{
    private readonly AppDbContext _db;

    public WithdrawalController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/withdrawal/balance → el vendedor ve su saldo y sus retiros
    [HttpGet("balance")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var seller = await _db.SellerProfiles
            .Include(s => s.WithdrawalRequests)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (seller == null)
            return NotFound(new { error = "Perfil de vendedor no encontrado." });

        var dto = new SellerBalanceDto
        {
            Balance = seller.Balance,
            Withdrawals = seller.WithdrawalRequests
                .OrderByDescending(w => w.RequestedAt)
                .Select(w => new WithdrawalResponseDto
                {
                    Id = w.Id,
                    Amount = w.Amount,
                    Alias = w.Alias,
                    Status = w.Status.ToString(),
                    RequestedAt = w.RequestedAt,
                    ResolvedAt = w.ResolvedAt,
                    AdminNote = w.AdminNote
                }).ToList()
        };

        return Ok(dto);
    }

    // POST api/withdrawal/request → el vendedor pide un retiro
    [HttpPost("request")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequestDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var seller = await _db.SellerProfiles
            .Include(s => s.WithdrawalRequests)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (seller == null)
            return NotFound(new { error = "Perfil de vendedor no encontrado." });

        if (string.IsNullOrWhiteSpace(seller.MercadoPagoAccount))
            return BadRequest(new { error = "Necesitás registrar tu alias o CBU de MercadoPago antes de retirar fondos." });

        if (dto.Amount <= 0)
            return BadRequest(new { error = "El monto debe ser mayor a cero." });

        if (dto.Amount > seller.Balance)
            return BadRequest(new { error = "Saldo insuficiente." });

        // Verificar que no tenga un retiro pendiente
        var hasPending = seller.WithdrawalRequests.Any(w => w.Status == WithdrawalStatus.Pending);
        if (hasPending)
            return BadRequest(new { error = "Ya tenés una solicitud de retiro pendiente." });

        // Descontar el balance inmediatamente (queda reservado)
        seller.Balance -= dto.Amount;

        var withdrawal = new WithdrawalRequest
        {
            SellerProfileId = seller.Id,
            Amount = dto.Amount,
            Alias = seller.MercadoPagoAccount,
            Status = WithdrawalStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _db.WithdrawalRequests.Add(withdrawal);
        await _db.SaveChangesAsync();

        return Ok(new WithdrawalResponseDto
        {
            Id = withdrawal.Id,
            Amount = withdrawal.Amount,
            Alias = withdrawal.Alias,
            Status = withdrawal.Status.ToString(),
            RequestedAt = withdrawal.RequestedAt
        });
    }

    // GET api/withdrawal/admin/pending → el admin ve todos los retiros pendientes
    [HttpGet("admin/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingWithdrawals()
    {
        var withdrawals = await _db.WithdrawalRequests
            .Include(w => w.SellerProfile)
                .ThenInclude(s => s.User)
            .Where(w => w.Status == WithdrawalStatus.Pending)
            .OrderBy(w => w.RequestedAt)
            .Select(w => new
            {
                w.Id,
                w.Amount,
                w.Alias,
                w.RequestedAt,
                Seller = w.SellerProfile.DisplayName,
                SellerEmail = w.SellerProfile.User.Email
            })
            .ToListAsync();

        return Ok(withdrawals);
    }

    // POST api/withdrawal/admin/resolve/{id} → el admin aprueba o rechaza
    [HttpPost("admin/resolve/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResolveWithdrawal(int id, [FromBody] ResolveWithdrawalDto dto)
    {
        var withdrawal = await _db.WithdrawalRequests
            .Include(w => w.SellerProfile)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (withdrawal == null)
            return NotFound(new { error = "Solicitud no encontrada." });

        if (withdrawal.Status != WithdrawalStatus.Pending)
            return BadRequest(new { error = "Esta solicitud ya fue resuelta." });

        withdrawal.Status = dto.Approve ? WithdrawalStatus.Approved : WithdrawalStatus.Rejected;
        withdrawal.ResolvedAt = DateTime.UtcNow;
        withdrawal.AdminNote = dto.AdminNote;

        // Si se rechaza, devolver el balance al vendedor
        if (!dto.Approve)
            withdrawal.SellerProfile.Balance += withdrawal.Amount;

        await _db.SaveChangesAsync();

        return Ok(new { message = dto.Approve ? "Retiro aprobado." : "Retiro rechazado y saldo devuelto." });
    }
}