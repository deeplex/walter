using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class ZahlungZugeordnetZu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "zugeordnet_zu_id",
                table: "buchungszeilen",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_buchungszeilen_zugeordnet_zu_id",
                table: "buchungszeilen",
                column: "zugeordnet_zu_id");

            migrationBuilder.AddForeignKey(
                name: "fk_buchungszeilen_buchungszeilen_zugeordnet_zu_id",
                table: "buchungszeilen",
                column: "zugeordnet_zu_id",
                principalTable: "buchungszeilen",
                principalColumn: "buchungszeile_id",
                onDelete: ReferentialAction.Restrict);

            // Bestand: Zahlungszeilen werden dem Posten zugeordnet, mit dem sie heute
            // ausgeglichen sind. Zahlungszeile = keine Geld-Zeile in einem Satz, der eine
            // Geld-Zeile hat (Bankkonto oder Zahlungskonto eines Vertrags, Garagenvertrags
            // oder einer Umlage), kein Storno-Satz, genau ein Ausgleich mit einem anderen
            // Satz. Ausgleiche ohne Geld-Zeile (Verteilung, Miet-Korrektur) bleiben unberührt.
            migrationBuilder.Sql(@"
WITH geld AS (
    SELECT buchungs_konto_id AS konto_id FROM bankkontos
    UNION SELECT zahlungs_konto_id FROM vertraege
    UNION SELECT zahlungs_konto_id FROM garage_vertraege
    UNION SELECT zahlungs_konto_id FROM umlagen
),
zahlungszeile AS (
    SELECT z.buchungszeile_id, z.buchungssatz_id
    FROM buchungszeilen z
    JOIN buchungssaetze s ON s.buchungssatz_id = z.buchungssatz_id
    WHERE s.storno_von_id IS NULL
      AND z.buchungskonto_id NOT IN (SELECT konto_id FROM geld)
      AND EXISTS (SELECT 1 FROM buchungszeilen g
                  WHERE g.buchungssatz_id = s.buchungssatz_id
                    AND g.buchungskonto_id IN (SELECT konto_id FROM geld))
),
ausgleich AS (
    SELECT zz.buchungszeile_id,
           CASE WHEN o.soll_zeile_id = zz.buchungszeile_id THEN o.haben_zeile_id ELSE o.soll_zeile_id END AS posten_id
    FROM zahlungszeile zz
    JOIN offene_posten_ausgleiche o
      ON o.soll_zeile_id = zz.buchungszeile_id OR o.haben_zeile_id = zz.buchungszeile_id
),
eindeutig AS (
    SELECT a.buchungszeile_id, min(a.posten_id::text)::uuid AS posten_id
    FROM ausgleich a
    JOIN buchungszeilen p ON p.buchungszeile_id = a.posten_id
    JOIN zahlungszeile zz ON zz.buchungszeile_id = a.buchungszeile_id
    WHERE p.buchungssatz_id <> zz.buchungssatz_id
    GROUP BY a.buchungszeile_id
    HAVING count(*) = 1
)
UPDATE buchungszeilen z
SET zugeordnet_zu_id = e.posten_id
FROM eindeutig e
WHERE z.buchungszeile_id = e.buchungszeile_id;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_buchungszeilen_buchungszeilen_zugeordnet_zu_id",
                table: "buchungszeilen");

            migrationBuilder.DropIndex(
                name: "ix_buchungszeilen_zugeordnet_zu_id",
                table: "buchungszeilen");

            migrationBuilder.DropColumn(
                name: "zugeordnet_zu_id",
                table: "buchungszeilen");
        }
    }
}
