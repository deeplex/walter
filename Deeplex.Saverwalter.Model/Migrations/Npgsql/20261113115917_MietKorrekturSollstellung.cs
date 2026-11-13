using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class MietKorrekturSollstellung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "korrigiert_sollstellung_id",
                table: "buchungssaetze",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_buchungssaetze_korrigiert_sollstellung_id",
                table: "buchungssaetze",
                column: "korrigiert_sollstellung_id");

            migrationBuilder.AddForeignKey(
                name: "fk_buchungssaetze_buchungssaetze_korrigiert_sollstellung_id",
                table: "buchungssaetze",
                column: "korrigiert_sollstellung_id",
                principalTable: "buchungssaetze",
                principalColumn: "buchungssatz_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_buchungssaetze_buchungssaetze_korrigiert_sollstellung_id",
                table: "buchungssaetze");

            migrationBuilder.DropIndex(
                name: "ix_buchungssaetze_korrigiert_sollstellung_id",
                table: "buchungssaetze");

            migrationBuilder.DropColumn(
                name: "korrigiert_sollstellung_id",
                table: "buchungssaetze");
        }
    }
}
