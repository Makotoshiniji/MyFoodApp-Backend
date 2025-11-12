//Data/CreateOrderRequest.cs
public class CreateOrderRequest
{
    public int UserId { get; set; }
    public int ShopId { get; set; }
    public string? VoucherCode { get; set; }
    public List<CreateOrderItemDto>? Items { get; set; }
}

public class CreateOrderItemDto
{
    public int MenuItemId { get; set; }
    public string? ItemName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}