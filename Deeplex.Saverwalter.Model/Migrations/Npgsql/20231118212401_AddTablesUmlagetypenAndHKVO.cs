using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class AddTablesUmlagetypenAndHKVO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE umlagen SET typ = typ + 1");

            migrationBuilder.DropForeignKey(
                name: "fk_umlagen_hkvo_hkvo_id",
                table: "umlagen");

            migrationBuilder.DropIndex(
                name: "ix_umlagen_hkvo_id",
                table: "umlagen");

            migrationBuilder.DropColumn(
                name: "hkvo_id",
                table: "umlagen");

            migrationBuilder.RenameColumn(
                name: "typ",
                table: "umlagen",
                newName: "typ_umlagetyp_id");

            migrationBuilder.AlterColumn<int>(
                name: "hkvo_p9",
                table: "hkvo",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "hkvo_p8",
                table: "hkvo",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "hkvo_p7",
                table: "hkvo",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "betriebsstrom_id",
                table: "hkvo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "hkvo",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "hkvo",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<double>(
                name: "strompauschale",
                table: "hkvo",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "umlagetypen",
                columns: table => new
                {
                    umlagetyp_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_umlagetypen", x => x.umlagetyp_id);
                });

            migrationBuilder.InsertData(
                table: "umlagetypen",
                columns: new string[] { "umlagetyp_id", "bezeichnung" },
                values: new object[,] {
                    {1, "Allgemeinstrom"},
                    {3, "Breitbandkabelanschluss"},
                    {5, "Dachrinnenreinigung"},
                    {7, "Entwässerung/Niederschlag"},
                    {9, "Entwässerung/Schmutzwasser"},
                    {11, "Gartenpflege"},
                    {13, "Ungezieferbekämpfung"},
                    {15, "Grundsteuer"},
                    {17, "Haftpflichtversicherung"},
                    {19, "Hauswartarbeiten"},
                    {21, "Müllbeseitigung"},
                    {23, "Sachversicherung"},
                    {25, "Schornsteinfegerarbeiten"},
                    {27, "Straßenreinigung"},
                    {29, "Wartung Therme/Speicher"},
                    {31, "Wasserversorgung"},
                    {33, "Sonstige Nebenkosten"},
                    {36, "Heizkosten" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_typ_umlagetyp_id",
                table: "umlagen",
                column: "typ_umlagetyp_id");

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_betriebsstrom_id",
                table: "hkvo",
                column: "betriebsstrom_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_hkvo_umlagen_betriebsstrom_id",
                table: "hkvo",
                column: "betriebsstrom_id",
                principalTable: "umlagen",
                principalColumn: "umlage_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_umlagen_umlagetypen_typ_umlagetyp_id",
                table: "umlagen",
                column: "typ_umlagetyp_id",
                principalTable: "umlagetypen",
                principalColumn: "umlagetyp_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hkvo_umlagen_betriebsstrom_id",
                table: "hkvo");

            migrationBuilder.DropForeignKey(
                name: "fk_umlagen_umlagetypen_typ_umlagetyp_id",
                table: "umlagen");

            migrationBuilder.DropTable(
                name: "umlagetypen");

            migrationBuilder.DropIndex(
                name: "ix_umlagen_typ_umlagetyp_id",
                table: "umlagen");

            migrationBuilder.DropIndex(
                name: "ix_hkvo_betriebsstrom_id",
                table: "hkvo");

            migrationBuilder.DropColumn(
                name: "betriebsstrom_id",
                table: "hkvo");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "hkvo");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "hkvo");

            migrationBuilder.DropColumn(
                name: "strompauschale",
                table: "hkvo");

            migrationBuilder.RenameColumn(
                name: "typ_umlagetyp_id",
                table: "umlagen",
                newName: "typ");

            migrationBuilder.AddColumn<int>(
                name: "hkvo_id",
                table: "umlagen",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "hkvo_p9",
                table: "hkvo",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "hkvo_p8",
                table: "hkvo",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "hkvo_p7",
                table: "hkvo",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_hkvo_id",
                table: "umlagen",
                column: "hkvo_id");

            migrationBuilder.AddForeignKey(
                name: "fk_umlagen_hkvo_hkvo_id",
                table: "umlagen",
                column: "hkvo_id",
                principalTable: "hkvo",
                principalColumn: "hkvo_id");
        }
    }
}
