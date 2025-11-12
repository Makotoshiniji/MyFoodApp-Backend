using System.ComponentModel.DataAnnotations;

namespace My_FoodApp.Models
{
    public class CartItemOption
    {
        [Key]
        public int Id { get; set; }

        public int CartItemId { get; set; }
        public CartItem? CartItem { get; set; }

        public int OptionId { get; set; }

        public decimal ExtraPrice { get; set; }
    }
}
