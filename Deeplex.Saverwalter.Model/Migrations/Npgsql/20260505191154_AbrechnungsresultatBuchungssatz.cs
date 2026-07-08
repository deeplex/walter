using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class AbrechnungsresultatBuchungssatz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "erhaltungsaufwands_konto_id",
                table: "wohnungen",
                newName: "aufwands_konto_id");

            migrationBuilder.RenameIndex(
                name: "ix_wohnungen_erhaltungsaufwands_konto_id",
                table: "wohnungen",
                newName: "ix_wohnungen_aufwands_konto_id");

            migrationBuilder.AddColumn<Guid>(
                name: "buchungssatz_id",
                table: "abrechnungsresultate",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_abrechnungsresultate_buchungssatz_id",
                table: "abrechnungsresultate",
                column: "buchungssatz_id");

            migrationBuilder.AddForeignKey(
                name: "fk_abrechnungsresultate_buchungssaetze_buchungssatz_id",
                table: "abrechnungsresultate",
                column: "buchungssatz_id",
                principalTable: "buchungssaetze",
                principalColumn: "buchungssatz_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(name: "kaltmiete", table: "abrechnungsresultate");
            migrationBuilder.DropColumn(name: "jahr", table: "abrechnungsresultate");
            migrationBuilder.DropColumn(name: "vorauszahlung", table: "abrechnungsresultate");
            migrationBuilder.DropColumn(name: "minderung", table: "abrechnungsresultate");
            migrationBuilder.DropColumn(name: "rechnungsbetrag", table: "abrechnungsresultate");
            migrationBuilder.DropColumn(name: "saldo", table: "abrechnungsresultate");

            // Remove rows without a Buchungssatz before adding the NOT NULL constraint
            migrationBuilder.Sql("DELETE FROM abrechnungsresultate WHERE buchungssatz_id IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "buchungssatz_id",
                table: "abrechnungsresultate",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mietminderungs_konto_id",
                table: "vertraege",
                type: "integer",
                nullable: true);

            // Buchungskonto (Aufwand=2) für jede bestehende Vertrag anlegen und verknüpfen
            migrationBuilder.Sql(@"
                WITH inserted AS (
                    INSERT INTO buchungskonten (kontonummer, bezeichnung, kontotyp, created_at, last_modified)
                    SELECT
                        'V' || LPAD(vertrag_id::text, 5, '0') || '-MM',
                        'Mietminderung',
                        2,
                        NOW(),
                        NOW()
                    FROM vertraege
                    RETURNING buchungskonto_id, kontonummer
                )
                UPDATE vertraege v
                SET mietminderungs_konto_id = i.buchungskonto_id
                FROM inserted i
                WHERE i.kontonummer = 'V' || LPAD(v.vertrag_id::text, 5, '0') || '-MM'
            ");

            migrationBuilder.AlterColumn<int>(
                name: "mietminderungs_konto_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_mietminderungs_konto_id",
                table: "vertraege",
                column: "mietminderungs_konto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_mietminderungs_konto_id",
                table: "vertraege",
                column: "mietminderungs_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "allgemein_waerme_id",
                table: "hkvo",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_allgemein_waerme_id",
                table: "hkvo",
                column: "allgemein_waerme_id");

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_zaehler_set_allgemein_waerme_id",
                table: "hkvo",
                column: "allgemein_waerme_id",
                principalTable: "zaehler_set",
                principalColumn: "zaehler_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "kaltmiete", table: "abrechnungsresultate", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<int>(name: "jahr", table: "abrechnungsresultate", type: "integer", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<decimal>(name: "vorauszahlung", table: "abrechnungsresultate", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "minderung", table: "abrechnungsresultate", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "rechnungsbetrag", table: "abrechnungsresultate", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "saldo", table: "abrechnungsresultate", type: "numeric", nullable: false, defaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "buchungssatz_id",
                table: "abrechnungsresultate",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_mietminderungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_mietminderungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "mietminderungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_abrechnungsresultate_buchungssaetze_buchungssatz_id",
                table: "abrechnungsresultate");

            migrationBuilder.DropIndex(
                name: "ix_abrechnungsresultate_buchungssatz_id",
                table: "abrechnungsresultate");

            migrationBuilder.DropColumn(
                name: "buchungssatz_id",
                table: "abrechnungsresultate");

            migrationBuilder.RenameColumn(
                name: "aufwands_konto_id",
                table: "wohnungen",
                newName: "erhaltungsaufwands_konto_id");

            migrationBuilder.RenameIndex(
                name: "ix_wohnungen_aufwands_konto_id",
                table: "wohnungen",
                newName: "ix_wohnungen_erhaltungsaufwands_konto_id");

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
    }
}
