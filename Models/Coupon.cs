using System;

namespace My_FoodApp.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = "amount"; // amount, percent
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal MinSubtotal { get; set; } = 0;
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; } = 0;
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Shop? Shop { get; set; }
    }
}
