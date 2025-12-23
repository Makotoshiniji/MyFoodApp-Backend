////ShopsController.cs
//using My_FoodApp.Dtos;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using My_FoodApp.Data;

//namespace My_FoodApp.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ShopsController : ControllerBase
//    {
//        private readonly AppDbContext _db;
//        public ShopsController(AppDbContext db) => _db = db;

//        // =========================
//        //  GET /api/shops
//        //  รายการร้านทั้งหมด + PromoUrl
//        // =========================
//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            var list = await _db.Shops
//                .OrderBy(s => s.Name)
//                .Select(s => new
//                {
//                    s.Id,
//                    s.Name,
//                    s.Description,
//                    s.IsOpen,
//                    s.RatingAvg,
//                    s.RatingCount,
//                    // เลือกรูปโปรโมทร้านรูปแรกตาม SortOrder
//                    PromoUrl = _db.ShopMedia
//                        .Where(m => m.ShopId == s.Id && m.MediaType == "promo")
//                        .OrderBy(m => m.SortOrder)
//                        .Select(m => m.Url)
//                        .FirstOrDefault()
//                })
//                .ToListAsync();

//            return Ok(list);
//        }

//        // =========================
//        //  GET /api/shops/popular
//        //  4 ร้านยอดนิยม (อิง RatingCount ชั่วคราว) + PromoUrl
//        // =========================
//        [HttpGet("popular")]
//        public async Task<IActionResult> GetPopular()
//        {
//            var top = await _db.Shops
//                .OrderByDescending(s => s.RatingCount)  // ภายหลังเปลี่ยนเป็นยอดออเดอร์จริง
//                .ThenBy(s => s.Name)
//                .Take(4)
//                .Select(s => new
//                {
//                    s.Id,
//                    s.Name,
//                    s.Description,
//                    s.IsOpen,
//                    s.RatingAvg,
//                    s.RatingCount,
//                    PromoUrl = _db.ShopMedia
//                        .Where(m => m.ShopId == s.Id && m.MediaType == "promo")
//                        .OrderBy(m => m.SortOrder)
//                        .Select(m => m.Url)
//                        .FirstOrDefault()
//                })
//                .ToListAsync();

//            return Ok(top);
//        }

//        // =========================
//        //  GET /api/shops/{id}
//        //  รายละเอียดร้าน + media ทั้งหมด + categories
//        // =========================
//        [HttpGet("{id:int}")]
//        public async Task<IActionResult> GetOne(int id)
//        {
//            var one = await _db.Shops
//                .Where(s => s.Id == id)
//                .Select(s => new
//                {
//                    s.Id,
//                    s.Name,
//                    s.Description,
//                    s.Phone,
//                    s.IsOpen,
//                    s.OpenTime,
//                    s.CloseTime,
//                    s.RatingAvg,
//                    s.RatingCount,

//                    Media = _db.ShopMedia
//                        .Where(m => m.ShopId == s.Id)
//                        .OrderBy(m => m.SortOrder)
//                        .Select(m => new
//                        {
//                            m.Id,
//                            Kind = m.MediaType, // ถ้าแมปคอลัมน์เป็น 'kind' อยู่ ให้ดูโน้ตด้านล่าง
//                            m.Url,
//                            m.SortOrder
//                        })
//                        .ToList(),

//                    Categories = _db.Categories
//                        .Where(c => c.ShopId == s.Id)
//                        .OrderBy(c => c.SortOrder)
//                        .Select(c => new { c.Id, c.Name, c.SortOrder })
//                        .ToList()
//                })
//                .FirstOrDefaultAsync();

//            return one is null ? NotFound() : Ok(one);
//        }

//        // =========================
//        //  POST /api/shops
//        // =========================
//        [HttpPost]
//        public async Task<IActionResult> Create([FromBody] Models.Shop shop)
//        {
//            _db.Shops.Add(shop);
//            await _db.SaveChangesAsync();
//            return CreatedAtAction(nameof(GetOne), new { id = shop.Id }, new { shop.Id, shop.Name });
//        }
//        // =========================
//        // ✅ 1. เช็คว่า User เป็นเจ้าของร้านหรือไม่?
//        // =========================
//        [HttpGet("check-owner/{userId}")]
//        public async Task<IActionResult> CheckOwner(int userId)
//        {
//            // หาว่า userId นี้ เป็น Owner ของร้านไหนไหม
//            var shop = await _db.Shops.FirstOrDefaultAsync(s => s.OwnerUserId == userId);

