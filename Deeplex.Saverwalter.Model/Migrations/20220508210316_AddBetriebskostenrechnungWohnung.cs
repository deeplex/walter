using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddBetriebskostenrechnungWohnung : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Betriebskostenrechnungsgruppen");

            migrationBuilder.CreateTable(
                name: "BetriebskostenrechnungWohnung",
                columns: table => new
                {
                    BetriebskostenrechnungenBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false),
                    WohnungenWohnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetriebskostenrechnungWohnung", x => new { x.BetriebskostenrechnungenBetriebskostenrechnungId, x.WohnungenWohnungId });
                    table.ForeignKey(
                        name: "FK_BetriebskostenrechnungWohnung_Betriebskostenrechnungen_BetriebskostenrechnungenBetriebskostenrechnungId",
                        column: x => x.BetriebskostenrechnungenBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BetriebskostenrechnungWohnung_Wohnungen_WohnungenWohnungId",
                        column: x => x.WohnungenWohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BetriebskostenrechnungWohnung_WohnungenWohnungId",
                table: "BetriebskostenrechnungWohnung",
                column: "WohnungenWohnungId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetriebskostenrechnungWohnung");

            migrationBuilder.CreateTable(
                name: "Betriebskostenrechnungsgruppen",
                columns: table => new
                {
                    BetriebskostenrechnungsGruppeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RechnungBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false),
                    WohnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Betriebskostenrechnungsgruppen", x => x.BetriebskostenrechnungsGruppeId);
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

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungsgruppen_RechnungBetriebskostenrechnungId",
                table: "Betriebskostenrechnungsgruppen",
                column: "RechnungBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungsgruppen_WohnungId",
                table: "Betriebskostenrechnungsgruppen",
                column: "WohnungId");
        }
    }
}
