//// Controllers/CartController.cs

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using My_FoodApp.Data;
//using My_FoodApp.Dtos;
//using My_FoodApp.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace My_FoodApp.Controllers
//{
//    // ---------- DTOs ----------
//    public class CartItemDetailDto
//    {
//        public int Id { get; set; }
//        public int MenuItemId { get; set; }
//        public string MenuItemName { get; set; } = "";
//        public int Quantity { get; set; }
//        public decimal Price { get; set; }         // ราคาต่อชิ้น (รวม Extra)
//        public string? ImageUrl { get; set; }

//        public int ShopId { get; set; }            // ✅ เพิ่ม property ใหม่
//        public string ShopName { get; set; } = ""; // ✅ เหลือ ShopName แค่ตัวเดียว

//        public List<SelectedOptionDto> Options { get; set; } = new();
//    }

//    public class SelectedOptionDto
//    {
//        public int OptionId { get; set; }
//        public string OptionName { get; set; } = "";
//        public decimal ExtraPrice { get; set; }
//    }


//    public class AddToCartDto
//    {
//        public int MenuItemId { get; set; }
//        public int Qty { get; set; } = 1;
//        public List<CartOptionDto> Options { get; set; } = new();
//    }

//    public class CartOptionDto
//    {
//        public string OptionName { get; set; } = string.Empty;
//        public decimal ExtraPrice { get; set; }
//    }

//    public class CartShopSummaryDto
//    {
//        public int CartId { get; set; }
//        public int ShopId { get; set; }
//        public string ShopName { get; set; } = string.Empty;
//        public int ItemCount { get; set; }
//        public decimal Total { get; set; }
//    }

//    // ---------- CONTROLLER ----------
//    [ApiController]
//    [Route("api/[controller]")]
//    public class CartController : ControllerBase
//    {
//        private readonly AppDbContext _db;
//        public CartController(AppDbContext db) => _db = db;

//        // ---------------- GET CART (โครงตะกร้าทั้งใบ) ----------------
//        // GET /api/Cart/{userId}
//        [HttpGet("{userId:int}")]
//        public async Task<ActionResult<Cart>> GetCart(int userId)
//        {
//            var cart = await _db.Cart
//                .Include(c => c.Items)
//                    .ThenInclude(i => i.MenuItem)
//                .Include(c => c.Items)
//                    .ThenInclude(i => i.Options)
//                .FirstOrDefaultAsync(c => c.UserId == userId);

//            if (cart == null)
//            {
//                cart = new Cart { UserId = userId };
//            }
//            return Ok(cart);
//        }

//        // ---------------- AddItem (เพิ่มของลงตะกร้า) ----------------
//        // POST /api/Cart/{userId}/items
//        [HttpPost("{userId:int}/items")]
//        public async Task<ActionResult> AddItem(int userId, [FromBody] AddToCartDto dto)
//        {
//            var menu = await _db.MenuItems.FindAsync(dto.MenuItemId);
//            if (menu == null) return NotFound("Menu item not found.");

//            var cart = await _db.Cart
//                .Include(c => c.Items)
//                    .ThenInclude(i => i.Options)
//                .FirstOrDefaultAsync(c => c.UserId == userId && c.ShopId == menu.ShopId);

//            if (cart == null)
//            {
//                cart = new Cart
//                {
//                    UserId = userId,
//                    ShopId = menu.ShopId, // ผูกกับร้านของเมนูนั้น
//                    CreatedAt = DateTime.UtcNow
//                };
//                _db.Cart.Add(cart);
//                await _db.SaveChangesAsync(); // Save ก่อนเพื่อให้ได้ cart.Id
//            }

//            var allMenuOptions = await _db.MenuOptions
//                .Include(mo => mo.Group)
//                .Where(mo => mo.Group != null && mo.Group.MenuItemId == dto.MenuItemId)
//                .ToListAsync();

