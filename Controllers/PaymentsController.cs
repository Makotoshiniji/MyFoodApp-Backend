//PaymentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models;
using My_FoodApp.Dtos; // 👈 import DTO
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

    // ❗️❗️ นี่คือ Endpoint ใหม่สำหรับสร้าง PDF (เวอร์ชันแก้ไขแล้ว) ❗️❗️
    [HttpGet("download-bill/{orderId:int}")] // ❗️ แก้ Route เป็น int
    public async Task<IActionResult> DownloadBill(int orderId)
    {
        // 1. ดึงข้อมูลบิล (แก้ _context เป็น _db)
        // ❗️ แก้ Include ให้ตรงกับโมเดลของคุณ
        var bill = await _db.Orders
            .Include(o => o.Shop)
            .Include(o => o.OrderItems) // ❗️❗️ แก้จาก Items เป็น OrderItems
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (bill == null)
        {
            return NotFound("ไม่พบบิลนี้");
        }

        // ❗️❗️ 1. เพิ่มการดึงข้อมูล User ❗️❗️
        var user = await _db.Users.FindAsync(bill.UserId);
        // (❗️ ถ้า property ใน User model ของคุณชื่อ "Name" หรือ "FullName" ให้แก้ตรงนี้)
        var userName = user?.Username ?? "N/A";

        // 2. ใช้ QuestPDF สร้างเอกสาร
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // ตั้งค่าหน้ากระดาษ
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(12));
                // ❗️ แนะนำ: เพิ่ม .FontFamily("Sarabun") ถ้าคุณติดตั้งฟอนต์ภาษาไทย

                // 2.1 ส่วนหัว
                page.Header().AlignCenter().Text("ใบเสร็จการสั่งซื้อ")
                    .SemiBold().FontSize(20).FontColor(Colors.Purple.Medium);

                // 2.2 ส่วนเนื้อหา
                page.Content().Column(col =>
                {
                    col.Spacing(20); // ระยะห่างระหว่างบล็อก

                    // ข้อมูลบิล (❗️ แก้ไข Property ให้ถูกต้อง)
                    col.Item().Row(row =>
                    {
                        row.RelativeColumn().Column(c =>
                        {
                            c.Item().Text($"รหัสคำสั่งซื้อ: {bill.OrderCode}");
                            // ❗️❗️ สมมติว่า Order model มี UserName, ถ้าไม่มี ให้ Include User มา
                            c.Item().Text($"ลูกค้า: {userName}");
                        });
                        row.RelativeColumn().Column(c =>
                        {
                            // ❗️❗️ แก้เป็น bill.Shop.Name
                            c.Item().Text($"ร้านค้า: {bill.Shop?.Name ?? "N/A"}");
                            // ❗️❗️ แก้เป็น bill.PlacedAt
                            c.Item().Text($"วันที่: {bill.PlacedAt.ToString("yyyy-MM-dd HH:mm")}");
                        });
                    });

                    // ตารางรายการสินค้า
                    col.Item().Table(table =>
                    {
                        // กำหนดคอลัมน์
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // รายการ
                            columns.RelativeColumn(1); // จำนวน
                            columns.RelativeColumn(1); // ราคา
                        });

                        // หัวตาราง
                        table.Header(header =>
                        {
                            header.Cell().Text("รายการ").Bold();
                            header.Cell().AlignCenter().Text("จำนวน").Bold();
                            header.Cell().AlignRight().Text("ราคา (บาท)").Bold();
                        });

                        // ข้อมูลสินค้า (วนลูป ❗️ แก้เป็น bill.OrderItems)
                        foreach (var item in bill.OrderItems)
                        {
                            // ❗️❗️ แก้ไข Property
                            table.Cell().Text(item.ItemName);
                            table.Cell().AlignCenter().Text(item.Quantity.ToString());
                            table.Cell().AlignRight().Text(item.LineTotal.ToString("N2")); // ❗️❗️ ใช้ LineTotal
                        }
                    });

                    // 2.3 ยอดรวม (❗️ แก้เป็น bill.GrandTotal)
                    col.Item().AlignRight().Text($"รวมทั้งหมด: {bill.GrandTotal.ToString("N2")} บาท")
                        .Bold().FontSize(14);
                });
            });
        });

        // 3. สร้าง PDF เป็น byte[]
        byte[] pdfBytes = document.GeneratePdf();

        // 4. ส่งไฟล์กลับไปให้ User
        return File(pdfBytes, "application/pdf", $"bill-{bill.OrderCode}.pdf");
    }


}
