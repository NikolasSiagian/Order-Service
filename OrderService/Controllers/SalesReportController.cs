using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Context;
using OrderService.Models;
using OrderService.Services;
using OrderService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderService.Controllers
{
    [Authorize]
    public class SalesReportController : Controller
    {
        private readonly OrderServiceContext _context;

        public SalesReportController(OrderServiceContext context)
        {
            _context = context;
        }

        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            List<Order> salesData = new List<Order>();
            var chartData = new List<object>(); // Ini buat grafik

            if (startDate.HasValue && endDate.HasValue)
            {
                salesData = _context.Orders
                    .Where(o => o.OrderDate.Date >= startDate.Value.Date &&
                                o.OrderDate.Date <= endDate.Value.Date &&
                                o.IsPaid)
                    .OrderBy(o => o.OrderDate)
                    .ToList();

                // Mengelompokkan berdasarkan tanggal dan menghitung total harga per tanggal
                var salesByDate = salesData
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        Tanggal = g.Key,
                        TotalHarga = g.Sum(o => o.TotalPrice),
                        JumlahOrder = g.Count(),
                        Orders = g.ToList() // Menyertakan detail order di dalam grup
                    })
                    .ToList();

                // Kirim data total per tanggal untuk ditampilkan
                ViewBag.SalesByDate = salesByDate;

                // Buat list tanggal dalam rentang yang dipilih
                var allDatesInRange = Enumerable.Range(0, (endDate.Value - startDate.Value).Days + 1)
                                                .Select(d => startDate.Value.AddDays(d).Date)
                                                .ToList();

                // Group berdasarkan tanggal dan gabungkan dengan tanggal dalam rentang
                chartData = allDatesInRange
                    .Select(date => new
                    {
                        Tanggal = date.ToString("dd/MM/yyyy"),
                        TotalPenjualan = salesData
                            .Where(o => o.OrderDate.Date == date)
                            .Sum(x => x.TotalPrice)
                    })
                    .ToList<object>();
            }

            // Kirim ke View salesData untuk table
            // Kirim ke View chartData untuk grafik
            ViewBag.ChartData = chartData;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(salesData);
        }

        public IActionResult DetailsByDate(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return NotFound();
            }

            var selectedDate = DateTime.Parse(date);

            var orders = _context.Orders
                .Where(o => o.OrderDate.Date == selectedDate.Date && o.IsPaid)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Food) // Include data makanan
                .OrderBy(o => o.OrderNo)
                .ToList();

            if (!orders.Any())
            {
                return NotFound();
            }

            ViewBag.Date = selectedDate.ToString("dd/MM/yyyy");
            return View(orders);
        }

        [HttpGet]
        public IActionResult ExportSummary(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
                return BadRequest();

            var salesData = _context.Orders
                .Where(o => o.OrderDate.Date >= startDate.Value.Date &&
                            o.OrderDate.Date <= endDate.Value.Date &&
                            o.IsPaid)
                .ToList();

            var summary = salesData
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesByDateViewModel
                {
                    Tanggal = g.Key,
                    TotalHarga = g.Sum(o => o.TotalPrice),
                    JumlahOrder = g.Count()
                })
                .OrderBy(x => x.Tanggal)
                .ToList();

            var periode = $"{startDate.Value:dd/MM/yyyy} - {endDate.Value:dd/MM/yyyy}";
            var pdfBytes = SalesSummaryPdfGenerator.Generate(summary, periode);

            return File(pdfBytes, "application/pdf", $"RingkasanPenjualan-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf");
        }



        [HttpGet]
        public IActionResult ExportToPdf(string date)
        {
            if (string.IsNullOrEmpty(date))
                return NotFound();

            DateTime selectedDate;
            try
            {
                selectedDate = DateTime.ParseExact(date, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return BadRequest("Format tanggal tidak valid. Gunakan format dd/MM/yyyy.");
            }


            var orders = _context.Orders
                .Where(o => o.OrderDate.Date == selectedDate.Date && o.IsPaid)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Food)
                .ToList();

            if (!orders.Any())
                return NotFound();

            var pdfBytes = SalesReportPdfGenerator.Generate(orders, selectedDate.ToString("dd/MM/yyyy"));
            return File(pdfBytes, "application/pdf", $"LaporanPenjualan-{date}.pdf");
        }



    }
}
