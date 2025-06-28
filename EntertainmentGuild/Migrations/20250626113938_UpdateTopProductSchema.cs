using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntertainmentGuild.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTopProductSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopProducts_Products_ProductId",
                table: "TopProducts");

            migrationBuilder.DropIndex(
                name: "IX_TopProducts_ProductId",
                table: "TopProducts");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "TopProducts");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "TopProducts");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "TopProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TopProducts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "TopProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TopProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "TopProducts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "TopProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "TopProducts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TopProducts");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "TopProducts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TopProducts");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "TopProducts");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "TopProducts");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "TopProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "TopProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TopProducts_ProductId",
                table: "TopProducts",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_TopProducts_Products_ProductId",
                table: "TopProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
