using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalikongkongNHS.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeAndRoomToSectionSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "SectionSubjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Time",
                table: "SectionSubjects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "SectionSubjects");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "SectionSubjects");
        }
    }
}
