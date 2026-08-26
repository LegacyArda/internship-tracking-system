using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "InternshipReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "InternshipReports");
        }
    }
}
