using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using My_FoodApp.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ======================================
// 🔹 Database Connection
// ======================================
var conn = builder.Configuration.GetConnectionString("Default")
           ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

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
        // ✅ แก้ปัญหา Serialization Loop เช่น Cart.Items.Cart.Items...
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // (optional) จัดรูปแบบ JSON ให้สวยเวลา debug
        // options.JsonSerializerOptions.WriteIndented = true;
    });

// ======================================
// 🔹 Swagger
// ======================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