//            var item = new CartItem
//            {
//                Cart = cart,
//                MenuItemId = menu.Id,
//                Qty = dto.Qty,
//                UnitPrice = menu.Price,
//            };

//            foreach (var opDto in dto.Options)
//            {
//                var matchingMenuOption = allMenuOptions
//                    .FirstOrDefault(m => m.Label == opDto.OptionName && m.ExtraPrice == opDto.ExtraPrice);

//                if (matchingMenuOption == null)
//                {
//                    return BadRequest($"Invalid option specified: {opDto.OptionName}");
//                }

//                item.Options.Add(new CartItemOption
//                {
//                    OptionId = matchingMenuOption.Id,
//                    ExtraPrice = matchingMenuOption.ExtraPrice
//                });
//            }

//            cart.Items.Add(item);
//            await _db.SaveChangesAsync();
//            return Ok();
//        }

//        // ---------------- SUMMARY CART (ตามร้าน) ----------------
//        // GET /api/Cart/user/{userId}
//        [HttpGet("user/{userId:int}")]
//        public async Task<ActionResult<IEnumerable<CartShopSummaryDto>>> GetUserCarts(int userId)
//        {

//            //return Ok(result);
//            var carts = await _db.Cart
//                .Where(c => c.UserId == userId)
//                .Include(c => c.Items).ThenInclude(i => i.Options)
//                .Include(c => c.Items).ThenInclude(i => i.MenuItem)
//                .ToListAsync();

//            // ดึงชื่อร้านมาแมพใส่
//            var shopIds = carts.Select(c => c.ShopId).Distinct().ToList();
//            var shops = await _db.Shops.Where(s => shopIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);

//            var result = carts.Select(c => new CartShopSummaryDto
//            {
//                CartId = c.Id,
//                ShopId = c.ShopId ?? 0,
//                // ใส่ชื่อร้าน (ถ้าหาไม่เจอให้ใส่ Unknown)
//                ShopName = (c.ShopId.HasValue && shops.ContainsKey(c.ShopId.Value)) ? shops[c.ShopId.Value] : "Unknown Shop",
//                ItemCount = c.Items.Sum(i => i.Qty),
//                Total = c.Items.Sum(i => i.Qty * (i.UnitPrice + i.Options.Sum(o => o.ExtraPrice)))
//            });

//            return Ok(result);
//        }

//        // ---------------- รายการสินค้าทั้งหมดในตะกร้า (ใช้ใน CartScreen) ----------------
//        // GET /api/Cart/{userId}/items
//        [HttpGet("{userId:int}/items")]
//        public async Task<ActionResult<IEnumerable<CartItemDetailDto>>> GetUserCartItems(int userId)
//        {
//            var carts = await _db.Cart
//                .Where(c => c.UserId == userId)
//                .Include(c => c.Items)
//                    .ThenInclude(i => i.Options)
//                .Include(c => c.Items)
//                    .ThenInclude(i => i.MenuItem)
//                .ToListAsync();

//            // map shopId -> shopName
//            var shopIds = carts.Select(c => c.ShopId).Distinct().ToList();
//            var shops = await _db.Shops
//                .Where(s => shopIds.Contains(s.Id))
//                .ToDictionaryAsync(s => s.Id, s => s.Name);

//            var items = carts
//                .SelectMany(c => c.Items.Select(i => new CartItemDetailDto
//                {
//                    Id = i.Id,
//                    MenuItemId = i.MenuItemId,
//                    MenuItemName = i.MenuItem.Name,
//                    Quantity = i.Qty,
//                    Price = i.UnitPrice + i.Options.Sum(o => o.ExtraPrice),
//                    ImageUrl = i.MenuItem.ImageUrl,
//                    ShopId = c.ShopId ?? 0,
//                    ShopName = c.ShopId.HasValue && shops.ContainsKey(c.ShopId.Value)
//                        ? shops[c.ShopId.Value]
//                        : "Unknown shop",
//                Options = i.Options.Select(o => new SelectedOptionDto
//                {
//                    OptionId = o.OptionId,
//                    // ถ้าใน Entity CartItemOption มี Option ให้ดึงชื่อมา ถ้าไม่มีให้ว่างไว้
//                    OptionName = o.Option?.Label ?? "Option",
//                    ExtraPrice = o.ExtraPrice
//                }).ToList()
//                }))
//                .ToList();

