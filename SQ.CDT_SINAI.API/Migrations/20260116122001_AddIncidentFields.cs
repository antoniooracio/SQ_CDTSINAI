using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQ.CDT_SINAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cip",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ClientAddress",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClientBirthDate",
                table: "Incidents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientCpf",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ClientEmail",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ClientPhone",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorCrm",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorEmail",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorPhone1",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorPhone2",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HealthInsurance",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProtocolNumber",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryContactName",
                table: "Incidents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cip",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ClientAddress",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ClientBirthDate",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ClientCpf",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ClientEmail",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ClientPhone",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DoctorCrm",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DoctorEmail",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DoctorPhone1",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DoctorPhone2",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "HealthInsurance",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ProtocolNumber",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "SecondaryContactName",
                table: "Incidents");
        }
    }
}
