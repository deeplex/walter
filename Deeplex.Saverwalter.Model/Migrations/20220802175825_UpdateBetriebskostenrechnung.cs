using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class UpdateBetriebskostenrechnung : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Betriebskostenrechnungen_ZaehlerSet_ZaehlerId",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropTable(
                name: "BetriebskostenrechnungWohnung");

            migrationBuilder.DropIndex(
                name: "IX_Betriebskostenrechnungen_ZaehlerId",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "Beschreibung",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "HKVO_P7",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "HKVO_P8",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "HKVO_P9",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "Schluessel",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "Typ",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "ZaehlerId",
                table: "Betriebskostenrechnungen");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Beschreibung",
                table: "Betriebskostenrechnungen",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HKVO_P7",
                table: "Betriebskostenrechnungen",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HKVO_P8",
                table: "Betriebskostenrechnungen",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HKVO_P9",
                table: "Betriebskostenrechnungen",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Schluessel",
                table: "Betriebskostenrechnungen",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Typ",
                table: "Betriebskostenrechnungen",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ZaehlerId",
                table: "Betriebskostenrechnungen",
                type: "INTEGER",
                nullable: true);

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
                name: "IX_Betriebskostenrechnungen_ZaehlerId",
                table: "Betriebskostenrechnungen",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_BetriebskostenrechnungWohnung_WohnungenWohnungId",
                table: "BetriebskostenrechnungWohnung",
                column: "WohnungenWohnungId");

            migrationBuilder.AddForeignKey(
                name: "FK_Betriebskostenrechnungen_ZaehlerSet_ZaehlerId",
                table: "Betriebskostenrechnungen",
                column: "ZaehlerId",
                principalTable: "ZaehlerSet",
                principalColumn: "ZaehlerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
