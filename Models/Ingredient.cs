using System;

namespace My_FoodApp.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string Name { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal SafetyStock { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Shop? Shop { get; set; }
    }
}
