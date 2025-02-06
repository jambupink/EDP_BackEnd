using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAPI.Migrations
{
    /// <inheritdoc />
    public partial class cardno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardNo",
                table: "Payments",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardNo",
                table: "Payments");
        }
    }
}
