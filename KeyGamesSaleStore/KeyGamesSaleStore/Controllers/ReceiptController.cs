using GameKeysStore.API.Data;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPdfService _pdfService;

    public ReceiptController(AppDbContext db, IPdfService pdfService)
    {
        _db = db;
        _pdfService = pdfService;
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> DownloadReceipt(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Game)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return NotFound();
        if (order.Status != Models.Entities.OrderStatus.Paid)
            return BadRequest(new { error = "La orden no está pagada." });

        var pdf = _pdfService.GenerateReceipt(order);
        return File(pdf, "application/pdf", $"comprobante-orden-{orderId}.pdf");
    }
}