using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "EstablishmentDocuments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "EstablishmentDocuments");
        }
    }
}
