using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class Abrechnungsverzicht : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "abrechnungsverzichte",
                columns: table => new
                {
                    abrechnungsverzicht_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    jahr = table.Column<int>(type: "integer", nullable: false),
                    grund = table.Column<string>(type: "text", nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_abrechnungsverzichte", x => x.abrechnungsverzicht_id);
                    table.ForeignKey(
                        name: "fk_abrechnungsverzichte_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_abrechnungsverzichte_vertrag_id_jahr",
                table: "abrechnungsverzichte",
                columns: new[] { "vertrag_id", "jahr" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abrechnungsverzichte");
        }
    }
}
