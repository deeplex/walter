using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class NkSonderVerrechnungsKonto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "nk_sonder_verrechnungs_konto_id",
                table: "umlagen",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_nk_sonder_verrechnungs_konto_id",
                table: "umlagen",
                column: "nk_sonder_verrechnungs_konto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_umlagen_buchungskonten_nk_sonder_verrechnungs_konto_id",
                table: "umlagen",
                column: "nk_sonder_verrechnungs_konto_id",
                principalTable: "buchungskonten",
                principalColumn: "buchungskonto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_umlagen_buchungskonten_nk_sonder_verrechnungs_konto_id",
                table: "umlagen");

            migrationBuilder.DropIndex(
                name: "ix_umlagen_nk_sonder_verrechnungs_konto_id",
                table: "umlagen");

            migrationBuilder.DropColumn(
                name: "nk_sonder_verrechnungs_konto_id",
                table: "umlagen");
        }
    }
}
