using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class Zahlungsmodell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "stand",
                table: "zaehlerstaende",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "wohnflaeche",
                table: "wohnungen",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "nutzflaeche",
                table: "wohnungen",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "miteigentumsanteile",
                table: "wohnungen",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "grundmiete",
                table: "vertrag_versionen",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<decimal>(
                name: "nebenkostenvorauszahlung",
                table: "vertrag_versionen",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "betrag",
                table: "transaktionen",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "minderung",
                table: "mietminderungen",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "strompauschale",
                table: "hkvo",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "hkvo_p8",
                table: "hkvo",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "hkvo_p7",
                table: "hkvo",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "betrag",
                table: "erhaltungsaufwendungen",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "betrag",
                table: "betriebskostenrechnungen",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "vorauszahlung",
                table: "abrechnungsresultate",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "saldo",
                table: "abrechnungsresultate",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "rechnungsbetrag",
                table: "abrechnungsresultate",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "minderung",
                table: "abrechnungsresultate",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "kaltmiete",
                table: "abrechnungsresultate",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.CreateTable(
                name: "buchungskonten",
                columns: table => new
                {
                    buchungskonto_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kontonummer = table.Column<string>(type: "text", nullable: false),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    kontotyp = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_buchungskonten", x => x.buchungskonto_id);
                });

            migrationBuilder.CreateTable(
                name: "buchungssaetze",
                columns: table => new
                {
                    buchungssatz_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buchungsdatum = table.Column<DateOnly>(type: "date", nullable: false),
                    beschreibung = table.Column<string>(type: "text", nullable: false),
                    buchungsnummer = table.Column<int>(type: "integer", nullable: true),
                    buchungsjahr = table.Column<int>(type: "integer", nullable: true),
                    is_abgeschlossen = table.Column<bool>(type: "boolean", nullable: false),
                    belegpfad = table.Column<string>(type: "text", nullable: true),
                    storno_von_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaktion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_buchungssaetze", x => x.buchungssatz_id);
                    table.ForeignKey(
                        name: "fk_buchungssaetze_buchungssaetze_storno_von_id",
                        column: x => x.storno_von_id,
                        principalTable: "buchungssaetze",
                        principalColumn: "buchungssatz_id");
                    table.ForeignKey(
                        name: "fk_buchungssaetze_transaktionen_transaktion_id",
                        column: x => x.transaktion_id,
                        principalTable: "transaktionen",
                        principalColumn: "transaktion_id");
                });

            migrationBuilder.CreateTable(
                name: "buchungszeilen",
                columns: table => new
                {
                    buchungszeile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buchungssatz_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buchungskonto_id = table.Column<int>(type: "integer", nullable: false),
                    soll_haben = table.Column<int>(type: "integer", nullable: false),
                    betrag = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_buchungszeilen", x => x.buchungszeile_id);
                    table.ForeignKey(
                        name: "fk_buchungszeilen_buchungskonten_buchungskonto_id",
                        column: x => x.buchungskonto_id,
                        principalTable: "buchungskonten",
                        principalColumn: "buchungskonto_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_buchungszeilen_buchungssaetze_buchungssatz_id",
                        column: x => x.buchungssatz_id,
                        principalTable: "buchungssaetze",
                        principalColumn: "buchungssatz_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_buchungssaetze_buchungsjahr_buchungsnummer",
                table: "buchungssaetze",
                columns: new[] { "buchungsjahr", "buchungsnummer" },
                unique: true,
                filter: "buchungsnummer IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_buchungssaetze_storno_von_id",
                table: "buchungssaetze",
                column: "storno_von_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_buchungssaetze_transaktion_id",
                table: "buchungssaetze",
                column: "transaktion_id");

            migrationBuilder.CreateIndex(
                name: "ix_buchungszeilen_buchungskonto_id",
                table: "buchungszeilen",
                column: "buchungskonto_id");

            migrationBuilder.CreateIndex(
                name: "ix_buchungszeilen_buchungssatz_id",
                table: "buchungszeilen",
                column: "buchungssatz_id");

            migrationBuilder.AddColumn<int>(
                name: "miet_buchungskonto_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "nk_buchungskonto_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "kautions_konto_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bk_abrechnungs_konto_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_miet_buchungskonto_id",
                table: "vertraege",
                column: "miet_buchungskonto_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_nk_buchungskonto_id",
                table: "vertraege",
                column: "nk_buchungskonto_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_kautions_konto_id",
                table: "vertraege",
                column: "kautions_konto_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_bk_abrechnungs_konto_id",
                table: "vertraege",
                column: "bk_abrechnungs_konto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_miet_buchungskonto_id",
                table: "vertraege",
                column: "miet_buchungskonto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_nk_buchungskonto_id",
                table: "vertraege",
                column: "nk_buchungskonto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_kautions_konto_id",
                table: "vertraege",
                column: "kautions_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_bk_abrechnungs_konto_id",
                table: "vertraege",
                column: "bk_abrechnungs_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<int>(
                name: "erhaltungsaufwands_konto_id",
                table: "wohnungen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "miet_ertragskonto_id",
                table: "wohnungen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_erhaltungsaufwands_konto_id",
                table: "wohnungen",
                column: "erhaltungsaufwands_konto_id");

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_miet_ertragskonto_id",
                table: "wohnungen",
                column: "miet_ertragskonto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_wohnungen_buchungskonten_erhaltungsaufwands_konto_id",
                table: "wohnungen",
                column: "erhaltungsaufwands_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_wohnungen_buchungskonten_miet_ertragskonto_id",
                table: "wohnungen",
                column: "miet_ertragskonto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<int>(
                name: "verbindlichkeits_konto_id",
                table: "kontakte",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_kontakte_verbindlichkeits_konto_id",
                table: "kontakte",
                column: "verbindlichkeits_konto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_kontakte_buchungskonten_verbindlichkeits_konto_id",
                table: "kontakte",
                column: "verbindlichkeits_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id");

            migrationBuilder.AddColumn<Guid>(
                name: "buchungssatz_id",
                table: "betriebskostenrechnungen",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_betriebskostenrechnungen_buchungssatz_id",
                table: "betriebskostenrechnungen",
                column: "buchungssatz_id");

            migrationBuilder.AddForeignKey(
                name: "fk_betriebskostenrechnungen_buchungssaetze_buchungssatz_id",
                table: "betriebskostenrechnungen",
                column: "buchungssatz_id",
                principalTable: "buchungssaetze",
                principalColumn: "buchungssatz_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<int>(
                name: "nk_verrechnungs_konto_id",
                table: "umlagen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_nk_verrechnungs_konto_id",
                table: "umlagen",
                column: "nk_verrechnungs_konto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_umlagen_buchungskonten_nk_verrechnungs_konto_id",
                table: "umlagen",
                column: "nk_verrechnungs_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<int>(
                name: "zahlungs_konto_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "zahlungs_konto_id",
                table: "umlagen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_zahlungs_konto_id",
                table: "vertraege",
                column: "zahlungs_konto_id");

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_zahlungs_konto_id",
                table: "umlagen",
                column: "zahlungs_konto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_buchungskonten_zahlungs_konto_id",
                table: "vertraege",
                column: "zahlungs_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_umlagen_buchungskonten_zahlungs_konto_id",
                table: "umlagen",
                column: "zahlungs_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_zahlungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_umlagen_buchungskonten_zahlungs_konto_id",
                table: "umlagen");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_zahlungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_umlagen_zahlungs_konto_id",
                table: "umlagen");

            migrationBuilder.DropColumn(
                name: "zahlungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "zahlungs_konto_id",
                table: "umlagen");

            migrationBuilder.DropForeignKey(
                name: "fk_umlagen_buchungskonten_nk_verrechnungs_konto_id",
                table: "umlagen");

            migrationBuilder.DropForeignKey(
                name: "fk_kontakte_buchungskonten_verbindlichkeits_konto_id",
                table: "kontakte");

            migrationBuilder.DropForeignKey(
                name: "fk_wohnungen_buchungskonten_erhaltungsaufwands_konto_id",
                table: "wohnungen");

            migrationBuilder.DropForeignKey(
                name: "fk_wohnungen_buchungskonten_miet_ertragskonto_id",
                table: "wohnungen");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_miet_buchungskonto_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_nk_buchungskonto_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_kautions_konto_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_buchungskonten_bk_abrechnungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_umlagen_nk_verrechnungs_konto_id",
                table: "umlagen");

            migrationBuilder.DropIndex(
                name: "ix_kontakte_verbindlichkeits_konto_id",
                table: "kontakte");

            migrationBuilder.DropIndex(
                name: "ix_wohnungen_erhaltungsaufwands_konto_id",
                table: "wohnungen");

            migrationBuilder.DropIndex(
                name: "ix_wohnungen_miet_ertragskonto_id",
                table: "wohnungen");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_miet_buchungskonto_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_nk_buchungskonto_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_kautions_konto_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_bk_abrechnungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "nk_verrechnungs_konto_id",
                table: "umlagen");

            migrationBuilder.DropColumn(
                name: "verbindlichkeits_konto_id",
                table: "kontakte");

            migrationBuilder.DropColumn(
                name: "erhaltungsaufwands_konto_id",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "miet_ertragskonto_id",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "miet_buchungskonto_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "nk_buchungskonto_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "kautions_konto_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "bk_abrechnungs_konto_id",
                table: "vertraege");

            migrationBuilder.DropTable(
                name: "buchungszeilen");

            migrationBuilder.DropTable(
                name: "buchungssaetze");

            migrationBuilder.DropTable(
                name: "buchungskonten");

            migrationBuilder.DropColumn(
                name: "nebenkostenvorauszahlung",
                table: "vertrag_versionen");

            migrationBuilder.AlterColumn<double>(
                name: "stand",
                table: "zaehlerstaende",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "wohnflaeche",
                table: "wohnungen",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "nutzflaeche",
                table: "wohnungen",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "miteigentumsanteile",
                table: "wohnungen",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "grundmiete",
                table: "vertrag_versionen",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "betrag",
                table: "transaktionen",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "minderung",
                table: "mietminderungen",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "strompauschale",
                table: "hkvo",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "hkvo_p8",
                table: "hkvo",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "hkvo_p7",
                table: "hkvo",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "betrag",
                table: "erhaltungsaufwendungen",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "betrag",
                table: "betriebskostenrechnungen",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.DropForeignKey(
                name: "fk_betriebskostenrechnungen_buchungssaetze_buchungssatz_id",
                table: "betriebskostenrechnungen");

            migrationBuilder.DropIndex(
                name: "ix_betriebskostenrechnungen_buchungssatz_id",
                table: "betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "buchungssatz_id",
                table: "betriebskostenrechnungen");

            migrationBuilder.AlterColumn<double>(
                name: "vorauszahlung",
                table: "abrechnungsresultate",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "saldo",
                table: "abrechnungsresultate",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "rechnungsbetrag",
                table: "abrechnungsresultate",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "minderung",
                table: "abrechnungsresultate",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "kaltmiete",
                table: "abrechnungsresultate",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
