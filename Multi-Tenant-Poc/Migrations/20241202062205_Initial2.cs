using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Multi_Tenant_Poc.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TemperatureC = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecasts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Forecasts",
                columns: new[] { "Id", "Date", "Summary", "TemperatureC" },
                values: new object[,]
                {
                    { 1, new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chilly", -17 },
                    { 2, new DateTime(2022, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Balmy", 38 },
                    { 3, new DateTime(2022, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sweltering", -7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Forecasts");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "integer",
                nullable: true);
        }
    }
}
