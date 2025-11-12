// Models/MenuOption.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace My_FoodApp.Models
{
    [Table("menu_options")] // ชื่อตารางใน DB
    public class MenuOption
    {
        [Key]
        public int Id { get; set; }

        // FK ไปยัง MenuItemOptionGroup.Id
        public int GroupId { get; set; }
        public MenuItemOptionGroup? Group { get; set; }

        // 👇 column จริงใน DB ชื่อ Name แต่เราใช้ชื่อ Label ในโค้ด
        [Column("Name")]
        [MaxLength(100)]
        public string Label { get; set; } = string.Empty;

        [Column("ExtraPrice")]
        public decimal ExtraPrice { get; set; }

        [Column("IsDefault")]
        public bool IsDefault { get; set; }

        [Column("SortOrder")]
        public int SortOrder { get; set; }

        // คอลัมน์ CreatedAt / UpdatedAt มีใน DB แต่
        // ถ้าไม่จำเป็นต้องใช้ สามารถปล่อยไม่ map ได้
        // หรือถ้าอยาก map ด้วย ก็ใส่:

        // public DateTime CreatedAt { get; set; }
        // public DateTime? UpdatedAt { get; set; }
    }
}
