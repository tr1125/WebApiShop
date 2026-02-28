using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddProductFieldsAttempt2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_products",
                table: "Products");

// removed DropIndex

// removed DropColumn IsAdmin

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Products");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

// removed AddColumn image_url

            migrationBuilder.AlterColumn<bool>(
                name: "is_admin",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

// removed AddColumn password

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

// removed AddColumn category_id

            migrationBuilder.CreateIndex(
                name: "IX_Products_category_id",
                table: "Products",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_products",
                table: "Products",
                column: "category_id",
                principalTable: "Categories",
                principalColumn: "cetegory_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_products",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_category_id",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "password",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "Products",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "Products",
                newName: "ImageURL");

            migrationBuilder.AlterColumn<string>(
                name: "is_admin",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "color",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_color",
                table: "Products",
                column: "color");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_products",
                table: "Products",
                column: "color",
                principalTable: "Categories",
                principalColumn: "cetegory_id");
        }
    }
}
