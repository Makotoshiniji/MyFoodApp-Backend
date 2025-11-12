// Models/User.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace My_FoodApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }

        // เก็บรหัสเป็น plain text ชั่วคราวในคอลัมน์ PasswordHash เดิม
        [Column("PasswordHash")]
        public string Password { get; set; } = string.Empty;

        public string Rank { get; set; } = "user";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
