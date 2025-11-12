using System;

namespace My_FoodApp.Models
{
    public class MenuItemIngredient
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public int IngredientId { get; set; }
        public decimal QtyPerUnit { get; set; }

        public MenuItem? MenuItem { get; set; }
        public Ingredient? Ingredient { get; set; }
    }
}
