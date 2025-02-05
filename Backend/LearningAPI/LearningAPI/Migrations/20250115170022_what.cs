using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;


#nullable disable

namespace LearningAPI.Migrations
{
    /// <inheritdoc />
    public partial class what : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.CreateTable(
							name: "Payments",
							columns: table => new
							{
								PaymentId = table.Column<int>(type: "int", nullable: false)
									.Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
								OrderId = table.Column<int>(type: "int", nullable: false),
								UserId = table.Column<int>(type: "int", nullable: false),
								PaymentMethod = table.Column<string>(type: "longtext", nullable: false),
								CustomerName = table.Column<string>(type: "longtext", nullable: false),
								Cvc = table.Column<string>(type: "longtext", nullable: false),
								Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
								PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
								Status = table.Column<string>(type: "longtext", nullable: false)
							},
							constraints: table =>
							{
								table.PrimaryKey("PK_Payments", x => x.PaymentId);
								table.ForeignKey(
									name: "FK_Payments_Orders_OrderId",
									column: x => x.OrderId,
									principalTable: "Orders",
									principalColumn: "OrderId",
									onDelete: ReferentialAction.Cascade);
								table.ForeignKey(
									name: "FK_Payments_Users_Id", // New foreign key constraint
									column: x => x.UserId,
									principalTable: "Users",   // Assuming the Users table exists
									principalColumn: "Id",
									onDelete: ReferentialAction.Cascade);
							})
							.Annotation("MySQL:Charset", "utf8mb4");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.DropTable(
				name: "Payments");
		}
    }
}
