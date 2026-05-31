using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GameKeysStore.API.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateReceipt(Order order)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, order));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("GameKeys Store © 2026  |  Orden #");
                    x.Span(order.Id.ToString());
                });
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("GameKeys Store")
                    .FontSize(24).Bold().FontColor("#e94560");
                col.Item().Text("Comprobante de compra")
                    .FontSize(12).FontColor("#666666");
            });
        });
    }

    private static void ComposeContent(IContainer container, Order order)
    {
        container.PaddingTop(20).Column(col =>
        {
            // Info de la orden
            col.Item().Background("#f5f5f5").Padding(15).Column(info =>
            {
                info.Item().Text($"Orden #: {order.Id}").Bold();
                info.Item().Text($"Comprador: {order.BuyerName}");
                info.Item().Text($"Email: {order.BuyerEmail}");
                info.Item().Text($"Fecha: {order.PaidAt?.ToLocalTime():dd/MM/yyyy HH:mm}");
                info.Item().Text($"Estado: Pagado ✓").FontColor("#27ae60");
            });

            col.Item().PaddingTop(20).Text("Detalle de la compra").FontSize(14).Bold();

            // Tabla de items
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(1);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background("#1a1a2e").Padding(8)
                        .Text("Juego").FontColor("#ffffff").Bold();
                    header.Cell().Background("#1a1a2e").Padding(8)
                        .Text("Plataforma").FontColor("#ffffff").Bold();
                    header.Cell().Background("#1a1a2e").Padding(8)
                        .Text("Precio").FontColor("#ffffff").Bold();
                });

                // Filas
                foreach (var item in order.Items)
                {
                    table.Cell().BorderBottom(1).BorderColor("#eeeeee").Padding(8)
                        .Text(item.Game?.Title ?? "");
                    table.Cell().BorderBottom(1).BorderColor("#eeeeee").Padding(8)
                        .Text(item.Game?.Platform ?? "");
                    table.Cell().BorderBottom(1).BorderColor("#eeeeee").Padding(8)
                        .Text($"${item.UnitPrice:F2}");
                }
            });

            // Total
            col.Item().PaddingTop(10).AlignRight()
                .Text($"Total: ${order.TotalAmount:F2}")
                .FontSize(14).Bold().FontColor("#e94560");
        });
    }
}