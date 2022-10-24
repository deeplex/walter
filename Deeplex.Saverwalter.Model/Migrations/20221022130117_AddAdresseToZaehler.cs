using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddAdresseToZaehler : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mieten_Vertraege_VertragId",
                table: "Mieten");

            migrationBuilder.DropForeignKey(
                name: "FK_Mietminderungen_Vertraege_VertragId",
                table: "Mietminderungen");

            migrationBuilder.AddColumn<int>(
                name: "AdresseId",
                table: "ZaehlerSet",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VertragId",
                table: "Mietminderungen",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VertragId",
                table: "Mieten",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerSet_AdresseId",
                table: "ZaehlerSet",
                column: "AdresseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mieten_Vertraege_VertragId",
                table: "Mieten",
                column: "VertragId",
                principalTable: "Vertraege",
                principalColumn: "VertragId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mietminderungen_Vertraege_VertragId",
                table: "Mietminderungen",
                column: "VertragId",
                principalTable: "Vertraege",
                principalColumn: "VertragId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ZaehlerSet_Adressen_AdresseId",
                table: "ZaehlerSet",
                column: "AdresseId",
                principalTable: "Adressen",
                principalColumn: "AdresseId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mieten_Vertraege_VertragId",
                table: "Mieten");

            migrationBuilder.DropForeignKey(
                name: "FK_Mietminderungen_Vertraege_VertragId",
                table: "Mietminderungen");

            migrationBuilder.DropForeignKey(
                name: "FK_ZaehlerSet_Adressen_AdresseId",
                table: "ZaehlerSet");

            migrationBuilder.DropIndex(
                name: "IX_ZaehlerSet_AdresseId",
                table: "ZaehlerSet");

            migrationBuilder.DropColumn(
                name: "AdresseId",
                table: "ZaehlerSet");

            migrationBuilder.AlterColumn<int>(
                name: "VertragId",
                table: "Mietminderungen",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "VertragId",
                table: "Mieten",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Mieten_Vertraege_VertragId",
                table: "Mieten",
                column: "VertragId",
                principalTable: "Vertraege",
                principalColumn: "VertragId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mietminderungen_Vertraege_VertragId",
                table: "Mietminderungen",
                column: "VertragId",
                principalTable: "Vertraege",
                principalColumn: "VertragId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
