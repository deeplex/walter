using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddUmlagen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HKVO",
                columns: table => new
                {
                    HKVOId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HKVO_P7 = table.Column<double>(type: "REAL", nullable: true),
                    HKVO_P8 = table.Column<double>(type: "REAL", nullable: true),
                    HKVO_P9 = table.Column<int>(type: "INTEGER", nullable: true),
                    ZaehlerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HKVO", x => x.HKVOId);
                    table.ForeignKey(
                        name: "FK_HKVO_ZaehlerSet_ZaehlerId",
                        column: x => x.ZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Umlage",
                columns: table => new
                {
                    UmlageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Typ = table.Column<int>(type: "INTEGER", nullable: false),
                    Schluessel = table.Column<int>(type: "INTEGER", nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", nullable: true),
                    HKVOId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Umlage", x => x.UmlageId);
                    table.ForeignKey(
                        name: "FK_Umlage_HKVO_HKVOId",
                        column: x => x.HKVOId,
                        principalTable: "HKVO",
                        principalColumn: "HKVOId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnhangUmlage",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UmlagenUmlageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangUmlage", x => new { x.AnhaengeAnhangId, x.UmlagenUmlageId });
                    table.ForeignKey(
                        name: "FK_AnhangUmlage_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangUmlage_Umlage_UmlagenUmlageId",
                        column: x => x.UmlagenUmlageId,
                        principalTable: "Umlage",
                        principalColumn: "UmlageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UmlageWohnung",
                columns: table => new
                {
                    UmlagenUmlageId = table.Column<int>(type: "INTEGER", nullable: false),
                    WohnungenWohnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmlageWohnung", x => new { x.UmlagenUmlageId, x.WohnungenWohnungId });
                    table.ForeignKey(
                        name: "FK_UmlageWohnung_Umlage_UmlagenUmlageId",
                        column: x => x.UmlagenUmlageId,
                        principalTable: "Umlage",
                        principalColumn: "UmlageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UmlageWohnung_Wohnungen_WohnungenWohnungId",
                        column: x => x.WohnungenWohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnhangUmlage_UmlagenUmlageId",
                table: "AnhangUmlage",
                column: "UmlagenUmlageId");

            migrationBuilder.CreateIndex(
                name: "IX_HKVO_ZaehlerId",
                table: "HKVO",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Umlage_HKVOId",
                table: "Umlage",
                column: "HKVOId");

            migrationBuilder.CreateIndex(
                name: "IX_UmlageWohnung_WohnungenWohnungId",
                table: "UmlageWohnung",
                column: "WohnungenWohnungId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnhangUmlage");

            migrationBuilder.DropTable(
                name: "UmlageWohnung");

            migrationBuilder.DropTable(
                name: "Umlage");

            migrationBuilder.DropTable(
                name: "HKVO");
        }
    }
}
