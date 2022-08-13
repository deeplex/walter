using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class RemoveIsMieterIsVermieterIsHandwerkerFromNatuerlicheAndJuristischePerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isHandwerker",
                table: "NatuerlichePersonen");

            migrationBuilder.DropColumn(
                name: "isMieter",
                table: "NatuerlichePersonen");

            migrationBuilder.DropColumn(
                name: "isVermieter",
                table: "NatuerlichePersonen");

            migrationBuilder.DropColumn(
                name: "isHandwerker",
                table: "JuristischePersonen");

            migrationBuilder.DropColumn(
                name: "isMieter",
                table: "JuristischePersonen");

            migrationBuilder.DropColumn(
                name: "isVermieter",
                table: "JuristischePersonen");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isHandwerker",
                table: "NatuerlichePersonen",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isMieter",
                table: "NatuerlichePersonen",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isVermieter",
                table: "NatuerlichePersonen",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isHandwerker",
                table: "JuristischePersonen",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isMieter",
                table: "JuristischePersonen",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isVermieter",
                table: "JuristischePersonen",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
