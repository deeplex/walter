using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class FixHKVORelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hkvo_umlagen_betriebsstrom_id",
                table: "hkvo");

            migrationBuilder.RenameColumn(
                name: "betriebsstrom_id",
                table: "hkvo",
                newName: "heizkosten_id");

            migrationBuilder.RenameIndex(
                name: "ix_hkvo_betriebsstrom_id",
                table: "hkvo",
                newName: "ix_hkvo_heizkosten_id");

            migrationBuilder.AddColumn<int>(
                name: "betriebsstrom_umlage_id",
                table: "hkvo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_betriebsstrom_umlage_id",
                table: "hkvo",
                column: "betriebsstrom_umlage_id");

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_umlagen_betriebsstrom_umlage_id",
                table: "hkvo",
                column: "betriebsstrom_umlage_id",
                principalTable: "umlagen",
                principalColumn: "umlage_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_umlagen_heizkosten_id",
                table: "hkvo",
                column: "heizkosten_id",
                principalTable: "umlagen",
                principalColumn: "umlage_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hkvo_umlagen_betriebsstrom_umlage_id",
                table: "hkvo");

            migrationBuilder.DropForeignKey(
                name: "fk_hkvo_umlagen_heizkosten_id",
                table: "hkvo");

            migrationBuilder.DropIndex(
                name: "ix_hkvo_betriebsstrom_umlage_id",
                table: "hkvo");

            migrationBuilder.DropColumn(
                name: "betriebsstrom_umlage_id",
                table: "hkvo");

            migrationBuilder.RenameColumn(
                name: "heizkosten_id",
                table: "hkvo",
                newName: "betriebsstrom_id");

            migrationBuilder.RenameIndex(
                name: "ix_hkvo_heizkosten_id",
                table: "hkvo",
                newName: "ix_hkvo_betriebsstrom_id");

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_umlagen_betriebsstrom_id",
                table: "hkvo",
                column: "betriebsstrom_id",
                principalTable: "umlagen",
                principalColumn: "umlage_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
