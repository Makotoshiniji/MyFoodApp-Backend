using My_FoodApp.Models;

public class OrderItemOption
{
    public int Id { get; set; }
    public int OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } // Link กลับไปหาแม่

    public string OptionName { get; set; } = ""; // เก็บชื่อ ณ เวลาสั่ง (Snapshot)
    public decimal ExtraPrice { get; set; }      // เก็บราคา ณ เวลาสั่ง
}