using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAPI.Migrations
{
    /// <inheritdoc />
    public partial class models3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Carts");
        }
    }
}
