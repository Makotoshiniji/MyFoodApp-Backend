using System;
using System.Collections.Generic;

namespace My_FoodApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int UserId { get; set; }
        public string OrderCode { get; set; } = null!;
        public string Status { get; set; } = "pending"; // pending, confirmed, preparing, ready, delivering, completed, cancelled
        public decimal Subtotal { get; set; } = 0;
        public decimal DeliveryFee { get; set; } = 0;
        public decimal DiscountTotal { get; set; } = 0;
        public decimal GrandTotal { get; set; } = 0;
        public string? Notes { get; set; }
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Shop? Shop { get; set; }
        public User? User { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
