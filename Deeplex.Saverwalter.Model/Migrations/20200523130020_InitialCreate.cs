using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adressen",
                columns: table => new
                {
                    AdresseId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hausnummer = table.Column<string>(nullable: false),
                    Strasse = table.Column<string>(nullable: false),
                    Postleitzahl = table.Column<string>(nullable: false),
                    Stadt = table.Column<string>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adressen", x => x.AdresseId);
                });

            migrationBuilder.CreateTable(
                name: "Betriebskostenrechnungen",
                columns: table => new
                {
                    BetriebskostenrechnungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Typ = table.Column<int>(nullable: false),
                    Betrag = table.Column<double>(nullable: false),
                    Datum = table.Column<DateTime>(nullable: false),
                    Schluessel = table.Column<int>(nullable: false),
                    Beschreibung = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Betriebskostenrechnungen", x => x.BetriebskostenrechnungId);
                });

            migrationBuilder.CreateTable(
                name: "JuristischePersonen",
                columns: table => new
                {
                    JuristischePersonId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bezeichnung = table.Column<string>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuristischePersonen", x => x.JuristischePersonId);
                });

            migrationBuilder.CreateTable(
                name: "Kontos",
                columns: table => new
                {
                    KontoId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bank = table.Column<string>(nullable: false),
                    Iban = table.Column<string>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kontos", x => x.KontoId);
                });

            migrationBuilder.CreateTable(
                name: "Mieten",
                columns: table => new
                {
                    MieteId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(nullable: false),
                    Datum = table.Column<DateTime>(nullable: false),
                    WarmMiete = table.Column<double>(nullable: true),
                    KaltMiete = table.Column<double>(nullable: true),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mieten", x => x.MieteId);
                });

            migrationBuilder.CreateTable(
                name: "Kontakte",
                columns: table => new
                {
                    KontaktId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vorname = table.Column<string>(nullable: true),
                    Nachname = table.Column<string>(nullable: false),
                    Anrede = table.Column<int>(nullable: false),
                    Telefon = table.Column<string>(nullable: true),
                    Mobil = table.Column<string>(nullable: true),
                    Fax = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    AdresseId = table.Column<int>(nullable: true),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kontakte", x => x.KontaktId);
                    table.ForeignKey(
                        name: "FK_Kontakte_Adressen_AdresseId",
                        column: x => x.AdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Garagen",
                columns: table => new
                {
                    GarageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdresseId = table.Column<int>(nullable: false),
                    Kennung = table.Column<string>(nullable: false),
                    BesitzerJuristischePersonId = table.Column<int>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Garagen", x => x.GarageId);
                    table.ForeignKey(
                        name: "FK_Garagen_Adressen_AdresseId",
                        column: x => x.AdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Garagen_JuristischePersonen_BesitzerJuristischePersonId",
                        column: x => x.BesitzerJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wohnungen",
                columns: table => new
                {
                    WohnungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bezeichnung = table.Column<string>(nullable: false),
                    Wohnflaeche = table.Column<double>(nullable: false),
                    Nutzflaeche = table.Column<double>(nullable: false),
                    Notiz = table.Column<string>(nullable: true),
                    BesitzerJuristischePersonId = table.Column<int>(nullable: false),
                    AdresseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wohnungen", x => x.WohnungId);
                    table.ForeignKey(
                        name: "FK_Wohnungen_Adressen_AdresseId",
                        column: x => x.AdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wohnungen_JuristischePersonen_BesitzerJuristischePersonId",
                        column: x => x.BesitzerJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JuristischePersonenMitglied",
                columns: table => new
                {
                    JuristischePersonenMitgliedId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KontaktId = table.Column<int>(nullable: false),
                    JuristischePersonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuristischePersonenMitglied", x => x.JuristischePersonenMitgliedId);
                    table.ForeignKey(
                        name: "FK_JuristischePersonenMitglied_JuristischePersonen_JuristischePersonId",
                        column: x => x.JuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JuristischePersonenMitglied_Kontakte_KontaktId",
                        column: x => x.KontaktId,
                        principalTable: "Kontakte",
                        principalColumn: "KontaktId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MieterSet",
                columns: table => new
                {
                    MieterId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KontaktId = table.Column<int>(nullable: false),
                    VertragId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MieterSet", x => x.MieterId);
                    table.ForeignKey(
                        name: "FK_MieterSet_Kontakte_KontaktId",
                        column: x => x.KontaktId,
                        principalTable: "Kontakte",
                        principalColumn: "KontaktId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MietobjektGaragen",
                columns: table => new
                {
                    MietobjektGarageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(nullable: false),
                    GarageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MietobjektGaragen", x => x.MietobjektGarageId);
                    table.ForeignKey(
                        name: "FK_MietobjektGaragen_Garagen_GarageId",
                        column: x => x.GarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Betriebskostenrechnungsgruppen",
                columns: table => new
                {
                    BetriebskostenrechnungsgruppeId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WohnungId = table.Column<int>(nullable: false),
                    RechnungBetriebskostenrechnungId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Betriebskostenrechnungsgruppen", x => x.BetriebskostenrechnungsgruppeId);
                    table.ForeignKey(
                        name: "FK_Betriebskostenrechnungsgruppen_Betriebskostenrechnungen_RechnungBetriebskostenrechnungId",
                        column: x => x.RechnungBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Betriebskostenrechnungsgruppen_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vertraege",
                columns: table => new
                {
                    rowid = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(nullable: false),
                    Version = table.Column<int>(nullable: false),
                    WohnungId = table.Column<int>(nullable: true),
                    Personenzahl = table.Column<int>(nullable: false),
                    Beginn = table.Column<DateTime>(nullable: false),
                    Ende = table.Column<DateTime>(nullable: true),
                    AnsprechpartnerKontaktId = table.Column<int>(nullable: false),
                    VersionsNotiz = table.Column<string>(nullable: true),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vertraege", x => x.rowid);
                    table.UniqueConstraint("AK_Vertraege_VertragId_Version", x => new { x.VertragId, x.Version });
                    table.ForeignKey(
                        name: "FK_Vertraege_Kontakte_AnsprechpartnerKontaktId",
                        column: x => x.AnsprechpartnerKontaktId,
                        principalTable: "Kontakte",
                        principalColumn: "KontaktId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vertraege_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZaehlerSet",
                columns: table => new
                {
                    ZaehlerId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kennnummer = table.Column<string>(nullable: false),
                    WohnungId = table.Column<int>(nullable: false),
                    Typ = table.Column<int>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaehlerSet", x => x.ZaehlerId);
                    table.ForeignKey(
                        name: "FK_ZaehlerSet_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zaehlerstaende",
                columns: table => new
                {
                    ZaehlerstandId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZaehlerId = table.Column<int>(nullable: false),
                    Datum = table.Column<DateTime>(nullable: false),
                    Stand = table.Column<double>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zaehlerstaende", x => x.ZaehlerstandId);
                    table.ForeignKey(
                        name: "FK_Zaehlerstaende_ZaehlerSet_ZaehlerId",
                        column: x => x.ZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungsgruppen_RechnungBetriebskostenrechnungId",
                table: "Betriebskostenrechnungsgruppen",
                column: "RechnungBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungsgruppen_WohnungId",
                table: "Betriebskostenrechnungsgruppen",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Garagen_AdresseId",
                table: "Garagen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Garagen_BesitzerJuristischePersonId",
                table: "Garagen",
                column: "BesitzerJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonenMitglied_JuristischePersonId",
                table: "JuristischePersonenMitglied",
                column: "JuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonenMitglied_KontaktId",
                table: "JuristischePersonenMitglied",
                column: "KontaktId");

            migrationBuilder.CreateIndex(
                name: "IX_Kontakte_AdresseId",
                table: "Kontakte",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_MieterSet_KontaktId",
                table: "MieterSet",
                column: "KontaktId");

            migrationBuilder.CreateIndex(
                name: "IX_MietobjektGaragen_GarageId",
                table: "MietobjektGaragen",
                column: "GarageId");

            migrationBuilder.CreateIndex(
                name: "IX_Vertraege_AnsprechpartnerKontaktId",
                table: "Vertraege",
                column: "AnsprechpartnerKontaktId");

            migrationBuilder.CreateIndex(
                name: "IX_Vertraege_WohnungId",
                table: "Vertraege",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_AdresseId",
                table: "Wohnungen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_BesitzerJuristischePersonId",
                table: "Wohnungen",
                column: "BesitzerJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerSet_WohnungId",
                table: "ZaehlerSet",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Zaehlerstaende_ZaehlerId",
                table: "Zaehlerstaende",
                column: "ZaehlerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Betriebskostenrechnungsgruppen");

            migrationBuilder.DropTable(
                name: "JuristischePersonenMitglied");

            migrationBuilder.DropTable(
                name: "Kontos");

            migrationBuilder.DropTable(
                name: "Mieten");

            migrationBuilder.DropTable(
                name: "MieterSet");

            migrationBuilder.DropTable(
                name: "MietobjektGaragen");

            migrationBuilder.DropTable(
                name: "Vertraege");

            migrationBuilder.DropTable(
                name: "Zaehlerstaende");

            migrationBuilder.DropTable(
                name: "Betriebskostenrechnungen");

            migrationBuilder.DropTable(
                name: "Garagen");

            migrationBuilder.DropTable(
                name: "Kontakte");

            migrationBuilder.DropTable(
                name: "ZaehlerSet");

            migrationBuilder.DropTable(
                name: "Wohnungen");

            migrationBuilder.DropTable(
                name: "Adressen");

            migrationBuilder.DropTable(
                name: "JuristischePersonen");
        }
    }
}
