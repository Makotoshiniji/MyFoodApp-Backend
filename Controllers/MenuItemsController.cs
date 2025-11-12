// Controllers/MenuItemsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_FoodApp.Controllers
{
    // ==============================================
    // DTOs
    // ==============================================

    public class MenuOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal ExtraPrice { get; set; }
        public bool IsDefault { get; set; }
    }

    public class MenuOptionGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; }
        public int MinSelect { get; set; }
        public int MaxSelect { get; set; }
        public List<MenuOptionDto> Options { get; set; } = new();
    }

    public class MenuItemDetailDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public List<MenuOptionGroupDto> OptionGroups { get; set; } = new();
    }

    // ==============================================
    // Controller
    // ==============================================

    [ApiController]
    [Route("api/[controller]")]
    public class MenuItemsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MenuItemsController(AppDbContext db)
        {
            _db = db;
        }

        // ==============================================================
        // GET: /api/MenuItems?shopId=1
        // ==============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItem>>> Get([FromQuery] int? shopId)
        {
            IQueryable<MenuItem> q = _db.MenuItems.AsNoTracking();

            if (shopId.HasValue)
            {
                q = q.Where(m => m.ShopId == shopId.Value);
            }

            var list = await q
                .OrderBy(m => m.Name)
                .ToListAsync();

            return Ok(list);
        }

        // ==============================================================
        // GET: /api/shops/1/menuitems
        // ==============================================================
        [HttpGet("~/api/shops/{shopId:int}/menuitems")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetByShop(int shopId)
        {
            var list = await _db.MenuItems
                .AsNoTracking()
                .Where(m => m.ShopId == shopId)
                .OrderBy(m => m.Name)
                .ToListAsync();

            return Ok(list);
        }

        // ==============================================================
        // GET: /api/MenuItems/5/detail
        // ==============================================================
        [HttpGet("{id:int}/detail")]
        public async Task<ActionResult<MenuItemDetailDto>> GetDetail(int id)
        {
            // ✅ ดึงข้อมูลหลักของเมนู
            var item = await _db.MenuItems.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
                return NotFound();

            // ✅ ดึงกลุ่มตัวเลือกของเมนูนี้ (menu_item_option_groups)
            var groups = await _db.MenuItemOptionGroups
                .AsNoTracking()
                .Where(g => g.MenuItemId == id)
                .OrderBy(g => g.SortOrder)
                .ToListAsync();

            var groupIds = groups.Select(g => g.Id).ToList();

            // ✅ ดึง options ของกลุ่มเหล่านี้ (menu_options)
            var options = await _db.MenuOptions
                .AsNoTracking()
                .Where(o => groupIds.Contains(o.GroupId))
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            // ✅ รวมข้อมูลทั้งหมดเป็น DTO
            var dto = new MenuItemDetailDto
            {
                Id = item.Id,
                ShopId = item.ShopId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                OptionGroups = groups.Select(g => new MenuOptionGroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsRequired = g.IsRequired,
                    MinSelect = g.MinSelection,
                    MaxSelect = g.MaxSelection,
                    Options = options
                        .Where(o => o.GroupId == g.Id)
                        .Select(o => new MenuOptionDto
                        {
                            Id = o.Id,
                            Name = o.Label,
                            ExtraPrice = o.ExtraPrice,
                            IsDefault = o.IsDefault
                        })
                        .ToList()
                }).ToList()
            };

            return Ok(dto);
        }
    }
}
