using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace My_FoodApp.Models
{
    public class MenuOptionGroup
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsRequired { get; set; }
        public int MinSelect { get; set; }
        public int MaxSelect { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<MenuOption> Options { get; set; } = new List<MenuOption>();
        public ICollection<MenuItemOptionGroup> MenuItems { get; set; } = new List<MenuItemOptionGroup>();
    }
}
