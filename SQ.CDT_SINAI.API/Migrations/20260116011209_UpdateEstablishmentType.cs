using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEstablishmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceLocationType",
                table: "EstablishmentTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstablishmentTypeId",
                table: "DocumentTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstablishmentTypeId1",
                table: "DocumentTypes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_EstablishmentTypeId",
                table: "DocumentTypes",
                column: "EstablishmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_EstablishmentTypeId1",
                table: "DocumentTypes",
                column: "EstablishmentTypeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_EstablishmentTypes_EstablishmentTypeId",
                table: "DocumentTypes",
                column: "EstablishmentTypeId",
                principalTable: "EstablishmentTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_EstablishmentTypes_EstablishmentTypeId1",
                table: "DocumentTypes",
                column: "EstablishmentTypeId1",
                principalTable: "EstablishmentTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_EstablishmentTypes_EstablishmentTypeId",
                table: "DocumentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_EstablishmentTypes_EstablishmentTypeId1",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_EstablishmentTypeId",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_EstablishmentTypeId1",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "ServiceLocationType",
                table: "EstablishmentTypes");

            migrationBuilder.DropColumn(
                name: "EstablishmentTypeId",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "EstablishmentTypeId1",
                table: "DocumentTypes");
        }
    }
}
