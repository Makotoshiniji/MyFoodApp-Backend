using Microsoft.EntityFrameworkCore;
using My_FoodApp.Models;

namespace My_FoodApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- ตารางหลัก ---
        public DbSet<User> Users { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<ShopMedia> ShopMedia { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<MenuItemIngredient> MenuItemIngredients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShopRating> ShopRatings { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        // --- ตาราง Custom Option ---
        public DbSet<MenuOptionGroup> MenuOptionGroups { get; set; }
        public DbSet<MenuItemOptionGroup> MenuItemOptionGroups { get; set; } // 🟢 แก้ชื่อ DbSet ให้สื่อความหมาย
        public DbSet<MenuOption> MenuOptions { get; set; }


        // --- ระบบตะกร้าสินค้า ---
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartItemOption> CartItemOptions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =======================
            // 1. Shop (เพิ่มการตั้งค่าความสัมพันธ์ที่นี่)
            // =======================
            modelBuilder.Entity<Shop>(e =>
            {
                e.ToTable("shops");

                // ✅ บอกชัดๆ ว่า Shop มี Media หลายอัน
                e.HasMany(s => s.Media)
                 .WithOne(m => m.Shop)      // Media มี Shop 1 อัน
                 .HasForeignKey(m => m.ShopId) // เชื่อมด้วย ShopId
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // =======================
            // 2. ShopMedia
            // =======================
            modelBuilder.Entity<ShopMedia>(e =>
            {
                e.ToTable("shop_media");
                e.Property(x => x.MediaType).HasColumnName("kind");
                e.Property(x => x.Url).HasColumnName("url").HasMaxLength(500);
                e.Property(x => x.SortOrder).HasColumnName("sort_order");
                e.Property(x => x.ShopId).HasColumnName("shop_id");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");

                // (ความสัมพันธ์ถูกกำหนดใน Shop แล้ว ไม่ต้องเขียนซ้ำที่นี่ก็ได้ หรือจะเขียนเพื่อความชัวร์ก็ได้)
            });

            // ... (ส่วนอื่นๆ เช่น User, Category, MenuItem ให้คงไว้เหมือนเดิม) ...

            // (ใส่โค้ดส่วนที่เหลือของ OnModelCreating ตามไฟล์เดิมของคุณต่อจากตรงนี้)
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Category>().ToTable("categories");

            // 2. MenuItemOptionGroup
            modelBuilder.Entity<MenuItemOptionGroup>(e =>
            {
                e.Property(x => x.MenuItemId).HasColumnName("menu_item_id");
                e.Property(x => x.GroupId).HasColumnName("group_id");
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
                e.Property(x => x.IsRequired).HasColumnName("is_required");
                e.Property(x => x.MinSelection).HasColumnName("min_selection");
                e.Property(x => x.MaxSelection).HasColumnName("max_selection");
                e.Property(x => x.SortOrder).HasColumnName("sort_order");
            });

            // 3. MenuOption (ตัวปัญหา ExtraPrice)
            modelBuilder.Entity<MenuOption>(e =>
            {
                e.Property(x => x.GroupId).HasColumnName("group_id");

                // Map ชื่อ Label -> name
                e.Property(x => x.Label).HasColumnName("name").HasMaxLength(100);

                // 🟢 บังคับชื่อ ExtraPrice -> extra_price
                e.Property(x => x.ExtraPrice)
                    .HasColumnName("extra_price")
                    .HasColumnType("decimal(10,2)");

                e.Property(x => x.IsDefault).HasColumnName("is_default");
                e.Property(x => x.SortOrder).HasColumnName("sort_order");
            });

            // 4. CartItemOption
            modelBuilder.Entity<CartItemOption>(e =>
            {
                e.Property(x => x.CartItemId).HasColumnName("cart_item_id");
                e.Property(x => x.OptionId).HasColumnName("option_id");
                e.Property(x => x.ExtraPrice)
                    .HasColumnName("extra_price")
                    .HasColumnType("decimal(10,2)");
            });

            // 5. Order
            modelBuilder.Entity<Order>(e =>
            {
                e.Property(x => x.ShopId).HasColumnName("shop_id");
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.OrderCode).HasColumnName("order_code");
                e.Property(x => x.Status).HasColumnName("status");
                e.Property(x => x.Subtotal).HasColumnName("subtotal");
                e.Property(x => x.DeliveryFee).HasColumnName("delivery_fee");
                e.Property(x => x.DiscountTotal).HasColumnName("discount_total");
                e.Property(x => x.GrandTotal).HasColumnName("grand_total");
                e.Property(x => x.Notes).HasColumnName("notes");
                e.Property(x => x.PlacedAt).HasColumnName("placed_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            });

            // 6. CartItem
            modelBuilder.Entity<CartItem>(e =>
            {
                e.Property(ci => ci.UnitPrice)
                    .HasColumnName("price")
                    .HasColumnType("decimal(10,2)");

                e.Property(ci => ci.CartId).HasColumnName("cart_id");
                e.Property(ci => ci.MenuItemId).HasColumnName("menu_item_id");
                e.Property(ci => ci.ShopId).HasColumnName("shop_id");
                e.Property(ci => ci.UserId).HasColumnName("user_id");
                e.Property(ci => ci.Qty).HasColumnName("qty");
            });
        }
    }
}