using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddContractRenewalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutomaticRenewal",
                table: "Contracts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RenewalMonths",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutomaticRenewal",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "RenewalMonths",
                table: "Contracts");
        }
    }
}
