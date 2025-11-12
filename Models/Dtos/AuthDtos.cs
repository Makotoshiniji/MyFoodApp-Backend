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
}
