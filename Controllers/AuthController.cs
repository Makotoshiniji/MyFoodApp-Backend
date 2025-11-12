using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models;
using My_FoodApp.Models.Dtos;

namespace My_FoodApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AuthController(AppDbContext db) => _db = db;

        // =========================
        // 🟩 Register (สมัครสมาชิก)
        // =========================
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("ข้อมูลไม่ครบ");

            var exists = await _db.Users.AnyAsync(u =>
                u.Email == dto.Email || u.Username == dto.Username);

            if (exists)
                return BadRequest("อีเมลหรือชื่อผู้ใช้นี้ถูกใช้แล้ว");

            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim(),
                Password = dto.Password,
                Rank = "user"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var res = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Rank = user.Rank
            };

            return CreatedAtAction(nameof(Register), res);
        }

        // =========================
        // 🟦 Login (เข้าสู่ระบบ)
        // =========================
        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginDto dto)
        {
            var identity = dto.Identity.Trim();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == identity || u.Username == identity);

            if (user == null || user.Password != dto.Password)
                return Unauthorized("อีเมล/ชื่อผู้ใช้ หรือรหัสผ่านไม่ถูกต้อง");

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Rank = user.Rank
            });
        }
    }
}
