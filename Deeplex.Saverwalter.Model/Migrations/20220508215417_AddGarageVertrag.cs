using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddGarageVertrag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarageVertrag",
                columns: table => new
                {
                    GaragenGarageId = table.Column<int>(type: "INTEGER", nullable: false),
                    Vertraegerowid = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarageVertrag", x => new { x.GaragenGarageId, x.Vertraegerowid });
                    table.ForeignKey(
                        name: "FK_GarageVertrag_Garagen_GaragenGarageId",
                        column: x => x.GaragenGarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarageVertrag_Vertraege_Vertraegerowid",
                        column: x => x.Vertraegerowid,
                        principalTable: "Vertraege",
                        principalColumn: "rowid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarageVertrag_Vertraegerowid",
                table: "GarageVertrag",
                column: "Vertraegerowid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarageVertrag");

            migrationBuilder.CreateTable(
                name: "MietobjektGaragen",
                columns: table => new
                {
                    MietobjektGarageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GarageId = table.Column<int>(type: "INTEGER", nullable: false),
                    VertragId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MietobjektGaragen", x => x.MietobjektGarageId);
                    table.ForeignKey(
                        name: "FK_MietobjektGaragen_Garagen_GarageId",
                        column: x => x.GarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MietobjektGaragen_GarageId",
                table: "MietobjektGaragen",
                column: "GarageId");
        }
    }
}
