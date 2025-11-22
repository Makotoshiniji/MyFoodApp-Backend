using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models;
using My_FoodApp.Dtos;

namespace My_FoodApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env; // ใช้หา path เซิร์ฟเวอร์

        public UsersController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: api/users/1 (ดึงข้อมูลโปรไฟล์)
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                UserProfilePath = user.UserProfilePath
            });
        }

        // PUT: api/users/1 (อัปเดตข้อมูล text)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Username = dto.Username;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.Bio = dto.Bio;

            await _db.SaveChangesAsync();
            return Ok(user); // ส่ง user ล่าสุดกลับไป
        }

        // POST: api/users/1/upload-profile (อัปโหลดรูป)
        [HttpPost("{id}/upload-profile")]
        public async Task<IActionResult> UploadProfile(int id, IFormFile file)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            // 1. สร้างโฟลเดอร์เก็บรูป (shop_uploads/users)
            var uploadFolder = Path.Combine(_env.ContentRootPath, "shop_uploads", "users");
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            // 2. ตั้งชื่อไฟล์ใหม่ (ป้องกันชื่อซ้ำ)
            var fileName = $"user_{id}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadFolder, fileName);

            // 3. บันทึกไฟล์
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. อัปเดต Path ใน Database
            // (path ที่ Frontend จะเรียกใช้ได้ต้องเริ่มด้วย /shop_uploads/...)
            var publicUrl = $"/shop_uploads/users/{fileName}";
            user.UserProfilePath = publicUrl;

            await _db.SaveChangesAsync();

            return Ok(new { path = publicUrl });
        }
    }
}