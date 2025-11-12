using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models;

namespace My_FoodApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db) => _db = db;

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
        {
            // ✅ ตรวจ request เบื้องต้น
            if (req == null || req.UserId <= 0 || req.ShopId <= 0)
                return BadRequest("invalid request");

            var items = new List<OrderItem>();
            decimal subtotal = 0m;

            // ✅ ถ้า frontend ส่ง items มา → ใช้ตามนั้น (ยังรองรับไว้เผื่ออนาคต)
            if (req.Items != null && req.Items.Any())
            {
                foreach (var it in req.Items)
                {
                    var lineTotal = it.UnitPrice * it.Quantity;

                    items.Add(new OrderItem
                    {
                        MenuItemId = it.MenuItemId,
                        ItemName = it.ItemName ?? string.Empty,
                        UnitPrice = it.UnitPrice,
                        Quantity = it.Quantity,
                        LineTotal = lineTotal
                    });

                    subtotal += lineTotal;
                }
            }
            else
            {
                // ✅ เคสที่เราอยากใช้ตอนนี้ → backend ดึงจาก Cart ของ user+shop เอง
                var cart = await _db.Cart
                    .Include(c => c.Items)
                        .ThenInclude(i => i.MenuItem)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Options)
                    .FirstOrDefaultAsync(c => c.UserId == req.UserId && c.ShopId == req.ShopId);

                if (cart == null || cart.Items == null || !cart.Items.Any())
                    return BadRequest("cart empty");

                foreach (var ci in cart.Items)
                {
                    var extra = ci.Options.Sum(o => o.ExtraPrice);               // ราคารวม option
                    var unit = ci.UnitPrice + extra;                           // ราคาต่อชิ้นจริง
                    var lineTotal = unit * ci.Qty;

                    items.Add(new OrderItem
                    {
                        MenuItemId = ci.MenuItemId,
                        ItemName = ci.MenuItem?.Name ?? "Unknown",
                        UnitPrice = unit,
                        Quantity = ci.Qty,
                        LineTotal = lineTotal
                    });

                    subtotal += lineTotal;
                }

                // 🧹 ล้างตะกร้าหลังสร้าง order (option + cartItem + cart)
                _db.CartItemOptions.RemoveRange(cart.Items.SelectMany(i => i.Options));
                _db.CartItems.RemoveRange(cart.Items);
                _db.Cart.Remove(cart);
            }

            // ✅ ใช้โค้ดส่วนลดจาก request แทนการลดทุกครั้ง
            decimal discount = 0m;

            // ถ้าอยากผูกกับชื่อโค้ด เช่น GRADANAJA
            if (!string.IsNullOrWhiteSpace(req.VoucherCode))
            {
                if (req.VoucherCode.Equals("GRADANAJA", StringComparison.OrdinalIgnoreCase))
                {
                    // ลด 99% (หรือจะปรับเป็น 25% ก็ได้)
                    discount = Math.Round(subtotal * 0.99m, 2);
                }
            }

            var vat = Math.Round(subtotal * 0.01m, 2);
            var delivery = 0m;
            var grandTotal = subtotal + vat + delivery - discount;

            var order = new Order
            {
                ShopId = req.ShopId,
                UserId = req.UserId,
                OrderCode = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Status = "pending",
                Subtotal = subtotal,
                DeliveryFee = delivery,
                DiscountTotal = discount,
                GrandTotal = grandTotal,
                PlacedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = items
            };

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                // 👇 ถ้าอยากผูกกับ Payment ไว้เลยก็ทำได้แบบนี้ (ออปชัน)
                // var payment = new Payment
                // {
                //     OrderId  = order.Id,
                //     Method   = "qr",
                //     Amount   = grandTotal,
                //     Status   = "pending",
                //     CreatedAt = DateTime.UtcNow
                // };
                // _db.Payments.Add(payment);
                // await _db.SaveChangesAsync();

                await tx.CommitAsync();
                return Ok(new { orderId = order.Id, grandTotal });
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                return StatusCode(500, "cannot create order");
            }
        }
    }
}
