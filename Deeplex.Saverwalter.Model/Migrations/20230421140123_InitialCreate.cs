using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations
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
                    adresseid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hausnummer = table.Column<string>(type: "text", nullable: false),
                    strasse = table.Column<string>(type: "text", nullable: false),
                    postleitzahl = table.Column<string>(type: "text", nullable: false),
                    stadt = table.Column<string>(type: "text", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adressen", x => x.adresseid);
                });

            migrationBuilder.CreateTable(
                name: "kontos",
                columns: table => new
                {
                    kontoid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank = table.Column<string>(type: "text", nullable: false),
                    iban = table.Column<string>(type: "text", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kontos", x => x.kontoid);
                });

            migrationBuilder.CreateTable(
                name: "useraccounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_useraccounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "juristischepersonen",
                columns: table => new
                {
                    juristischepersonid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personid = table.Column<Guid>(type: "uuid", nullable: false),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    telefon = table.Column<string>(type: "text", nullable: true),
                    mobil = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    adresseid = table.Column<int>(type: "integer", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_juristischepersonen", x => x.juristischepersonid);
                    table.UniqueConstraint("ak_juristischepersonen_personid", x => x.personid);
                    table.ForeignKey(
                        name: "fk_juristischepersonen_adressen_adresseid",
                        column: x => x.adresseid,
                        principalTable: "adressen",
                        principalColumn: "adresseid");
                });

            migrationBuilder.CreateTable(
                name: "natuerlichepersonen",
                columns: table => new
                {
                    natuerlichepersonid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personid = table.Column<Guid>(type: "uuid", nullable: false),
                    nachname = table.Column<string>(type: "text", nullable: false),
                    vorname = table.Column<string>(type: "text", nullable: true),
                    titel = table.Column<int>(type: "integer", nullable: false),
                    anrede = table.Column<int>(type: "integer", nullable: false),
                    telefon = table.Column<string>(type: "text", nullable: true),
                    mobil = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    adresseid = table.Column<int>(type: "integer", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_natuerlichepersonen", x => x.natuerlichepersonid);
                    table.UniqueConstraint("ak_natuerlichepersonen_personid", x => x.personid);
                    table.ForeignKey(
                        name: "fk_natuerlichepersonen_adressen_adresseid",
                        column: x => x.adresseid,
                        principalTable: "adressen",
                        principalColumn: "adresseid");
                });

            migrationBuilder.CreateTable(
                name: "pbkdf2passwordcredentials",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    salt = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    passwordhash = table.Column<byte[]>(type: "bytea", maxLength: 64, nullable: false),
                    iterations = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pbkdf2passwordcredentials", x => x.id);
                    table.ForeignKey(
                        name: "fk_pbkdf2passwordcredentials_useraccounts_userid",
                        column: x => x.userid,
                        principalTable: "useraccounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "garagen",
                columns: table => new
                {
                    garageid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kennung = table.Column<string>(type: "text", nullable: false),
                    adresseid = table.Column<int>(type: "integer", nullable: true),
                    besitzerid = table.Column<Guid>(type: "uuid", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    juristischepersonid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garagen", x => x.garageid);
                    table.ForeignKey(
                        name: "fk_garagen_adressen_adresseid",
                        column: x => x.adresseid,
                        principalTable: "adressen",
                        principalColumn: "adresseid");
                    table.ForeignKey(
                        name: "fk_garagen_juristischepersonen_juristischepersonid",
                        column: x => x.juristischepersonid,
                        principalTable: "juristischepersonen",
                        principalColumn: "juristischepersonid");
                });

            migrationBuilder.CreateTable(
                name: "juristischepersonjuristischeperson",
                columns: table => new
                {
                    juristischemitgliederjuristischepersonid = table.Column<int>(type: "integer", nullable: false),
                    juristischepersonenjuristischepersonid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_juristischepersonjuristischeperson", x => new { x.juristischemitgliederjuristischepersonid, x.juristischepersonenjuristischepersonid });
                    table.ForeignKey(
                        name: "fk_juristischepersonjuristischeperson_juristischepersonen_juri~",
                        column: x => x.juristischemitgliederjuristischepersonid,
                        principalTable: "juristischepersonen",
                        principalColumn: "juristischepersonid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_juristischepersonjuristischeperson_juristischepersonen_jur~1",
                        column: x => x.juristischepersonenjuristischepersonid,
                        principalTable: "juristischepersonen",
                        principalColumn: "juristischepersonid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wohnungen",
                columns: table => new
                {
                    wohnungid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    wohnflaeche = table.Column<double>(type: "double precision", nullable: false),
                    nutzflaeche = table.Column<double>(type: "double precision", nullable: false),
                    nutzeinheit = table.Column<int>(type: "integer", nullable: false),
                    adresseid = table.Column<int>(type: "integer", nullable: true),
                    besitzerid = table.Column<Guid>(type: "uuid", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    juristischepersonid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wohnungen", x => x.wohnungid);
                    table.ForeignKey(
                        name: "fk_wohnungen_adressen_adresseid",
                        column: x => x.adresseid,
                        principalTable: "adressen",
                        principalColumn: "adresseid");
                    table.ForeignKey(
                        name: "fk_wohnungen_juristischepersonen_juristischepersonid",
                        column: x => x.juristischepersonid,
                        principalTable: "juristischepersonen",
                        principalColumn: "juristischepersonid");
                });

            migrationBuilder.CreateTable(
                name: "juristischepersonnatuerlicheperson",
                columns: table => new
                {
                    juristischepersonenjuristischepersonid = table.Column<int>(type: "integer", nullable: false),
                    natuerlichemitgliedernatuerlichepersonid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_juristischepersonnatuerlicheperson", x => new { x.juristischepersonenjuristischepersonid, x.natuerlichemitgliedernatuerlichepersonid });
                    table.ForeignKey(
                        name: "fk_juristischepersonnatuerlicheperson_juristischepersonen_juri~",
                        column: x => x.juristischepersonenjuristischepersonid,
                        principalTable: "juristischepersonen",
                        principalColumn: "juristischepersonid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_juristischepersonnatuerlicheperson_natuerlichepersonen_natu~",
                        column: x => x.natuerlichemitgliedernatuerlichepersonid,
                        principalTable: "natuerlichepersonen",
                        principalColumn: "natuerlichepersonid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erhaltungsaufwendungen",
                columns: table => new
                {
                    erhaltungsaufwendungid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    datum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ausstellerid = table.Column<Guid>(type: "uuid", nullable: false),
                    bezeichnung = table.Column<string>(type: "text", nullable: false),
                    betrag = table.Column<double>(type: "double precision", nullable: false),
                    wohnungid = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_erhaltungsaufwendungen", x => x.erhaltungsaufwendungid);
                    table.ForeignKey(
                        name: "fk_erhaltungsaufwendungen_wohnungen_wohnungid",
                        column: x => x.wohnungid,
                        principalTable: "wohnungen",
                        principalColumn: "wohnungid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vertraege",
                columns: table => new
                {
                    vertragid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wohnungid = table.Column<int>(type: "integer", nullable: false),
                    ansprechpartnerid = table.Column<Guid>(type: "uuid", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    ende = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vertraege", x => x.vertragid);
                    table.ForeignKey(
                        name: "fk_vertraege_wohnungen_wohnungid",
                        column: x => x.wohnungid,
                        principalTable: "wohnungen",
                        principalColumn: "wohnungid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zaehlerset",
                columns: table => new
                {
                    zaehlerid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kennnummer = table.Column<string>(type: "text", nullable: false),
                    typ = table.Column<int>(type: "integer", nullable: false),
                    wohnungid = table.Column<int>(type: "integer", nullable: true),
                    adresseid = table.Column<int>(type: "integer", nullable: true),
                    allgemeinzaehlerzaehlerid = table.Column<int>(type: "integer", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zaehlerset", x => x.zaehlerid);
                    table.ForeignKey(
                        name: "fk_zaehlerset_adressen_adresseid",
                        column: x => x.adresseid,
                        principalTable: "adressen",
                        principalColumn: "adresseid");
                    table.ForeignKey(
                        name: "fk_zaehlerset_wohnungen_wohnungid",
                        column: x => x.wohnungid,
                        principalTable: "wohnungen",
                        principalColumn: "wohnungid");
                    table.ForeignKey(
                        name: "fk_zaehlerset_zaehlerset_allgemeinzaehlerzaehlerid",
                        column: x => x.allgemeinzaehlerzaehlerid,
                        principalTable: "zaehlerset",
                        principalColumn: "zaehlerid");
                });

            migrationBuilder.CreateTable(
                name: "garagevertrag",
                columns: table => new
                {
                    garagengarageid = table.Column<int>(type: "integer", nullable: false),
                    vertraegevertragid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garagevertrag", x => new { x.garagengarageid, x.vertraegevertragid });
                    table.ForeignKey(
                        name: "fk_garagevertrag_garagen_garagengarageid",
                        column: x => x.garagengarageid,
                        principalTable: "garagen",
                        principalColumn: "garageid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_garagevertrag_vertraege_vertraegevertragid",
                        column: x => x.vertraegevertragid,
                        principalTable: "vertraege",
                        principalColumn: "vertragid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mieten",
                columns: table => new
                {
                    mieteid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vertragid = table.Column<int>(type: "integer", nullable: false),
                    zahlungsdatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    betreffendermonat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    betrag = table.Column<double>(type: "double precision", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mieten", x => x.mieteid);
                    table.ForeignKey(
                        name: "fk_mieten_vertraege_vertragid",
                        column: x => x.vertragid,
                        principalTable: "vertraege",
                        principalColumn: "vertragid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mieterset",
                columns: table => new
                {
                    mieterid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personid = table.Column<Guid>(type: "uuid", nullable: false),
                    vertragid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mieterset", x => x.mieterid);
                    table.ForeignKey(
                        name: "fk_mieterset_vertraege_vertragid",
                        column: x => x.vertragid,
                        principalTable: "vertraege",
                        principalColumn: "vertragid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mietminderungen",
                columns: table => new
                {
                    mietminderungid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vertragid = table.Column<int>(type: "integer", nullable: false),
                    beginn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    minderung = table.Column<double>(type: "double precision", nullable: false),
                    ende = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mietminderungen", x => x.mietminderungid);
                    table.ForeignKey(
                        name: "fk_mietminderungen_vertraege_vertragid",
                        column: x => x.vertragid,
                        principalTable: "vertraege",
                        principalColumn: "vertragid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vertragversionen",
                columns: table => new
                {
                    vertragversionid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personenzahl = table.Column<int>(type: "integer", nullable: false),
                    vertragid = table.Column<int>(type: "integer", nullable: false),
                    beginn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    grundmiete = table.Column<double>(type: "double precision", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vertragversionen", x => x.vertragversionid);
                    table.ForeignKey(
                        name: "fk_vertragversionen_vertraege_vertragid",
                        column: x => x.vertragid,
                        principalTable: "vertraege",
                        principalColumn: "vertragid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hkvo",
                columns: table => new
                {
                    hkvoid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hkvo_p7 = table.Column<double>(type: "double precision", nullable: true),
                    hkvo_p8 = table.Column<double>(type: "double precision", nullable: true),
                    hkvo_p9 = table.Column<int>(type: "integer", nullable: true),
                    zaehlerid = table.Column<int>(type: "integer", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hkvo", x => x.hkvoid);
                    table.ForeignKey(
                        name: "fk_hkvo_zaehlerset_zaehlerid",
                        column: x => x.zaehlerid,
                        principalTable: "zaehlerset",
                        principalColumn: "zaehlerid");
                });

            migrationBuilder.CreateTable(
                name: "zaehlerstaende",
                columns: table => new
                {
                    zaehlerstandid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zaehlerid = table.Column<int>(type: "integer", nullable: false),
                    datum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    stand = table.Column<double>(type: "double precision", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zaehlerstaende", x => x.zaehlerstandid);
                    table.ForeignKey(
                        name: "fk_zaehlerstaende_zaehlerset_zaehlerid",
                        column: x => x.zaehlerid,
                        principalTable: "zaehlerset",
                        principalColumn: "zaehlerid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "umlagen",
                columns: table => new
                {
                    umlageid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    typ = table.Column<int>(type: "integer", nullable: false),
                    schluessel = table.Column<int>(type: "integer", nullable: false),
                    beschreibung = table.Column<string>(type: "text", nullable: true),
                    notiz = table.Column<string>(type: "text", nullable: true),
                    hkvoid = table.Column<int>(type: "integer", nullable: true),
                    zaehlerid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_umlagen", x => x.umlageid);
                    table.ForeignKey(
                        name: "fk_umlagen_hkvo_hkvoid",
                        column: x => x.hkvoid,
                        principalTable: "hkvo",
                        principalColumn: "hkvoid");
                    table.ForeignKey(
                        name: "fk_umlagen_zaehlerset_zaehlerid",
                        column: x => x.zaehlerid,
                        principalTable: "zaehlerset",
                        principalColumn: "zaehlerid");
                });

            migrationBuilder.CreateTable(
                name: "betriebskostenrechnungen",
                columns: table => new
                {
                    betriebskostenrechnungid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    betrag = table.Column<double>(type: "double precision", nullable: false),
                    datum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    betreffendesjahr = table.Column<int>(type: "integer", nullable: false),
                    umlageid = table.Column<int>(type: "integer", nullable: false),
                    notiz = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_betriebskostenrechnungen", x => x.betriebskostenrechnungid);
                    table.ForeignKey(
                        name: "fk_betriebskostenrechnungen_umlagen_umlageid",
                        column: x => x.umlageid,
                        principalTable: "umlagen",
                        principalColumn: "umlageid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "umlagewohnung",
                columns: table => new
                {
                    umlagenumlageid = table.Column<int>(type: "integer", nullable: false),
                    wohnungenwohnungid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_umlagewohnung", x => new { x.umlagenumlageid, x.wohnungenwohnungid });
                    table.ForeignKey(
                        name: "fk_umlagewohnung_umlagen_umlagenumlageid",
                        column: x => x.umlagenumlageid,
                        principalTable: "umlagen",
                        principalColumn: "umlageid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_umlagewohnung_wohnungen_wohnungenwohnungid",
                        column: x => x.wohnungenwohnungid,
                        principalTable: "wohnungen",
                        principalColumn: "wohnungid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_betriebskostenrechnungen_umlageid",
                table: "betriebskostenrechnungen",
                column: "umlageid");

            migrationBuilder.CreateIndex(
                name: "ix_erhaltungsaufwendungen_wohnungid",
                table: "erhaltungsaufwendungen",
                column: "wohnungid");

            migrationBuilder.CreateIndex(
                name: "ix_garagen_adresseid",
                table: "garagen",
                column: "adresseid");

            migrationBuilder.CreateIndex(
                name: "ix_garagen_juristischepersonid",
                table: "garagen",
                column: "juristischepersonid");

            migrationBuilder.CreateIndex(
                name: "ix_garagevertrag_vertraegevertragid",
                table: "garagevertrag",
                column: "vertraegevertragid");

            migrationBuilder.CreateIndex(
                name: "ix_hkvo_zaehlerid",
                table: "hkvo",
                column: "zaehlerid");

            migrationBuilder.CreateIndex(
                name: "ix_juristischepersonen_adresseid",
                table: "juristischepersonen",
                column: "adresseid");

            migrationBuilder.CreateIndex(
                name: "ix_juristischepersonjuristischeperson_juristischepersonenjuris~",
                table: "juristischepersonjuristischeperson",
                column: "juristischepersonenjuristischepersonid");

            migrationBuilder.CreateIndex(
                name: "ix_juristischepersonnatuerlicheperson_natuerlichemitgliedernat~",
                table: "juristischepersonnatuerlicheperson",
                column: "natuerlichemitgliedernatuerlichepersonid");

            migrationBuilder.CreateIndex(
                name: "ix_mieten_vertragid",
                table: "mieten",
                column: "vertragid");

            migrationBuilder.CreateIndex(
                name: "ix_mieterset_vertragid",
                table: "mieterset",
                column: "vertragid");

            migrationBuilder.CreateIndex(
                name: "ix_mietminderungen_vertragid",
                table: "mietminderungen",
                column: "vertragid");

            migrationBuilder.CreateIndex(
                name: "ix_natuerlichepersonen_adresseid",
                table: "natuerlichepersonen",
                column: "adresseid");

            migrationBuilder.CreateIndex(
                name: "ix_pbkdf2passwordcredentials_userid",
                table: "pbkdf2passwordcredentials",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_hkvoid",
                table: "umlagen",
                column: "hkvoid");

            migrationBuilder.CreateIndex(
                name: "ix_umlagen_zaehlerid",
                table: "umlagen",
                column: "zaehlerid");

            migrationBuilder.CreateIndex(
                name: "ix_umlagewohnung_wohnungenwohnungid",
                table: "umlagewohnung",
                column: "wohnungenwohnungid");

            migrationBuilder.CreateIndex(
                name: "ix_useraccounts_username",
                table: "useraccounts",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vertraege_wohnungid",
                table: "vertraege",
                column: "wohnungid");

            migrationBuilder.CreateIndex(
                name: "ix_vertragversionen_vertragid",
                table: "vertragversionen",
                column: "vertragid");

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_adresseid",
                table: "wohnungen",
                column: "adresseid");

            migrationBuilder.CreateIndex(
                name: "ix_wohnungen_juristischepersonid",
                table: "wohnungen",
                column: "juristischepersonid");

            migrationBuilder.CreateIndex(
                name: "ix_zaehlerset_adresseid",
                table: "zaehlerset",
                column: "adresseid");

            migrationBuilder.CreateIndex(
                name: "ix_zaehlerset_allgemeinzaehlerzaehlerid",
                table: "zaehlerset",
                column: "allgemeinzaehlerzaehlerid");

            migrationBuilder.CreateIndex(
                name: "ix_zaehlerset_wohnungid",
                table: "zaehlerset",
                column: "wohnungid");

            migrationBuilder.CreateIndex(
                name: "ix_zaehlerstaende_zaehlerid",
                table: "zaehlerstaende",
                column: "zaehlerid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "betriebskostenrechnungen");

            migrationBuilder.DropTable(
                name: "erhaltungsaufwendungen");

            migrationBuilder.DropTable(
                name: "garagevertrag");

            migrationBuilder.DropTable(
                name: "juristischepersonjuristischeperson");

            migrationBuilder.DropTable(
                name: "juristischepersonnatuerlicheperson");

            migrationBuilder.DropTable(
                name: "kontos");

            migrationBuilder.DropTable(
                name: "mieten");

            migrationBuilder.DropTable(
                name: "mieterset");

            migrationBuilder.DropTable(
                name: "mietminderungen");

            migrationBuilder.DropTable(
                name: "pbkdf2passwordcredentials");

            migrationBuilder.DropTable(
                name: "umlagewohnung");

            migrationBuilder.DropTable(
                name: "vertragversionen");

            migrationBuilder.DropTable(
                name: "zaehlerstaende");

            migrationBuilder.DropTable(
                name: "garagen");

            migrationBuilder.DropTable(
                name: "natuerlichepersonen");

            migrationBuilder.DropTable(
                name: "useraccounts");

            migrationBuilder.DropTable(
                name: "umlagen");

            migrationBuilder.DropTable(
                name: "vertraege");

            migrationBuilder.DropTable(
                name: "hkvo");

            migrationBuilder.DropTable(
                name: "zaehlerset");

            migrationBuilder.DropTable(
                name: "wohnungen");

            migrationBuilder.DropTable(
                name: "juristischepersonen");

            migrationBuilder.DropTable(
                name: "adressen");
        }
    }
}
