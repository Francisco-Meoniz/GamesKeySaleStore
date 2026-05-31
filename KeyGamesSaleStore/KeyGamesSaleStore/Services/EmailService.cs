using GameKeysStore.API.Data;
using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace GameKeysStore.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly IPdfService _pdfService;

    public EmailService(IConfiguration config, IPdfService pdfService)
    {
        _config = config;
        _pdfService = pdfService;
    }

    public async Task SendPurchaseConfirmationAsync(Order order)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["Email:FromName"]!,
            _config["Email:Username"]!));
        message.To.Add(new MailboxAddress(order.BuyerName, order.BuyerEmail));
        message.Subject = $"✅ Confirmación de compra #{order.Id} - GameKeys Store";

        // Generar PDF
        var pdfBytes = _pdfService.GenerateReceipt(order);

        var builder = new BodyBuilder();
        builder.HtmlBody = BuildEmailHtml(order);
        builder.Attachments.Add($"comprobante-orden-{order.Id}.pdf",
            pdfBytes, new ContentType("application", "pdf"));

        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _config["Email:SmtpHost"]!,
            int.Parse(_config["Email:SmtpPort"]!),
            SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(
            _config["Email:Username"]!,
            _config["Email:Password"]!);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

    private static string BuildEmailHtml(Order order)
    {
        var itemsHtml = string.Join("", order.Items.Select(i => $"""
            <tr>
                <td style="padding:8px;border-bottom:1px solid #eee">{i.Game?.Title}</td>
                <td style="padding:8px;border-bottom:1px solid #eee">{i.Game?.Platform}</td>
                <td style="padding:8px;border-bottom:1px solid #eee;text-align:right">
                    ${i.UnitPrice:F2}
                </td>
            </tr>
        """));

        return $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
            <div style="background:#1a1a2e;padding:30px;text-align:center">
                <h1 style="color:#e94560;margin:0">GameKeys Store</h1>
                <p style="color:#fff;margin:10px 0 0">Confirmación de compra</p>
            </div>
            <div style="padding:30px;background:#f9f9f9">
                <h2 style="color:#333">¡Gracias por tu compra, {order.BuyerName}!</h2>
                <p style="color:#666">Tu orden <strong>#{order.Id}</strong> fue procesada correctamente.</p>
                <table style="width:100%;border-collapse:collapse;margin-top:20px">
                    <thead>
                        <tr style="background:#1a1a2e;color:#fff">
                            <th style="padding:10px;text-align:left">Juego</th>
                            <th style="padding:10px;text-align:left">Plataforma</th>
                            <th style="padding:10px;text-align:right">Precio</th>
                        </tr>
                    </thead>
                    <tbody>{itemsHtml}</tbody>
                    <tfoot>
                        <tr>
                            <td colspan="2" style="padding:10px;font-weight:bold">Total</td>
                            <td style="padding:10px;font-weight:bold;text-align:right;color:#e94560">
                                ${order.TotalAmount:F2}
                            </td>
                        </tr>
                    </tfoot>
                </table>
                <p style="color:#666;margin-top:20px">
                    Encontrás el comprobante adjunto en este email.
                </p>
            </div>
            <div style="background:#1a1a2e;padding:15px;text-align:center">
                <p style="color:#888;margin:0;font-size:12px">GameKeys Store © 2026</p>
            </div>
        </div>
        """;
    }
}