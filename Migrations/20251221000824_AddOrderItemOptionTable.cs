using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace My_FoodApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemOptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🟢 ลบส่วน RenameColumn ทิ้งทั้งหมด เพราะ Database จริงเปลี่ยนชื่อไปแล้ว
            // เราจะโฟกัสแค่การสร้างตารางใหม่เท่านั้น

            // สร้างตาราง order_item_option (เป้าหมายหลักของเรา)
            migrationBuilder.CreateTable(
                name: "order_item_option",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_item_id = table.Column<int>(type: "int", nullable: false),
                    option_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    extra_price = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item_option", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_item_option_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_option_order_item_id",
                table: "order_item_option",
                column: "order_item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_item_option");

            // ไม่ต้องใส่คำสั่งย้อนกลับการ Rename เพราะเราไม่ได้ Rename ตั้งแต่แรก
        }
    }
}