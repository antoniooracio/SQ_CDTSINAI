using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHtmlAndPermanentToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentHtml",
                table: "EstablishmentDocuments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsPermanent",
                table: "EstablishmentDocuments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentHtml",
                table: "EstablishmentDocuments");

            migrationBuilder.DropColumn(
                name: "IsPermanent",
                table: "EstablishmentDocuments");
        }
    }
}
