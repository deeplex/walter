using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class AddEndeToZaehler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ende",
                table: "zaehler_set",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_modified",
                table: "vertrag_versionen",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "vertrag_versionen",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ende",
                table: "zaehler_set");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "last_modified",
                table: "vertrag_versionen",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "created_at",
                table: "vertrag_versionen",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
