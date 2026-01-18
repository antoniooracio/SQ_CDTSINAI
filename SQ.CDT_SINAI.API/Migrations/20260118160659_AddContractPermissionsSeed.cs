using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddContractPermissionsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Action", "Module", "RoleId" },
                values: new object[,]
                {
                    { 5, "View", "Contract", 2 },
                    { 6, "Create", "Contract", 2 },
                    { 7, "Edit", "Contract", 2 },
                    { 8, "View", "ContractRenewalLog", 2 },
                    { 9, "Revert", "ContractRenewalLog", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_EstablishmentId",
                table: "Contracts",
                column: "EstablishmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Establishments_EstablishmentId",
                table: "Contracts",
                column: "EstablishmentId",
                principalTable: "Establishments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Establishments_EstablishmentId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_EstablishmentId",
                table: "Contracts");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
