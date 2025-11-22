//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using My_FoodApp.Data;
//using My_FoodApp.Models;
//using My_FoodApp.Dtos;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace My_FoodApp.Controllers
//{
//    // ==============================================
//    // DTOs
//    // ==============================================
//    public class MenuItemFormDto
//    {
//        public int ShopId { get; set; }
//        public string Name { get; set; } = string.Empty;
//        public string? Description { get; set; }
//        public decimal Price { get; set; }
//        public string? Type { get; set; }
//        public bool IsAvailable { get; set; }
//    }

//    public class MenuOptionDto
//    {
//        public int Id { get; set; }
//        public string Name { get; set; } = null!;
//        public decimal ExtraPrice { get; set; }
//        public bool IsDefault { get; set; }
//    }

//    public class MenuOptionGroupDto
//    {
//        public int Id { get; set; }
//        public string Name { get; set; } = null!;
//        public bool IsRequired { get; set; }
//        public int MinSelect { get; set; }
//        public int MaxSelect { get; set; }
//        public List<MenuOptionDto> Options { get; set; } = new();
//    }

//    public class MenuItemDetailDto
//    {
//        public int Id { get; set; }
//        public int ShopId { get; set; }
//        public string Name { get; set; } = null!;
//        public string? Description { get; set; }
//        public decimal Price { get; set; }
//        public string? ImageUrl { get; set; }
//        public string? Type { get; set; }
//        public bool IsAvailable { get; set; }
//        public List<MenuOptionGroupDto> OptionGroups { get; set; } = new();
//    }

//    // ==============================================
//    // Controller
//    // ==============================================

//    [ApiController]
//    [Route("api/[controller]")]
//    public class MenuItemsController : ControllerBase
//    {
//        private readonly AppDbContext _db;
//        private readonly IWebHostEnvironment _env;

//        public MenuItemsController(AppDbContext db, IWebHostEnvironment env)
//        {
//            _db = db;
//            _env = env;
//        }

//        // GET: /api/MenuItems?shopId=1
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<MenuItem>>> Get([FromQuery] int? shopId)
//        {
//            IQueryable<MenuItem> q = _db.MenuItems.AsNoTracking();
//            if (shopId.HasValue)
//            {
//                q = q.Where(m => m.ShopId == shopId.Value);
//            }
//            var list = await q.OrderBy(m => m.Name).ToListAsync();
//            return Ok(list);
//        }

//        // GET: /api/shops/1/menuitems
//        [HttpGet("~/api/shops/{shopId:int}/menuitems")]
//        public async Task<ActionResult<IEnumerable<MenuItem>>> GetByShop(int shopId)
//        {
//            var list = await _db.MenuItems
//                .AsNoTracking()
//                .Where(m => m.ShopId == shopId)
//                .OrderBy(m => m.Name)
//                .ToListAsync();
//            return Ok(list);
//        }

//        // GET: /api/MenuItems/5/detail
//        [HttpGet("{id:int}/detail")]
//        public async Task<ActionResult<MenuItemDetailDto>> GetDetail(int id)
//        {
//            var item = await _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
//            if (item == null) return NotFound();

//            var groups = await _db.MenuItemOptionGroups
//                .AsNoTracking()
//                .Where(g => g.MenuItemId == id)
//                .OrderBy(g => g.SortOrder)
//                .ToListAsync();

//            var groupIds = groups.Select(g => g.Id).ToList();

//            var options = await _db.MenuOptions
//                .AsNoTracking()
//                .Where(o => groupIds.Contains(o.GroupId))
//                .OrderBy(o => o.SortOrder)
//                .ToListAsync();