//            return Ok(items);
//        }

//        // ---------------- CHECKOUT (ล้างตะกร้า) ----------------
//        // POST /api/Cart/checkout/{userId}
//        [HttpPost("checkout/{userId:int}")]
//        public async Task<ActionResult> Checkout(int userId)
//        {
//            var cart = await _db.Cart
//                .Include(c => c.Items)
//                    .ThenInclude(i => i.Options)
//                .FirstOrDefaultAsync(c => c.UserId == userId);

//            if (cart == null) return BadRequest("Cart is empty.");

//            _db.CartItemOptions.RemoveRange(cart.Items.SelectMany(i => i.Options));
//            _db.CartItems.RemoveRange(cart.Items);
//            await _db.SaveChangesAsync();
//            return Ok();
//        }

//        public class UpdateCartItemQtyDto
//        {
//            public int Qty { get; set; }
//        }

//        // PUT /api/Cart/items/{cartItemId}/qty
//        [HttpPut("items/{cartItemId:int}/qty")]
//        public async Task<ActionResult> UpdateItemQty(int cartItemId, [FromBody] UpdateCartItemQtyDto dto)
//        {
//            var item = await _db.CartItems
//                .FirstOrDefaultAsync(i => i.Id == cartItemId);

//            if (item == null)
//                return NotFound("Cart item not found.");

//            if (dto.Qty <= 0)
//                return BadRequest("Quantity must be greater than zero.");

//            item.Qty = dto.Qty;

//            await _db.SaveChangesAsync();
//            return Ok();
//        }

//        // DELETE /api/Cart/items/{cartItemId}
//        [HttpDelete("items/{cartItemId:int}")]
//        public async Task<ActionResult> RemoveItem(int cartItemId)
//        {
//            var item = await _db.CartItems
//                .Include(i => i.Options)
//                .FirstOrDefaultAsync(i => i.Id == cartItemId);

//            if (item == null)
//                return NotFound("Cart item not found.");

//            // เก็บ cartId ไว้ก่อนลบ
//            var cartId = item.CartId;   // ✅ ต้องมี CartId ใน CartItem model

//            // ลบ option + item
//            _db.CartItemOptions.RemoveRange(item.Options);
//            _db.CartItems.Remove(item);
//            await _db.SaveChangesAsync();

//            // เช็กว่าตะกร้าใบนั้นยังเหลือ item ไหม
//            var cart = await _db.Cart
//                .Include(c => c.Items)
//                .FirstOrDefaultAsync(c => c.Id == cartId);

//            if (cart != null && !cart.Items.Any())
//            {
//                // ถ้าไม่มี item แล้ว → ลบ row Cart ทิ้งด้วย
//                _db.Cart.Remove(cart);
//                await _db.SaveChangesAsync();
//            }

//            return Ok();
//        }

//        [HttpPut("update-item/{cartItemId}")]
//        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
//        {
//            using var transaction = await _db.Database.BeginTransactionAsync();
//            try
//            {
//                // 1. ดึง CartItem มาแก้ไข
//                var cartItem = await _db.CartItems.FindAsync(cartItemId);
//                if (cartItem == null) return NotFound("ไม่พบสินค้าในตะกร้า");

//                // อัปเดตข้อมูลพื้นฐาน
//                cartItem.Qty = request.Quantity;
//                cartItem.SpecialRequest = request.SpecialRequest;

