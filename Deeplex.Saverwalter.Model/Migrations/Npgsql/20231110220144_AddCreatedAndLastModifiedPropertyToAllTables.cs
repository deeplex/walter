using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class AddCreatedAndLastModifiedPropertyToAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "zaehlerstaende",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "zaehlerstaende",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "zaehler_set",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "zaehler_set",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "wohnungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "wohnungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateOnly>(
                name: "created_at",
                table: "vertrag_versionen",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "last_modified",
                table: "vertrag_versionen",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "vertraege",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "vertraege",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "umlagen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "umlagen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "natuerliche_personen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "natuerliche_personen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "mietminderungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "mietminderungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "mieter_set",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "mieter_set",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "mieten",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "mieten",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "kontos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "kontos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "juristische_personen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "juristische_personen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "garagen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "garagen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "erhaltungsaufwendungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "erhaltungsaufwendungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "betriebskostenrechnungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "betriebskostenrechnungen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "adressen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "adressen",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.CreateTable(
                name: "vertrags_betriebskostenrechnung",
                columns: table => new
                {
                    vertrags_betriebskostenrechnung_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    rechnung_betriebskostenrechnung_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vertrags_betriebskostenrechnung", x => x.vertrags_betriebskostenrechnung_id);
                    table.ForeignKey(
                        name: "fk_vertrags_betriebskostenrechnung_betriebskostenrechnungen_re",
                        column: x => x.rechnung_betriebskostenrechnung_id,
                        principalTable: "betriebskostenrechnungen",
                        principalColumn: "betriebskostenrechnung_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vertrags_betriebskostenrechnung_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vertrags_betriebskostenrechnung_rechnung_betriebskostenrech",
                table: "vertrags_betriebskostenrechnung",
                column: "rechnung_betriebskostenrechnung_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertrags_betriebskostenrechnung_vertrag_id",
                table: "vertrags_betriebskostenrechnung",
                column: "vertrag_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vertrags_betriebskostenrechnung");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "zaehlerstaende");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "zaehlerstaende");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "zaehler_set");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "zaehler_set");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "vertrag_versionen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "vertrag_versionen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "umlagen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "umlagen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "natuerliche_personen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "natuerliche_personen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "mietminderungen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "mietminderungen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "mieter_set");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "mieter_set");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "mieten");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "mieten");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "kontos");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "kontos");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "juristische_personen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "juristische_personen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "garagen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "garagen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "erhaltungsaufwendungen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "erhaltungsaufwendungen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "betriebskostenrechnungen");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "adressen");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "adressen");
        }
    }
}
