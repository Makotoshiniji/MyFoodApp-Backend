namespace My_FoodApp.Models.Dtos
{
    public class LoginRequest
    {
        public string Identity { get; set; } = default!; // username หรือ email
        public string Password { get; set; } = default!;
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string? Email { get; set; }
        public string Rank { get; set; } = "user";
    }
}