//                // 2. จัดการ Options
//                if (request.OptionIds != null)
//                {
//                    // 2.1 ลบ Option เก่าทิ้งทั้งหมด
//                    var oldOptions = await _db.CartItemOptions
//                                              .Where(o => o.CartItemId == cartItemId)
//                                              .ToListAsync();
//                    _db.CartItemOptions.RemoveRange(oldOptions);
//                    await _db.SaveChangesAsync();

//                    // 2.2 สร้าง Option ใหม่ (✅ แก้ไข: ดึงราคาจริงจาก DB มาใส่)
//                    if (request.OptionIds.Any())
//                    {
//                        // ดึงข้อมูล Option จาก Master Data เพื่อเอา ExtraPrice
//                        var validOptions = await _db.MenuOptions
//                            .Where(mo => request.OptionIds.Contains(mo.Id))
//                            .ToListAsync();

//                        var newOptions = validOptions.Select(mo => new CartItemOption
//                        {
//                            CartItemId = cartItemId,
//                            OptionId = mo.Id,
//                            ExtraPrice = mo.ExtraPrice // ✅ ต้องใส่ราคานี้ ไม่งั้นยอดเงินจะหาย
//                        }).ToList();

//                        await _db.CartItemOptions.AddRangeAsync(newOptions);
//                    }
//                }

//                // 3. บันทึกทุกอย่าง
//                await _db.SaveChangesAsync();
//                await transaction.CommitAsync();

//                return Ok(new { message = "แก้ไขรายการเรียบร้อยแล้ว ✅" });
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                return StatusCode(500, $"เกิดข้อผิดพลาด: {ex.Message}");
//            }
//        }


