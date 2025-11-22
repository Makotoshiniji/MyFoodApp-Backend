namespace My_FoodApp.Dtos
{
    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
    }

    // DTO สำหรับส่งข้อมูล User กลับไปให้ Frontend (รวมรูปด้วย)
    public class UserDetailDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public string? UserProfilePath { get; set; }
    }
}