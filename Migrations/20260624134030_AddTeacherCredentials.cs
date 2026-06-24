using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalikongkongNHS.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Credentials",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Credentials",
                table: "Teachers");
        }
    }
}
