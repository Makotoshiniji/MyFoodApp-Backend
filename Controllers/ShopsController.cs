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
    }
}