//            if (shop == null)
//            {
//                return Ok(new { hasShop = false });
//            }

//            // ถ้ามีร้าน ให้ส่ง ID และชื่อร้านกลับไปด้วย
//            return Ok(new { hasShop = true, shopId = shop.Id, shopName = shop.Name });
//        }

//        // =========================
//        // ✅ 2. สมัครร้านค้าใหม่
//        // =========================
//        [HttpPost("register")]
//        public async Task<IActionResult> RegisterShop([FromBody] RegisterShopDto dto)
//        {
//            // เช็คว่ามีร้านอยู่แล้วหรือยัง (กันเหนียว)
//            var existing = await _db.Shops.AnyAsync(s => s.OwnerUserId == dto.OwnerUserId);
//            if (existing) return BadRequest("ผู้ใช้นี้มีร้านค้าอยู่แล้ว");

//            var shop = new Models.Shop // ใช้ Models.Shop ให้ชัดเจน
//            {
//                OwnerUserId = dto.OwnerUserId,
//                Name = dto.Name,
//                Description = dto.Description,
//                Phone = dto.Phone,
//                IsOpen = true, // เปิดร้านอัตโนมัติเลย
//                CreatedAt = DateTime.UtcNow,
//                UpdatedAt = DateTime.UtcNow,
//                RatingAvg = 0,
//                RatingCount = 0
//            };

//            _db.Shops.Add(shop);
//            await _db.SaveChangesAsync();

//            return Ok(new { message = "สร้างร้านค้าสำเร็จ", shopId = shop.Id });
//        }
//        public class ShopDashboardDto
//        {
//            public int RunningOrders { get; set; }
//            public int OrderRequest { get; set; }
//            public decimal TotalRevenueToday { get; set; }
//            public decimal RatingAvg { get; set; }
//            public int RatingCount { get; set; }
//            public List<PopularItemDto> PopularItems { get; set; } = new();
//        }

//        public class PopularItemDto
//        {
//            public int Id { get; set; }
//            public string Name { get; set; } = string.Empty;
//            public string? ImageUrl { get; set; }
//            public int OrderCount { get; set; }
//        }
//        // =========================
//        // ✅ GET /api/shops/{id}/dashboard
//        // =========================
//        [HttpGet("{id}/dashboard")]
//        public async Task<IActionResult> GetDashboard(int id)
//        {
//            var shop = await _db.Shops.FindAsync(id);
//            if (shop == null) return NotFound("Shop not found");

//            // 1. Running Orders (สถานะที่ยังไม่เสร็จ)
//            var runningCount = await _db.Orders
//                .Where(o => o.ShopId == id && (o.Status == "pending" || o.Status == "preparing" || o.Status == "delivering"))
//                .CountAsync();

//            // 2. Order Request (สถานะรอรับออเดอร์)
//            var requestCount = await _db.Orders
//                .Where(o => o.ShopId == id && o.Status == "pending_confirm")
//                .CountAsync();

//            // 3. Total Revenue Today (ยอดขายวันนี้)
//            var today = DateTime.UtcNow.Date;
//            var revenue = await _db.Orders
//                .Where(o => o.ShopId == id && o.Status == "completed" && o.PlacedAt.Date == today)
//                .SumAsync(o => o.GrandTotal);

