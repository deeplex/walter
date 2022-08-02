using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddZaehlerToUmlagen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Betriebskostenrechnungen_Umlagen_UmlageId",
                table: "Betriebskostenrechnungen");

            migrationBuilder.AddColumn<int>(
                name: "ZaehlerId",
                table: "Umlagen",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UmlageId",
                table: "Betriebskostenrechnungen",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Umlagen_ZaehlerId",
                table: "Umlagen",
                column: "ZaehlerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Betriebskostenrechnungen_Umlagen_UmlageId",
                table: "Betriebskostenrechnungen",
                column: "UmlageId",
                principalTable: "Umlagen",
                principalColumn: "UmlageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Umlagen_ZaehlerSet_ZaehlerId",
                table: "Umlagen",
                column: "ZaehlerId",
                principalTable: "ZaehlerSet",
                principalColumn: "ZaehlerId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Betriebskostenrechnungen_Umlagen_UmlageId",
                table: "Betriebskostenrechnungen");

            migrationBuilder.DropForeignKey(
                name: "FK_Umlagen_ZaehlerSet_ZaehlerId",
                table: "Umlagen");

            migrationBuilder.DropIndex(
                name: "IX_Umlagen_ZaehlerId",
                table: "Umlagen");

            migrationBuilder.DropColumn(
                name: "ZaehlerId",
                table: "Umlagen");

            migrationBuilder.AlterColumn<int>(
                name: "UmlageId",
                table: "Betriebskostenrechnungen",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Betriebskostenrechnungen_Umlagen_UmlageId",
                table: "Betriebskostenrechnungen",
                column: "UmlageId",
                principalTable: "Umlagen",
                principalColumn: "UmlageId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
