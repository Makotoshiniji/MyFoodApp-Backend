using System;
using System.Collections.Generic;

namespace My_FoodApp.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public string Name { get; set; } = string.Empty;   // <- แก้ warning
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsOpen { get; set; } = true;
        public decimal RatingAvg { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation (ไม่ให้เป็น null)
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<ShopMedia> ShopMedia { get; set; } = new List<ShopMedia>(); // <- สำคัญ

        public List<ShopMedia> Media { get; set; } = new();
    }
}
