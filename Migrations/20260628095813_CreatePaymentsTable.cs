using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalikongkongNHS.Migrations
{
    /// <inheritdoc />
    public partial class CreatePaymentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cashier",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeeType",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNo",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Students_StudentId",
                table: "Payments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Students_StudentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StudentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Cashier",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FeeType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReceiptNo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payments");
        }
    }
}
