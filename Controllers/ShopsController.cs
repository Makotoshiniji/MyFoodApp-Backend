//ShopsController.cs
using My_FoodApp.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;

namespace My_FoodApp.Controllers
{
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
            // (ซับซ้อนหน่อย ถ้ายังไม่มี OrderItem เยอะ ให้ Mock ไปก่อน หรือเขียน Query)
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
    }
}
