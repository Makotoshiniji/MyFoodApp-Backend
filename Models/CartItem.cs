using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // ✅ ต้องเพิ่ม
using System.Text.Json.Serialization;

namespace My_FoodApp.Models
{
    public class CartItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("shop_id")]
        public int ShopId { get; set; }

        [Column("cart_id")]
        public int CartId { get; set; }

        [JsonIgnore]
        public Cart? Cart { get; set; }

        [Column("menu_item_id")]
        public int MenuItemId { get; set; }

        public MenuItem? MenuItem { get; set; }

        [Column("qty")]
        public int Qty { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        public ICollection<CartItemOption> Options { get; set; } = new List<CartItemOption>();
    }
}
