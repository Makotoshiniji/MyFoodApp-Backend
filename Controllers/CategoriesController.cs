using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;

namespace My_FoodApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db) => _db = db;

        // GET /api/categories/1  (1 = ShopId)
        [HttpGet("{shopId:int}")]
        public async Task<IActionResult> GetByShop(int shopId)
            => Ok(await _db.Categories
                .Where(c => c.ShopId == shopId)
                .OrderBy(c => c.SortOrder).ToListAsync());
    }
}