//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Dtos;
using My_FoodApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_FoodApp.Controllers
{
    // ---------- DTOs ----------
    public class CartItemDetailDto
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }         // ราคาต่อชิ้น (รวม Extra)
        public string? ImageUrl { get; set; }

        public int ShopId { get; set; }
        public string ShopName { get; set; } = "";
        public string? SpecialRequest { get; set; }

        public List<SelectedOptionDto> Options { get; set; } = new();
    }

    public class SelectedOptionDto
    {
        public int OptionId { get; set; }
        public string OptionName { get; set; } = "";
        public decimal ExtraPrice { get; set; }
    }

    public class AddToCartDto
    {
        public int MenuItemId { get; set; }
        public int Qty { get; set; } = 1;
        public List<CartOptionDto> Options { get; set; } = new();
        public string? SpecialRequest { get; set; }
    }

    public class CartOptionDto
    {
        public string OptionName { get; set; } = string.Empty;
        public decimal ExtraPrice { get; set; }
    }

    public class CartShopSummaryDto
    {
        public int CartId { get; set; }
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
    }

    // ---------- CONTROLLER ----------
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CartController(AppDbContext db) => _db = db;

        // ---------------- GET CART (โครงตะกร้าทั้งใบ) ----------------
        [HttpGet("{userId:int}")]
        public async Task<ActionResult<Cart>> GetCart(int userId)
        {
            var cart = await _db.Cart
                .Include(c => c.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Options)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
            }
            return Ok(cart);
        }

        // ---------------- AddItem (เพิ่มของลงตะกร้า) ----------------
        [HttpPost("{userId:int}/items")]
        public async Task<ActionResult> AddItem(int userId, [FromBody] AddToCartDto dto)
        {
            var menu = await _db.MenuItems.FindAsync(dto.MenuItemId);
            if (menu == null) return NotFound("Menu item not found.");

            var cart = await _db.Cart
                .Include(c => c.Items)
                    .ThenInclude(i => i.Options)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ShopId == menu.ShopId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    ShopId = menu.ShopId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Cart.Add(cart);
                await _db.SaveChangesAsync();
            }

            var allMenuOptions = await _db.MenuOptions
                .Include(mo => mo.Group)
                .Where(mo => mo.Group != null && mo.Group.MenuItemId == dto.MenuItemId)
                .ToListAsync();

            var item = new CartItem
            {
                Cart = cart,
                MenuItemId = menu.Id,
                Qty = dto.Qty,
                UnitPrice = menu.Price,
                SpecialRequest = dto.SpecialRequest
            };

            foreach (var opDto in dto.Options)
            {
                var matchingMenuOption = allMenuOptions
                    .FirstOrDefault(m => m.Label == opDto.OptionName && m.ExtraPrice == opDto.ExtraPrice);

                if (matchingMenuOption == null)
                {
                    return BadRequest($"Invalid option specified: {opDto.OptionName}");
                }

                item.Options.Add(new CartItemOption
                {
                    OptionId = matchingMenuOption.Id,
                    ExtraPrice = matchingMenuOption.ExtraPrice
                });
            }

            cart.Items.Add(item);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ---------------- SUMMARY CART (ตามร้าน) ----------------
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<CartShopSummaryDto>>> GetUserCarts(int userId)
        {
            var carts = await _db.Cart
                .Where(c => c.UserId == userId)
                .Include(c => c.Items).ThenInclude(i => i.Options)
                .Include(c => c.Items).ThenInclude(i => i.MenuItem)
                .ToListAsync();

            var shopIds = carts.Select(c => c.ShopId).Distinct().ToList();
            var shops = await _db.Shops.Where(s => shopIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);

            var result = carts.Select(c => new CartShopSummaryDto
            {
                CartId = c.Id,
                ShopId = c.ShopId ?? 0,
                ShopName = (c.ShopId.HasValue && shops.ContainsKey(c.ShopId.Value)) ? shops[c.ShopId.Value] : "Unknown Shop",
                ItemCount = c.Items.Sum(i => i.Qty),
                Total = c.Items.Sum(i => i.Qty * (i.UnitPrice + i.Options.Sum(o => o.ExtraPrice)))
            });

            return Ok(result);
        }

        // ---------------- รายการสินค้าทั้งหมดในตะกร้า (แก้แล้ว) ----------------
        [HttpGet("{userId:int}/items")]
        public async Task<ActionResult<IEnumerable<CartItemDetailDto>>> GetUserCartItems(int userId)
        {
            var carts = await _db.Cart
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Options) // ❌ ตัด .ThenInclude(o => o.Option) ออก เพราะ Model ไม่มี
                .Include(c => c.Items)
                    .ThenInclude(i => i.MenuItem)
                .ToListAsync();

            // 1. ดึงชื่อร้าน
            var shopIds = carts.Select(c => c.ShopId).Distinct().ToList();
            var shops = await _db.Shops
                .Where(s => shopIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            // 2. ดึงชื่อตัวเลือก (Options) มาเตรียมไว้ (Manual Join)
            var allOptionIds = carts.SelectMany(c => c.Items).SelectMany(i => i.Options).Select(o => o.OptionId).Distinct().ToList();
            var optionNames = await _db.MenuOptions
                .Where(mo => allOptionIds.Contains(mo.Id))
                .ToDictionaryAsync(mo => mo.Id, mo => mo.Label); // สมมติว่าในตาราง MenuOptions ชื่อฟิลด์คือ Label (ถ้าไม่ใช่ให้แก้เป็น Name)

            var items = carts
                .SelectMany(c => c.Items.Select(i => new CartItemDetailDto
                {
                    Id = i.Id,
                    MenuItemId = i.MenuItemId,
                    MenuItemName = i.MenuItem.Name,
                    Quantity = i.Qty,
                    Price = i.UnitPrice + i.Options.Sum(o => o.ExtraPrice),
                    ImageUrl = i.MenuItem.ImageUrl,
                    ShopId = c.ShopId ?? 0,

                    // ✅ ใส่ Comma ตรงนี้ที่เคยขาดไป
                    ShopName = c.ShopId.HasValue && shops.ContainsKey(c.ShopId.Value)
                        ? shops[c.ShopId.Value]
                        : "Unknown shop",
                    SpecialRequest = i.SpecialRequest,

                    // ✅ Map ข้อมูล Options โดยดึงชื่อจาก Dictionary ที่เตรียมไว้
                    Options = i.Options.Select(o => new SelectedOptionDto
                    {
                        OptionId = o.OptionId,
                        OptionName = optionNames.ContainsKey(o.OptionId) ? optionNames[o.OptionId] : "Unknown Option",
                        ExtraPrice = o.ExtraPrice
                    }).ToList()
                }))
                .ToList();

            return Ok(items);
        }

        // ---------------- CHECKOUT (ล้างตะกร้า) ----------------
        [HttpPost("checkout/{userId:int}")]
        public async Task<ActionResult> Checkout(int userId)
        {
            var cart = await _db.Cart
                .Include(c => c.Items)
                    .ThenInclude(i => i.Options)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return BadRequest("Cart is empty.");

            _db.CartItemOptions.RemoveRange(cart.Items.SelectMany(i => i.Options));
            _db.CartItems.RemoveRange(cart.Items);
            await _db.SaveChangesAsync();
            return Ok();
        }

        public class UpdateCartItemQtyDto
        {
            public int Qty { get; set; }
        }

        // PUT /api/Cart/items/{cartItemId}/qty
        [HttpPut("items/{cartItemId:int}/qty")]
        public async Task<ActionResult> UpdateItemQty(int cartItemId, [FromBody] UpdateCartItemQtyDto dto)
        {
            var item = await _db.CartItems
                .FirstOrDefaultAsync(i => i.Id == cartItemId);

            if (item == null) return NotFound("Cart item not found.");
            if (dto.Qty <= 0) return BadRequest("Quantity must be greater than zero.");

            item.Qty = dto.Qty;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // DELETE /api/Cart/items/{cartItemId}
        [HttpDelete("items/{cartItemId:int}")]
        public async Task<ActionResult> RemoveItem(int cartItemId)
        {
            var item = await _db.CartItems
                .Include(i => i.Options)
                .FirstOrDefaultAsync(i => i.Id == cartItemId);

            if (item == null) return NotFound("Cart item not found.");

            var cartId = item.CartId;

            _db.CartItemOptions.RemoveRange(item.Options);
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();

            var cart = await _db.Cart
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart != null && !cart.Items.Any())
            {
                _db.Cart.Remove(cart);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        // ✅ ฟังก์ชัน UpdateCartItem ที่แก้ไขแล้ว (มีดึงราคา Option)
        [HttpPut("update-item/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var cartItem = await _db.CartItems.FindAsync(cartItemId);
                if (cartItem == null) return NotFound("ไม่พบสินค้าในตะกร้า");

                cartItem.Qty = request.Quantity;
                cartItem.SpecialRequest = request.SpecialRequest;

                if (request.OptionIds != null)
                {
                    var oldOptions = await _db.CartItemOptions
                                              .Where(o => o.CartItemId == cartItemId)
                                              .ToListAsync();
                    _db.CartItemOptions.RemoveRange(oldOptions);
                    await _db.SaveChangesAsync();

                    if (request.OptionIds.Any())
                    {
                        var validOptions = await _db.MenuOptions
                            .Where(mo => request.OptionIds.Contains(mo.Id))
                            .ToListAsync();

                        var newOptions = validOptions.Select(mo => new CartItemOption
                        {
                            CartItemId = cartItemId,
                            OptionId = mo.Id,
                            ExtraPrice = mo.ExtraPrice
                        }).ToList();

                        await _db.CartItemOptions.AddRangeAsync(newOptions);
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "แก้ไขรายการเรียบร้อยแล้ว ✅" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"เกิดข้อผิดพลาด: {ex.Message}");
            }
        }
    }
}