using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddUmlagenToBetriebskostenrechnung : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnhangUmlage_Umlage_UmlagenUmlageId",
                table: "AnhangUmlage");

            migrationBuilder.DropForeignKey(
                name: "FK_Umlage_HKVO_HKVOId",
                table: "Umlage");

            migrationBuilder.DropForeignKey(
                name: "FK_UmlageWohnung_Umlage_UmlagenUmlageId",
                table: "UmlageWohnung");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Umlage",
                table: "Umlage");

            migrationBuilder.RenameTable(
                name: "Umlage",
                newName: "Umlagen");

            migrationBuilder.RenameIndex(
                name: "IX_Umlage_HKVOId",
                table: "Umlagen",
                newName: "IX_Umlagen_HKVOId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Umlagen",
                table: "Umlagen",
                column: "UmlageId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnhangUmlage_Umlagen_UmlagenUmlageId",
                table: "AnhangUmlage",
                column: "UmlagenUmlageId",
                principalTable: "Umlagen",
                principalColumn: "UmlageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Umlagen_HKVO_HKVOId",
                table: "Umlagen",
                column: "HKVOId",
                principalTable: "HKVO",
                principalColumn: "HKVOId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UmlageWohnung_Umlagen_UmlagenUmlageId",
                table: "UmlageWohnung",
                column: "UmlagenUmlageId",
                principalTable: "Umlagen",
                principalColumn: "UmlageId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnhangUmlage_Umlagen_UmlagenUmlageId",
                table: "AnhangUmlage");

            migrationBuilder.DropForeignKey(
                name: "FK_Umlagen_HKVO_HKVOId",
                table: "Umlagen");

            migrationBuilder.DropForeignKey(
                name: "FK_UmlageWohnung_Umlagen_UmlagenUmlageId",
                table: "UmlageWohnung");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Umlagen",
                table: "Umlagen");

            migrationBuilder.RenameTable(
                name: "Umlagen",
                newName: "Umlage");

            migrationBuilder.RenameIndex(
                name: "IX_Umlagen_HKVOId",
                table: "Umlage",
                newName: "IX_Umlage_HKVOId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Umlage",
                table: "Umlage",
                column: "UmlageId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnhangUmlage_Umlage_UmlagenUmlageId",
                table: "AnhangUmlage",
                column: "UmlagenUmlageId",
                principalTable: "Umlage",
                principalColumn: "UmlageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Umlage_HKVO_HKVOId",
                table: "Umlage",
                column: "HKVOId",
                principalTable: "HKVO",
                principalColumn: "HKVOId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UmlageWohnung_Umlage_UmlagenUmlageId",
                table: "UmlageWohnung",
                column: "UmlagenUmlageId",
                principalTable: "Umlage",
                principalColumn: "UmlageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
