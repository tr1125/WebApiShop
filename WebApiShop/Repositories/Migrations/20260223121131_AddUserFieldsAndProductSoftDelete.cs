using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFieldsAndProductSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "is_admin");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "Products",
                newName: "color");

            migrationBuilder.RenameIndex(
                name: "IX_Products_category_id",
                table: "Products",
                newName: "IX_Products_color");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Orders",
                newName: "status");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_user_id",
                table: "Orders",
                newName: "IX_Orders_status");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "order_item_id",
                table: "Order_items",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateTable(
                name: "RATING",
                columns: table => new
                {
                    RATING_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HOST = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    METHOD = table.Column<string>(type: "nchar(10)", fixedLength: true, maxLength: 10, nullable: false),
                    PATH = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    REFERER = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    USER_AGENT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Record_Date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RATING", x => x.RATING_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RATING");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "is_admin",
                table: "Users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "color",
                table: "Products",
                newName: "category_id");

            migrationBuilder.RenameIndex(
                name: "IX_Products_color",
                table: "Products",
                newName: "IX_Products_category_id");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Orders",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_status",
                table: "Orders",
                newName: "IX_Orders_user_id");

            migrationBuilder.AlterColumn<int>(
                name: "order_item_id",
                table: "Order_items",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