//            var dto = new MenuItemDetailDto
//            {
//                Id = item.Id,
//                ShopId = item.ShopId,
//                Name = item.Name,
//                Description = item.Description,
//                Price = item.Price,
//                ImageUrl = item.MainPhotoUrl,
//                Type = item.Type,
//                IsAvailable = item.IsAvailable,
//                OptionGroups = groups.Select(g => new MenuOptionGroupDto
//                {
//                    Id = g.Id,
//                    Name = g.Name,
//                    IsRequired = g.IsRequired,
//                    MinSelect = g.MinSelection,
//                    MaxSelect = g.MaxSelection,
//                    Options = options
//                        .Where(o => o.GroupId == g.Id)
//                        .Select(o => new MenuOptionDto
//                        {
//                            Id = o.Id,
//                            Name = o.Label,
//                            ExtraPrice = o.ExtraPrice,
//                            IsDefault = o.IsDefault
//                        })
//                        .ToList()
//                }).ToList()
//            };
//            return Ok(dto);
//        }

//        // POST: เพิ่มสินค้าใหม่
//        [HttpPost]
//        public async Task<IActionResult> Create([FromForm] MenuItemFormDto dto, IFormFile? file)
//        {
//            var item = new MenuItem
//            {
//                ShopId = dto.ShopId,
//                Name = dto.Name,
//                Description = dto.Description,
//                Price = dto.Price,
//                Type = dto.Type,
//                IsAvailable = dto.IsAvailable,
//                CreatedAt = DateTime.UtcNow,
//                UpdatedAt = DateTime.UtcNow
//            };

//            // 1. บันทึกครั้งแรกเพื่อให้ได้ item.Id ก่อน
//            _db.MenuItems.Add(item);
//            await _db.SaveChangesAsync();

//            // 2. จัดการรูปภาพ (ถ้ามี)
//            if (file != null && file.Length > 0)
//            {
//                // โฟลเดอร์: shop_uploads/menu/{shopId}
//                var uploadFolder = Path.Combine(_env.ContentRootPath, "shop_uploads", "menu", dto.ShopId.ToString());
//                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

//                // 🟢 ตั้งชื่อไฟล์ใหม่: {shopId}_{itemId}.นามสกุลเดิม
//                var fileName = $"{dto.ShopId}_{item.Id}{Path.GetExtension(file.FileName)}";
//                var filePath = Path.Combine(uploadFolder, fileName);

//                using (var stream = new FileStream(filePath, FileMode.Create))
//                {
//                    await file.CopyToAsync(stream);
//                }

//                // อัปเดต Path ใน DB
//                item.MainPhotoUrl = $"/shop_uploads/menu/{dto.ShopId}/{fileName}";

//                // 3. บันทึกซ้ำอีกรอบเพื่อเก็บ URL
//                await _db.SaveChangesAsync();
//            }

//            return Ok(item);
//        }

//        // PUT: แก้ไขสินค้า
//        [HttpPut("{id}")]
//        public async Task<IActionResult> Update(int id, [FromForm] MenuItemFormDto dto, IFormFile? file)
//        {
//            var item = await _db.MenuItems.FindAsync(id);
//            if (item == null) return NotFound("ไม่พบสินค้านี้");

//            item.Name = dto.Name;
//            item.Description = dto.Description;
//            item.Price = dto.Price;
//            item.Type = dto.Type;
//            item.IsAvailable = dto.IsAvailable;
//            item.UpdatedAt = DateTime.UtcNow;

//            if (file != null && file.Length > 0)
//            {
//                var uploadFolder = Path.Combine(_env.ContentRootPath, "shop_uploads", "menu", item.ShopId.ToString());
//                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

//                // 🟢 ตั้งชื่อไฟล์ใหม่: {shopId}_{itemId}.นามสกุลเดิม (ทับไฟล์เก่าถ้ามี)
//                var fileName = $"{item.ShopId}_{item.Id}{Path.GetExtension(file.FileName)}";
//                var filePath = Path.Combine(uploadFolder, fileName);

//                using (var stream = new FileStream(filePath, FileMode.Create))
//                {
//                    await file.CopyToAsync(stream);
//                }

//                item.MainPhotoUrl = $"/shop_uploads/menu/{item.ShopId}/{fileName}";
//            }

//            await _db.SaveChangesAsync();
//            return Ok(item);
//        }

//        // DELETE: ลบสินค้า
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var item = await _db.MenuItems.FindAsync(id);
//            if (item == null) return NotFound();

