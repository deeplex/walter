using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class HkvoAllgemeinWaermeMehrereZaehler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hkvo_allgemein_waerme_zaehler",
                columns: table => new
                {
                    hkvo_id = table.Column<int>(type: "integer", nullable: false),
                    zaehler_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hkvo_allgemein_waerme_zaehler", x => new { x.hkvo_id, x.zaehler_id });
                    table.ForeignKey(
                        name: "fk_hkvo_allgemein_waerme_zaehler_hkvo_hkvo_id",
                        column: x => x.hkvo_id,
                        principalTable: "hkvo",
                        principalColumn: "hkvo_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_hkvo_allgemein_waerme_zaehler_zaehler_set_zaehler_id",
                        column: x => x.zaehler_id,
                        principalTable: "zaehler_set",
                        principalColumn: "zaehler_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_allgemein_waerme_zaehler_zaehler_id",
                table: "hkvo_allgemein_waerme_zaehler",
                column: "zaehler_id");

            // Bestehende Einzel-Zuordnung (allgemein_waerme_id) in die Join-Tabelle übernehmen.
            migrationBuilder.Sql(
                "INSERT INTO hkvo_allgemein_waerme_zaehler (hkvo_id, zaehler_id) " +
                "SELECT hkvo_id, allgemein_waerme_id FROM hkvo WHERE allgemein_waerme_id IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "fk_hkvo_zaehler_set_allgemein_waerme_id",
                table: "hkvo");

            migrationBuilder.DropIndex(
                name: "ix_hkvo_allgemein_waerme_id",
                table: "hkvo");

            migrationBuilder.DropColumn(
                name: "allgemein_waerme_id",
                table: "hkvo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "allgemein_waerme_id",
                table: "hkvo",
                type: "integer",
                nullable: true);

            // Ersten zugeordneten Zähler je HKVO zurück in die Einzelspalte übernehmen.
            migrationBuilder.Sql(
                "UPDATE hkvo SET allgemein_waerme_id = sub.zaehler_id FROM (" +
                "SELECT hkvo_id, MIN(zaehler_id) AS zaehler_id FROM hkvo_allgemein_waerme_zaehler GROUP BY hkvo_id" +
                ") sub WHERE hkvo.hkvo_id = sub.hkvo_id;");

            migrationBuilder.DropTable(
                name: "hkvo_allgemein_waerme_zaehler");

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_allgemein_waerme_id",
                table: "hkvo",
                column: "allgemein_waerme_id");

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_zaehler_set_allgemein_waerme_id",
                table: "hkvo",
                column: "allgemein_waerme_id",
                principalTable: "zaehler_set",
                principalColumn: "zaehler_id");
        }
    }
}
