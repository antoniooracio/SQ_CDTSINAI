using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIncidentInternalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvolvedArea",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InvolvedBrand",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InvolvedRegional",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LocationOrUnit",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TargetArea",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvolvedArea",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "InvolvedBrand",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "InvolvedRegional",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "LocationOrUnit",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "TargetArea",
                table: "Incidents");
        }
    }
}
