using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class RemoveDeprecatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erhaltungsaufwendungen");

            migrationBuilder.DropTable(
                name: "mieten");

            migrationBuilder.DropTable(
                name: "vertrags_betriebskostenrechnung");

            migrationBuilder.DropTable(
                name: "betriebskostenrechnungen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "betriebskostenrechnungen",
                columns: table => new
                {
                    betriebskostenrechnung_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    buchungssatz_id = table.Column<Guid>(type: "uuid", nullable: false),
                    umlage_id = table.Column<int>(type: "integer", nullable: false),
                    betrag = table.Column<decimal>(type: "numeric", nullable: false),
                    betreffendes_jahr = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_betriebskostenrechnungen", x => x.betriebskostenrechnung_id);
                    table.ForeignKey(
                        name: "fk_betriebskostenrechnungen_buchungssaetze_buchungssatz_id",
                        column: x => x.buchungssatz_id,
                        principalTable: "buchungssaetze",
                        principalColumn: "buchungssatz_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_betriebskostenrechnungen_umlagen_umlage_id",
                        column: x => x.umlage_id,
                        principalTable: "umlagen",
                        principalColumn: "umlage_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erhaltungsaufwendungen",
                columns: table => new
                {
                    erhaltungsaufwendung_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aussteller_kontakt_id = table.Column<int>(type: "integer", nullable: false),
                    buchungssatz_id = table.Column<Guid>(type: "uuid", nullable: true),
                    wohnung_id = table.Column<int>(type: "integer", nullable: false),
                    betrag = table.Column<decimal>(type: "numeric", nullable: false),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_erhaltungsaufwendungen", x => x.erhaltungsaufwendung_id);
                    table.ForeignKey(
                        name: "fk_erhaltungsaufwendungen_buchungssaetze_buchungssatz_id",
                        column: x => x.buchungssatz_id,
                        principalTable: "buchungssaetze",
                        principalColumn: "buchungssatz_id");
                    table.ForeignKey(
                        name: "fk_erhaltungsaufwendungen_kontakte_aussteller_kontakt_id",
                        column: x => x.aussteller_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_erhaltungsaufwendungen_wohnungen_wohnung_id",
                        column: x => x.wohnung_id,
                        principalTable: "wohnungen",
                        principalColumn: "wohnung_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mieten",
                columns: table => new
                {
                    miete_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    betrag = table.Column<double>(type: "double precision", nullable: false),
                    betreffender_monat = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    zahlungsdatum = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mieten", x => x.miete_id);
                    table.ForeignKey(
                        name: "fk_mieten_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vertrags_betriebskostenrechnung",
                columns: table => new
                {
                    vertrags_betriebskostenrechnung_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rechnung_betriebskostenrechnung_id = table.Column<int>(type: "integer", nullable: false),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vertrags_betriebskostenrechnung", x => x.vertrags_betriebskostenrechnung_id);
                    table.ForeignKey(
                        name: "fk_vertrags_betriebskostenrechnung_betriebskostenrechnungen_re",
                        column: x => x.rechnung_betriebskostenrechnung_id,
                        principalTable: "betriebskostenrechnungen",
                        principalColumn: "betriebskostenrechnung_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vertrags_betriebskostenrechnung_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_betriebskostenrechnungen_buchungssatz_id",
                table: "betriebskostenrechnungen",
                column: "buchungssatz_id");

            migrationBuilder.CreateIndex(
                name: "ix_betriebskostenrechnungen_umlage_id",
                table: "betriebskostenrechnungen",
                column: "umlage_id");

            migrationBuilder.CreateIndex(
                name: "ix_erhaltungsaufwendungen_aussteller_kontakt_id",
                table: "erhaltungsaufwendungen",
                column: "aussteller_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_erhaltungsaufwendungen_buchungssatz_id",
                table: "erhaltungsaufwendungen",
                column: "buchungssatz_id");

            migrationBuilder.CreateIndex(
                name: "ix_erhaltungsaufwendungen_wohnung_id",
                table: "erhaltungsaufwendungen",
                column: "wohnung_id");

            migrationBuilder.CreateIndex(
                name: "ix_mieten_vertrag_id",
                table: "mieten",
                column: "vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertrags_betriebskostenrechnung_rechnung_betriebskostenrech",
                table: "vertrags_betriebskostenrechnung",
                column: "rechnung_betriebskostenrechnung_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertrags_betriebskostenrechnung_vertrag_id",
                table: "vertrags_betriebskostenrechnung",
                column: "vertrag_id");
        }
    }
}
