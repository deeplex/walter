using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class Abrechnungsresultate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "abrechnungsresultate",
                columns: table => new
                {
                    abrechnungsresultat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    jahr = table.Column<int>(type: "integer", nullable: false),
                    kaltmiete = table.Column<double>(type: "double precision", nullable: false),
                    vorauszahlung = table.Column<double>(type: "double precision", nullable: false),
                    minderung = table.Column<double>(type: "double precision", nullable: false),
                    rechnungsbetrag = table.Column<double>(type: "double precision", nullable: false),
                    ist_beglichen = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_abrechnungsresultate", x => x.abrechnungsresultat_id);
                    table.ForeignKey(
                        name: "fk_abrechnungsresultate_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_abrechnungsresultate_vertrag_id",
                table: "abrechnungsresultate",
                column: "vertrag_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abrechnungsresultate");
        }
    }
}
