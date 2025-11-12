using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace My_FoodApp.Data   // <- ปรับให้ตรง
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var conn = cfg.GetConnectionString("Default");
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(conn, ServerVersion.AutoDetect(conn))
                .Options;

            return new AppDbContext(opts);
        }
    }
}
