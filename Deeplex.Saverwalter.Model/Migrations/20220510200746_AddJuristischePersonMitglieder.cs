using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class AddJuristischePersonMitglieder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JuristischePersonenMitglieder");

            migrationBuilder.CreateTable(
                name: "JuristischePersonJuristischePerson",
                columns: table => new
                {
                    JuristischeMitgliederJuristischePersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    JuristischePersonenJuristischePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuristischePersonJuristischePerson", x => new { x.JuristischeMitgliederJuristischePersonId, x.JuristischePersonenJuristischePersonId });
                    table.ForeignKey(
                        name: "FK_JuristischePersonJuristischePerson_JuristischePersonen_JuristischeMitgliederJuristischePersonId",
                        column: x => x.JuristischeMitgliederJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JuristischePersonJuristischePerson_JuristischePersonen_JuristischePersonenJuristischePersonId",
                        column: x => x.JuristischePersonenJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JuristischePersonNatuerlichePerson",
                columns: table => new
                {
                    JuristischePersonenJuristischePersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    NatuerlicheMitgliederNatuerlichePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuristischePersonNatuerlichePerson", x => new { x.JuristischePersonenJuristischePersonId, x.NatuerlicheMitgliederNatuerlichePersonId });
                    table.ForeignKey(
                        name: "FK_JuristischePersonNatuerlichePerson_JuristischePersonen_JuristischePersonenJuristischePersonId",
                        column: x => x.JuristischePersonenJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JuristischePersonNatuerlichePerson_NatuerlichePersonen_NatuerlicheMitgliederNatuerlichePersonId",
                        column: x => x.NatuerlicheMitgliederNatuerlichePersonId,
                        principalTable: "NatuerlichePersonen",
                        principalColumn: "NatuerlichePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonJuristischePerson_JuristischePersonenJuristischePersonId",
                table: "JuristischePersonJuristischePerson",
                column: "JuristischePersonenJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonNatuerlichePerson_NatuerlicheMitgliederNatuerlichePersonId",
                table: "JuristischePersonNatuerlichePerson",
                column: "NatuerlicheMitgliederNatuerlichePersonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JuristischePersonJuristischePerson");

            migrationBuilder.DropTable(
                name: "JuristischePersonNatuerlichePerson");

            migrationBuilder.CreateTable(
                name: "JuristischePersonenMitglieder",
                columns: table => new
                {
                    JuristischePersonenMitgliedId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JuristischePersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    NatuerlichePersonId = table.Column<int>(type: "INTEGER", nullable: true),
                    PersonId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuristischePersonenMitglieder", x => x.JuristischePersonenMitgliedId);
                    table.ForeignKey(
                        name: "FK_JuristischePersonenMitglieder_JuristischePersonen_JuristischePersonId",
                        column: x => x.JuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JuristischePersonenMitglieder_NatuerlichePersonen_NatuerlichePersonId",
                        column: x => x.NatuerlichePersonId,
                        principalTable: "NatuerlichePersonen",
                        principalColumn: "NatuerlichePersonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonenMitglieder_JuristischePersonId",
                table: "JuristischePersonenMitglieder",
                column: "JuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonenMitglieder_NatuerlichePersonId",
                table: "JuristischePersonenMitglieder",
                column: "NatuerlichePersonId");
        }
    }
}
