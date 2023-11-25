using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class MergePersonToKontakt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kontakte",
                columns: table => new
                {
                    kontakt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    old_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    rechtsform = table.Column<int>(type: "integer", nullable: false),
                    vorname = table.Column<string>(type: "text", nullable: true),
                    anrede = table.Column<int>(type: "integer", nullable: false),
                    telefon = table.Column<string>(type: "text", nullable: true),
                    mobil = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    adresse_id = table.Column<int>(type: "integer", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kontakte", x => x.kontakt_id);
                    table.ForeignKey(
                        name: "fk_kontakte_adressen_adresse_id",
                        column: x => x.adresse_id,
                        principalTable: "adressen",
                        principalColumn: "adresse_id");
                });

            migrationBuilder.Sql(
                @"INSERT INTO kontakte(old_guid, name, rechtsform, anrede, telefon, mobil, fax, email, adresse_id, notiz)
                  SELECT person_id, bezeichnung, 1, 2, telefon, mobil, fax, email, adresse_id, notiz
                  FROM juristische_personen
                 "
                );

            migrationBuilder.Sql(
                @"INSERT INTO kontakte(old_guid, name, rechtsform, anrede, telefon, mobil, fax, email, adresse_id, notiz, vorname)
                  SELECT person_id, nachname, 0, anrede, telefon, mobil, fax, email, adresse_id, notiz, vorname
                  FROM natuerliche_personen
                "
                );

            migrationBuilder.CreateTable(
                name: "kontakt_kontakt",
                columns: table => new
                {
                    juristische_personen_kontakt_id = table.Column<int>(type: "integer", nullable: false),
                    mitglieder_kontakt_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kontakt_kontakt", x => new { x.juristische_personen_kontakt_id, x.mitglieder_kontakt_id });
                    table.ForeignKey(
                        name: "fk_kontakt_kontakt_kontakte_juristische_personen_kontakt_id",
                        column: x => x.juristische_personen_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_kontakt_kontakt_kontakte_mitglieder_kontakt_id",
                        column: x => x.mitglieder_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kontakt_vertrag",
                columns: table => new
                {
                    mieter_kontakt_id = table.Column<int>(type: "integer", nullable: false),
                    mietvertraege_vertrag_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kontakt_vertrag", x => new { x.mieter_kontakt_id, x.mietvertraege_vertrag_id });
                    table.ForeignKey(
                        name: "fk_kontakt_vertrag_kontakte_mieter_kontakt_id",
                        column: x => x.mieter_kontakt_id,
                        principalTable: "kontakte",
                        principalColumn: "kontakt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_kontakt_vertrag_vertraege_mietvertraege_vertrag_id",
                        column: x => x.mietvertraege_vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
               @"INSERT INTO kontakt_vertrag(mieter_kontakt_id, mietvertraege_vertrag_id)
                 SELECT k.kontakt_id AS mieter_kontakt_id, ms.vertrag_id AS mietvertraege_vertrag_id
                 FROM mieter_set ms
                 JOIN kontakte k ON k.old_guid = ms.person_id
                ");

            migrationBuilder.AddColumn<int>(
                name: "besitzer_kontakt_id",
                table: "wohnungen",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE wohnungen w
                  SET besitzer_kontakt_id = k.kontakt_id
                  FROM kontakte k
                  WHERE w.besitzer_id = k.old_guid;
                 ");

            migrationBuilder.AddColumn<int>(
                name: "besitzer_kontakt_id",
                table: "kontos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ansprechpartner_kontakt_id",
                table: "vertraege",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                 @"UPDATE vertraege v
                  SET ansprechpartner_kontakt_id = k.kontakt_id
                  FROM kontakte k
                  WHERE v.ansprechpartner_id = k.old_guid;
                 ");

            migrationBuilder.AddColumn<int>(
                name: "besitzer_kontakt_id",
                table: "garagen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "aussteller_kontakt_id",
                table: "erhaltungsaufwendungen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                @"UPDATE erhaltungsaufwendungen e
                  SET aussteller_kontakt_id = k.kontakt_id
                  FROM kontakte k
                  WHERE e.aussteller_id = k.old_guid;
                 ");

            migrationBuilder.DropForeignKey(
                name: "fk_garagen_juristische_personen_juristische_person_id",
                table: "garagen");

            migrationBuilder.DropForeignKey(
                name: "fk_wohnungen_juristische_personen_juristische_person_id",
                table: "wohnungen");

            migrationBuilder.DropTable(
                name: "juristische_person_juristische_person");

            migrationBuilder.DropTable(
                name: "juristische_person_natuerliche_person");

            migrationBuilder.DropTable(
                name: "mieter_set");

            migrationBuilder.DropTable(
                name: "juristische_personen");

            migrationBuilder.DropTable(
                name: "natuerliche_personen");

            migrationBuilder.DropIndex(
                name: "ix_wohnungen_juristische_person_id",
                table: "wohnungen");

            migrationBuilder.DropIndex(
                name: "ix_garagen_juristische_person_id",
                table: "garagen");

            migrationBuilder.DropColumn(
                name: "besitzer_id",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "juristische_person_id",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "ansprechpartner_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "besitzer_id",
                table: "garagen");

            migrationBuilder.DropColumn(
                name: "juristische_person_id",
                table: "garagen");

            migrationBuilder.DropColumn(
                name: "aussteller_id",
                table: "erhaltungsaufwendungen");

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_besitzer_kontakt_id",
                table: "wohnungen",
                column: "besitzer_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_ansprechpartner_kontakt_id",
                table: "vertraege",
                column: "ansprechpartner_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_kontos_besitzer_kontakt_id",
                table: "kontos",
                column: "besitzer_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_garagen_besitzer_kontakt_id",
                table: "garagen",
                column: "besitzer_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_erhaltungsaufwendungen_aussteller_kontakt_id",
                table: "erhaltungsaufwendungen",
                column: "aussteller_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_kontakt_kontakt_mitglieder_kontakt_id",
                table: "kontakt_kontakt",
                column: "mitglieder_kontakt_id");

            migrationBuilder.CreateIndex(
                name: "ix_kontakt_vertrag_mietvertraege_vertrag_id",
                table: "kontakt_vertrag",
                column: "mietvertraege_vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_kontakte_adresse_id",
                table: "kontakte",
                column: "adresse_id");

            migrationBuilder.AddForeignKey(
                name: "fk_erhaltungsaufwendungen_kontakte_aussteller_kontakt_id",
                table: "erhaltungsaufwendungen",
                column: "aussteller_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_garagen_kontakte_besitzer_kontakt_id",
                table: "garagen",
                column: "besitzer_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_kontos_kontakte_besitzer_kontakt_id",
                table: "kontos",
                column: "besitzer_kontakt_id",
                principalTable: "kontakte",
                principalColumn: "kontakt_id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.DropColumn(
                name: "old_guid",
                table: "kontakte");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_erhaltungsaufwendungen_kontakte_aussteller_kontakt_id",
                table: "erhaltungsaufwendungen");

            migrationBuilder.DropForeignKey(
                name: "fk_garagen_kontakte_besitzer_kontakt_id",
                table: "garagen");

            migrationBuilder.DropForeignKey(
                name: "fk_kontos_kontakte_besitzer_kontakt_id",
                table: "kontos");

            migrationBuilder.DropForeignKey(
                name: "fk_vertraege_kontakte_ansprechpartner_kontakt_id",
                table: "vertraege");

            migrationBuilder.DropForeignKey(
                name: "fk_wohnungen_kontakte_besitzer_kontakt_id",
                table: "wohnungen");

            migrationBuilder.DropTable(
                name: "kontakt_kontakt");

            migrationBuilder.DropTable(
                name: "kontakt_vertrag");

            migrationBuilder.DropTable(
                name: "kontakte");

            migrationBuilder.DropIndex(
                name: "ix_wohnungen_besitzer_kontakt_id",
                table: "wohnungen");

            migrationBuilder.DropIndex(
                name: "ix_vertraege_ansprechpartner_kontakt_id",
                table: "vertraege");

            migrationBuilder.DropIndex(
                name: "ix_kontos_besitzer_kontakt_id",
                table: "kontos");

            migrationBuilder.DropIndex(
                name: "ix_garagen_besitzer_kontakt_id",
                table: "garagen");

            migrationBuilder.DropIndex(
                name: "ix_erhaltungsaufwendungen_aussteller_kontakt_id",
                table: "erhaltungsaufwendungen");

            migrationBuilder.DropColumn(
                name: "besitzer_kontakt_id",
                table: "wohnungen");

            migrationBuilder.DropColumn(
                name: "ansprechpartner_kontakt_id",
                table: "vertraege");

            migrationBuilder.DropColumn(
                name: "besitzer_kontakt_id",
                table: "kontos");

            migrationBuilder.DropColumn(
                name: "besitzer_kontakt_id",
                table: "garagen");

            migrationBuilder.DropColumn(
                name: "aussteller_kontakt_id",
                table: "erhaltungsaufwendungen");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AddColumn<Guid>(
                name: "besitzer_id",
                table: "wohnungen",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "juristische_person_id",
                table: "wohnungen",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ansprechpartner_id",
                table: "vertraege",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "besitzer_id",
                table: "garagen",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "juristische_person_id",
                table: "garagen",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "aussteller_id",
                table: "erhaltungsaufwendungen",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "juristische_personen",
                columns: table => new
                {
                    juristische_person_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adresse_id = table.Column<int>(type: "integer", nullable: true),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    email = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    mobil = table.Column<string>(type: "text", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telefon = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_juristische_personen", x => x.juristische_person_id);
                    table.UniqueConstraint("ak_juristische_personen_person_id", x => x.person_id);
                    table.ForeignKey(
                        name: "fk_juristische_personen_adressen_adresse_id",
                        column: x => x.adresse_id,
                        principalTable: "adressen",
                        principalColumn: "adresse_id");
                });

            migrationBuilder.CreateTable(
                name: "mieter_set",
                columns: table => new
                {
                    mieter_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mieter_set", x => x.mieter_id);
                    table.ForeignKey(
                        name: "fk_mieter_set_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "natuerliche_personen",
                columns: table => new
                {
                    natuerliche_person_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adresse_id = table.Column<int>(type: "integer", nullable: true),
                    anrede = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    email = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    mobil = table.Column<string>(type: "text", nullable: true),
                    nachname = table.Column<string>(type: "text", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telefon = table.Column<string>(type: "text", nullable: true),
                    titel = table.Column<int>(type: "integer", nullable: false),
                    vorname = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_natuerliche_personen", x => x.natuerliche_person_id);
                    table.UniqueConstraint("ak_natuerliche_personen_person_id", x => x.person_id);
                    table.ForeignKey(
                        name: "fk_natuerliche_personen_adressen_adresse_id",
                        column: x => x.adresse_id,
                        principalTable: "adressen",
                        principalColumn: "adresse_id");
                });

            migrationBuilder.CreateTable(
                name: "juristische_person_juristische_person",
                columns: table => new
                {
                    juristische_mitglieder_juristische_person_id = table.Column<int>(type: "integer", nullable: false),
                    juristische_personen_juristische_person_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_juristische_person_juristische_person", x => new { x.juristische_mitglieder_juristische_person_id, x.juristische_personen_juristische_person_id });
                    table.ForeignKey(
                        name: "fk_juristische_person_juristische_person_juristische_personen_",
                        column: x => x.juristische_mitglieder_juristische_person_id,
                        principalTable: "juristische_personen",
                        principalColumn: "juristische_person_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_juristische_person_juristische_person_juristische_personen_1",
                        column: x => x.juristische_personen_juristische_person_id,
                        principalTable: "juristische_personen",
                        principalColumn: "juristische_person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "juristische_person_natuerliche_person",
                columns: table => new
                {
                    juristische_personen_juristische_person_id = table.Column<int>(type: "integer", nullable: false),
                    natuerliche_mitglieder_natuerliche_person_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_juristische_person_natuerliche_person", x => new { x.juristische_personen_juristische_person_id, x.natuerliche_mitglieder_natuerliche_person_id });
                    table.ForeignKey(
                        name: "fk_juristische_person_natuerliche_person_juristische_personen_",
                        column: x => x.juristische_personen_juristische_person_id,
                        principalTable: "juristische_personen",
                        principalColumn: "juristische_person_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_juristische_person_natuerliche_person_natuerliche_personen_",
                        column: x => x.natuerliche_mitglieder_natuerliche_person_id,
                        principalTable: "natuerliche_personen",
                        principalColumn: "natuerliche_person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_juristische_person_id",
                table: "wohnungen",
                column: "juristische_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_garagen_juristische_person_id",
                table: "garagen",
                column: "juristische_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_juristische_person_juristische_person_juristische_personen_",
                table: "juristische_person_juristische_person",
                column: "juristische_personen_juristische_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_juristische_person_natuerliche_person_natuerliche_mitgliede",
                table: "juristische_person_natuerliche_person",
                column: "natuerliche_mitglieder_natuerliche_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_juristische_personen_adresse_id",
                table: "juristische_personen",
                column: "adresse_id");

            migrationBuilder.CreateIndex(
                name: "ix_mieter_set_vertrag_id",
                table: "mieter_set",
                column: "vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_natuerliche_personen_adresse_id",
                table: "natuerliche_personen",
                column: "adresse_id");

            migrationBuilder.AddForeignKey(
                name: "fk_garagen_juristische_personen_juristische_person_id",
                table: "garagen",
                column: "juristische_person_id",
                principalTable: "juristische_personen",
                principalColumn: "juristische_person_id");

            migrationBuilder.AddForeignKey(
                name: "fk_wohnungen_juristische_personen_juristische_person_id",
                table: "wohnungen",
                column: "juristische_person_id",
                principalTable: "juristische_personen",
                principalColumn: "juristische_person_id");
        }
    }
}
