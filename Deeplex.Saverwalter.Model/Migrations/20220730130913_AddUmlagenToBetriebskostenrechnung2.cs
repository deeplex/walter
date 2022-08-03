using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddUmlagenToBetriebskostenrechnung2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UmlageId",
                table: "Betriebskostenrechnungen",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungen_UmlageId",
                table: "Betriebskostenrechnungen",
                column: "UmlageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Betriebskostenrechnungen_Umlagen_UmlageId",
                table: "Betriebskostenrechnungen",
                column: "UmlageId",
                principalTable: "Umlagen",
                principalColumn: "UmlageId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Betriebskostenrechnungen_Umlagen_UmlageId",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropIndex(
                name: "IX_Betriebskostenrechnungen_UmlageId",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "UmlageId",
                table: "Betriebskostenrechnungen");
        }
    }
}
