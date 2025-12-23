//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.FileProviders;
//using My_FoodApp.Data;
//using My_FoodApp.Services;
//// ⭐️⭐️ 2 บรรทัดนี้คือตัวแก้ Error CS1061 ครับ ⭐️⭐️
//using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
//using Pomelo.EntityFrameworkCore.MySql.Storage;
//using QuestPDF.Infrastructure;
//using System.Text.Json.Serialization;


//var builder = WebApplication.CreateBuilder(args);

//// ======================================
//// 🔹 Database Connection
//// ======================================
//var conn = builder.Configuration.GetConnectionString("Default")
//           ?? throw new InvalidOperationException("Connection string 'Default' not found.");

//// ⭐️ (ดูจาก db_my_foodapp.sql คุณใช้ MariaDB 10.4.32)
//var serverVersion = new MariaDbServerVersion(new Version(10, 4, 32));

//builder.Services.AddDbContext<AppDbContext>(options =>
//{
//    options.UseMySql(conn, serverVersion);
//    options.UseSnakeCaseNamingConvention(); // ✅ ต้องอยู่ตรงนี้ (บรรทัดแยกออกมา)
//    options.EnableSensitiveDataLogging();
//});

//// ======================================
//// 🔹 CORS Policy (สำหรับ React Native)
//// ======================================
//const string corsPolicy = "_rnCors";
//builder.Services.AddCors(o => o.AddPolicy(corsPolicy, p =>
//    p.AllowAnyHeader()
//     .AllowAnyMethod()
//     .SetIsOriginAllowed(_ => true)
//     .AllowCredentials()));

//// ======================================
//// 🔹 Controllers + JSON Fix (Ignore Cycles)
//// ======================================
//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//    });

//// ======================================
//// 🔹 Swagger
//// ======================================
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//QuestPDF.Settings.License = LicenseType.Community;
//builder.Services.AddTransient<EmailService>();

//var app = builder.Build();

//// ======================================
//// 🔹 Swagger UI
//// ======================================
//app.UseSwagger();
//app.UseSwaggerUI();

//// ======================================
//// 🔹 Static Files (shop_uploads)
//// ======================================
//var uploadsRoot = @"C:\My_FoodApp\My_FoodApp\shop_uploads";
//Directory.CreateDirectory(uploadsRoot);

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(uploadsRoot),
//    RequestPath = "/shop_uploads",
//    ServeUnknownFileTypes = true,
//    OnPrepareResponse = ctx =>
//    {
//        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=86400";
//    }
//});

//// ======================================
//// 🔹 HTTPS (เฉพาะ Production)
//// ======================================
//#if !DEBUG
//app.UseHttpsRedirection();
//#endif

//// ======================================
//// 🔹 Middleware Pipeline
//// ======================================
//app.UseCors(corsPolicy);
//app.MapControllers();

//// ======================================
//// 🔹 Run Application
//// ======================================
//app.Run();


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using My_FoodApp.Data;
using My_FoodApp.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using QuestPDF.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ======================================
// 🔹 1. Database Connection
// ======================================
var conn = builder.Configuration.GetConnectionString("Default")
           ?? throw new InvalidOperationException("Connection string 'Default' not found.");

// (MariaDB 10.4.32 ตามที่คุณระบุ)
var serverVersion = new MariaDbServerVersion(new Version(10, 4, 32));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(conn, serverVersion);
    options.UseSnakeCaseNamingConvention();
    options.EnableSensitiveDataLogging();
});

// ======================================
// 🔹 2. CORS Policy (สำหรับ React Native)
// ======================================
const string corsPolicy = "_rnCors";
builder.Services.AddCors(o => o.AddPolicy(corsPolicy, p =>
    p.AllowAnyHeader()
     .AllowAnyMethod()
     .SetIsOriginAllowed(_ => true)
     .AllowCredentials()));

// ======================================
// 🔹 3. Controllers + JSON Config
// ======================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ป้องกัน Error เรื่อง Loop Reference (A -> B -> A)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ======================================
// 🔹 4. Services & Swagger
// ======================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddTransient<EmailService>();

// ======================================
// 🚀 Build App
// ======================================
var app = builder.Build();

// ======================================
// 🔹 5. Swagger UI
// ======================================
app.UseSwagger();
app.UseSwaggerUI();

// ======================================
// 🔹 6. Static Files Configuration (สำคัญ!)
// ======================================

// 6.1 เปิดใช้งาน Static Files ปกติ (เผื่อใช้ wwwroot)
app.UseStaticFiles();

// 6.2 เปิดใช้งาน Custom Static Files สำหรับโฟลเดอร์ shop_uploads
// ใช้ ContentRootPath เพื่อให้ Path ตรงกับที่อยู่โปรเจกต์อัตโนมัติ (C:\My_FoodApp\My_FoodApp\)
var uploadsRoot = Path.Combine(builder.Environment.ContentRootPath, "shop_uploads");

// ตรวจสอบว่ามีโฟลเดอร์ไหม ถ้าไม่มีให้สร้าง
if (!Directory.Exists(uploadsRoot))
{
    Directory.CreateDirectory(uploadsRoot);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/shop_uploads", // URL จะเป็น http://host/shop_uploads/...
    ServeUnknownFileTypes = true,
    OnPrepareResponse = ctx =>
    {
        // เพิ่ม Cache เพื่อให้รูปโหลดเร็วขึ้นในการเรียกครั้งถัดไป
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=86400";
    }
});

// ======================================
// 🔹 7. HTTPS (เฉพาะ Production)
// ======================================
#if !DEBUG
app.UseHttpsRedirection();
#endif

// ======================================
// 🔹 8. Middleware Pipeline
// ======================================
// วาง UseCors ก่อน MapControllers
app.UseCors(corsPolicy);

app.MapControllers();

// ======================================
// 🔹 Run Application
// ======================================
app.Run();