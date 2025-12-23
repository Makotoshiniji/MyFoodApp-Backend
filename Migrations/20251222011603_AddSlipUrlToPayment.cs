using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace My_FoodApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSlipUrlToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slip_url",
                table: "payments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "slip_url",
                table: "payments");
        }
    }
}
