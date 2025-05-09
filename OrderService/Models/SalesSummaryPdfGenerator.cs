using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OrderService.ViewModels;
using System;
using System.Collections.Generic;

namespace OrderService.Services
{
    public class SalesSummaryPdfGenerator
    {
        public static byte[] Generate(List<SalesByDateViewModel> summaries, string periode)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(10)); // Menggunakan monospace font

                    // Header Section
                    page.Header().Column(header =>
                    {
                        header.Item().AlignCenter().Text("Food Order Service").Bold().FontSize(14); // Judul Toko
                        header.Item().AlignCenter().Text($"Ringkasan Penjualan").FontSize(12); // Subjudul
                        header.Item().AlignCenter().Text(periode).FontSize(10); // Periode Tanggal
                        header.Item().PaddingVertical(5).LineHorizontal(1); // Garis Horizontal
                    });

                    // Content Section
                    page.Content().Element(container =>
                    {
                        container.Column(col =>
                        {
                            decimal totalKeseluruhan = 0;

                            // Tabel untuk Ringkasan Penjualan
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Tanggal
                                    columns.RelativeColumn(3); // Total Penjualan
                                    columns.RelativeColumn(2); // Jumlah Order
                                });

                                // Header Tabel
                                table.Header(header =>
                                {
                                    header.Cell().Text("Tanggal").Bold();
                                    header.Cell().Text("Total Penjualan (Rp)").Bold();
                                    header.Cell().Text("Jumlah Order").Bold();
                                });

                                // Menampilkan data dari summaries
                                foreach (var item in summaries)
                                {
                                    table.Cell().Text(item.Tanggal.ToString("dd/MM/yyyy"));
                                    table.Cell().Text(item.TotalHarga.ToString("N0"));
                                    table.Cell().Text(item.JumlahOrder.ToString());

                                    totalKeseluruhan += item.TotalHarga; // Menambahkan Total Keseluruhan
                                }
                            });

                            // Divider antara section
                            col.Item().PaddingVertical(10).LineHorizontal(0.5f);

                            // Menampilkan Total Keseluruhan
                            col.Item().PaddingTop(10).AlignRight().Text($"Total Keseluruhan: Rp {totalKeseluruhan:N0}")
                                .Bold().FontSize(12); // Total keseluruhan di bagian bawah
                        });
                    });

                    // Footer Section
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Dicetak pada ").FontSize(8);
                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).SemiBold(); // Tanggal dan Waktu
                    });
                });
            }).GeneratePdf();
        }
    }
}
