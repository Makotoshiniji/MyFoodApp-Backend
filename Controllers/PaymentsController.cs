using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models;
using My_FoodApp.Dtos; // 👈 import DTO

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("upload-slip")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadSlip([FromForm] UploadSlipRequest request)
    {
        Console.WriteLine($"📦 DEBUG: OrderId={request.OrderId}, File={(request.SlipFile != null ? request.SlipFile.FileName : "null")}");
        // ✅ เช็กเองแทน ApiController auto-validation
        if (!request.OrderId.HasValue)
        {
            return BadRequest("orderId is required");
        }

        if (request.SlipFile is null || request.SlipFile.Length == 0)
        {
            return BadRequest("กรุณาเลือกรูปสลิป");
        }

        var orderId = request.OrderId.Value;
        var slipFile = request.SlipFile;

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            return NotFound("ไม่พบคำสั่งซื้อ");
        }

        var uploadsRoot = Path.Combine(@"C:\My_FoodApp\My_FoodApp\shop_uploads", "payment_slips");
        if (!Directory.Exists(uploadsRoot))
        {
            Directory.CreateDirectory(uploadsRoot);
        }

        var ext = Path.GetExtension(slipFile.FileName);
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

        var fileName = $"order_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await slipFile.CopyToAsync(stream);
        }

        var relativePath = $"/shop_uploads/payment_slips/{fileName}";

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
        if (payment == null)
        {
            payment = new Payment
            {
                OrderId = order.Id,
                Method = "qr",
                Amount = order.GrandTotal,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Payments.Add(payment);
        }

        payment.SlipImagePath = relativePath;
        payment.SlipUploadedAt = DateTime.UtcNow;
        payment.Status = "waiting_confirm";

        order.Status = "pending_confirm";
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "อัปโหลดสลิปเรียบร้อยแล้ว ✅",
            slipUrl = relativePath
        });
    }

    // GET: api/payments/user/{userId}/bills
    [HttpGet("user/{userId:int}/bills")]
    public async Task<IActionResult> GetBillsForUser(int userId)
    {
        // ถ้ามี Payment แล้ว (มีสลิป/สถานะ) จะดึงจากตาราง Payments เป็นหลัก
        var bills = await _db.Payments
            .Include(p => p.Order)            // ต้องมี Order เพื่อรู้ user/shop/total
                .ThenInclude(o => o.Shop)     // ถ้ามี relation Shop
            .Where(p => p.Order != null && p.Order.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                id = p.Id,
                orderId = p.OrderId,
                orderCode = p.Order!.OrderCode,
                shopName = p.Order!.Shop != null ? p.Order.Shop.Name : "Unknown shop",
                grandTotal = (decimal?)(p.Amount) ?? p.Order!.GrandTotal,
                createdAt = p.CreatedAt,           // วันที่มีการชำระ/อัปโหลดสลิป
                status = p.Status,                 // waiting_confirm / confirmed / rejected ...
                slipUrl = p.SlipImagePath
            })
            .ToListAsync();

        // กรณีผู้ใช้ยังไม่มี record ใน Payments (เช่น เพิ่งสร้างออเดอร์ไว้)
        if (bills.Count == 0)
        {
            var orderBills = await _db.Orders
                .Include(o => o.Shop)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.PlacedAt)
                .Select(o => new
                {
                    id = o.Id,                     // ใช้ order id เป็น id ชั่วคราว
                    orderId = o.Id,
                    orderCode = o.OrderCode,
                    shopName = o.Shop != null ? o.Shop.Name : "Unknown shop",
                    grandTotal = o.GrandTotal,
                    createdAt = o.PlacedAt,
                    status = o.Status,             // pending / pending_confirm / paid ...
                    slipUrl = (string?)null
                })
                .ToListAsync();

            return Ok(orderBills);
        }

        return Ok(bills);
    }


    // GET: api/payments/bill/{orderId}
    [HttpGet("bill/{orderId:int}")]
    public async Task<IActionResult> GetBillByOrder(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Shop)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return NotFound();

        // สรุป VAT/Discount ถ้ามีในตาราง
        var vat = (order.Subtotal * 0.01m); // ถ้ามีฟิลด์ภาษีจริง ให้ใช้ของจริงแทน
        var discount = order.DiscountTotal;

        return Ok(new
        {
            orderId = order.Id,
            orderCode = order.OrderCode,
            shopName = order.Shop != null ? order.Shop.Name : "Unknown shop",
            createdAt = order.PlacedAt,
            status = order.Status,
            grandTotal = order.GrandTotal,
            vat = vat,
            discount = discount,
            items = order.OrderItems.Select(i => new {
                itemName = i.ItemName,
                quantity = i.Quantity,
                unitPrice = i.UnitPrice,
                lineTotal = i.LineTotal
            }).ToList()
        });
    }




}
