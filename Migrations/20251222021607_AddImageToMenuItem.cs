using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace My_FoodApp.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "menu_items",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "image_path",
                table: "menu_items",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "image_path",
                table: "menu_items");
        }
    }
}
