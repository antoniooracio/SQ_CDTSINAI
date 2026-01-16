using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndDataScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Collaborators",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CollaboratorEstablishment",
                columns: table => new
                {
                    CollaboratorsId = table.Column<int>(type: "int", nullable: false),
                    EstablishmentsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollaboratorEstablishment", x => new { x.CollaboratorsId, x.EstablishmentsId });
                    table.ForeignKey(
                        name: "FK_CollaboratorEstablishment_Collaborators_CollaboratorsId",
                        column: x => x.CollaboratorsId,
                        principalTable: "Collaborators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollaboratorEstablishment_Establishments_EstablishmentsId",
                        column: x => x.EstablishmentsId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Coordenador" },
                    { 3, "Colaborador" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collaborators_RoleId",
                table: "Collaborators",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaboratorEstablishment_EstablishmentsId",
                table: "CollaboratorEstablishment",
                column: "EstablishmentsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collaborators_Roles_RoleId",
                table: "Collaborators",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collaborators_Roles_RoleId",
                table: "Collaborators");

            migrationBuilder.DropTable(
                name: "CollaboratorEstablishment");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Collaborators_RoleId",
                table: "Collaborators");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Collaborators");
        }
    }
}
