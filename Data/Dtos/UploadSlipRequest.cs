using Microsoft.AspNetCore.Http;

namespace My_FoodApp.Dtos
{
    public class UploadSlipRequest
    {
        public int? OrderId { get; set; }       // 👈 ทำให้ nullable
        public IFormFile? SlipFile { get; set; } // 👈 ทำให้ nullable
    }
}
