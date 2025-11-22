using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_FoodApp.Data;
using My_FoodApp.Models;
using My_FoodApp.Models.Dtos; // หรือ My_FoodApp.Dtos
using My_FoodApp.Services;   // ✅ ต้องมี

namespace My_FoodApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService; // ✅ ประกาศตัวแปร

        // ✅ Constructor: ต้องรับค่ามาแล้ว "เก็บเข้าตัวแปร" ด้วย
        public AuthController(AppDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService; // 👈 บรรทัดนี้สำคัญมาก!
        }

        // =========================
        // 🟩 Register
        // =========================
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("ข้อมูลไม่ครบ");

            var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username);
            if (exists) return BadRequest("อีเมลหรือชื่อผู้ใช้นี้ถูกใช้แล้ว");

            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = dto.Password, // Plain text
                Rank = "user",
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new UserResponseDto { Id = user.Id, Username = user.Username, Email = user.Email, Rank = user.Rank });
        }

        // =========================
        // 🟦 Login
        // =========================
        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginDto dto)
        {
            var identity = dto.Identity.Trim();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == identity || u.Username == identity);

            if (user == null || user.PasswordHash != dto.Password)
                return Unauthorized("อีเมล/ชื่อผู้ใช้ หรือรหัสผ่านไม่ถูกต้อง");

            return Ok(new UserResponseDto { Id = user.Id, Username = user.Username, Email = user.Email, Rank = user.Rank });
        }

        // =========================
        // 🟧 1. Forgot Password (ส่งเมลจริง)
        // =========================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return Ok("หากอีเมลนี้ถูกต้อง เราได้ส่ง OTP ให้คุณแล้ว");

            // สร้าง OTP
            var otp = new Random().Next(100000, 999999).ToString();
            user.PasswordResetToken = otp;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync();

            // ✅ ส่งอีเมลจริงผ่าน Gmail
            try
            {
                var subject = "รหัส OTP เปลี่ยนรหัสผ่าน - MyFoodApp";
                var body = $@"
                    <h3>รหัส OTP ของคุณคือ: <span style='color:red; font-size: 20px;'>{otp}</span></h3>
                    <p>รหัสนี้จะหมดอายุใน 10 นาที</p>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
                Console.WriteLine($"✅ Email sent to {user.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email Error: {ex.Message}");
                return StatusCode(500, "ส่งอีเมลไม่สำเร็จ: " + ex.Message);
            }

            return Ok("ส่งรหัส OTP เรียบร้อยแล้ว");
        }

        // =========================
        // 🟧 2. Verify OTP
        // =========================
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.PasswordResetToken != request.Otp) return BadRequest("รหัส OTP ไม่ถูกต้อง");
            if (user.PasswordResetTokenExpires < DateTime.UtcNow) return BadRequest("รหัส OTP หมดอายุ");

            return Ok("OTP ถูกต้อง");
        }

        // =========================
        // 🟧 3. Reset Password
        // =========================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.PasswordResetToken != request.Otp || user.PasswordResetTokenExpires < DateTime.UtcNow)
                return BadRequest("คำขอไม่ถูกต้องหรือหมดอายุ");

            user.PasswordHash = request.NewPassword; // บันทึกรหัสใหม่
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;
            await _db.SaveChangesAsync();

            return Ok("เปลี่ยนรหัสผ่านสำเร็จ");
        }
    }
}