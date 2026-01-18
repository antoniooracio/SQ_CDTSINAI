using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "Collaborators",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Collaborators",
                columns: new[] { "Id", "Active", "AddressCity", "AddressNeighborhood", "AddressNumber", "AddressState", "AddressStreet", "AddressZipCode", "AdmissionDate", "BirthDate", "Cpf", "Email", "JobTitle", "Name", "Password", "PhoneNumber", "ProfessionalLicense", "ProfilePictureUrl", "Rg", "RoleId" },
                values: new object[] { 1, true, "Palmas", "Centro", "1", "TO", "Sede", "77000-000", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "000.000.000-00", "admin@sinai.com.br", "Super Admin", "Administrador do Sistema", "admin", "00000000000", "N/A", null, "", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Collaborators",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "Collaborators");
        }
    }
}
