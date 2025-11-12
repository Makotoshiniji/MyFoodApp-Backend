using System;

namespace My_FoodApp.Models
{
    public class ShopMedia
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string MediaType { get; set; } = "profile"; // profile, promo, gallery
        public string Url { get; set; } = null!;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Shop? Shop { get; set; }
    }
}
