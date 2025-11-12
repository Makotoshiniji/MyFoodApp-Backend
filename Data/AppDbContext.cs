// Data/AppDbContext.cs
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


        // --- ระบบตะกร้าสินค้า ---
        // ใช้ชื่อ Cart (เอกพจน์) ให้ตรงกับ CartController ที่เรียก _db.Cart
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartItemOption> CartItemOptions { get; set; }

        // --- custom option สำหรับเมนู ---
        public DbSet<MenuItemOptionGroup> MenuItemOptionGroups { get; set; }
        public DbSet<MenuOption> MenuOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =======================
            // TABLE NAME หลัก ๆ
            // =======================
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Shop>().ToTable("shops");
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<Ingredient>().ToTable("ingredients");
            modelBuilder.Entity<Inventory>().ToTable("inventory");
            modelBuilder.Entity<MenuItemIngredient>().ToTable("menu_item_ingredients");
            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<OrderItem>().ToTable("order_items");
            modelBuilder.Entity<Payment>().ToTable("payments");
            modelBuilder.Entity<ShopRating>().ToTable("shop_ratings");
            modelBuilder.Entity<Coupon>().ToTable("coupons");

            // =======================
            // MenuItem – ให้ใช้ Data Annotations เป็นหลัก
            // ชนิด column ใน db_my_foodapp.sql เป็น Id, ShopId, CategoryId, Name, Description ฯลฯ
            // ซึ่งตรงกับ property ใน Model อยู่แล้ว
            // =======================
            modelBuilder.Entity<MenuItem>(e =>
            {
                e.ToTable("menu_items");
                e.HasKey(x => x.Id);
                // ไม่ต้อง map ImageUrl (มี [NotMapped] + ไม่มี setter)
                // ปล่อยให้ EF map field ที่มีใน DB (MainPhotoUrl ฯลฯ) จาก DataAnnotations
            });

            // =======================
            // ShopMedia (snake_case)
            // =======================
            modelBuilder.Entity<ShopMedia>(e =>
            {
                e.ToTable("shop_media");

                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.ShopId).HasColumnName("shop_id");
                e.Property(x => x.Url).HasColumnName("url").HasMaxLength(500);
                e.Property(x => x.MediaType).HasColumnName("kind");
                e.Property(x => x.SortOrder).HasColumnName("sort_order");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");

                e.HasOne(x => x.Shop)
                    .WithMany(s => s.Media)
                    .HasForeignKey(x => x.ShopId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =======================
            // MenuItemOptionGroup
            // =======================
            modelBuilder.Entity<MenuItemOptionGroup>(e =>
            {
                e.ToTable("menu_item_option_groups");

                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("Id");
                e.Property(x => x.MenuItemId).HasColumnName("MenuItemId");
                e.Property(x => x.GroupId).HasColumnName("GroupId");
                e.Property(x => x.SortOrder).HasColumnName("SortOrder");
                e.Property(x => x.Name).HasColumnName("Name").HasMaxLength(200);
                e.Property(x => x.IsRequired).HasColumnName("IsRequired");
                e.Property(x => x.MinSelection).HasColumnName("MinSelection");
                e.Property(x => x.MaxSelection).HasColumnName("MaxSelection");

                e.HasOne(x => x.MenuItem)
                    .WithMany(m => m.OptionGroups)
                    .HasForeignKey(x => x.MenuItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =======================
            // MenuOption
            // =======================
            modelBuilder.Entity<MenuOption>(e =>
            {
                e.ToTable("menu_options");

                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("Id");
                e.Property(x => x.GroupId).HasColumnName("GroupId");

                // ใน Model ใช้ property ชื่อ Label ซึ่งมี [Column("Name")]
                e.Property(x => x.Label)
                    .HasColumnName("Name")
                    .HasMaxLength(100);

                e.Property(x => x.ExtraPrice)
                    .HasColumnName("ExtraPrice")
                    .HasColumnType("decimal(10,2)");

                e.Property(x => x.IsDefault).HasColumnName("IsDefault");
                e.Property(x => x.SortOrder).HasColumnName("SortOrder");

                e.HasOne(x => x.Group)
                    .WithMany(g => g.Options)
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =======================
            // Cart
            // =======================
            modelBuilder.Entity<Cart>(e =>
            {
                e.ToTable("cart");

                e.HasKey(c => c.Id);

                e.Property(c => c.Id).HasColumnName("id");
                e.Property(c => c.UserId).HasColumnName("user_id");
                e.Property(c => c.ShopId).HasColumnName("shop_id");
                e.Property(c => c.CreatedAt).HasColumnName("created_at");

                e.HasMany(c => c.Items)
                 .WithOne(i => i.Cart!)
                 .HasForeignKey(i => i.CartId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // =======================
            // CartItem
            // =======================
            modelBuilder.Entity<CartItem>(e =>
            {
                e.ToTable("cart_items");

                e.HasKey(ci => ci.Id);

                e.Property(ci => ci.Id).HasColumnName("id");
                e.Property(ci => ci.CartId).HasColumnName("cart_id");
                e.Property(ci => ci.MenuItemId).HasColumnName("menu_item_id");
                e.Property(ci => ci.Qty).HasColumnName("qty");
                e.Property(ci => ci.UnitPrice)
                    .HasColumnName("price")
                    .HasColumnType("decimal(10,2)");

                e.HasOne(ci => ci.Cart)
                  .WithMany(c => c.Items)
                  .HasForeignKey(ci => ci.CartId);

                e.HasOne(ci => ci.MenuItem)
                  .WithMany()
                  .HasForeignKey(ci => ci.MenuItemId);
            });

            // =======================
            // CartItemOption
            // =======================
            modelBuilder.Entity<CartItemOption>(e =>
            {
                e.ToTable("cart_item_options");

                e.HasKey(co => co.Id);

                e.Property(co => co.Id).HasColumnName("id");
                e.Property(co => co.CartItemId).HasColumnName("cart_item_id");
                e.Property(co => co.ExtraPrice)
                        .HasColumnName("extra_price")
                        .HasColumnType("decimal(10,2)");

                // ==== ลบอันเก่าที่เกี่ยวกับ OptionName (ถ้ามี) ====
                // e.Property(co => co.OptionName)... 

                // ==== และเพิ่มการ Map ที่ถูกต้องสำหรับ OptionId ====
                e.Property(co => co.OptionId)
                        .HasColumnName("option_id"); // <--- บอกให้มันรู้จักคอลัมน์นี้
            });
        }
    }
}
