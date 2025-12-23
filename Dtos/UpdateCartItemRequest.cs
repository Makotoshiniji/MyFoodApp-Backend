using System.ComponentModel.DataAnnotations;

namespace My_FoodApp.Dtos // 👈 namespace จะตามชื่อโฟลเดอร์
{
    public class UpdateCartItemRequest
    {
        [Range(1, 100, ErrorMessage = "จำนวนต้องอย่างน้อย 1 ชิ้น")]
        public int Quantity { get; set; }

        public string? SpecialRequest { get; set; }

        public List<int>? OptionIds { get; set; }
    }
}