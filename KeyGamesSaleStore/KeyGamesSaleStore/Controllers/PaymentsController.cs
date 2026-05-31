using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameKeysStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;

    public PaymentsController(IPaymentService paymentService, IOrderService orderService)
    {
        _paymentService = paymentService;
        _orderService = orderService;
    }

    // Generar link de pago para una orden
    [HttpPost("{orderId}/create-preference")]
    public async Task<IActionResult> CreatePreference(int orderId)
    {
        try
        {
            var url = await _paymentService.CreatePreferenceAsync(orderId);
            return Ok(new { paymentUrl = url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Webhook que llama MercadoPago cuando se aprueba el pago
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] PaymentWebhookDto dto)
    {
        if (dto.Action == "payment.updated" || dto.Action == "payment.created")
        {
            await _orderService.ConfirmPaymentAsync(dto.Data.Id, "");
        }
        return Ok();
    }

    // Endpoint que llama el frontend cuando MP redirige al success
    [HttpGet("success")]
    public async Task<IActionResult> PaymentSuccess(
    [FromQuery] string? payment_id,
    [FromQuery] string? external_reference)
    {
        try
        {
            await _orderService.ConfirmPaymentAsync(
                payment_id ?? string.Empty,
                external_reference ?? string.Empty);
            return Ok(new { message = "Pago confirmado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{orderId}/confirm")]
    public async Task<IActionResult> ConfirmOrder(int orderId)
    {
        try
        {
            await _orderService.ConfirmPaymentAsync("manual", orderId.ToString());
            return Ok(new { message = "Orden confirmada." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}