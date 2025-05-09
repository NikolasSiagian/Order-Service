using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Models
{
    [Table("Order")]
    public class Order
    {
        public int OrderId { get; set; }

        [DisplayName("Nomor Pesanan")]
        public string OrderNo { get; set; }

        [DisplayName("Tanggal Pesanan")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [DisplayName("Total Harga(Rp)")]
        public decimal TotalPrice { get; set; }

        [DisplayName("Status")]
        public bool IsPaid { get; set; }

        [DisplayName("Nama Pelanggan")]
        public string? CustomerName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public ICollection<OrderItem> OrderItems {  get; set; }
    }
}
