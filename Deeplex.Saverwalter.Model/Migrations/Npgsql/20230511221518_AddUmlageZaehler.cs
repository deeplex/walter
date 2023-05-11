using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class AddUmlageZaehler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hkvo_zaehler_set_zaehler_id",
                table: "hkvo");

            migrationBuilder.DropForeignKey(
                name: "fk_umlagen_zaehler_set_zaehler_id",
                table: "umlagen");

            migrationBuilder.DropForeignKey(
                name: "fk_zaehler_set_zaehler_set_allgemeinzaehler_zaehler_id",
                table: "zaehler_set");

            migrationBuilder.DropIndex(
                name: "ix_zaehler_set_allgemeinzaehler_zaehler_id",
                table: "zaehler_set");

            migrationBuilder.DropIndex(
                name: "ix_umlagen_zaehler_id",
                table: "umlagen");

            migrationBuilder.DropIndex(
                name: "ix_hkvo_zaehler_id",
                table: "hkvo");

            migrationBuilder.DropColumn(
                name: "allgemeinzaehler_zaehler_id",
                table: "zaehler_set");

            migrationBuilder.DropColumn(
                name: "zaehler_id",
                table: "umlagen");

            migrationBuilder.DropColumn(
                name: "zaehler_id",
                table: "hkvo");

            migrationBuilder.CreateTable(
                name: "umlage_zaehler",
                columns: table => new
                {
                    umlagen_umlage_id = table.Column<int>(type: "integer", nullable: false),
                    zaehler_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_umlage_zaehler", x => new { x.umlagen_umlage_id, x.zaehler_id });
                    table.ForeignKey(
                        name: "fk_umlage_zaehler_umlagen_umlagen_umlage_id",
                        column: x => x.umlagen_umlage_id,
                        principalTable: "umlagen",
                        principalColumn: "umlage_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_umlage_zaehler_zaehler_set_zaehler_id",
                        column: x => x.zaehler_id,
                        principalTable: "zaehler_set",
                        principalColumn: "zaehler_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_umlage_zaehler_zaehler_id",
                table: "umlage_zaehler",
                column: "zaehler_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "umlage_zaehler");

            migrationBuilder.AddColumn<int>(
                name: "allgemeinzaehler_zaehler_id",
                table: "zaehler_set",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "zaehler_id",
                table: "umlagen",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "zaehler_id",
                table: "hkvo",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_zaehler_set_allgemeinzaehler_zaehler_id",
                table: "zaehler_set",
                column: "allgemeinzaehler_zaehler_id");

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_zaehler_id",
                table: "umlagen",
                column: "zaehler_id");

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_zaehler_id",
                table: "hkvo",
                column: "zaehler_id");

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_zaehler_set_zaehler_id",
                table: "hkvo",
                column: "zaehler_id",
                principalTable: "zaehler_set",
                principalColumn: "zaehler_id");

            migrationBuilder.AddForeignKey(
                name: "fk_umlagen_zaehler_set_zaehler_id",
                table: "umlagen",
                column: "zaehler_id",
                principalTable: "zaehler_set",
                principalColumn: "zaehler_id");

            migrationBuilder.AddForeignKey(
                name: "fk_zaehler_set_zaehler_set_allgemeinzaehler_zaehler_id",
                table: "zaehler_set",
                column: "allgemeinzaehler_zaehler_id",
                principalTable: "zaehler_set",
                principalColumn: "zaehler_id");
        }
    }
}
