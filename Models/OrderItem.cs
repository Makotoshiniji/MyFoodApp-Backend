using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace My_FoodApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }

        [Column("item_name")]
        public string ItemName { get; set; } = null!;

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        public Order? Order { get; set; }
        public MenuItem? MenuItem { get; set; }

        [Column("item_option_ids")] // ระบุชื่อคอลัมน์ใน DB ให้ตรงเป๊ะ
        public string? ItemOptionIds { get; set; }

        [Column("special_request")] // ระบุชื่อคอลัมน์ใน DB ให้ตรงเป๊ะ
        public string? SpecialRequest { get; set; }
        public List<OrderItemOption> Options { get; set; } = new();
    }
}
