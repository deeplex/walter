using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class AddAuthorizationStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_kontakte_ansprechpartner_kontakt_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_wohnungen_kontakte_besitzer_kontakt_id",
                table: "wohnungen");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AlterColumn<int>(
                name: "besitzer_kontakt_id",
                table: "wohnungen",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ansprechpartner_kontakt_id",
                table: "vertraege",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "user_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "user_accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "kontakt_id",
                table: "user_accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "user_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "user_accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "role",
                table: "user_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE user_accounts SET name = 'Admin'");
            migrationBuilder.Sql("UPDATE user_accounts SET role = 2");

            migrationBuilder.CreateTable(
                name: "user_reset_credentials",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<byte[]>(type: "bytea", maxLength: 16, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_reset_credentials", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_reset_credentials_user_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "user_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verwalter_set",
                columns: table => new
                {
                    verwalter_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wohnung_id = table.Column<int>(type: "integer", nullable: false),
                    rolle = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verwalter_set", x => x.verwalter_id);
                    table.ForeignKey(
                        name: "fk_verwalter_set_user_accounts_user_account_id",
                        column: x => x.user_account_id,
                        principalTable: "user_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_verwalter_set_wohnungen_wohnung_id",
                        column: x => x.wohnung_id,
                        principalTable: "wohnungen",
                        principalColumn: "wohnung_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_accounts_kontakt_id",
                table: "user_accounts",
                column: "kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_reset_credentials_token",
                table: "user_reset_credentials",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_reset_credentials_user_id",
                table: "user_reset_credentials",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verwalter_set_user_account_id",
                table: "verwalter_set",
                column: "user_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_verwalter_set_wohnung_id",
                table: "verwalter_set",
                column: "wohnung_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_accounts_kontakte_kontakt_id",
                table: "user_accounts",
                column: "kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_kontakte_ansprechpartner_kontakt_id",
                table: "vertraege",
                column: "ansprechpartner_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id");

            migrationBuilder.AddForeignKey(
                name: "fk_wohnungen_kontakte_besitzer_kontakt_id",
                table: "wohnungen",
                column: "besitzer_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_accounts_kontakte_kontakt_id",
                table: "user_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_kontakte_ansprechpartner_kontakt_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_wohnungen_kontakte_besitzer_kontakt_id",
                table: "wohnungen");

            migrationBuilder.DropTable(
                name: "user_reset_credentials");

            migrationBuilder.DropTable(
                name: "verwalter_set");

            migrationBuilder.DropIndex(
                name: "ix_user_accounts_kontakt_id",
                table: "user_accounts");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "user_accounts");

            migrationBuilder.DropColumn(
                name: "email",
                table: "user_accounts");

            migrationBuilder.DropColumn(
                name: "kontakt_id",
                table: "user_accounts");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "user_accounts");

            migrationBuilder.DropColumn(
                name: "name",
                table: "user_accounts");

            migrationBuilder.DropColumn(
                name: "role",
                table: "user_accounts");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AlterColumn<int>(
                name: "besitzer_kontakt_id",
                table: "wohnungen",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ansprechpartner_kontakt_id",
                table: "vertraege",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "titel",
                table: "kontakte",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "fk_vertraege_kontakte_ansprechpartner_kontakt_id",
                table: "vertraege",
                column: "ansprechpartner_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_wohnungen_kontakte_besitzer_kontakt_id",
                table: "wohnungen",
                column: "besitzer_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
