using OrderService.Models;
using System;
using System.Collections.Generic;

namespace OrderService.ViewModels
{
    public class SalesReportViewModel
    {
        public IEnumerable<Order> Orders { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
