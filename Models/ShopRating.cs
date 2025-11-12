using System;

namespace My_FoodApp.Models
{
    public class ShopRating
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? PhotosJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Shop? Shop { get; set; }
        public User? User { get; set; }
        public Order? Order { get; set; }
    }
}
