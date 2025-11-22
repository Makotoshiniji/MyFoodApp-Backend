using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using My_FoodApp.Data;
using My_FoodApp.Services;
// ⭐️⭐️ 2 บรรทัดนี้คือตัวแก้ Error CS1061 ครับ ⭐️⭐️
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using QuestPDF.Infrastructure;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// ======================================
// 🔹 Database Connection
// ======================================
var conn = builder.Configuration.GetConnectionString("Default")
           ?? throw new InvalidOperationException("Connection string 'Default' not found.");

// ⭐️ (ดูจาก db_my_foodapp.sql คุณใช้ MariaDB 10.4.32)
var serverVersion = new MariaDbServerVersion(new Version(10, 4, 32));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(conn, serverVersion);
    options.UseSnakeCaseNamingConvention(); // ✅ ต้องอยู่ตรงนี้ (บรรทัดแยกออกมา)
    options.EnableSensitiveDataLogging();
});

// ======================================
// 🔹 CORS Policy (สำหรับ React Native)
// ======================================
const string corsPolicy = "_rnCors";
builder.Services.AddCors(o => o.AddPolicy(corsPolicy, p =>
    p.AllowAnyHeader()
     .AllowAnyMethod()
     .SetIsOriginAllowed(_ => true)
     .AllowCredentials()));

// ======================================
// 🔹 Controllers + JSON Fix (Ignore Cycles)
// ======================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ======================================
// 🔹 Swagger
// ======================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddTransient<EmailService>();

var app = builder.Build();

// ======================================
// 🔹 Swagger UI
// ======================================
app.UseSwagger();
app.UseSwaggerUI();

// ======================================
// 🔹 Static Files (shop_uploads)
// ======================================
var uploadsRoot = @"C:\My_FoodApp\My_FoodApp\shop_uploads";
Directory.CreateDirectory(uploadsRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/shop_uploads",
    ServeUnknownFileTypes = true,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=86400";
    }
});

// ======================================
// 🔹 HTTPS (เฉพาะ Production)
// ======================================
#if !DEBUG
app.UseHttpsRedirection();
#endif

// ======================================
// 🔹 Middleware Pipeline
// ======================================
app.UseCors(corsPolicy);
app.MapControllers();

// ======================================
// 🔹 Run Application
// ======================================
app.Run();