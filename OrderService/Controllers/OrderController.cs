using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrderService.Context;
using OrderService.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;


namespace OrderService.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OrderServiceContext _context;

        public OrderController(OrderServiceContext context)
        {
            _context = context;
        }

        // GET: Order
        public async Task<IActionResult> Index(string status)
        {
            ViewBag.StatusFilter = status;

            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .AsQueryable();

            //filter berdasarkan status
            if (status == "Lunas")
            {
                orders = orders.Where(o => o.IsPaid);
            }
            else if (status == "Belum")
            {
                orders = orders.Where(o => !o.IsPaid);
            }

            var result = await orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(result);
        }



        // GET: Order/Details/5
        [HttpGet("Order/DetailsByOrderNo/{orderNo}")]
        public async Task<IActionResult> DetailsByOrderNo(string orderNo)
        {
            if (string.IsNullOrEmpty(orderNo))
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .FirstOrDefaultAsync(o => o.OrderNo == orderNo);

            if (order == null)
                return NotFound();

            return View("Details", order);
        }

        //proses pembayaran
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int OrderId, decimal paymentAmount)
        {
            // Cari Order berdasarkan OrderId
            var order = await _context.Orders.Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == OrderId);

            if (order == null)
            {
                TempData["Error"] = "Pesanan tidak ditemukan.";
                return RedirectToAction("Index");
            }

            // Periksa apakah pembayaran cukup
            if (paymentAmount < order.TotalPrice)
            {
                TempData["Error"] = "Jumlah pembayaran tidak mencukupi.";
                return RedirectToAction("DetailsByOrderNo", new { orderNo = order.OrderNo });
            }

            // Pembayaran cukup, update status
            order.IsPaid = true;
            _context.Update(order);
            await _context.SaveChangesAsync();

            // Setelah pembayaran berhasil, arahkan ke halaman Index
            TempData["Success"] = "Pembayaran berhasil. Pesanan sudah dibayar.";
            return RedirectToAction("Index");
        }




        // GET: Order/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Foods = await _context.Foods.ToListAsync();
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string customerName, List<OrderItem> OrderItems)
        {
            if (OrderItems == null || !OrderItems.Any())
            {
                ViewBag.Foods = await _context.Foods.ToListAsync();
                ModelState.AddModelError("", "Tidak ada item di pesanan.");
                return View();
            }

            // Membuat Order baru
            var order = new Order
            {
                OrderNo = GenerateOrderNo(),
                CustomerName = customerName,
                OrderDate = DateTime.Now,
                IsPaid = false,
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in OrderItems)
            {
                var food = await _context.Foods.FindAsync(item.FoodId);
                if (food == null) continue;

                item.TotalPrice = food.Price * item.Quantity;
                item.OrderNo = order.OrderNo;
                item.OrderId = order.OrderId;
                order.OrderItems.Add(item);
            }

            order.TotalPrice = order.OrderItems.Sum(i => i.TotalPrice);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("DetailsByOrderNo", new { orderNo = order.OrderNo });

            string GenerateOrderNo()
            {
                return "ORD-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            }
        }









        // POST: Order/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SaveOrder(string customerName, string itemsJson)
        //{
        //    if (string.IsNullOrEmpty(itemsJson))
        //    {
        //        ModelState.AddModelError("", "Tidak ada item di pesanan.");
        //        return View("Create");
        //    }

        //    var orderItems = JsonSerializer.Deserialize<List<OrderItem>>(itemsJson);
        //    if (orderItems == null || !orderItems.Any())
        //    {
        //        ModelState.AddModelError("", "Daftar pesanan kosong.");
        //        return View("Create");
        //    }

        //    var totalPrice = orderItems.Sum(i => i.TotalPrice);

        //    var order = new Order
        //    {
        //        CustomerName = customerName,
        //        OrderDate = DateTime.Now,
        //        TotalPrice = totalPrice,
        //        IsPaid = false,
        //        OrderItems = orderItems.Select(i => new OrderItem
        //        {
        //            FoodId = i.FoodId,
        //            Quantity = i.Quantity,
        //            TotalPrice = i.TotalPrice
        //        }).ToList()
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));

        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SaveOrder(string customerName, string itemsJson)
        //{
        //    if (string.IsNullOrEmpty(itemsJson))
        //    {
        //        ModelState.AddModelError("", "Tidak ada item dalam pesanan.");
        //        return View("Create");
        //    }

        //    var orderItems = JsonSerializer.Deserialize<List<OrderItem>>(itemsJson);
        //    if (orderItems == null || !orderItems.Any())
        //    {
        //        ModelState.AddModelError("", "Daftar pesanan kosong.");
        //        return View("Create");
        //    }

        //    var totalPrice = orderItems.Sum(i => i.TotalPrice);

        //    var order = new Order
        //    {
        //        CustomerName = customerName,
        //        OrderDate = DateTime.Now,
        //        TotalPrice = totalPrice,
        //        IsPaid = false,
        //        OrderItems = orderItems.Select(i => new OrderItem
        //        {
        //            FoodId = i.FoodId,
        //            Quantity = i.Quantity,
        //            TotalPrice = i.TotalPrice
        //        }).ToList()
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}


        // GET: Order/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var order = await _context.Orders.FindAsync(id);
        //    if (order == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(order);
        //}

        //// POST: Order/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderNo,OrderDate,TotalPrice,IsPaid,CustomerName,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy")] Order order)
        //{
        //    if (id != order.OrderId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(order);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!OrderExists(order.OrderId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(order);
        //}

        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
