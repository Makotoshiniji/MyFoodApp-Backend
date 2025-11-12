using Microsoft.AspNetCore.Mvc;
using My_FoodApp.Data;
using My_FoodApp.Models;

namespace My_FoodApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SeedController(AppDbContext db) => _db = db;

        [HttpPost("shops")]
        public async Task<IActionResult> SeedShops()
        {
            if (_db.Shops.Any()) return Ok("Already seeded.");
            var shops = new[]
            {
                new Shop { Name = "KFC", Description = "ไก่ทอดสูตรต้นตำรับ", IsOpen = true },
                new Shop { Name = "McDonald", Description = "เบอร์เกอร์/ของหวาน", IsOpen = true },
                new Shop { Name = "Burger King", Description = "วอปเปอร์ย่างไฟ", IsOpen = true }
            };
            _db.Shops.AddRange(shops);
            await _db.SaveChangesAsync();
            return Ok(shops);
        }
    }
}
