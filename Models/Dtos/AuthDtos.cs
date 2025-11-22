//AuthDtos.cs
namespace My_FoodApp.Models.Dtos
{
    public class RegisterDto
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class LoginDto
    {
        public string Identity { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string? Email { get; set; }
        public string Rank { get; set; } = "user";
    }

    // 1. สำหรับหน้ากรอกอีเมลขอ OTP
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    // 2. สำหรับหน้ากรอกรหัส OTP
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    // 3. สำหรับหน้าตั้งรหัสผ่านใหม่
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
