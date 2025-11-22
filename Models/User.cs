using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace My_FoodApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }

        // ⭐️ กลับมาใช้ชื่อ PasswordHash
        public string PasswordHash { get; set; } = string.Empty;

        public string Rank { get; set; } = "user";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ⭐️ 2 บรรทัดนี้ถูกต้อง
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
        public string? UserProfilePath { get; set; }  // เก็บ path รูป
        public string? PhoneNumber { get; set; }      // เบอร์โทร
        public string? Bio { get; set; }              // คำอธิบายตัวเอง

    }
}