using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class Transaktionen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ist_beglichen",
                table: "abrechnungsresultate");

            migrationBuilder.AddColumn<double>(
                name: "saldo",
                table: "abrechnungsresultate",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "transaktionen",
                columns: table => new
                {
                    transaktion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    zahler_kontakt_id = table.Column<int>(type: "integer", nullable: false),
                    zahlungsempfaenger_kontakt_id = table.Column<int>(type: "integer", nullable: false),
                    zahlungsdatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    betrag = table.Column<double>(type: "double precision", nullable: false),
                    verwendungszweck = table.Column<string>(type: "text", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaktionen", x => x.transaktion_id);
                    table.ForeignKey(
                        name: "fk_transaktionen_kontakte_zahler_kontakt_id",
                        column: x => x.zahler_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaktionen_kontakte_zahlungsempfaenger_kontakt_id",
                        column: x => x.zahlungsempfaenger_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transaktionen_zahler_kontakt_id",
                table: "transaktionen",
                column: "zahler_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaktionen_zahlungsempfaenger_kontakt_id",
                table: "transaktionen",
                column: "zahlungsempfaenger_kontakt_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaktionen");

            migrationBuilder.DropColumn(
                name: "saldo",
                table: "abrechnungsresultate");

            migrationBuilder.AddColumn<bool>(
                name: "ist_beglichen",
                table: "abrechnungsresultate",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
