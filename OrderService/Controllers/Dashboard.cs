using Microsoft.AspNetCore.Mvc;
using System;
using OrderService.Context;
using OrderService.Models;// Tambahkan ini supaya bisa akses database
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace OrderService.Controllers
{
    [Authorize]
    public class Dashboard : Controller
    {
        private readonly OrderServiceContext _context;

        public Dashboard(OrderServiceContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var thisYear = new DateTime(today.Year, 1, 1);

            var totalHarian = _context.Orders
                .Where(o => o.OrderDate.Date == today && o.IsPaid)
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;

            var totalBulanan = _context.Orders
                .Where(o => o.OrderDate.Date >= thisMonth && o.IsPaid)
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;

            var totalTahunan = _context.Orders
                .Where(o => o.OrderDate.Date >= thisYear && o.IsPaid)
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;

            ViewBag.TotalHarian = totalHarian;
            ViewBag.TotalBulanan = totalBulanan;
            ViewBag.TotalTahunan = totalTahunan;

            return View();
        }
    }
}
