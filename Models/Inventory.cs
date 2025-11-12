using System;

namespace My_FoodApp.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Shop? Shop { get; set; }
        public Ingredient? Ingredient { get; set; }
    }
}
