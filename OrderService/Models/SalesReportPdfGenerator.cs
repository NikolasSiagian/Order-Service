using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OrderService.Models;

namespace OrderService.Services
{
    public class SalesReportPdfGenerator
    {
        public static byte[] Generate(List<Order> orders, string tanggal)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(10)); // Monospace font seperti struk

                    page.Header().Column(header =>
                    {
                        header.Item().AlignCenter().Text("Food Order Service").Bold().FontSize(14);
                        header.Item().AlignCenter().Text($"Laporan Penjualan").FontSize(12);
                        header.Item().AlignCenter().Text(tanggal).FontSize(10);
                        header.Item().PaddingVertical(5).LineHorizontal(1);
                    });

                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            decimal totalKeseluruhan = 0;

                            foreach (var orderGroup in orders.GroupBy(o => o.OrderNo))
                            {
                                var order = orderGroup.First();
                                decimal subtotal = orderGroup.SelectMany(o => o.OrderItems).Sum(i => i.TotalPrice);
                                totalKeseluruhan += subtotal;

                                col.Item().Text($"Order: {order.OrderNo}").SemiBold();
                                col.Item().Text($"Tanggal: {order.OrderDate:dd/MM/yyyy} | Pelanggan: {order.CustomerName}").Italic();

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4); // Makanan
                                        columns.RelativeColumn(1); // Qty
                                        columns.RelativeColumn(2); // Total
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Item").Bold();
                                        header.Cell().Text("Qty").Bold();
                                        header.Cell().Text("Total").Bold();
                                    });

                                    foreach (var item in orderGroup.SelectMany(o => o.OrderItems))
                                    {
                                        table.Cell().Text(item.Food?.Name ?? "N/A");
                                        table.Cell().Text(item.Quantity.ToString());
                                        table.Cell().Text(item.TotalPrice.ToString("N0"));
                                    }
                                });

                                col.Item().AlignRight().Text($"Subtotal: Rp {subtotal:N0}").Bold();
                                col.Item().PaddingBottom(10).LineHorizontal(0.5f);
                            }

                            col.Item().PaddingTop(10).AlignRight().Text($"Total Keseluruhan: Rp {totalKeseluruhan:N0}").Bold().FontSize(12);
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Dicetak pada ").FontSize(8);
                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).SemiBold();
                    });
                });
            }).GeneratePdf();
        }
    }
}
