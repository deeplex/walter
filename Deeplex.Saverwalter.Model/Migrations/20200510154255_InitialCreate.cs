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
                    Stadt = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adressen", x => x.AdresseId);
                });

            migrationBuilder.CreateTable(
                name: "Allgemeinzaehler",
                columns: table => new
                {
                    AllgemeinzaehlerId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Beschreibung = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allgemeinzaehler", x => x.AllgemeinzaehlerId);
                });

            migrationBuilder.CreateTable(
                name: "JuristischePersonen",
                columns: table => new
                {
                    JuristischePersonId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bezeichnung = table.Column<string>(nullable: false)
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
                    Iban = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kontos", x => x.KontoId);
                });

            migrationBuilder.CreateTable(
                name: "KalteBetriebskosten",
                columns: table => new
                {
                    KalteBetriebskostenpunktId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bezeichnung = table.Column<int>(nullable: false),
                    AdresseId = table.Column<int>(nullable: false),
                    Beschreibung = table.Column<string>(nullable: true),
                    Schluessel = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KalteBetriebskosten", x => x.KalteBetriebskostenpunktId);
                    table.ForeignKey(
                        name: "FK_KalteBetriebskosten_Adressen_AdresseId",
                        column: x => x.AdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
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
                    AdresseId = table.Column<int>(nullable: true)
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
                name: "WarmeBetriebskostenRechnungen",
                columns: table => new
                {
                    WarmeBetriebskostenRechnungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllgemeinzaehlerId = table.Column<int>(nullable: false),
                    Jahr = table.Column<int>(nullable: false),
                    Betrag = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarmeBetriebskostenRechnungen", x => x.WarmeBetriebskostenRechnungId);
                    table.ForeignKey(
                        name: "FK_WarmeBetriebskostenRechnungen_Allgemeinzaehler_AllgemeinzaehlerId",
                        column: x => x.AllgemeinzaehlerId,
                        principalTable: "Allgemeinzaehler",
                        principalColumn: "AllgemeinzaehlerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Garagen",
                columns: table => new
                {
                    GarageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kennung = table.Column<string>(nullable: false),
                    BesitzerJuristischePersonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Garagen", x => x.GarageId);
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
                name: "KalteBetriebskostenRechnungen",
                columns: table => new
                {
                    KalteBetriebskostenRechnungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KalteBetriebskostenpunktId = table.Column<int>(nullable: false),
                    Jahr = table.Column<int>(nullable: false),
                    Betrag = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KalteBetriebskostenRechnungen", x => x.KalteBetriebskostenRechnungId);
                    table.ForeignKey(
                        name: "FK_KalteBetriebskostenRechnungen_KalteBetriebskosten_KalteBetriebskostenpunktId",
                        column: x => x.KalteBetriebskostenpunktId,
                        principalTable: "KalteBetriebskosten",
                        principalColumn: "KalteBetriebskostenpunktId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vertraege",
                columns: table => new
                {
                    rowid = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(nullable: false),
                    Version = table.Column<int>(nullable: false, defaultValue: 0)
                        .Annotation("Sqlite:Autoincrement", true),
                    WohnungId = table.Column<int>(nullable: true),
                    Personenzahl = table.Column<int>(nullable: false),
                    Beginn = table.Column<DateTime>(nullable: false),
                    Ende = table.Column<DateTime>(nullable: true),
                    AnsprechpartnerKontaktId = table.Column<int>(nullable: false)
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
                name: "Zaehlergemeinschaften",
                columns: table => new
                {
                    ZaehlergemeinschaftId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllgemeinzaehlerId = table.Column<int>(nullable: false),
                    WohnungId = table.Column<int>(nullable: false),
                    Typ = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zaehlergemeinschaften", x => x.ZaehlergemeinschaftId);
                    table.ForeignKey(
                        name: "FK_Zaehlergemeinschaften_Allgemeinzaehler_AllgemeinzaehlerId",
                        column: x => x.AllgemeinzaehlerId,
                        principalTable: "Allgemeinzaehler",
                        principalColumn: "AllgemeinzaehlerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Zaehlergemeinschaften_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZaehlerSet",
                columns: table => new
                {
                    ZaehlerId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WohnungId = table.Column<int>(nullable: false),
                    Typ = table.Column<int>(nullable: false)
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
                name: "MieterSet",
                columns: table => new
                {
                    MieterId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KontaktId = table.Column<int>(nullable: false),
                    VertragId = table.Column<int>(nullable: false)
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
                    table.ForeignKey(
                        name: "FK_MieterSet_Vertraege_VertragId",
                        column: x => x.VertragId,
                        principalTable: "Vertraege",
                        principalColumn: "rowid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MietobjektGaragen",
                columns: table => new
                {
                    MietobjektGarageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<int>(nullable: false),
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
                    table.ForeignKey(
                        name: "FK_MietobjektGaragen_Vertraege_VertragId",
                        column: x => x.VertragId,
                        principalTable: "Vertraege",
                        principalColumn: "rowid",
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
                    Stand = table.Column<double>(nullable: false)
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
                name: "IX_Garagen_BesitzerJuristischePersonId",
                table: "Garagen",
                column: "BesitzerJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_KalteBetriebskosten_AdresseId",
                table: "KalteBetriebskosten",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_KalteBetriebskostenRechnungen_KalteBetriebskostenpunktId",
                table: "KalteBetriebskostenRechnungen",
                column: "KalteBetriebskostenpunktId");

            migrationBuilder.CreateIndex(
                name: "IX_Kontakte_AdresseId",
                table: "Kontakte",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_MieterSet_KontaktId",
                table: "MieterSet",
                column: "KontaktId");

            migrationBuilder.CreateIndex(
                name: "IX_MieterSet_VertragId",
                table: "MieterSet",
                column: "VertragId");

            migrationBuilder.CreateIndex(
                name: "IX_MietobjektGaragen_GarageId",
                table: "MietobjektGaragen",
                column: "GarageId");

            migrationBuilder.CreateIndex(
                name: "IX_MietobjektGaragen_VertragId",
                table: "MietobjektGaragen",
                column: "VertragId");

            migrationBuilder.CreateIndex(
                name: "IX_Vertraege_AnsprechpartnerKontaktId",
                table: "Vertraege",
                column: "AnsprechpartnerKontaktId");

            migrationBuilder.CreateIndex(
                name: "IX_Vertraege_WohnungId",
                table: "Vertraege",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_WarmeBetriebskostenRechnungen_AllgemeinzaehlerId",
                table: "WarmeBetriebskostenRechnungen",
                column: "AllgemeinzaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_AdresseId",
                table: "Wohnungen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_BesitzerJuristischePersonId",
                table: "Wohnungen",
                column: "BesitzerJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Zaehlergemeinschaften_AllgemeinzaehlerId",
                table: "Zaehlergemeinschaften",
                column: "AllgemeinzaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Zaehlergemeinschaften_WohnungId",
                table: "Zaehlergemeinschaften",
                column: "WohnungId");

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
                name: "KalteBetriebskostenRechnungen");

            migrationBuilder.DropTable(
                name: "Kontos");

            migrationBuilder.DropTable(
                name: "MieterSet");

            migrationBuilder.DropTable(
                name: "MietobjektGaragen");

            migrationBuilder.DropTable(
                name: "WarmeBetriebskostenRechnungen");

            migrationBuilder.DropTable(
                name: "Zaehlergemeinschaften");

            migrationBuilder.DropTable(
                name: "Zaehlerstaende");

            migrationBuilder.DropTable(
                name: "KalteBetriebskosten");

            migrationBuilder.DropTable(
                name: "Garagen");

            migrationBuilder.DropTable(
                name: "Vertraege");

            migrationBuilder.DropTable(
                name: "Allgemeinzaehler");

            migrationBuilder.DropTable(
                name: "ZaehlerSet");

            migrationBuilder.DropTable(
                name: "Kontakte");

            migrationBuilder.DropTable(
                name: "Wohnungen");

            migrationBuilder.DropTable(
                name: "Adressen");

            migrationBuilder.DropTable(
                name: "JuristischePersonen");
        }
    }
}
