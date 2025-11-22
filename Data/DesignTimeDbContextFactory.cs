using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace My_FoodApp.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // 1. อ่าน Config จาก appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // 2. สร้างตัวสร้าง Options
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("Default");

            // 3. ตั้งค่า Database และ ✅ ใส่ Snake Case ตรงนี้ด้วย!
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                   .UseSnakeCaseNamingConvention(); // 👈 บรรทัดสำคัญที่ขาดหายไป!

            return new AppDbContext(builder.Options);
        }
    }
}