//            _db.MenuItems.Remove(item);
//            await _db.SaveChangesAsync();
//            return Ok(new { message = "ลบสินค้าเรียบร้อยแล้ว" });
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models;
using My_FoodApp.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_FoodApp.Controllers
{
    // ==============================================
    // DTOs
    // ==============================================
    public class MenuItemFormDto
    {
        public int ShopId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Type { get; set; }
        public bool IsAvailable { get; set; }
    }

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
        public string? Type { get; set; }
        public bool IsAvailable { get; set; }
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
        private readonly IWebHostEnvironment _env;

        public MenuItemsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /api/MenuItems?shopId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItem>>> Get([FromQuery] int? shopId)
        {
            IQueryable<MenuItem> q = _db.MenuItems.AsNoTracking();
            if (shopId.HasValue)
            {
                q = q.Where(m => m.ShopId == shopId.Value);
            }
            var list = await q.OrderBy(m => m.Name).ToListAsync();
            return Ok(list);
        }

        // GET: /api/shops/1/menuitems
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

        // GET: /api/MenuItems/5/detail
        [HttpGet("{id:int}/detail")]
        public async Task<ActionResult<MenuItemDetailDto>> GetDetail(int id)
        {
            var item = await _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();

            var groups = await _db.MenuItemOptionGroups
                .AsNoTracking()
                .Where(g => g.MenuItemId == id)
                .OrderBy(g => g.SortOrder)
                .ToListAsync();

            var groupIds = groups.Select(g => g.Id).ToList();

            var options = await _db.MenuOptions
                .AsNoTracking()
                .Where(o => groupIds.Contains(o.GroupId))
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            var dto = new MenuItemDetailDto
            {
                Id = item.Id,
                ShopId = item.ShopId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.MainPhotoUrl,
                Type = item.Type,
                IsAvailable = item.IsAvailable,
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

        // POST: เพิ่มสินค้าใหม่
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MenuItemFormDto dto, IFormFile? file)
        {
            var item = new MenuItem
            {
                ShopId = dto.ShopId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Type = dto.Type,
                IsAvailable = dto.IsAvailable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 1. บันทึกครั้งแรกเพื่อให้ได้ item.Id ก่อน
            _db.MenuItems.Add(item);
            await _db.SaveChangesAsync();

            // 2. จัดการรูปภาพ (ถ้ามี)
            if (file != null && file.Length > 0)
            {
                // โฟลเดอร์: shop_uploads/menu/{shopId}
                var uploadFolder = Path.Combine(_env.ContentRootPath, "shop_uploads", "menu", dto.ShopId.ToString());
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                // 🟢 ตั้งชื่อไฟล์ใหม่: {shopId}_{itemId}.นามสกุลเดิม
                var fileName = $"{dto.ShopId}_{item.Id}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // อัปเดต Path ใน DB
                item.MainPhotoUrl = $"/shop_uploads/menu/{dto.ShopId}/{fileName}";

                // 3. บันทึกซ้ำอีกรอบเพื่อเก็บ URL
                await _db.SaveChangesAsync();
            }

            return Ok(item);
        }

        // PUT: แก้ไขสินค้า
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] MenuItemFormDto dto, IFormFile? file)
        {
            var item = await _db.MenuItems.FindAsync(id);
            if (item == null) return NotFound("ไม่พบสินค้านี้");

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Price = dto.Price;
            item.Type = dto.Type;
            item.IsAvailable = dto.IsAvailable;
            item.UpdatedAt = DateTime.UtcNow;

            if (file != null && file.Length > 0)
            {
                var uploadFolder = Path.Combine(_env.ContentRootPath, "shop_uploads", "menu", item.ShopId.ToString());
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                // 🟢 ตั้งชื่อไฟล์ใหม่: {shopId}_{itemId}.นามสกุลเดิม (ทับไฟล์เก่าถ้ามี)
                var fileName = $"{item.ShopId}_{item.Id}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                item.MainPhotoUrl = $"/shop_uploads/menu/{item.ShopId}/{fileName}";
            }

            await _db.SaveChangesAsync();
            return Ok(item);
        }

        // DELETE: ลบสินค้า
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            _db.MenuItems.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "ลบสินค้าเรียบร้อยแล้ว" });
        }
    }
}