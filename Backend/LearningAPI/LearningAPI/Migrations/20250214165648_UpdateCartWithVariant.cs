using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartWithVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Carts");

			migrationBuilder.DropColumn(
                name: "Price", 
                table: "Carts");

			migrationBuilder.AddColumn<int>(
				  name: "VariantId",
				  table: "Carts",
				  nullable: false,
				  defaultValue: 0);

			migrationBuilder.CreateIndex(
                name: "IX_Carts_ProductId",
                table: "Carts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_VariantId",
                table: "Carts",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Products_ProductId",
                table: "Carts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Variants_VariantId",
                table: "Carts",
                column: "VariantId",
                principalTable: "Variants",
                principalColumn: "VariantId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Products_ProductId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Variants_VariantId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_ProductId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_VariantId",
                table: "Carts");

			migrationBuilder.DropColumn(
                name: "VariantId", 
                table: "Carts");

			migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Carts",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Carts",
                type: "longtext",
                nullable: false);
			migrationBuilder.AddColumn<decimal>(
	            name: "Price",
	            table: "Carts",
	            type: "int",
	            nullable: false,
	            defaultValue: 0);
		}
    }
}
