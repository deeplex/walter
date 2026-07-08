using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class BackfillHkvoAllgemeinWaerme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rückverdrahtung: AllgemeinWärme-Zähler (§9(2) Q) war früher implizit ein
            // Wärmequelle-Zähler an der Adresse ohne Wohnung. Neue Struktur ist explizit
            // (hkvo.allgemein_waerme_id). Setze ihn, wo die Heizkosten-Umlage GENAU EINEN
            // Nicht-Wohnung-Wärmequelle-Zähler (Gas=3 / Wärme=4) hat — bei Mehrdeutigkeit
            // bleibt NULL und wird manuell über die HKVO-UI gesetzt. Idempotent.
            migrationBuilder.Sql(@"
UPDATE hkvo h
SET allgemein_waerme_id = sub.zaehler_id
FROM (
    SELECT h2.hkvo_id, MIN(z.zaehler_id) AS zaehler_id
    FROM hkvo h2
    JOIN umlage_zaehler uz ON uz.umlagen_umlage_id = h2.heizkosten_id
    JOIN zaehler_set z ON z.zaehler_id = uz.zaehler_id
    WHERE z.wohnung_id IS NULL
      AND z.typ IN (3, 4)
    GROUP BY h2.hkvo_id
    HAVING COUNT(*) = 1
) sub
WHERE h.hkvo_id = sub.hkvo_id
  AND h.allgemein_waerme_id IS NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
