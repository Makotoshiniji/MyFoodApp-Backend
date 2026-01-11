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
    // DTO สำหรับส่งข้อมูลกลับไปหน้าบ้าน
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; }
        public string PlacedAt { get; set; }
        public int ItemsCount { get; set; }
        public string Notes { get; set; }
        public List<OrderDetailItemDto> OrderItems { get; set; } = new();
    }

    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; }
    }

    // DTOs สำหรับส่งข้อมูลรายละเอียดออเดอร์กลับไป
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public decimal GrandTotal { get; set; }
        public string Status { get; set; } = "";
        public DateTime PlacedAt { get; set; }
        public string? Notes { get; set; }
        public List<OrderDetailItemDto> Items { get; set; } = new();
        public string ShopName { get; set; } = "";
        public string? SlipUrl { get; set; }
        public string? ImagePath { get; set; }
        public string? Category { get; set; }
    }

    public class OrderDetailItemDto
    {
        public int Id { get; set; }
        public string MenuItemName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
        public List<OrderDetailOptionDto> Options { get; set; } = new();

        // ✅ แก้ไข 1: เพิ่มตัวแปรที่ขาดหายไปตรงนี้ เพื่อให้ DTO รู้จัก
        public string? ImagePath { get; set; }
        public string? Category { get; set; }
        public int MenuItemId { get; set; }
    }

    public class OrderDetailOptionDto
    {
        public string OptionName { get; set; } = "";
        public decimal ExtraPrice { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        // ... (CreateOrder และ CreateOrderRequest เหมือนเดิม ไม่ต้องแก้) ...
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
        {
            if (req == null || req.UserId <= 0 || req.ShopId <= 0)
                return BadRequest("invalid request");

            var items = new List<OrderItem>();
            decimal subtotal = 0m;

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
                var cart = await _db.Cart
                    .Include(c => c.Items).ThenInclude(i => i.MenuItem)
                    .Include(c => c.Items).ThenInclude(i => i.Options)
                    .FirstOrDefaultAsync(c => c.UserId == req.UserId && c.ShopId == req.ShopId);

                if (cart == null || cart.Items == null || !cart.Items.Any())
                    return BadRequest("cart empty");

                var allOptionIds = cart.Items.SelectMany(i => i.Options).Select(o => o.OptionId).Distinct().ToList();
                var optionNames = await _db.MenuOptions
                    .Where(mo => allOptionIds.Contains(mo.Id))
                    .ToDictionaryAsync(k => k.Id, v => v.Label);

                foreach (var ci in cart.Items)
                {
                    var extra = ci.Options.Sum(o => o.ExtraPrice);
                    var unit = ci.UnitPrice + extra;
                    var lineTotal = unit * ci.Qty;

                    var orderItem = new OrderItem
                    {
                        MenuItemId = ci.MenuItemId,
                        ItemName = ci.MenuItem?.Name ?? "Unknown",
                        UnitPrice = unit,
                        Quantity = ci.Qty,
                        LineTotal = lineTotal,
                        SpecialRequest = ci.SpecialRequest,
                        Options = ci.Options.Select(o => new OrderItemOption
                        {
                            OptionName = optionNames.ContainsKey(o.OptionId) ? optionNames[o.OptionId] : "Unknown Option",
                            ExtraPrice = o.ExtraPrice
                        }).ToList()
                    };

                    items.Add(orderItem);
                    subtotal += lineTotal;
                }

                _db.CartItemOptions.RemoveRange(cart.Items.SelectMany(i => i.Options));
                _db.CartItems.RemoveRange(cart.Items);
                _db.Cart.Remove(cart);
            }

            decimal discount = 0m;
            if (!string.IsNullOrWhiteSpace(req.VoucherCode) && req.VoucherCode.Equals("GRADANAJA", StringComparison.OrdinalIgnoreCase))
            {
                discount = Math.Round(subtotal * 0.25m, 2);
            }

            var vat = Math.Round(subtotal * 0.07m, 2);
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
                await tx.CommitAsync();
                return Ok(new { orderId = order.Id, grandTotal });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, "cannot create order: " + ex.Message);
            }
        }


        // =========================================================
        // 2️⃣ ดึงออเดอร์ตามร้าน (แก้ไขแล้ว)
        // =========================================================
        [HttpGet("shop/{shopId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByShop(int shopId)
        {
            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.MenuItem)
                .Where(o => o.ShopId == shopId)
                .OrderByDescending(o => o.PlacedAt)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderCode = !string.IsNullOrEmpty(o.OrderCode) ? o.OrderCode : $"#{o.Id}",
                    CustomerName = o.User != null ? o.User.Username : "Guest",
                    GrandTotal = o.GrandTotal,
                    Status = o.Status,
                    PlacedAt = o.PlacedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    ItemsCount = o.OrderItems.Count,
                    Notes = o.Notes,

                    OrderItems = o.OrderItems.Select(i => new OrderDetailItemDto
                    {
                        Id = i.Id,
                        MenuItemId = i.MenuItemId,
                        MenuItemName = i.MenuItem != null ? i.MenuItem.Name : (i.ItemName ?? "Unknown"),
                        Quantity = i.Quantity,
                        Price = i.UnitPrice,

                        // ✅ แก้ไข 2: ใช้ค่า Default ไปก่อน เพราะ MenuItem Model ยังไม่มีคอลัมน์นี้
                        // ถ้าคุณเพิ่ม Category/ImagePath ใน Models/MenuItem.cs แล้ว ให้เอา Comment ออก

                        // ImagePath = i.MenuItem.ImagePath, 
                        ImagePath = null,

                        // Category = i.MenuItem.Category, 
                        Category = "Food" // ใส่ Food เป็นค่าเริ่มต้นไปก่อนเพื่อให้ Code รันผ่าน
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // ... (UpdateStatus เหมือนเดิม) ...
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound("Order not found");

            order.Status = req.NewStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Status updated", status = order.Status });
        }

        //// ... (GetOrderById เหมือนเดิม) ...
        //[HttpGet("{id:int}")]
        //public async Task<ActionResult<OrderDetailDto>> GetOrderById(int id)
        //{
        //    var order = await _db.Orders
        //        .Include(o => o.User)
        //        .Include(o => o.Shop)
        //        .Include(o => o.OrderItems).ThenInclude(i => i.MenuItem)
        //        .Include(o => o.OrderItems).ThenInclude(i => i.Options)
        //        .Include(o => o.Payments)
        //        .FirstOrDefaultAsync(o => o.Id == id);

        //    if (order == null) return NotFound();

        //    var payment = order.Payments?.OrderByDescending(p => p.Id).FirstOrDefault();

        //    var dto = new OrderDetailDto
        //    {
        //        Id = order.Id,
        //        OrderCode = !string.IsNullOrEmpty(order.OrderCode) ? order.OrderCode : order.Id.ToString("D6"),
        //        CustomerName = order.User?.Username ?? "ลูกค้าทั่วไป",
        //        ShopName = order.Shop?.Name ?? "Unknown Shop",
        //        GrandTotal = order.GrandTotal,
        //        Status = order.Status,
        //        PlacedAt = order.PlacedAt,
        //        Notes = order.Notes,
        //        SlipUrl = order.SlipPath,

        //        Items = order.OrderItems.Select(i => new OrderDetailItemDto
        //        {
        //            Id = i.Id,
        //            MenuItemName = i.MenuItem != null ? i.MenuItem.Name : (i.ItemName ?? "Unknown Item"),
        //            Quantity = i.Quantity,
        //            Price = i.UnitPrice,
        //            Notes = i.SpecialRequest,

        //            // ✅ ใส่ Default ไว้ก่อนเหมือนกัน
        //            ImagePath = null,
        //            Category = "Food",

        //            Options = i.Options.Select(o => new OrderDetailOptionDto
        //            {
        //                OptionName = o.OptionName,
        //                ExtraPrice = o.ExtraPrice
        //            }).ToList()
        //        }).ToList()
        //    };

        //    return Ok(dto);
        //}

        // ... (UploadSlip เหมือนเดิม) ...
        [HttpPost("{id}/slip")]
        public async Task<IActionResult> UploadSlip(int id, IFormFile SlipFile)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound("Order not found");

            if (SlipFile == null || SlipFile.Length == 0)
                return BadRequest("No file uploaded");

            var folderName = Path.Combine("uploads", "slips");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            Directory.CreateDirectory(pathToSave);

            var fileName = $"slip_{id}_{DateTime.Now.Ticks}{Path.GetExtension(SlipFile.FileName)}";
            var fullPath = Path.Combine(pathToSave, fileName);
            var dbPath = Path.Combine(folderName, fileName).Replace("\\", "/");

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await SlipFile.CopyToAsync(stream);
            }

            order.SlipPath = dbPath;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Upload successful", slipPath = dbPath });
        }
        // GET: api/Orders/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrdersByUser(int userId)
        {
            var orders = await _db.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.PlacedAt) // เรียงจากล่าสุดไปเก่าสุด
                .Select(o => new
                {
                    id = o.Id,
                    orderCode = o.OrderCode,
                    totalAmount = o.GrandTotal,
                    status = o.Status,
                    // แปลงวันที่เป็น string ให้สวยงาม หรือส่งไปเป็น DateTime ก็ได้
                    date = o.PlacedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Ok(orders);
        }
        // GET: api/Orders/{id}
        // สำหรับหน้าใบเสร็จ (NewBillDetailScreen)
        //[HttpGet("{id}")]
        //public async Task<ActionResult> GetOrderDetail(int id)
        //{
        //    var order = await _db.Orders
        //        .Include(o => o.Shop)           // 1. Join เพื่อเอาชื่อร้าน
        //        .Include(o => o.User)
        //        .Include(o => o.OrderItems)     // 2. Join เอารายการอาหาร
        //            .ThenInclude(oi => oi.MenuItem) // 3. Join เอาชื่ออาหาร
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(o => o.Id == id);

        //    if (order == null) return NotFound();

        //    // 4. แปลงข้อมูลให้ตรงกับที่หน้า Frontend (NewBillDetailScreen) ต้องการ
        //    var result = new
        //    {
        //        id = order.Id,
        //        customerName = order.User?.Username ?? "Unknown",
        //        shopName = order.Shop?.Name ?? "Unknown Shop", // ส่งชื่อร้าน
        //        placedAt = order.PlacedAt,
        //        grandTotal = order.GrandTotal,
        //        status = order.Status,
        //        notes = order.Notes,

        //        // แปลง OrderItems -> items (ตัวเล็ก)
        //        items = order.OrderItems.Select(oi => new
        //        {
        //            quantity = oi.Quantity,
        //            menuItemName = oi.MenuItem?.Name ?? "Unknown Item", // ส่งชื่อเมนู
        //            price = oi.UnitPrice,

        //            // (Optional) ถ้ามีตาราง OrderItemOptions ก็ต้อง Join มาใส่ตรงนี้
        //            // ตอนนี้ใส่เป็น list ว่างๆ ไปก่อนเพื่อไม่ให้ error
        //            options = new List<object>()
        //        })
        //    };

        //    return Ok(result);
        //}


        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrderDetail(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Shop)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderItems) // ✅ 1. เพิ่มการ Join เพื่อดึง Options ของรายการอาหาร
                    .ThenInclude(oi => oi.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var result = new
            {
                id = order.Id,
                orderCode = order.OrderCode, // ✅ เพิ่มเพื่อให้แสดงผลได้ตาม Interface
                customerName = order.User?.Username ?? "Unknown",
                shopName = order.Shop?.Name ?? "Unknown Shop",
                placedAt = order.PlacedAt,
                grandTotal = order.GrandTotal,
                status = order.Status,
                notes = order.Notes, // ✅ โน้ตของออเดอร์ (รวม)
                slipUrl = order.SlipPath,

                items = order.OrderItems.Select(oi => new
                {
                    id = oi.Id,
                    menuItemName = oi.MenuItem?.Name ?? "Unknown Item",
                    quantity = oi.Quantity,
                    price = oi.UnitPrice,
                    // ✅ 2. แก้ไข: ส่งโน้ตของแต่ละเมนู (เช่น ไม่เผ็ด, ไม่ผัก)
                    notes = oi.SpecialRequest,
                    // ✅ 3. แก้ไข: ส่งรายการตัวเลือกเสริมที่ลูกค้าเลือก
                    options = oi.Options.Select(opt => new {
                        optionName = opt.OptionName,
                        extraPrice = opt.ExtraPrice
                    }).ToList()
                })
            };

            return Ok(result);
        }
        // DTO สำหรับรับข้อมูลการสั่งซื้อจากหน้าบ้าน
        public class CreateOrderRequest
        {
            public int UserId { get; set; }
            public int ShopId { get; set; }
            public string? VoucherCode { get; set; }
            public string? Notes { get; set; }
            // เพิ่มส่วนนี้เพื่อรองรับการส่งรายการอาหารโดยตรงจากตะกร้า
            public List<CreateOrderItemRequest>? Items { get; set; }
        }

        public class CreateOrderItemRequest
        {
            public int MenuItemId { get; set; }
            public string? ItemName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
        }
    }
}