//            // 4. Popular Items (สินค้าขายดี 2 อันดับแรก)
//            // (ซับซ้อนหน่อย ถ้ายังไม่มี OrderItem เยอะ ให้ Mock ไปก่อน หรือเขียน Query)
//            var popular = await _db.OrderItems
//                .Where(oi => oi.Order!.ShopId == id)
//                .GroupBy(oi => oi.MenuItemId)
//                .Select(g => new {
//                    ItemId = g.Key,
//                    Count = g.Sum(x => x.Quantity)
//                })
//                .OrderByDescending(x => x.Count)
//                .Take(2)
//                .Join(_db.MenuItems,
//                      g => g.ItemId,
//                      m => m.Id,
//                      (g, m) => new PopularItemDto
//                      {
//                          Id = m.Id,
//                          Name = m.Name,
//                          OrderCount = g.Count,
//                          ImageUrl = m.MainPhotoUrl // หรือจะดึงจาก ShopMedia ก็ได้ถ้ามี
//                      })
//                .ToListAsync();

//            return Ok(new ShopDashboardDto
//            {
//                RunningOrders = runningCount,
//                OrderRequest = requestCount,
//                TotalRevenueToday = revenue,
//                RatingAvg = shop.RatingAvg,
//                RatingCount = shop.RatingCount,
//                PopularItems = popular
//            });
//        }

//        // =========================
//        // ✅ PUT /api/shops/{id}
//        // แก้ไขข้อมูลร้านค้า
//        // =========================
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateShop(int id, [FromBody] RegisterShopDto dto)
//        {
//            var shop = await _db.Shops.FindAsync(id);
//            if (shop == null) return NotFound("Shop not found");

//            // อัปเดตข้อมูล
//            shop.Name = dto.Name;
//            shop.Description = dto.Description;
//            shop.Phone = dto.Phone;
//            shop.UpdatedAt = DateTime.UtcNow;

//            await _db.SaveChangesAsync();

//            return Ok(new { message = "อัปเดตข้อมูลร้านค้าเรียบร้อยแล้ว" });
//        }
//    }
//}
using My_FoodApp.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models; // อย่าลืม using Models เพื่อใช้ ShopMedia
using System.IO; // จำเป็นสำหรับการจัดการไฟล์

namespace My_FoodApp.Controllers
{
    // DTO สำหรับรับข้อมูลแก้ไขร้านค้า (แบบมีรูป)
    public class UpdateShopRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public IFormFile? ImageFile { get; set; } // รับไฟล์รูป
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ShopsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ShopsController(AppDbContext db) => _db = db;

