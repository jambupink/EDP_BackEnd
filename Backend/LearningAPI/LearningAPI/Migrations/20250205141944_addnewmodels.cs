using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;


#nullable disable

namespace LearningAPI.Migrations
{
    /// <inheritdoc />
    public partial class addnewmodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Products",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");
		}
        



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

			migrationBuilder.DropTable(
				name: "Carts");

			migrationBuilder.DropTable(
				name: "OrderItems");


			migrationBuilder.DropTable(
				name: "Orders");

        }
    }
}
