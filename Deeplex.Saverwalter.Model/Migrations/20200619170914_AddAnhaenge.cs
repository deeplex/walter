using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddAnhaenge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anhaenge",
                columns: table => new
                {
                    AnhangId = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(nullable: false),
                    Sha256Hash = table.Column<byte[]>(nullable: false),
                    ContentType = table.Column<string>(nullable: true),
                    Content = table.Column<byte[]>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anhaenge", x => x.AnhangId);
                });

            migrationBuilder.CreateTable(
                name: "AdresseAnhaenge",
                columns: table => new
                {
                    AdresseAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdresseId = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdresseAnhaenge", x => x.AdresseAnhangId);
                    table.ForeignKey(
                        name: "FK_AdresseAnhaenge_Adressen_AdresseId",
                        column: x => x.AdresseId,
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
                    BetriebskostenrechnungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BetriebskostenrechnungId = table.Column<int>(nullable: false),
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
                        name: "FK_BetriebskostenrechnungAnhaenge_Betriebskostenrechnungen_BetriebskostenrechnungId",
                        column: x => x.BetriebskostenrechnungId,
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
                    GarageId = table.Column<int>(nullable: false),
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
                        name: "FK_GarageAnhaenge_Garagen_GarageId",
                        column: x => x.GarageId,
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
                    PersonJuristischePersonId = table.Column<int>(nullable: false),
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
                        name: "FK_JuristischePersonAnhaenge_JuristischePersonen_PersonJuristischePersonId",
                        column: x => x.PersonJuristischePersonId,
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
                    KontoId = table.Column<int>(nullable: false),
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
                        name: "FK_KontoAnhaenge_Kontos_KontoId",
                        column: x => x.KontoId,
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
                    MieteId = table.Column<int>(nullable: false),
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
                        name: "FK_MieteAnhaenge_Mieten_MieteId",
                        column: x => x.MieteId,
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
                    MietMinderungId = table.Column<int>(nullable: false),
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
                        name: "FK_MietMinderungAnhaenge_MietMinderungen_MietMinderungId",
                        column: x => x.MietMinderungId,
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
                    PersonNatuerlichePersonId = table.Column<int>(nullable: false),
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
                        name: "FK_NatuerlichePersonAnhaenge_NatuerlichePersonen_PersonNatuerlichePersonId",
                        column: x => x.PersonNatuerlichePersonId,
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
                    Vertragrowid = table.Column<int>(nullable: false),
                    AnhangId = table.Column<Guid>(nullable: false)
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
                    table.ForeignKey(
                        name: "FK_VertragAnhaenge_Vertraege_Vertragrowid",
                        column: x => x.Vertragrowid,
                        principalTable: "Vertraege",
                        principalColumn: "rowid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WohnungAnhaenge",
                columns: table => new
                {
                    WohnungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WohnungId = table.Column<int>(nullable: false),
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
                        name: "FK_WohnungAnhaenge_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
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
                    ZaehlerId = table.Column<int>(nullable: false),
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
                        name: "FK_ZaehlerAnhaenge_ZaehlerSet_ZaehlerId",
                        column: x => x.ZaehlerId,
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
                    ZaehlerstandId = table.Column<int>(nullable: false),
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
                        name: "FK_ZaehlerstandAnhaenge_Zaehlerstaende_ZaehlerstandId",
                        column: x => x.ZaehlerstandId,
                        principalTable: "Zaehlerstaende",
                        principalColumn: "ZaehlerstandId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdresseAnhaenge_AdresseId",
                table: "AdresseAnhaenge",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_AdresseAnhaenge_AnhangId",
                table: "AdresseAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_BetriebskostenrechnungAnhaenge_AnhangId",
                table: "BetriebskostenrechnungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_BetriebskostenrechnungAnhaenge_BetriebskostenrechnungId",
                table: "BetriebskostenrechnungAnhaenge",
                column: "BetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageAnhaenge_AnhangId",
                table: "GarageAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageAnhaenge_GarageId",
                table: "GarageAnhaenge",
                column: "GarageId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonAnhaenge_AnhangId",
                table: "JuristischePersonAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonAnhaenge_PersonJuristischePersonId",
                table: "JuristischePersonAnhaenge",
                column: "PersonJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_KontoAnhaenge_AnhangId",
                table: "KontoAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_KontoAnhaenge_KontoId",
                table: "KontoAnhaenge",
                column: "KontoId");

            migrationBuilder.CreateIndex(
                name: "IX_MieteAnhaenge_AnhangId",
                table: "MieteAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_MieteAnhaenge_MieteId",
                table: "MieteAnhaenge",
                column: "MieteId");

            migrationBuilder.CreateIndex(
                name: "IX_MietMinderungAnhaenge_AnhangId",
                table: "MietMinderungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_MietMinderungAnhaenge_MietMinderungId",
                table: "MietMinderungAnhaenge",
                column: "MietMinderungId");

            migrationBuilder.CreateIndex(
                name: "IX_NatuerlichePersonAnhaenge_AnhangId",
                table: "NatuerlichePersonAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_NatuerlichePersonAnhaenge_PersonNatuerlichePersonId",
                table: "NatuerlichePersonAnhaenge",
                column: "PersonNatuerlichePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_VertragAnhaenge_AnhangId",
                table: "VertragAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_VertragAnhaenge_Vertragrowid",
                table: "VertragAnhaenge",
                column: "Vertragrowid");

            migrationBuilder.CreateIndex(
                name: "IX_WohnungAnhaenge_AnhangId",
                table: "WohnungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_WohnungAnhaenge_WohnungId",
                table: "WohnungAnhaenge",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerAnhaenge_AnhangId",
                table: "ZaehlerAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerAnhaenge_ZaehlerId",
                table: "ZaehlerAnhaenge",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerstandAnhaenge_AnhangId",
                table: "ZaehlerstandAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerstandAnhaenge_ZaehlerstandId",
                table: "ZaehlerstandAnhaenge",
                column: "ZaehlerstandId");
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

            migrationBuilder.DropTable(
                name: "Anhaenge");
        }
    }
}
