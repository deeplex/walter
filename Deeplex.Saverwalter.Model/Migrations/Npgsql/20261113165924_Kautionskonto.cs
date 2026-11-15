using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class Kautionskonto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Kautionsdaten werden nicht stillschweigend verworfen: Gibt es Beträge oder
            // Daten, bricht die Migration ab — dann erst auf dem KautionsKonto buchen.
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM vertraege
               WHERE kaution_betrag IS NOT NULL
                  OR kaution_eingangsdatum IS NOT NULL
                  OR kaution_rueckgabedatum IS NOT NULL) THEN
        RAISE EXCEPTION 'Verträge mit Kautionsdaten vorhanden — diese zuerst als Buchung erfassen.';
    END IF;
END $$;");

            migrationBuilder.AddColumn<int>(
                name: "kautions_konto_id",
                table: "vertraege",
                type: "integer",
                nullable: true);

            // Je Vertrag ein KautionsKonto (Passiv); Kontonummer aus der Vertrags-Id.
            migrationBuilder.Sql(@"
DO $$
DECLARE
    v record;
    neu integer;
BEGIN
    FOR v IN SELECT vertrag_id FROM vertraege LOOP
        INSERT INTO buchungskonten (kontonummer, bezeichnung, kontotyp)
        VALUES ('V' || lpad(v.vertrag_id::text, 5, '0') || '-KA', 'Kaution', 1)
        RETURNING buchungskonto_id INTO neu;
        UPDATE vertraege SET kautions_konto_id = neu WHERE vertrag_id = v.vertrag_id;
    END LOOP;
END $$;");

            migrationBuilder.AlterColumn<int>(
                name: "kautions_konto_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_kautions_konto_id",
                table: "vertraege",
                column: "kautions_konto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_kautions_konto_id",
                table: "vertraege",
                column: "kautions_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropColumn(
                name: "kaution_art",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "kaution_betrag",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "kaution_eingangsdatum",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "kaution_rueckgabedatum",
                table: "vertraege");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_kautions_konto_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_kautions_konto_id",
                table: "vertraege");

            migrationBuilder.Sql(@"
CREATE TEMP TABLE kautionskonten_alt AS SELECT kautions_konto_id AS id FROM vertraege;");

            migrationBuilder.DropColumn(
                name: "kautions_konto_id",
                table: "vertraege");

            // Schlägt fehl, wenn auf einem KautionsKonto schon gebucht wurde.
            migrationBuilder.Sql(@"
DELETE FROM buchungskonten WHERE buchungskonto_id IN (SELECT id FROM kautionskonten_alt);
DROP TABLE kautionskonten_alt;");

            migrationBuilder.AddColumn<string>(
                name: "kaution_art",
                table: "vertraege",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "kaution_betrag",
                table: "vertraege",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "kaution_eingangsdatum",
                table: "vertraege",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "kaution_rueckgabedatum",
                table: "vertraege",
                type: "date",
                nullable: true);
        }
    }
}
