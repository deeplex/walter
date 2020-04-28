using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Garagen",
                columns: table => new
                {
                    GarageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Garagen", x => x.GarageId);
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
                name: "KalteBetriebskosten",
                columns: table => new
                {
                    KalteBetriebskostenpunktId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bezeichnung = table.Column<string>(nullable: false),
                    Beschreibung = table.Column<string>(nullable: true),
                    Schluessel = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KalteBetriebskosten", x => x.KalteBetriebskostenpunktId);
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
                name: "Staedte",
                columns: table => new
                {
                    StadtId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staedte", x => x.StadtId);
                });

            migrationBuilder.CreateTable(
                name: "Postleitzahlen",
                columns: table => new
                {
                    PostleitzahlId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bezeichnung = table.Column<string>(nullable: false),
                    StadtId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postleitzahlen", x => x.PostleitzahlId);
                    table.ForeignKey(
                        name: "FK_Postleitzahlen_Staedte_StadtId",
                        column: x => x.StadtId,
                        principalTable: "Staedte",
                        principalColumn: "StadtId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strassen",
                columns: table => new
                {
                    StrasseId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    PostleitzahlId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strassen", x => x.StrasseId);
                    table.ForeignKey(
                        name: "FK_Strassen_Postleitzahlen_PostleitzahlId",
                        column: x => x.PostleitzahlId,
                        principalTable: "Postleitzahlen",
                        principalColumn: "PostleitzahlId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adressen",
                columns: table => new
                {
                    AdresseId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hausnummer = table.Column<string>(nullable: false),
                    StrasseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adressen", x => x.AdresseId);
                    table.ForeignKey(
                        name: "FK_Adressen_Strassen_StrasseId",
                        column: x => x.StrasseId,
                        principalTable: "Strassen",
                        principalColumn: "StrasseId",
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
                name: "Wohnungen",
                columns: table => new
                {
                    WohnungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bezeichnung = table.Column<string>(nullable: false),
                    Wohnflaeche = table.Column<double>(nullable: false),
                    Nutzflaeche = table.Column<double>(nullable: false),
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
                    VermieterJuristischePersonId = table.Column<int>(nullable: false),
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
                        name: "FK_Vertraege_JuristischePersonen_VermieterJuristischePersonId",
                        column: x => x.VermieterJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
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
                name: "MietobjektWohnungen",
                columns: table => new
                {
                    MietobjektWohnungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<int>(nullable: false),
                    WohnungId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MietobjektWohnungen", x => x.MietobjektWohnungId);
                    table.ForeignKey(
                        name: "FK_MietobjektWohnungen_Vertraege_VertragId",
                        column: x => x.VertragId,
                        principalTable: "Vertraege",
                        principalColumn: "rowid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MietobjektWohnungen_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adressen_StrasseId",
                table: "Adressen",
                column: "StrasseId");

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
                name: "IX_MietobjektWohnungen_VertragId",
                table: "MietobjektWohnungen",
                column: "VertragId");

            migrationBuilder.CreateIndex(
                name: "IX_MietobjektWohnungen_WohnungId",
                table: "MietobjektWohnungen",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Postleitzahlen_StadtId",
                table: "Postleitzahlen",
                column: "StadtId");

            migrationBuilder.CreateIndex(
                name: "IX_Strassen_PostleitzahlId",
                table: "Strassen",
                column: "PostleitzahlId");

            migrationBuilder.CreateIndex(
                name: "IX_Vertraege_AnsprechpartnerKontaktId",
                table: "Vertraege",
                column: "AnsprechpartnerKontaktId");

            migrationBuilder.CreateIndex(
                name: "IX_Vertraege_VermieterJuristischePersonId",
                table: "Vertraege",
                column: "VermieterJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_AdresseId",
                table: "Wohnungen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerSet_WohnungId",
                table: "ZaehlerSet",
                column: "WohnungId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KalteBetriebskosten");

            migrationBuilder.DropTable(
                name: "Kontos");

            migrationBuilder.DropTable(
                name: "MieterSet");

            migrationBuilder.DropTable(
                name: "MietobjektGaragen");

            migrationBuilder.DropTable(
                name: "MietobjektWohnungen");

            migrationBuilder.DropTable(
                name: "ZaehlerSet");

            migrationBuilder.DropTable(
                name: "Garagen");

            migrationBuilder.DropTable(
                name: "Vertraege");

            migrationBuilder.DropTable(
                name: "Wohnungen");

            migrationBuilder.DropTable(
                name: "Kontakte");

            migrationBuilder.DropTable(
                name: "JuristischePersonen");

            migrationBuilder.DropTable(
                name: "Adressen");

            migrationBuilder.DropTable(
                name: "Strassen");

            migrationBuilder.DropTable(
                name: "Postleitzahlen");

            migrationBuilder.DropTable(
                name: "Staedte");
        }
    }
}
