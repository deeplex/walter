using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class TransaktionenDatumIsDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "zahlungsdatum",
                table: "transaktionen",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "zahlungsdatum",
                table: "transaktionen",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
