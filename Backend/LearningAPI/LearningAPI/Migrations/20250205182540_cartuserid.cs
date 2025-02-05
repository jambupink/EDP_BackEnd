using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAPI.Migrations
{
    /// <inheritdoc />
    public partial class cartuserid : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Carts_Users_UserId",
				table: "Carts");

			migrationBuilder.DropIndex(
				name: "IX_Carts_UserId",
				table: "Carts");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
				name: "IX_Carts_UserId",
				table: "Carts",
				column: "UserId",
				unique: false);

			migrationBuilder.AddForeignKey(
				name: "FK_Carts_Users_UserId",
				table: "Carts",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}
	}
}
