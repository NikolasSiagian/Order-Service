namespace OrderService.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; } //relasi ke tabel Order

        public string OrderNo { get; set; }

        public int FoodId { get; set; }
        public Food? Food { get; set; } //relasi ke tabel Food

        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
