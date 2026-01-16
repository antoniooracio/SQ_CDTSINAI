using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEstablishmentType1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "EstablishmentTypeId",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "EstablishmentTypeId1",
                table: "DocumentTypes");

            migrationBuilder.CreateTable(
                name: "EstablishmentTypeClosingDocuments",
                columns: table => new
                {
                    ClosingDocumentsId = table.Column<int>(type: "int", nullable: false),
                    EstablishmentType1Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentTypeClosingDocuments", x => new { x.ClosingDocumentsId, x.EstablishmentType1Id });
                    table.ForeignKey(
                        name: "FK_EstablishmentTypeClosingDocuments_DocumentTypes_ClosingDocum~",
                        column: x => x.ClosingDocumentsId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstablishmentTypeClosingDocuments_EstablishmentTypes_Establi~",
                        column: x => x.EstablishmentType1Id,
                        principalTable: "EstablishmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EstablishmentTypeNecessaryDocuments",
                columns: table => new
                {
                    EstablishmentTypeId = table.Column<int>(type: "int", nullable: false),
                    NecessaryDocumentsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentTypeNecessaryDocuments", x => new { x.EstablishmentTypeId, x.NecessaryDocumentsId });
                    table.ForeignKey(
                        name: "FK_EstablishmentTypeNecessaryDocuments_DocumentTypes_NecessaryD~",
                        column: x => x.NecessaryDocumentsId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstablishmentTypeNecessaryDocuments_EstablishmentTypes_Estab~",
                        column: x => x.EstablishmentTypeId,
                        principalTable: "EstablishmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentTypeClosingDocuments_EstablishmentType1Id",
                table: "EstablishmentTypeClosingDocuments",
                column: "EstablishmentType1Id");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentTypeNecessaryDocuments_NecessaryDocumentsId",
                table: "EstablishmentTypeNecessaryDocuments",
                column: "NecessaryDocumentsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstablishmentTypeClosingDocuments");

            migrationBuilder.DropTable(
                name: "EstablishmentTypeNecessaryDocuments");

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
    }
}
