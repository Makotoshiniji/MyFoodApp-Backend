using System;
using System.Collections.Generic;

namespace My_FoodApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string Name { get; set; } = null!;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Shop? Shop { get; set; }
        public ICollection<MenuItem>? MenuItems { get; set; }
    }
}