        // =========================
        //  GET /api/shops
        //  รายการร้านทั้งหมด + PromoUrl
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Shops
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.IsOpen,
                    s.RatingAvg,
                    s.RatingCount,
                    // เลือกรูปโปรโมทร้านรูปแรกตาม SortOrder
                    PromoUrl = _db.ShopMedia
                        .Where(m => m.ShopId == s.Id && m.MediaType == "promo")
                        .OrderBy(m => m.SortOrder)
                        .Select(m => m.Url)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(list);
        }

        // =========================
        //  GET /api/shops/popular
        //  4 ร้านยอดนิยม (อิง RatingCount ชั่วคราว) + PromoUrl
        // =========================
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            var top = await _db.Shops
                .OrderByDescending(s => s.RatingCount)  // ภายหลังเปลี่ยนเป็นยอดออเดอร์จริง
                .ThenBy(s => s.Name)
                .Take(4)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.IsOpen,
                    s.RatingAvg,
                    s.RatingCount,
                    PromoUrl = _db.ShopMedia
                        .Where(m => m.ShopId == s.Id && m.MediaType == "promo")
                        .OrderBy(m => m.SortOrder)
                        .Select(m => m.Url)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(top);
        }

        // =========================
        //  GET /api/shops/{id}
        //  รายละเอียดร้าน + media ทั้งหมด + categories
        // =========================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var one = await _db.Shops
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Phone,
                    s.IsOpen,
                    s.OpenTime,
                    s.CloseTime,
                    s.RatingAvg,
                    s.RatingCount,

                    Media = _db.ShopMedia
                        .Where(m => m.ShopId == s.Id)
                        .OrderBy(m => m.SortOrder)
                        .Select(m => new
                        {
                            m.Id,
                            Kind = m.MediaType, // ถ้าแมปคอลัมน์เป็น 'kind' อยู่ ให้ดูโน้ตด้านล่าง
                            m.Url,
                            m.SortOrder
                        })
                        .ToList(),

                    Categories = _db.Categories
                        .Where(c => c.ShopId == s.Id)
                        .OrderBy(c => c.SortOrder)
                        .Select(c => new { c.Id, c.Name, c.SortOrder })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return one is null ? NotFound() : Ok(one);
        }

        // =========================
        //  POST /api/shops
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Models.Shop shop)
        {
            _db.Shops.Add(shop);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = shop.Id }, new { shop.Id, shop.Name });
        }

        // =========================
        // ✅ 1. เช็คว่า User เป็นเจ้าของร้านหรือไม่?
        // =========================
        [HttpGet("check-owner/{userId}")]
        public async Task<IActionResult> CheckOwner(int userId)
        {
            // หาว่า userId นี้ เป็น Owner ของร้านไหนไหม
            var shop = await _db.Shops.FirstOrDefaultAsync(s => s.OwnerUserId == userId);

            if (shop == null)
            {
                return Ok(new { hasShop = false });
            }

            // ถ้ามีร้าน ให้ส่ง ID และชื่อร้านกลับไปด้วย
            return Ok(new { hasShop = true, shopId = shop.Id, shopName = shop.Name });
        }

        // =========================
        // ✅ 2. สมัครร้านค้าใหม่
        // =========================
        [HttpPost("register")]
        public async Task<IActionResult> RegisterShop([FromBody] RegisterShopDto dto)
        {
            // เช็คว่ามีร้านอยู่แล้วหรือยัง (กันเหนียว)
            var existing = await _db.Shops.AnyAsync(s => s.OwnerUserId == dto.OwnerUserId);
            if (existing) return BadRequest("ผู้ใช้นี้มีร้านค้าอยู่แล้ว");

            var shop = new Models.Shop // ใช้ Models.Shop ให้ชัดเจน
            {
                OwnerUserId = dto.OwnerUserId,
                Name = dto.Name,
                Description = dto.Description,
                Phone = dto.Phone,
                IsOpen = true, // เปิดร้านอัตโนมัติเลย
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RatingAvg = 0,
                RatingCount = 0
            };

            _db.Shops.Add(shop);
            await _db.SaveChangesAsync();

            return Ok(new { message = "สร้างร้านค้าสำเร็จ", shopId = shop.Id });
        }

        public class ShopDashboardDto
        {
            public int RunningOrders { get; set; }
            public int OrderRequest { get; set; }
            public decimal TotalRevenueToday { get; set; }
            public decimal RatingAvg { get; set; }
            public int RatingCount { get; set; }
            public List<PopularItemDto> PopularItems { get; set; } = new();
        }

        public class PopularItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? ImageUrl { get; set; }
            public int OrderCount { get; set; }
        }

        public class ToggleShopStatusDto
        {
            public bool IsOpen { get; set; }
        }

        // =========================
        // ✅ GET /api/shops/{id}/dashboard
        // =========================
        [HttpGet("{id}/dashboard")]
        public async Task<IActionResult> GetDashboard(int id)
        {
            var shop = await _db.Shops.FindAsync(id);
            if (shop == null) return NotFound("Shop not found");

            // 1. Running Orders (สถานะที่ยังไม่เสร็จ)
            var runningCount = await _db.Orders
                .Where(o => o.ShopId == id && (o.Status == "pending" || o.Status == "preparing" || o.Status == "delivering"))
                .CountAsync();

            // 2. Order Request (สถานะรอรับออเดอร์)
            var requestCount = await _db.Orders
                .Where(o => o.ShopId == id && o.Status == "pending_confirm")
                .CountAsync();

            // 3. Total Revenue Today (ยอดขายวันนี้)
            var today = DateTime.UtcNow.Date;
            var revenue = await _db.Orders
                .Where(o => o.ShopId == id && o.Status == "completed" && o.PlacedAt.Date == today)
                .SumAsync(o => o.GrandTotal);

            // 4. Popular Items (สินค้าขายดี 2 อันดับแรก)
            var popular = await _db.OrderItems
                .Where(oi => oi.Order!.ShopId == id)
                .GroupBy(oi => oi.MenuItemId)
                .Select(g => new {
                    ItemId = g.Key,
                    Count = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Count)
                .Take(2)
                .Join(_db.MenuItems,
                      g => g.ItemId,
                      m => m.Id,
                      (g, m) => new PopularItemDto
                      {
                          Id = m.Id,
                          Name = m.Name,
                          OrderCount = g.Count,
                          ImageUrl = m.MainPhotoUrl // หรือจะดึงจาก ShopMedia ก็ได้ถ้ามี
                      })
                .ToListAsync();

            return Ok(new ShopDashboardDto
            {
                RunningOrders = runningCount,
                OrderRequest = requestCount,
                TotalRevenueToday = revenue,
                RatingAvg = shop.RatingAvg,
                RatingCount = shop.RatingCount,
                PopularItems = popular
            });
        }

        // =========================
        // ✅ PUT /api/shops/{id} (แก้ไขใหม่: รองรับการอัปโหลดรูป)
        // แก้ไขข้อมูลร้านค้า + อัปรูป
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShop(int id, [FromForm] UpdateShopRequest req)
        {
            var shop = await _db.Shops.FindAsync(id);
            if (shop == null) return NotFound("Shop not found");

            // 1. อัปเดตข้อมูล Text
            shop.Name = req.Name;
            shop.Description = req.Description;
            shop.Phone = req.Phone;
            shop.UpdatedAt = DateTime.UtcNow;

            // 2. ถ้ามีการส่งรูปมาด้วย (ImageFile ไม่เป็น null และมีข้อมูล)
            if (req.ImageFile != null && req.ImageFile.Length > 0)
            {
                // -- A. Save File ลงเครื่อง Server --
                // สร้างโฟลเดอร์: wwwroot/shop_uploads/shops/{id}/promo/
                var folderName = Path.Combine("shop_uploads", "shops", id.ToString(), "promo");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
                Directory.CreateDirectory(pathToSave); // สร้างโฟลเดอร์ถ้ายังไม่มี

                // ตั้งชื่อไฟล์ (ใช้เวลาปัจจุบันต่อท้ายเพื่อไม่ให้ชื่อซ้ำ)
                var fileName = $"banner_{DateTime.Now.Ticks}{Path.GetExtension(req.ImageFile.FileName)}";
                var fullPath = Path.Combine(pathToSave, fileName);

                // Path ที่จะเก็บใน DB (ใช้ / แทน \)
                var dbPath = Path.Combine(folderName, fileName).Replace("\\", "/");
                if (!dbPath.StartsWith("/")) dbPath = "/" + dbPath;

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await req.ImageFile.CopyToAsync(stream);
                }

                // -- B. Update Database (ShopMedia) --
                // เช็คว่ามีรูป Promo เดิมไหม ถ้ามีให้อัปเดต Url
                var existingMedia = await _db.ShopMedia
                    .FirstOrDefaultAsync(m => m.ShopId == id && m.MediaType == "promo");

                if (existingMedia != null)
                {
                    // อัปเดต URL ใหม่
                    existingMedia.Url = dbPath;
                }
                else
                {
                    // ถ้ายังไม่เคยมี ให้สร้าง record ใหม่
                    _db.ShopMedia.Add(new ShopMedia
                    {
                        ShopId = id,
                        MediaType = "promo",
                        Url = dbPath,
                        SortOrder = 1
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "อัปเดตข้อมูลและรูปภาพสำเร็จ" });
        }
        // =========================
        // ✅ PUT /api/shops/{id}/status
        // สำหรับเปิด-ปิดร้านโดยเฉพาะ (รวดเร็ว ไม่ต้องส่งรูป)
        // =========================
        [HttpPut("{id}/status")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] ToggleShopStatusDto req)
        {
            var shop = await _db.Shops.FindAsync(id);
            if (shop == null) return NotFound("Shop not found");

            // อัปเดตสถานะ
            shop.IsOpen = req.IsOpen;
            shop.UpdatedAt = DateTime.UtcNow; // อัปเดตเวลาด้วย

            await _db.SaveChangesAsync();
            return Ok(new { message = $"สถานะร้านเปลี่ยนเป็น {(req.IsOpen ? "เปิด" : "ปิด")} แล้ว", isOpen = shop.IsOpen });
        }
    }
}