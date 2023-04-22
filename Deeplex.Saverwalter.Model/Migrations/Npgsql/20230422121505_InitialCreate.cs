using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "adressen",
                columns: table => new
                {
                    adresse_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hausnummer = table.Column<string>(type: "text", nullable: false),
                    strasse = table.Column<string>(type: "text", nullable: false),
                    postleitzahl = table.Column<string>(type: "text", nullable: false),
                    stadt = table.Column<string>(type: "text", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adressen", x => x.adresse_id);
                });

            migrationBuilder.CreateTable(
                name: "kontos",
                columns: table => new
                {
                    konto_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank = table.Column<string>(type: "text", nullable: false),
                    iban = table.Column<string>(type: "text", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kontos", x => x.konto_id);
                });

            migrationBuilder.CreateTable(
                name: "user_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "juristische_personen",
                columns: table => new
                {
                    juristische_person_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    telefon = table.Column<string>(type: "text", nullable: true),
                    mobil = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    adresse_id = table.Column<int>(type: "integer", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
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
                name: "natuerliche_personen",
                columns: table => new
                {
                    natuerliche_person_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nachname = table.Column<string>(type: "text", nullable: false),
                    vorname = table.Column<string>(type: "text", nullable: true),
                    titel = table.Column<int>(type: "integer", nullable: false),
                    anrede = table.Column<int>(type: "integer", nullable: false),
                    telefon = table.Column<string>(type: "text", nullable: true),
                    mobil = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    adresse_id = table.Column<int>(type: "integer", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
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
                name: "pbkdf2password_credentials",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    salt = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    password_hash = table.Column<byte[]>(type: "bytea", maxLength: 64, nullable: false),
                    iterations = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pbkdf2password_credentials", x => x.id);
                    table.ForeignKey(
                        name: "fk_pbkdf2password_credentials_user_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "user_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "garagen",
                columns: table => new
                {
                    garage_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adresse_id = table.Column<int>(type: "integer", nullable: true),
                    kennung = table.Column<string>(type: "text", nullable: false),
                    besitzer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    juristische_person_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garagen", x => x.garage_id);
                    table.ForeignKey(
                        name: "fk_garagen_adressen_adresse_id",
                        column: x => x.adresse_id,
                        principalTable: "adressen",
                        principalColumn: "adresse_id");
                    table.ForeignKey(
                        name: "fk_garagen_juristische_personen_juristische_person_id",
                        column: x => x.juristische_person_id,
                        principalTable: "juristische_personen",
                        principalColumn: "juristische_person_id");
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
                name: "wohnungen",
                columns: table => new
                {
                    wohnung_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adresse_id = table.Column<int>(type: "integer", nullable: true),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    besitzer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wohnflaeche = table.Column<double>(type: "double precision", nullable: false),
                    nutzflaeche = table.Column<double>(type: "double precision", nullable: false),
                    nutzeinheit = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    juristische_person_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wohnungen", x => x.wohnung_id);
                    table.ForeignKey(
                        name: "fk_wohnungen_adressen_adresse_id",
                        column: x => x.adresse_id,
                        principalTable: "adressen",
                        principalColumn: "adresse_id");
                    table.ForeignKey(
                        name: "fk_wohnungen_juristische_personen_juristische_person_id",
                        column: x => x.juristische_person_id,
                        principalTable: "juristische_personen",
                        principalColumn: "juristische_person_id");
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

            migrationBuilder.CreateTable(
                name: "erhaltungsaufwendungen",
                columns: table => new
                {
                    erhaltungsaufwendung_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    betrag = table.Column<double>(type: "double precision", nullable: false),
                    wohnung_id = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    aussteller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_erhaltungsaufwendungen", x => x.erhaltungsaufwendung_id);
                    table.ForeignKey(
                        name: "fk_erhaltungsaufwendungen_wohnungen_wohnung_id",
                        column: x => x.wohnung_id,
                        principalTable: "wohnungen",
                        principalColumn: "wohnung_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vertraege",
                columns: table => new
                {
                    vertrag_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wohnung_id = table.Column<int>(type: "integer", nullable: false),
                    ansprechpartner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    ende = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vertraege", x => x.vertrag_id);
                    table.ForeignKey(
                        name: "fk_vertraege_wohnungen_wohnung_id",
                        column: x => x.wohnung_id,
                        principalTable: "wohnungen",
                        principalColumn: "wohnung_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zaehler_set",
                columns: table => new
                {
                    zaehler_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adresse_id = table.Column<int>(type: "integer", nullable: true),
                    allgemeinzaehler_zaehler_id = table.Column<int>(type: "integer", nullable: true),
                    kennnummer = table.Column<string>(type: "text", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    typ = table.Column<int>(type: "integer", nullable: false),
                    wohnung_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zaehler_set", x => x.zaehler_id);
                    table.ForeignKey(
                        name: "fk_zaehler_set_adressen_adresse_id",
                        column: x => x.adresse_id,
                        principalTable: "adressen",
                        principalColumn: "adresse_id");
                    table.ForeignKey(
                        name: "fk_zaehler_set_wohnungen_wohnung_id",
                        column: x => x.wohnung_id,
                        principalTable: "wohnungen",
                        principalColumn: "wohnung_id");
                    table.ForeignKey(
                        name: "fk_zaehler_set_zaehler_set_allgemeinzaehler_zaehler_id",
                        column: x => x.allgemeinzaehler_zaehler_id,
                        principalTable: "zaehler_set",
                        principalColumn: "zaehler_id");
                });

            migrationBuilder.CreateTable(
                name: "garage_vertrag",
                columns: table => new
                {
                    garagen_garage_id = table.Column<int>(type: "integer", nullable: false),
                    vertraege_vertrag_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garage_vertrag", x => new { x.garagen_garage_id, x.vertraege_vertrag_id });
                    table.ForeignKey(
                        name: "fk_garage_vertrag_garagen_garagen_garage_id",
                        column: x => x.garagen_garage_id,
                        principalTable: "garagen",
                        principalColumn: "garage_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_garage_vertrag_vertraege_vertraege_vertrag_id",
                        column: x => x.vertraege_vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mieten",
                columns: table => new
                {
                    miete_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    betrag = table.Column<double>(type: "double precision", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    zahlungsdatum = table.Column<DateOnly>(type: "date", nullable: false),
                    betreffender_monat = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mieten", x => x.miete_id);
                    table.ForeignKey(
                        name: "fk_mieten_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mieter_set",
                columns: table => new
                {
                    mieter_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
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
                name: "mietminderungen",
                columns: table => new
                {
                    mietminderung_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    beginn = table.Column<DateOnly>(type: "date", nullable: false),
                    minderung = table.Column<double>(type: "double precision", nullable: false),
                    ende = table.Column<DateOnly>(type: "date", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mietminderungen", x => x.mietminderung_id);
                    table.ForeignKey(
                        name: "fk_mietminderungen_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vertrag_versionen",
                columns: table => new
                {
                    vertrag_version_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personenzahl = table.Column<int>(type: "integer", nullable: false),
                    vertrag_id = table.Column<int>(type: "integer", nullable: false),
                    grundmiete = table.Column<double>(type: "double precision", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    beginn = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vertrag_versionen", x => x.vertrag_version_id);
                    table.ForeignKey(
                        name: "fk_vertrag_versionen_vertraege_vertrag_id",
                        column: x => x.vertrag_id,
                        principalTable: "vertraege",
                        principalColumn: "vertrag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hkvo",
                columns: table => new
                {
                    hkvo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hkvo_p7 = table.Column<double>(type: "double precision", nullable: true),
                    hkvo_p8 = table.Column<double>(type: "double precision", nullable: true),
                    hkvo_p9 = table.Column<int>(type: "integer", nullable: true),
                    zaehler_id = table.Column<int>(type: "integer", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hkvo", x => x.hkvo_id);
                    table.ForeignKey(
                        name: "fk_hkvo_zaehler_set_zaehler_id",
                        column: x => x.zaehler_id,
                        principalTable: "zaehler_set",
                        principalColumn: "zaehler_id");
                });

            migrationBuilder.CreateTable(
                name: "zaehlerstaende",
                columns: table => new
                {
                    zaehlerstand_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zaehler_id = table.Column<int>(type: "integer", nullable: false),
                    stand = table.Column<double>(type: "double precision", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    datum = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zaehlerstaende", x => x.zaehlerstand_id);
                    table.ForeignKey(
                        name: "fk_zaehlerstaende_zaehler_set_zaehler_id",
                        column: x => x.zaehler_id,
                        principalTable: "zaehler_set",
                        principalColumn: "zaehler_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "umlagen",
                columns: table => new
                {
                    umlage_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    typ = table.Column<int>(type: "integer", nullable: false),
                    schluessel = table.Column<int>(type: "integer", nullable: false),
                    beschreibung = table.Column<string>(type: "text", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    hkvo_id = table.Column<int>(type: "integer", nullable: true),
                    zaehler_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_umlagen", x => x.umlage_id);
                    table.ForeignKey(
                        name: "fk_umlagen_hkvo_hkvo_id",
                        column: x => x.hkvo_id,
                        principalTable: "hkvo",
                        principalColumn: "hkvo_id");
                    table.ForeignKey(
                        name: "fk_umlagen_zaehler_set_zaehler_id",
                        column: x => x.zaehler_id,
                        principalTable: "zaehler_set",
                        principalColumn: "zaehler_id");
                });

            migrationBuilder.CreateTable(
                name: "betriebskostenrechnungen",
                columns: table => new
                {
                    betriebskostenrechnung_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    betrag = table.Column<double>(type: "double precision", nullable: false),
                    betreffendes_jahr = table.Column<int>(type: "integer", nullable: false),
                    umlage_id = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    datum = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_betriebskostenrechnungen", x => x.betriebskostenrechnung_id);
                    table.ForeignKey(
                        name: "fk_betriebskostenrechnungen_umlagen_umlage_id",
                        column: x => x.umlage_id,
                        principalTable: "umlagen",
                        principalColumn: "umlage_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "umlage_wohnung",
                columns: table => new
                {
                    umlagen_umlage_id = table.Column<int>(type: "integer", nullable: false),
                    wohnungen_wohnung_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_umlage_wohnung", x => new { x.umlagen_umlage_id, x.wohnungen_wohnung_id });
                    table.ForeignKey(
                        name: "fk_umlage_wohnung_umlagen_umlagen_umlage_id",
                        column: x => x.umlagen_umlage_id,
                        principalTable: "umlagen",
                        principalColumn: "umlage_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_umlage_wohnung_wohnungen_wohnungen_wohnung_id",
                        column: x => x.wohnungen_wohnung_id,
                        principalTable: "wohnungen",
                        principalColumn: "wohnung_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_betriebskostenrechnungen_umlage_id",
                table: "betriebskostenrechnungen",
                column: "umlage_id");

            migrationBuilder.CreateIndex(
                name: "ix_erhaltungsaufwendungen_wohnung_id",
                table: "erhaltungsaufwendungen",
                column: "wohnung_id");

            migrationBuilder.CreateIndex(
                name: "ix_garage_vertrag_vertraege_vertrag_id",
                table: "garage_vertrag",
                column: "vertraege_vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_garagen_adresse_id",
                table: "garagen",
                column: "adresse_id");

            migrationBuilder.CreateIndex(
                name: "ix_garagen_juristische_person_id",
                table: "garagen",
                column: "juristische_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_zaehler_id",
                table: "hkvo",
                column: "zaehler_id");

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
                name: "ix_mieten_vertrag_id",
                table: "mieten",
                column: "vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_mieter_set_vertrag_id",
                table: "mieter_set",
                column: "vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_mietminderungen_vertrag_id",
                table: "mietminderungen",
                column: "vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_natuerliche_personen_adresse_id",
                table: "natuerliche_personen",
                column: "adresse_id");

            migrationBuilder.CreateIndex(
                name: "ix_pbkdf2password_credentials_user_id",
                table: "pbkdf2password_credentials",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_umlage_wohnung_wohnungen_wohnung_id",
                table: "umlage_wohnung",
                column: "wohnungen_wohnung_id");

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_hkvo_id",
                table: "umlagen",
                column: "hkvo_id");

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_zaehler_id",
                table: "umlagen",
                column: "zaehler_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_accounts_username",
                table: "user_accounts",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_wohnung_id",
                table: "vertraege",
                column: "wohnung_id");

            migrationBuilder.CreateIndex(
                name: "ix_vertrag_versionen_vertrag_id",
                table: "vertrag_versionen",
                column: "vertrag_id");

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_adresse_id",
                table: "wohnungen",
                column: "adresse_id");

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_juristische_person_id",
                table: "wohnungen",
                column: "juristische_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_zaehler_set_adresse_id",
                table: "zaehler_set",
                column: "adresse_id");

            migrationBuilder.CreateIndex(
                name: "ix_zaehler_set_allgemeinzaehler_zaehler_id",
                table: "zaehler_set",
                column: "allgemeinzaehler_zaehler_id");

            migrationBuilder.CreateIndex(
                name: "ix_zaehler_set_wohnung_id",
                table: "zaehler_set",
                column: "wohnung_id");

            migrationBuilder.CreateIndex(
                name: "ix_zaehlerstaende_zaehler_id",
                table: "zaehlerstaende",
                column: "zaehler_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "betriebskostenrechnungen");

            migrationBuilder.DropTable(
                name: "erhaltungsaufwendungen");

            migrationBuilder.DropTable(
                name: "garage_vertrag");

            migrationBuilder.DropTable(
                name: "juristische_person_juristische_person");

            migrationBuilder.DropTable(
                name: "juristische_person_natuerliche_person");

            migrationBuilder.DropTable(
                name: "kontos");

            migrationBuilder.DropTable(
                name: "mieten");

            migrationBuilder.DropTable(
                name: "mieter_set");

            migrationBuilder.DropTable(
                name: "mietminderungen");

            migrationBuilder.DropTable(
                name: "pbkdf2password_credentials");

            migrationBuilder.DropTable(
                name: "umlage_wohnung");

            migrationBuilder.DropTable(
                name: "vertrag_versionen");

            migrationBuilder.DropTable(
                name: "zaehlerstaende");

            migrationBuilder.DropTable(
                name: "garagen");

            migrationBuilder.DropTable(
                name: "natuerliche_personen");

            migrationBuilder.DropTable(
                name: "user_accounts");

            migrationBuilder.DropTable(
                name: "umlagen");

            migrationBuilder.DropTable(
                name: "vertraege");

            migrationBuilder.DropTable(
                name: "hkvo");

            migrationBuilder.DropTable(
                name: "zaehler_set");

            migrationBuilder.DropTable(
                name: "wohnungen");

            migrationBuilder.DropTable(
                name: "juristische_personen");

            migrationBuilder.DropTable(
                name: "adressen");
        }
    }
}
