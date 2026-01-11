using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using My_FoodApp.Models;

[Table("menu_item_option_groups")] // ชื่อตารางใน DB
public class MenuItemOptionGroup
{
    [Key]
    public int Id { get; set; }

    // FK ไปยัง MenuItems.Id
    public int MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    // FK ไปยังกลุ่ม option (ใช้คู่กับ MenuOption.GroupId)
    //public int GroupId { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsRequired { get; set; }
    public int MinSelection { get; set; }
    public int MaxSelection { get; set; }

    public int SortOrder { get; set; }

    public ICollection<MenuOption> Options { get; set; } = new List<MenuOption>();
}
