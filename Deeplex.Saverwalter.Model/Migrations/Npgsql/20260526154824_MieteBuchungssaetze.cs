using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class MieteBuchungssaetze : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create new tables before data is read from structures that will be dropped
            migrationBuilder.CreateTable(
                name: "kontakt_mitgliedschaften",
                columns: table => new
                {
                    kontakt_mitgliedschaft_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    von = table.Column<DateOnly>(type: "date", nullable: false),
                    bis = table.Column<DateOnly>(type: "date", nullable: true),
                    anteil = table.Column<decimal>(type: "numeric", nullable: true),
                    juristische_person_id = table.Column<int>(type: "integer", nullable: false),
                    mitglied_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kontakt_mitgliedschaften", x => x.kontakt_mitgliedschaft_id);
                    table.ForeignKey(
                        name: "fk_kontakt_mitgliedschaften_kontakte_juristische_person_id",
                        column: x => x.juristische_person_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_kontakt_mitgliedschaften_kontakte_mitglied_id",
                        column: x => x.mitglied_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wohnung_eigentuemer",
                columns: table => new
                {
                    wohnung_eigentuemer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    von = table.Column<DateOnly>(type: "date", nullable: false),
                    bis = table.Column<DateOnly>(type: "date", nullable: true),
                    anteil = table.Column<decimal>(type: "numeric", nullable: true),
                    wohnung_id = table.Column<int>(type: "integer", nullable: false),
                    kontakt_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wohnung_eigentuemer", x => x.wohnung_eigentuemer_id);
                    table.ForeignKey(
                        name: "fk_wohnung_eigentuemer_kontakte_kontakt_id",
                        column: x => x.kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_wohnung_eigentuemer_wohnungen_wohnung_id",
                        column: x => x.wohnung_id,
                        principalTable: "wohnungen",
                        principalColumn: "wohnung_id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Migrate Wohnung.BesitzerKontaktId → WohnungEigentuemer before the column is dropped
            migrationBuilder.Sql(@"
DO $$
BEGIN
    INSERT INTO wohnung_eigentuemer (von, wohnung_id, kontakt_id, created_at, last_modified)
    SELECT
        COALESCE(w.created_at::date, '2000-01-01'::date),
        w.wohnung_id,
        w.besitzer_kontakt_id,
        NOW(),
        NOW()
    FROM wohnungen w
    WHERE w.besitzer_kontakt_id IS NOT NULL;
END;
$$;
");

            // Migrate KontaktKontakt join table → KontaktMitgliedschaft before the table is dropped
            migrationBuilder.Sql(@"
DO $$
BEGIN
    INSERT INTO kontakt_mitgliedschaften (von, juristische_person_id, mitglied_id, created_at, last_modified)
    SELECT
        NOW()::date,
        kk.juristische_personen_kontakt_id,
        kk.mitglieder_kontakt_id,
        NOW(),
        NOW()
    FROM kontakt_kontakt kk;
END;
$$;
");

            migrationBuilder.DropForeignKey(
                name: "fk_abrechnungsresultate_buchungssaetze_buchungssatz_id",
                table: "abrechnungsresultate");

            migrationBuilder.DropForeignKey(
                name: "fk_hkvo_zaehler_set_allgemein_waerme_id",
                table: "hkvo");

            migrationBuilder.DropForeignKey(
                name: "fk_transaktionen_kontakte_zahler_kontakt_id",
                table: "transaktionen");

            migrationBuilder.DropForeignKey(
                name: "fk_transaktionen_kontakte_zahlungsempfaenger_kontakt_id",
                table: "transaktionen");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_kautions_konto_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_mietminderungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_wohnungen_kontakte_besitzer_kontakt_id",
                table: "wohnungen");

            migrationBuilder.DropTable(
                name: "garage_vertrag");

            migrationBuilder.DropTable(
                name: "kontakt_kontakt");

            migrationBuilder.DropTable(
                name: "kontos");

            migrationBuilder.DropIndex(
                name: "ix_wohnungen_besitzer_kontakt_id",
                table: "wohnungen");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_kautions_konto_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_transaktionen_zahler_kontakt_id",
                table: "transaktionen");

            migrationBuilder.DropIndex(
                name: "ix_transaktionen_zahlungsempfaenger_kontakt_id",
                table: "transaktionen");

            migrationBuilder.DropIndex(
                name: "ix_hkvo_heizkosten_id",
                table: "hkvo");

            migrationBuilder.DropIndex(
                name: "ix_buchungssaetze_buchungsjahr_buchungsnummer",
                table: "buchungssaetze");

            migrationBuilder.DropColumn(
                name: "besitzer_kontakt_id",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "miteigentumsanteile",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "nutzeinheit",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "nutzflaeche",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "wohnflaeche",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "kautions_konto_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "schluessel",
                table: "umlagen");

            migrationBuilder.DropColumn(
                name: "zahler_kontakt_id",
                table: "transaktionen");

            migrationBuilder.DropColumn(
                name: "zahlungsempfaenger_kontakt_id",
                table: "transaktionen");

            migrationBuilder.DropColumn(
                name: "is_abgeschlossen",
                table: "buchungssaetze");

            migrationBuilder.RenameColumn(
                name: "mietminderungs_konto_id",
                table: "vertraege",
                newName: "mietminderungs_konto_buchungskonto_id");

            migrationBuilder.RenameIndex(
                name: "ix_vertraege_mietminderungs_konto_id",
                table: "vertraege",
                newName: "ix_vertraege_mietminderungs_konto_buchungskonto_id");

            migrationBuilder.CreateSequence(
                name: "buchungsnummer_seq");

            // Backfill nullable buchungsjahr/buchungsnummer before AlterColumn makes them NOT NULL
            migrationBuilder.Sql("UPDATE buchungssaetze SET buchungsjahr = EXTRACT(YEAR FROM buchungsdatum)::int WHERE buchungsjahr IS NULL");
            migrationBuilder.Sql("UPDATE buchungssaetze SET buchungsnummer = nextval('buchungsnummer_seq') WHERE buchungsnummer IS NULL");

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

            migrationBuilder.AddColumn<int>(
                name: "betr_kv_nummer",
                table: "umlagetypen",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "zahler_bankkonto_id",
                table: "transaktionen",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "zahlungsempfaenger_bankkonto_id",
                table: "transaktionen",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "beginn",
                table: "hkvo",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "ertragskonto_id",
                table: "garagen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "buchungssatz_id",
                table: "erhaltungsaufwendungen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "buchungsnummer",
                table: "buchungssaetze",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('buchungsnummer_seq')",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "buchungsjahr",
                table: "buchungssaetze",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "bankkontos",
                columns: table => new
                {
                    bankkonto_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank = table.Column<string>(type: "text", nullable: true),
                    iban = table.Column<string>(type: "text", nullable: true),
                    buchungs_konto_id = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bankkontos", x => x.bankkonto_id);
                    table.ForeignKey(
                        name: "fk_bankkontos_buchungskonten_buchungs_konto_id",
                        column: x => x.buchungs_konto_id,
                        principalTable: "buchungskonten",
                        principalColumn: "buchungskonto_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "garage_vertraege",
                columns: table => new
                {
                    garage_vertrag_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    garage_id = table.Column<int>(type: "integer", nullable: false),
                    vertrag_id = table.Column<int>(type: "integer", nullable: true),
                    ende = table.Column<DateOnly>(type: "date", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    miet_buchungskonto_id = table.Column<int>(type: "integer", nullable: false),
                    zahlungs_konto_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garage_vertraege", x => x.garage_vertrag_id);
                    table.ForeignKey(
                        name: "fk_garage_vertraege_buchungskonten_miet_buchungskonto_id",
                        column: x => x.miet_buchungskonto_id,
                        principalTable: "buchungskonten",
                        principalColumn: "buchungskonto_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_garage_vertraege_buchungskonten_zahlungs_konto_id",
                        column: x => x.zahlungs_konto_id,
                        principalTable: "buchungskonten",
                        principalColumn: "buchungskonto_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_garage_vertraege_garagen_garage_id",
                        column: x => x.garage_id,
                        principalTable: "garagen",
                        principalColumn: "garage_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_garage_vertraege_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id");
                });

            migrationBuilder.CreateTable(
                name: "offene_posten_ausgleiche",
                columns: table => new
                {
                    offener_posten_ausgleich_id = table.Column<Guid>(type: "uuid", nullable: false),
                    soll_zeile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    haben_zeile_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offene_posten_ausgleiche", x => x.offener_posten_ausgleich_id);
                    table.ForeignKey(
                        name: "fk_offene_posten_ausgleiche_buchungszeilen_haben_zeile_id",
                        column: x => x.haben_zeile_id,
                        principalTable: "buchungszeilen",
                        principalColumn: "buchungszeile_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_offene_posten_ausgleiche_buchungszeilen_soll_zeile_id",
                        column: x => x.soll_zeile_id,
                        principalTable: "buchungszeilen",
                        principalColumn: "buchungszeile_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "umlage_versionen",
                columns: table => new
                {
                    umlage_version_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    umlage_id = table.Column<int>(type: "integer", nullable: false),
                    beginn = table.Column<DateOnly>(type: "date", nullable: false),
                    schluessel = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_umlage_versionen", x => x.umlage_version_id);
                    table.ForeignKey(
                        name: "fk_umlage_versionen_umlagen_umlage_id",
                        column: x => x.umlage_id,
                        principalTable: "umlagen",
                        principalColumn: "umlage_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wohnung_versionen",
                columns: table => new
                {
                    wohnung_version_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wohnung_id = table.Column<int>(type: "integer", nullable: false),
                    beginn = table.Column<DateOnly>(type: "date", nullable: false),
                    wohnflaeche = table.Column<decimal>(type: "numeric", nullable: false),
                    nutzflaeche = table.Column<decimal>(type: "numeric", nullable: false),
                    miteigentumsanteile = table.Column<decimal>(type: "numeric", nullable: false),
                    nutzeinheit = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wohnung_versionen", x => x.wohnung_version_id);
                    table.ForeignKey(
                        name: "fk_wohnung_versionen_wohnungen_wohnung_id",
                        column: x => x.wohnung_id,
                        principalTable: "wohnungen",
                        principalColumn: "wohnung_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bankkonto_besitzer",
                columns: table => new
                {
                    bankkonto_id = table.Column<int>(type: "integer", nullable: false),
                    kontakt_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bankkonto_besitzer", x => new { x.bankkonto_id, x.kontakt_id });
                    table.ForeignKey(
                        name: "fk_bankkonto_besitzer_bankkontos_bankkonto_id",
                        column: x => x.bankkonto_id,
                        principalTable: "bankkontos",
                        principalColumn: "bankkonto_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bankkonto_besitzer_kontakte_kontakt_id",
                        column: x => x.kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "garage_vertrag_mieter",
                columns: table => new
                {
                    garage_vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    kontakt_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garage_vertrag_mieter", x => new { x.garage_vertrag_id, x.kontakt_id });
                    table.ForeignKey(
                        name: "fk_garage_vertrag_mieter_garage_vertraege_garage_vertrag_id",
                        column: x => x.garage_vertrag_id,
                        principalTable: "garage_vertraege",
                        principalColumn: "garage_vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_garage_vertrag_mieter_kontakte_kontakt_id",
                        column: x => x.kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "garage_vertrag_versionen",
                columns: table => new
                {
                    garage_vertrag_version_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    garage_vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    beginn = table.Column<DateOnly>(type: "date", nullable: false),
                    garagen_miete = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garage_vertrag_versionen", x => x.garage_vertrag_version_id);
                    table.ForeignKey(
                        name: "fk_garage_vertrag_versionen_garage_vertraege_garage_vertrag_id",
                        column: x => x.garage_vertrag_id,
                        principalTable: "garage_vertraege",
                        principalColumn: "garage_vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transaktionen_zahler_bankkonto_id",
                table: "transaktionen",
                column: "zahler_bankkonto_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaktionen_zahlungsempfaenger_bankkonto_id",
                table: "transaktionen",
                column: "zahlungsempfaenger_bankkonto_id");

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_heizkosten_id",
                table: "hkvo",
                column: "heizkosten_id");

            migrationBuilder.CreateIndex(
                name: "ix_garagen_ertragskonto_id",
                table: "garagen",
                column: "ertragskonto_id");

            migrationBuilder.CreateIndex(
                name: "ix_erhaltungsaufwendungen_buchungssatz_id",
                table: "erhaltungsaufwendungen",
                column: "buchungssatz_id");

            migrationBuilder.CreateIndex(
                name: "ix_buchungssaetze_buchungsjahr_buchungsnummer",
                table: "buchungssaetze",
                columns: new[] { "buchungsjahr", "buchungsnummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bankkonto_besitzer_kontakt_id",
                table: "bankkonto_besitzer",
                column: "kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_bankkontos_buchungs_konto_id",
                table: "bankkontos",
                column: "buchungs_konto_id");

            migrationBuilder.CreateIndex(
                name: "ix_garage_vertraege_garage_id",
                table: "garage_vertraege",
                column: "garage_id");

            migrationBuilder.CreateIndex(
                name: "ix_garage_vertraege_miet_buchungskonto_id",
                table: "garage_vertraege",
                column: "miet_buchungskonto_id");

            migrationBuilder.CreateIndex(
                name: "ix_garage_vertraege_vertrag_id",
                table: "garage_vertraege",
                column: "vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_garage_vertraege_zahlungs_konto_id",
                table: "garage_vertraege",
                column: "zahlungs_konto_id");

            migrationBuilder.CreateIndex(
                name: "ix_garage_vertrag_mieter_kontakt_id",
                table: "garage_vertrag_mieter",
                column: "kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_garage_vertrag_versionen_garage_vertrag_id",
                table: "garage_vertrag_versionen",
                column: "garage_vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_kontakt_mitgliedschaften_juristische_person_id",
                table: "kontakt_mitgliedschaften",
                column: "juristische_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_kontakt_mitgliedschaften_mitglied_id",
                table: "kontakt_mitgliedschaften",
                column: "mitglied_id");

            migrationBuilder.CreateIndex(
                name: "ix_offene_posten_ausgleiche_haben_zeile_id",
                table: "offene_posten_ausgleiche",
                column: "haben_zeile_id");

            migrationBuilder.CreateIndex(
                name: "ix_offene_posten_ausgleiche_soll_zeile_id",
                table: "offene_posten_ausgleiche",
                column: "soll_zeile_id");

            migrationBuilder.CreateIndex(
                name: "ix_umlage_versionen_umlage_id",
                table: "umlage_versionen",
                column: "umlage_id");

            migrationBuilder.CreateIndex(
                name: "ix_wohnung_eigentuemer_kontakt_id",
                table: "wohnung_eigentuemer",
                column: "kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_wohnung_eigentuemer_wohnung_id",
                table: "wohnung_eigentuemer",
                column: "wohnung_id");

            migrationBuilder.CreateIndex(
                name: "ix_wohnung_versionen_wohnung_id",
                table: "wohnung_versionen",
                column: "wohnung_id");

            migrationBuilder.AddForeignKey(
                name: "fk_abrechnungsresultate_buchungssaetze_buchungssatz_id",
                table: "abrechnungsresultate",
                column: "buchungssatz_id",
                principalTable: "buchungssaetze",
                principalColumn: "buchungssatz_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_erhaltungsaufwendungen_buchungssaetze_buchungssatz_id",
                table: "erhaltungsaufwendungen",
                column: "buchungssatz_id",
                principalTable: "buchungssaetze",
                principalColumn: "buchungssatz_id");

            migrationBuilder.AddForeignKey(
                name: "fk_garagen_buchungskonten_ertragskonto_id",
                table: "garagen",
                column: "ertragskonto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_zaehler_set_allgemein_waerme_id",
                table: "hkvo",
                column: "allgemein_waerme_id",
                principalTable: "zaehler_set",
                principalColumn: "zaehler_id");

            migrationBuilder.AddForeignKey(
                name: "fk_transaktionen_bankkontos_zahler_bankkonto_id",
                table: "transaktionen",
                column: "zahler_bankkonto_id",
                principalTable: "bankkontos",
                principalColumn: "bankkonto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_transaktionen_bankkontos_zahlungsempfaenger_bankkonto_id",
                table: "transaktionen",
                column: "zahlungsempfaenger_bankkonto_id",
                principalTable: "bankkontos",
                principalColumn: "bankkonto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_mietminderungs_konto_buchungskonto",
                table: "vertraege",
                column: "mietminderungs_konto_buchungskonto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            // ── BK-Datenmigration: Buchungssätze für BK-Einträge ohne echten Buchungssatz ──
            migrationBuilder.Sql(@"
DO $$
DECLARE
    r RECORD;
    satz_id UUID;
BEGIN
    FOR r IN
        SELECT bk.betriebskostenrechnung_id, bk.betrag, bk.datum, bk.betreffendes_jahr,
               u.nk_verrechnungs_konto_id,
               'Betriebskosten ' || typ.bezeichnung || ' ' || bk.betreffendes_jahr AS beschr
        FROM betriebskostenrechnungen bk
        JOIN umlagen u ON u.umlage_id = bk.umlage_id
        JOIN umlagetypen typ ON typ.umlagetyp_id = u.umlagetyp_id
        WHERE bk.buchungssatz_id IS NULL
           OR bk.buchungssatz_id = '00000000-0000-0000-0000-000000000000'
    LOOP
        satz_id := gen_random_uuid();
        INSERT INTO buchungssaetze (buchungssatz_id, buchungsdatum, beschreibung, buchungsjahr, buchungsnummer, created_at, last_modified)
        VALUES (satz_id, r.datum, r.beschr, r.betreffendes_jahr, nextval('buchungsnummer_seq'), NOW(), NOW());
        INSERT INTO buchungszeilen (buchungszeile_id, buchungssatz_id, buchungskonto_id, soll_haben, betrag, created_at, last_modified)
        VALUES (gen_random_uuid(), satz_id, r.nk_verrechnungs_konto_id, 1, r.betrag, NOW(), NOW());
        UPDATE betriebskostenrechnungen
        SET buchungssatz_id = satz_id
        WHERE betriebskostenrechnung_id = r.betriebskostenrechnung_id;
    END LOOP;
END $$;
");

            // ── EA-Datenmigration: Buchungssätze für alle Erhaltungsaufwendungen ──
            migrationBuilder.Sql(@"
DO $$
DECLARE
    r RECORD;
    satz_id UUID;
    verbindl_id INT;
BEGIN
    FOR r IN
        SELECT ea.erhaltungsaufwendung_id, ea.betrag, ea.datum, ea.bezeichnung,
               ea.aussteller_kontakt_id AS aussteller_id,
               k.verbindlichkeits_konto_id,
               COALESCE(k.vorname || ' ', '') || k.name AS aussteller_name,
               w.aufwands_konto_id
        FROM erhaltungsaufwendungen ea
        JOIN wohnungen w ON w.wohnung_id = ea.wohnung_id
        JOIN kontakte k ON k.kontakt_id = ea.aussteller_kontakt_id
        WHERE ea.buchungssatz_id IS NULL
          AND ea.aussteller_kontakt_id IS NOT NULL
    LOOP
        -- Verbindlichkeitskonto für Aussteller anlegen falls fehlend
        verbindl_id := r.verbindlichkeits_konto_id;
        IF verbindl_id IS NULL THEN
            INSERT INTO buchungskonten (kontonummer, bezeichnung, kontotyp, created_at, last_modified)
            VALUES ('VK-' || r.aussteller_id, 'Verbindlichkeiten ' || r.aussteller_name, 1, NOW(), NOW())
            RETURNING buchungskonto_id INTO verbindl_id;
            UPDATE kontakte SET verbindlichkeits_konto_id = verbindl_id WHERE kontakt_id = r.aussteller_id;
        END IF;

        satz_id := gen_random_uuid();
        INSERT INTO buchungssaetze (buchungssatz_id, buchungsdatum, beschreibung, buchungsjahr, buchungsnummer, created_at, last_modified)
        VALUES (satz_id, r.datum, 'Erhaltungsaufwendung: ' || r.bezeichnung, EXTRACT(YEAR FROM r.datum)::int, nextval('buchungsnummer_seq'), NOW(), NOW());

        -- Soll: Aufwandskonto der Wohnung (soll_haben = 0)
        INSERT INTO buchungszeilen (buchungszeile_id, buchungssatz_id, buchungskonto_id, soll_haben, betrag, created_at, last_modified)
        VALUES (gen_random_uuid(), satz_id, r.aufwands_konto_id, 0, r.betrag, NOW(), NOW());
        -- Haben: Verbindlichkeitskonto des Ausstellers (soll_haben = 1)
        INSERT INTO buchungszeilen (buchungszeile_id, buchungssatz_id, buchungskonto_id, soll_haben, betrag, created_at, last_modified)
        VALUES (gen_random_uuid(), satz_id, verbindl_id, 1, r.betrag, NOW(), NOW());

        UPDATE erhaltungsaufwendungen SET buchungssatz_id = satz_id WHERE erhaltungsaufwendung_id = r.erhaltungsaufwendung_id;
    END LOOP;
END $$;
");

            // ── Miete-Datenmigration: Zahlungs-Buchungssätze für alle historischen Mietzahlungen ──
            migrationBuilder.Sql(@"
DO $$
DECLARE
    m RECORD;
    satz_id UUID;
BEGIN
    FOR m IN
        SELECT mi.miete_id, mi.zahlungsdatum, mi.betreffender_monat,
               ROUND(mi.betrag::numeric, 2) AS betrag,
               v.zahlungs_konto_id, v.miet_buchungskonto_id
        FROM mieten mi
        JOIN vertraege v ON v.vertrag_id = mi.vertrag_id
        WHERE v.zahlungs_konto_id IS NOT NULL
          AND v.miet_buchungskonto_id IS NOT NULL
    LOOP
        satz_id := gen_random_uuid();
        INSERT INTO buchungssaetze (buchungssatz_id, buchungsdatum, beschreibung, buchungsjahr, buchungsnummer, created_at, last_modified)
        VALUES (
            satz_id,
            m.zahlungsdatum,
            'Miete ' || TO_CHAR(m.betreffender_monat, 'YYYY-MM'),
            EXTRACT(YEAR FROM m.zahlungsdatum)::int,
            nextval('buchungsnummer_seq'),
            NOW(),
            NOW()
        );
        -- Soll: ZahlungsKonto (bank; soll_haben = 0)
        INSERT INTO buchungszeilen (buchungszeile_id, buchungssatz_id, buchungskonto_id, soll_haben, betrag, created_at, last_modified)
        VALUES (gen_random_uuid(), satz_id, m.zahlungs_konto_id, 0, m.betrag, NOW(), NOW());
        -- Haben: MietBuchungskonto (soll_haben = 1)
        INSERT INTO buchungszeilen (buchungszeile_id, buchungssatz_id, buchungskonto_id, soll_haben, betrag, created_at, last_modified)
        VALUES (gen_random_uuid(), satz_id, m.miet_buchungskonto_id, 1, m.betrag, NOW(), NOW());
    END LOOP;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_abrechnungsresultate_buchungssaetze_buchungssatz_id",
                table: "abrechnungsresultate");

            migrationBuilder.DropForeignKey(
                name: "fk_erhaltungsaufwendungen_buchungssaetze_buchungssatz_id",
                table: "erhaltungsaufwendungen");

            migrationBuilder.DropForeignKey(
                name: "fk_garagen_buchungskonten_ertragskonto_id",
                table: "garagen");

            migrationBuilder.DropForeignKey(
                name: "fk_hkvo_zaehler_set_allgemein_waerme_id",
                table: "hkvo");

            migrationBuilder.DropForeignKey(
                name: "fk_transaktionen_bankkontos_zahler_bankkonto_id",
                table: "transaktionen");

            migrationBuilder.DropForeignKey(
                name: "fk_transaktionen_bankkontos_zahlungsempfaenger_bankkonto_id",
                table: "transaktionen");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_mietminderungs_konto_buchungskonto",
                table: "vertraege");

            migrationBuilder.DropTable(
                name: "bankkonto_besitzer");

            migrationBuilder.DropTable(
                name: "garage_vertrag_mieter");

            migrationBuilder.DropTable(
                name: "garage_vertrag_versionen");

            migrationBuilder.DropTable(
                name: "kontakt_mitgliedschaften");

            migrationBuilder.DropTable(
                name: "offene_posten_ausgleiche");

            migrationBuilder.DropTable(
                name: "umlage_versionen");

            migrationBuilder.DropTable(
                name: "wohnung_eigentuemer");

            migrationBuilder.DropTable(
                name: "wohnung_versionen");

            migrationBuilder.DropTable(
                name: "bankkontos");

            migrationBuilder.DropTable(
                name: "garage_vertraege");

            migrationBuilder.DropIndex(
                name: "ix_transaktionen_zahler_bankkonto_id",
                table: "transaktionen");

            migrationBuilder.DropIndex(
                name: "ix_transaktionen_zahlungsempfaenger_bankkonto_id",
                table: "transaktionen");

            migrationBuilder.DropIndex(
                name: "ix_hkvo_heizkosten_id",
                table: "hkvo");

            migrationBuilder.DropIndex(
                name: "ix_garagen_ertragskonto_id",
                table: "garagen");

            migrationBuilder.DropIndex(
                name: "ix_erhaltungsaufwendungen_buchungssatz_id",
                table: "erhaltungsaufwendungen");

            migrationBuilder.DropIndex(
                name: "ix_buchungssaetze_buchungsjahr_buchungsnummer",
                table: "buchungssaetze");

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

            migrationBuilder.DropColumn(
                name: "betr_kv_nummer",
                table: "umlagetypen");

            migrationBuilder.DropColumn(
                name: "zahler_bankkonto_id",
                table: "transaktionen");

            migrationBuilder.DropColumn(
                name: "zahlungsempfaenger_bankkonto_id",
                table: "transaktionen");

            migrationBuilder.DropColumn(
                name: "beginn",
                table: "hkvo");

            migrationBuilder.DropColumn(
                name: "ertragskonto_id",
                table: "garagen");

            migrationBuilder.DropColumn(
                name: "buchungssatz_id",
                table: "erhaltungsaufwendungen");

            migrationBuilder.DropSequence(
                name: "buchungsnummer_seq");

            migrationBuilder.RenameColumn(
                name: "mietminderungs_konto_buchungskonto_id",
                table: "vertraege",
                newName: "mietminderungs_konto_id");

            migrationBuilder.RenameIndex(
                name: "ix_vertraege_mietminderungs_konto_buchungskonto_id",
                table: "vertraege",
                newName: "ix_vertraege_mietminderungs_konto_id");

            migrationBuilder.AddColumn<int>(
                name: "besitzer_kontakt_id",
                table: "wohnungen",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "miteigentumsanteile",
                table: "wohnungen",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "nutzeinheit",
                table: "wohnungen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "nutzflaeche",
                table: "wohnungen",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "wohnflaeche",
                table: "wohnungen",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "kautions_konto_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "schluessel",
                table: "umlagen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "zahler_kontakt_id",
                table: "transaktionen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "zahlungsempfaenger_kontakt_id",
                table: "transaktionen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "buchungsnummer",
                table: "buchungssaetze",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('buchungsnummer_seq')");

            migrationBuilder.AlterColumn<int>(
                name: "buchungsjahr",
                table: "buchungssaetze",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "is_abgeschlossen",
                table: "buchungssaetze",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "garage_vertrag",
                columns: table => new
                {
                    garagen_garage_id = table.Column<int>(type: "integer", nullable: false),
                    vertraege_vertrag_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garage_vertrag", x => new { x.garagen_garage_id, x.vertraege_vertrag_id });
                    table.ForeignKey(
                        name: "fk_garage_vertrag_garagen_garagen_garage_id",
                        column: x => x.garagen_garage_id,
                        principalTable: "garagen",
                        principalColumn: "garage_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_garage_vertrag_vertraege_vertraege_vertrag_id",
                        column: x => x.vertraege_vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kontakt_kontakt",
                columns: table => new
                {
                    juristische_personen_kontakt_id = table.Column<int>(type: "integer", nullable: false),
                    mitglieder_kontakt_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kontakt_kontakt", x => new { x.juristische_personen_kontakt_id, x.mitglieder_kontakt_id });
                    table.ForeignKey(
                        name: "fk_kontakt_kontakt_kontakte_juristische_personen_kontakt_id",
                        column: x => x.juristische_personen_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_kontakt_kontakt_kontakte_mitglieder_kontakt_id",
                        column: x => x.mitglieder_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kontos",
                columns: table => new
                {
                    konto_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    besitzer_kontakt_id = table.Column<int>(type: "integer", nullable: false),
                    bank = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    iban = table.Column<string>(type: "text", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kontos", x => x.konto_id);
                    table.ForeignKey(
                        name: "fk_kontos_kontakte_besitzer_kontakt_id",
                        column: x => x.besitzer_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_besitzer_kontakt_id",
                table: "wohnungen",
                column: "besitzer_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_kautions_konto_id",
                table: "vertraege",
                column: "kautions_konto_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaktionen_zahler_kontakt_id",
                table: "transaktionen",
                column: "zahler_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaktionen_zahlungsempfaenger_kontakt_id",
                table: "transaktionen",
                column: "zahlungsempfaenger_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_heizkosten_id",
                table: "hkvo",
                column: "heizkosten_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_buchungssaetze_buchungsjahr_buchungsnummer",
                table: "buchungssaetze",
                columns: new[] { "buchungsjahr", "buchungsnummer" },
                unique: true,
                filter: "buchungsnummer IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_garage_vertrag_vertraege_vertrag_id",
                table: "garage_vertrag",
                column: "vertraege_vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_kontakt_kontakt_mitglieder_kontakt_id",
                table: "kontakt_kontakt",
                column: "mitglieder_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_kontos_besitzer_kontakt_id",
                table: "kontos",
                column: "besitzer_kontakt_id");

            migrationBuilder.AddForeignKey(
                name: "fk_abrechnungsresultate_buchungssaetze_buchungssatz_id",
                table: "abrechnungsresultate",
                column: "buchungssatz_id",
                principalTable: "buchungssaetze",
                principalColumn: "buchungssatz_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_zaehler_set_allgemein_waerme_id",
                table: "hkvo",
                column: "allgemein_waerme_id",
                principalTable: "zaehler_set",
                principalColumn: "zaehler_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_transaktionen_kontakte_zahler_kontakt_id",
                table: "transaktionen",
                column: "zahler_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_transaktionen_kontakte_zahlungsempfaenger_kontakt_id",
                table: "transaktionen",
                column: "zahlungsempfaenger_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_kautions_konto_id",
                table: "vertraege",
                column: "kautions_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_mietminderungs_konto_id",
                table: "vertraege",
                column: "mietminderungs_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_wohnungen_kontakte_besitzer_kontakt_id",
                table: "wohnungen",
                column: "besitzer_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id");
        }
    }
}
