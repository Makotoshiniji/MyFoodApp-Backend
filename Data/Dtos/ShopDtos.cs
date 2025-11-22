namespace My_FoodApp.Dtos
{
    public class RegisterShopDto
    {
        public int OwnerUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}