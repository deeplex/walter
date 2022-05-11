using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class RefactorAnhang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdresseAnhaenge");

            migrationBuilder.DropTable(
                name: "BetriebskostenrechnungAnhaenge");

            migrationBuilder.DropTable(
                name: "ErhaltungsaufwendungAnhaenge");

            migrationBuilder.DropTable(
                name: "GarageAnhaenge");

            migrationBuilder.DropTable(
                name: "JuristischePersonAnhaenge");

            migrationBuilder.DropTable(
                name: "KontoAnhaenge");

            migrationBuilder.DropTable(
                name: "MieteAnhaenge");

            migrationBuilder.DropTable(
                name: "MietMinderungAnhaenge");

            migrationBuilder.DropTable(
                name: "NatuerlichePersonAnhaenge");

            migrationBuilder.DropTable(
                name: "VertragAnhaenge");

            migrationBuilder.DropTable(
                name: "WohnungAnhaenge");

            migrationBuilder.DropTable(
                name: "ZaehlerAnhaenge");

            migrationBuilder.DropTable(
                name: "ZaehlerstandAnhaenge");

            migrationBuilder.CreateTable(
                name: "AdresseAnhang",
                columns: table => new
                {
                    AdressenAdresseId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdresseAnhang", x => new { x.AdressenAdresseId, x.AnhaengeAnhangId });
                    table.ForeignKey(
                        name: "FK_AdresseAnhang_Adressen_AdressenAdresseId",
                        column: x => x.AdressenAdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdresseAnhang_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangBetriebskostenrechnung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BetriebskostenrechnungenBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangBetriebskostenrechnung", x => new { x.AnhaengeAnhangId, x.BetriebskostenrechnungenBetriebskostenrechnungId });
                    table.ForeignKey(
                        name: "FK_AnhangBetriebskostenrechnung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangBetriebskostenrechnung_Betriebskostenrechnungen_BetriebskostenrechnungenBetriebskostenrechnungId",
                        column: x => x.BetriebskostenrechnungenBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangErhaltungsaufwendung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ErhaltungsaufwendungenErhaltungsaufwendungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangErhaltungsaufwendung", x => new { x.AnhaengeAnhangId, x.ErhaltungsaufwendungenErhaltungsaufwendungId });
                    table.ForeignKey(
                        name: "FK_AnhangErhaltungsaufwendung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangErhaltungsaufwendung_Erhaltungsaufwendungen_ErhaltungsaufwendungenErhaltungsaufwendungId",
                        column: x => x.ErhaltungsaufwendungenErhaltungsaufwendungId,
                        principalTable: "Erhaltungsaufwendungen",
                        principalColumn: "ErhaltungsaufwendungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangGarage",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GaragenGarageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangGarage", x => new { x.AnhaengeAnhangId, x.GaragenGarageId });
                    table.ForeignKey(
                        name: "FK_AnhangGarage_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangGarage_Garagen_GaragenGarageId",
                        column: x => x.GaragenGarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangJuristischePerson",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JuristischePersonenJuristischePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangJuristischePerson", x => new { x.AnhaengeAnhangId, x.JuristischePersonenJuristischePersonId });
                    table.ForeignKey(
                        name: "FK_AnhangJuristischePerson_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangJuristischePerson_JuristischePersonen_JuristischePersonenJuristischePersonId",
                        column: x => x.JuristischePersonenJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangKonto",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KontenKontoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangKonto", x => new { x.AnhaengeAnhangId, x.KontenKontoId });
                    table.ForeignKey(
                        name: "FK_AnhangKonto_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangKonto_Kontos_KontenKontoId",
                        column: x => x.KontenKontoId,
                        principalTable: "Kontos",
                        principalColumn: "KontoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangMiete",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MietenMieteId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangMiete", x => new { x.AnhaengeAnhangId, x.MietenMieteId });
                    table.ForeignKey(
                        name: "FK_AnhangMiete_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangMiete_Mieten_MietenMieteId",
                        column: x => x.MietenMieteId,
                        principalTable: "Mieten",
                        principalColumn: "MieteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangMietMinderung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MietminderungenMietMinderungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangMietMinderung", x => new { x.AnhaengeAnhangId, x.MietminderungenMietMinderungId });
                    table.ForeignKey(
                        name: "FK_AnhangMietMinderung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangMietMinderung_MietMinderungen_MietminderungenMietMinderungId",
                        column: x => x.MietminderungenMietMinderungId,
                        principalTable: "MietMinderungen",
                        principalColumn: "MietMinderungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangNatuerlichePerson",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NatuerlichePersonenNatuerlichePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangNatuerlichePerson", x => new { x.AnhaengeAnhangId, x.NatuerlichePersonenNatuerlichePersonId });
                    table.ForeignKey(
                        name: "FK_AnhangNatuerlichePerson_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangNatuerlichePerson_NatuerlichePersonen_NatuerlichePersonenNatuerlichePersonId",
                        column: x => x.NatuerlichePersonenNatuerlichePersonId,
                        principalTable: "NatuerlichePersonen",
                        principalColumn: "NatuerlichePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangVertrag",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Vertraegerowid = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangVertrag", x => new { x.AnhaengeAnhangId, x.Vertraegerowid });
                    table.ForeignKey(
                        name: "FK_AnhangVertrag_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangVertrag_Vertraege_Vertraegerowid",
                        column: x => x.Vertraegerowid,
                        principalTable: "Vertraege",
                        principalColumn: "rowid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangWohnung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WohnungenWohnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangWohnung", x => new { x.AnhaengeAnhangId, x.WohnungenWohnungId });
                    table.ForeignKey(
                        name: "FK_AnhangWohnung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangWohnung_Wohnungen_WohnungenWohnungId",
                        column: x => x.WohnungenWohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangZaehler",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ZaehlerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangZaehler", x => new { x.AnhaengeAnhangId, x.ZaehlerId });
                    table.ForeignKey(
                        name: "FK_AnhangZaehler_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangZaehler_ZaehlerSet_ZaehlerId",
                        column: x => x.ZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangZaehlerstand",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ZaehlerstaendeZaehlerstandId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangZaehlerstand", x => new { x.AnhaengeAnhangId, x.ZaehlerstaendeZaehlerstandId });
                    table.ForeignKey(
                        name: "FK_AnhangZaehlerstand_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangZaehlerstand_Zaehlerstaende_ZaehlerstaendeZaehlerstandId",
                        column: x => x.ZaehlerstaendeZaehlerstandId,
                        principalTable: "Zaehlerstaende",
                        principalColumn: "ZaehlerstandId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VertragsBetriebskostenrechnung",
                columns: table => new
                {
                    VertragsBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RechnungBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VertragsBetriebskostenrechnung", x => x.VertragsBetriebskostenrechnungId);
                    table.ForeignKey(
                        name: "FK_VertragsBetriebskostenrechnung_Betriebskostenrechnungen_RechnungBetriebskostenrechnungId",
                        column: x => x.RechnungBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnhangVertragsBetriebskostenrechnung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangVertragsBetriebskostenrechnung", x => new { x.AnhaengeAnhangId, x.VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId });
                    table.ForeignKey(
                        name: "FK_AnhangVertragsBetriebskostenrechnung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangVertragsBetriebskostenrechnung_VertragsBetriebskostenrechnung_VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId",
                        column: x => x.VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId,
                        principalTable: "VertragsBetriebskostenrechnung",
                        principalColumn: "VertragsBetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdresseAnhang_AnhaengeAnhangId",
                table: "AdresseAnhang",
                column: "AnhaengeAnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangBetriebskostenrechnung_BetriebskostenrechnungenBetriebskostenrechnungId",
                table: "AnhangBetriebskostenrechnung",
                column: "BetriebskostenrechnungenBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangErhaltungsaufwendung_ErhaltungsaufwendungenErhaltungsaufwendungId",
                table: "AnhangErhaltungsaufwendung",
                column: "ErhaltungsaufwendungenErhaltungsaufwendungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangGarage_GaragenGarageId",
                table: "AnhangGarage",
                column: "GaragenGarageId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangJuristischePerson_JuristischePersonenJuristischePersonId",
                table: "AnhangJuristischePerson",
                column: "JuristischePersonenJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangKonto_KontenKontoId",
                table: "AnhangKonto",
                column: "KontenKontoId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangMiete_MietenMieteId",
                table: "AnhangMiete",
                column: "MietenMieteId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangMietMinderung_MietminderungenMietMinderungId",
                table: "AnhangMietMinderung",
                column: "MietminderungenMietMinderungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangNatuerlichePerson_NatuerlichePersonenNatuerlichePersonId",
                table: "AnhangNatuerlichePerson",
                column: "NatuerlichePersonenNatuerlichePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangVertrag_Vertraegerowid",
                table: "AnhangVertrag",
                column: "Vertraegerowid");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangVertragsBetriebskostenrechnung_VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId",
                table: "AnhangVertragsBetriebskostenrechnung",
                column: "VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangWohnung_WohnungenWohnungId",
                table: "AnhangWohnung",
                column: "WohnungenWohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangZaehler_ZaehlerId",
                table: "AnhangZaehler",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangZaehlerstand_ZaehlerstaendeZaehlerstandId",
                table: "AnhangZaehlerstand",
                column: "ZaehlerstaendeZaehlerstandId");

            migrationBuilder.CreateIndex(
                name: "IX_VertragsBetriebskostenrechnung_RechnungBetriebskostenrechnungId",
                table: "VertragsBetriebskostenrechnung",
                column: "RechnungBetriebskostenrechnungId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdresseAnhang");

            migrationBuilder.DropTable(
                name: "AnhangBetriebskostenrechnung");

            migrationBuilder.DropTable(
                name: "AnhangErhaltungsaufwendung");

            migrationBuilder.DropTable(
                name: "AnhangGarage");

            migrationBuilder.DropTable(
                name: "AnhangJuristischePerson");

            migrationBuilder.DropTable(
                name: "AnhangKonto");

            migrationBuilder.DropTable(
                name: "AnhangMiete");

            migrationBuilder.DropTable(
                name: "AnhangMietMinderung");

            migrationBuilder.DropTable(
                name: "AnhangNatuerlichePerson");

            migrationBuilder.DropTable(
                name: "AnhangVertrag");

            migrationBuilder.DropTable(
                name: "AnhangVertragsBetriebskostenrechnung");

            migrationBuilder.DropTable(
                name: "AnhangWohnung");

            migrationBuilder.DropTable(
                name: "AnhangZaehler");

            migrationBuilder.DropTable(
                name: "AnhangZaehlerstand");

            migrationBuilder.DropTable(
                name: "VertragsBetriebskostenrechnung");

            migrationBuilder.CreateTable(
                name: "AdresseAnhaenge",
                columns: table => new
                {
                    AdresseAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetAdresseId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdresseAnhaenge", x => x.AdresseAnhangId);
                    table.ForeignKey(
                        name: "FK_AdresseAnhaenge_Adressen_TargetAdresseId",
                        column: x => x.TargetAdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdresseAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BetriebskostenrechnungAnhaenge",
                columns: table => new
                {
                    BetriebskostenrechnungAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetriebskostenrechnungAnhaenge", x => x.BetriebskostenrechnungAnhangId);
                    table.ForeignKey(
                        name: "FK_BetriebskostenrechnungAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BetriebskostenrechnungAnhaenge_Betriebskostenrechnungen_TargetBetriebskostenrechnungId",
                        column: x => x.TargetBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErhaltungsaufwendungAnhaenge",
                columns: table => new
                {
                    ErhaltungsaufwendungAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetErhaltungsaufwendungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErhaltungsaufwendungAnhaenge", x => x.ErhaltungsaufwendungAnhangId);
                    table.ForeignKey(
                        name: "FK_ErhaltungsaufwendungAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ErhaltungsaufwendungAnhaenge_Erhaltungsaufwendungen_TargetErhaltungsaufwendungId",
                        column: x => x.TargetErhaltungsaufwendungId,
                        principalTable: "Erhaltungsaufwendungen",
                        principalColumn: "ErhaltungsaufwendungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GarageAnhaenge",
                columns: table => new
                {
                    GarageAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetGarageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarageAnhaenge", x => x.GarageAnhangId);
                    table.ForeignKey(
                        name: "FK_GarageAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarageAnhaenge_Garagen_TargetGarageId",
                        column: x => x.TargetGarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JuristischePersonAnhaenge",
                columns: table => new
                {
                    JuristischePersonAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetJuristischePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuristischePersonAnhaenge", x => x.JuristischePersonAnhangId);
                    table.ForeignKey(
                        name: "FK_JuristischePersonAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JuristischePersonAnhaenge_JuristischePersonen_TargetJuristischePersonId",
                        column: x => x.TargetJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KontoAnhaenge",
                columns: table => new
                {
                    KontoAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetKontoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KontoAnhaenge", x => x.KontoAnhangId);
                    table.ForeignKey(
                        name: "FK_KontoAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KontoAnhaenge_Kontos_TargetKontoId",
                        column: x => x.TargetKontoId,
                        principalTable: "Kontos",
                        principalColumn: "KontoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MieteAnhaenge",
                columns: table => new
                {
                    MieteAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetMieteId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MieteAnhaenge", x => x.MieteAnhangId);
                    table.ForeignKey(
                        name: "FK_MieteAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MieteAnhaenge_Mieten_TargetMieteId",
                        column: x => x.TargetMieteId,
                        principalTable: "Mieten",
                        principalColumn: "MieteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MietMinderungAnhaenge",
                columns: table => new
                {
                    MietMinderungAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetMietMinderungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MietMinderungAnhaenge", x => x.MietMinderungAnhangId);
                    table.ForeignKey(
                        name: "FK_MietMinderungAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MietMinderungAnhaenge_MietMinderungen_TargetMietMinderungId",
                        column: x => x.TargetMietMinderungId,
                        principalTable: "MietMinderungen",
                        principalColumn: "MietMinderungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NatuerlichePersonAnhaenge",
                columns: table => new
                {
                    NatuerlichePersonAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetNatuerlichePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NatuerlichePersonAnhaenge", x => x.NatuerlichePersonAnhangId);
                    table.ForeignKey(
                        name: "FK_NatuerlichePersonAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NatuerlichePersonAnhaenge_NatuerlichePersonen_TargetNatuerlichePersonId",
                        column: x => x.TargetNatuerlichePersonId,
                        principalTable: "NatuerlichePersonen",
                        principalColumn: "NatuerlichePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VertragAnhaenge",
                columns: table => new
                {
                    VertragAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Target = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VertragAnhaenge", x => x.VertragAnhangId);
                    table.ForeignKey(
                        name: "FK_VertragAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WohnungAnhaenge",
                columns: table => new
                {
                    WohnungAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetWohnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WohnungAnhaenge", x => x.WohnungAnhangId);
                    table.ForeignKey(
                        name: "FK_WohnungAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WohnungAnhaenge_Wohnungen_TargetWohnungId",
                        column: x => x.TargetWohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZaehlerAnhaenge",
                columns: table => new
                {
                    ZaehlerAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetZaehlerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaehlerAnhaenge", x => x.ZaehlerAnhangId);
                    table.ForeignKey(
                        name: "FK_ZaehlerAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZaehlerAnhaenge_ZaehlerSet_TargetZaehlerId",
                        column: x => x.TargetZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZaehlerstandAnhaenge",
                columns: table => new
                {
                    ZaehlerstandAnhangId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetZaehlerstandId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaehlerstandAnhaenge", x => x.ZaehlerstandAnhangId);
                    table.ForeignKey(
                        name: "FK_ZaehlerstandAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZaehlerstandAnhaenge_Zaehlerstaende_TargetZaehlerstandId",
                        column: x => x.TargetZaehlerstandId,
                        principalTable: "Zaehlerstaende",
                        principalColumn: "ZaehlerstandId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdresseAnhaenge_AnhangId",
                table: "AdresseAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_AdresseAnhaenge_TargetAdresseId",
                table: "AdresseAnhaenge",
                column: "TargetAdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_BetriebskostenrechnungAnhaenge_AnhangId",
                table: "BetriebskostenrechnungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_BetriebskostenrechnungAnhaenge_TargetBetriebskostenrechnungId",
                table: "BetriebskostenrechnungAnhaenge",
                column: "TargetBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_ErhaltungsaufwendungAnhaenge_AnhangId",
                table: "ErhaltungsaufwendungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_ErhaltungsaufwendungAnhaenge_TargetErhaltungsaufwendungId",
                table: "ErhaltungsaufwendungAnhaenge",
                column: "TargetErhaltungsaufwendungId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageAnhaenge_AnhangId",
                table: "GarageAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageAnhaenge_TargetGarageId",
                table: "GarageAnhaenge",
                column: "TargetGarageId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonAnhaenge_AnhangId",
                table: "JuristischePersonAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonAnhaenge_TargetJuristischePersonId",
                table: "JuristischePersonAnhaenge",
                column: "TargetJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_KontoAnhaenge_AnhangId",
                table: "KontoAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_KontoAnhaenge_TargetKontoId",
                table: "KontoAnhaenge",
                column: "TargetKontoId");

            migrationBuilder.CreateIndex(
                name: "IX_MieteAnhaenge_AnhangId",
                table: "MieteAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_MieteAnhaenge_TargetMieteId",
                table: "MieteAnhaenge",
                column: "TargetMieteId");

            migrationBuilder.CreateIndex(
                name: "IX_MietMinderungAnhaenge_AnhangId",
                table: "MietMinderungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_MietMinderungAnhaenge_TargetMietMinderungId",
                table: "MietMinderungAnhaenge",
                column: "TargetMietMinderungId");

            migrationBuilder.CreateIndex(
                name: "IX_NatuerlichePersonAnhaenge_AnhangId",
                table: "NatuerlichePersonAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_NatuerlichePersonAnhaenge_TargetNatuerlichePersonId",
                table: "NatuerlichePersonAnhaenge",
                column: "TargetNatuerlichePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_VertragAnhaenge_AnhangId",
                table: "VertragAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_WohnungAnhaenge_AnhangId",
                table: "WohnungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_WohnungAnhaenge_TargetWohnungId",
                table: "WohnungAnhaenge",
                column: "TargetWohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerAnhaenge_AnhangId",
                table: "ZaehlerAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerAnhaenge_TargetZaehlerId",
                table: "ZaehlerAnhaenge",
                column: "TargetZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerstandAnhaenge_AnhangId",
                table: "ZaehlerstandAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerstandAnhaenge_TargetZaehlerstandId",
                table: "ZaehlerstandAnhaenge",
                column: "TargetZaehlerstandId");
        }
    }
}
