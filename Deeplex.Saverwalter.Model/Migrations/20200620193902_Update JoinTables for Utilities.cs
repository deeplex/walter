using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class UpdateJoinTablesforUtilities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdresseAnhaenge",
                columns: table => new
                {
                    AdresseAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetAdresseId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdresseAnhaenge", x => x.AdresseAnhangId);
                    table.ForeignKey(
                        name: "FK_AdresseAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdresseAnhaenge_Adressen_TargetAdresseId",
                        column: x => x.TargetAdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BetriebskostenrechnungAnhaenge",
                columns: table => new
                {
                    BetriebskostenrechnungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetBetriebskostenrechnungId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                name: "GarageAnhaenge",
                columns: table => new
                {
                    GarageAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetGarageId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    JuristischePersonAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetJuristischePersonId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    KontoAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetKontoId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    MieteAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetMieteId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    MietMinderungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetMietMinderungId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    NatuerlichePersonAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetNatuerlichePersonId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    VertragAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Target = table.Column<Guid>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false),
                    Typ = table.Column<int>(nullable: false)
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
                    WohnungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetWohnungId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    ZaehlerAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetZaehlerId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    ZaehlerstandAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetZaehlerstandId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdresseAnhaenge");

            migrationBuilder.DropTable(
                name: "BetriebskostenrechnungAnhaenge");

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
        }
    }
}
