using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Food
    {
        public int FoodId { get; set; }

        [Required(ErrorMessage = "Nama wajib diisi")]

        [DisplayName("Nama Menu")]
        public string? Name { get; set; }

        [DisplayName("Harga(Rp)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Harga harus lebih besar dari 0")]
        public decimal Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();


    }
}
