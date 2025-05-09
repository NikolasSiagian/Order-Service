using System;
using System.Collections.Generic;
using OrderService.Models;

namespace OrderService.ViewModels
{
    public class SalesByDateViewModel
    {
        public DateTime Tanggal { get; set; }
        public decimal TotalHarga { get; set; }
        public int JumlahOrder { get; set; }
    }
}
