using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace My_FoodApp.Models
{
    [Table("menu_items")]
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        public int ShopId { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public decimal? BaseCost { get; set; }

        public bool IsAvailable { get; set; }

        // ชื่อไฟล์รูป เช่น "1_1.png"
        public string? MainPhotoUrl { get; set; }

        public string? Type { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // ✅ ใช้โฟลเดอร์ /shop_uploads/menu/{ShopId}/{MainPhotoUrl}
        [NotMapped]
        public string? ImageUrl =>
            string.IsNullOrWhiteSpace(MainPhotoUrl)
                ? null
                : $"/shop_uploads/menu/{ShopId}/{MainPhotoUrl}";

        // relation ไปกลุ่มตัวเลือก (menu_item_option_groups)
        public ICollection<MenuItemOptionGroup> OptionGroups { get; set; }
            = new List<MenuItemOptionGroup>();

        // ✅ ความสัมพันธ์กับ Shop (navigation property)
        [ForeignKey(nameof(ShopId))]
        public Shop? Shop { get; set; }
    }
}
