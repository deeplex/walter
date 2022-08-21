using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Deeplex.Saverwalter.Model.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adressen",
                columns: table => new
                {
                    AdresseId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hausnummer = table.Column<string>(type: "TEXT", nullable: false),
                    Strasse = table.Column<string>(type: "TEXT", nullable: false),
                    Postleitzahl = table.Column<string>(type: "TEXT", nullable: false),
                    Stadt = table.Column<string>(type: "TEXT", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adressen", x => x.AdresseId);
                });

            migrationBuilder.CreateTable(
                name: "Anhaenge",
                columns: table => new
                {
                    AnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    Sha256Hash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anhaenge", x => x.AnhangId);
                });

            migrationBuilder.CreateTable(
                name: "Kontos",
                columns: table => new
                {
                    KontoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bank = table.Column<string>(type: "TEXT", nullable: false),
                    Iban = table.Column<string>(type: "TEXT", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kontos", x => x.KontoId);
                });

            migrationBuilder.CreateTable(
                name: "JuristischePersonen",
                columns: table => new
                {
                    JuristischePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    Mobil = table.Column<string>(type: "TEXT", nullable: true),
                    Fax = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    AdresseId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true),
                    Anrede = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuristischePersonen", x => x.JuristischePersonId);
                    table.UniqueConstraint("AK_JuristischePersonen_PersonId", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_JuristischePersonen_Adressen_AdresseId",
                        column: x => x.AdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NatuerlichePersonen",
                columns: table => new
                {
                    NatuerlichePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Vorname = table.Column<string>(type: "TEXT", nullable: true),
                    Nachname = table.Column<string>(type: "TEXT", nullable: false),
                    Titel = table.Column<int>(type: "INTEGER", nullable: false),
                    Anrede = table.Column<int>(type: "INTEGER", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    Mobil = table.Column<string>(type: "TEXT", nullable: true),
                    Fax = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    AdresseId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NatuerlichePersonen", x => x.NatuerlichePersonId);
                    table.UniqueConstraint("AK_NatuerlichePersonen_PersonId", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_NatuerlichePersonen_Adressen_AdresseId",
                        column: x => x.AdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdresseAnhang",
                columns: table => new
                {
                    AdressenAdresseId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdresseAnhang", x => new { x.AdressenAdresseId, x.AnhaengeAnhangId });
                    table.ForeignKey(
                        name: "FK_AdresseAnhang_Adressen_AdressenAdresseId",
                        column: x => x.AdressenAdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdresseAnhang_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangKonto",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KontenKontoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangKonto", x => new { x.AnhaengeAnhangId, x.KontenKontoId });
                    table.ForeignKey(
                        name: "FK_AnhangKonto_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangKonto_Kontos_KontenKontoId",
                        column: x => x.KontenKontoId,
                        principalTable: "Kontos",
                        principalColumn: "KontoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangJuristischePerson",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JuristischePersonenJuristischePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangJuristischePerson", x => new { x.AnhaengeAnhangId, x.JuristischePersonenJuristischePersonId });
                    table.ForeignKey(
                        name: "FK_AnhangJuristischePerson_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangJuristischePerson_JuristischePersonen_JuristischePersonenJuristischePersonId",
                        column: x => x.JuristischePersonenJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Garagen",
                columns: table => new
                {
                    GarageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdresseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kennung = table.Column<string>(type: "TEXT", nullable: false),
                    BesitzerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true),
                    JuristischePersonId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Garagen", x => x.GarageId);
                    table.ForeignKey(
                        name: "FK_Garagen_Adressen_AdresseId",
                        column: x => x.AdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Garagen_JuristischePersonen_JuristischePersonId",
                        column: x => x.JuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "Wohnungen",
                columns: table => new
                {
                    WohnungId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdresseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", nullable: false),
                    BesitzerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Wohnflaeche = table.Column<double>(type: "REAL", nullable: false),
                    Nutzflaeche = table.Column<double>(type: "REAL", nullable: false),
                    Nutzeinheit = table.Column<int>(type: "INTEGER", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true),
                    JuristischePersonId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wohnungen", x => x.WohnungId);
                    table.ForeignKey(
                        name: "FK_Wohnungen_Adressen_AdresseId",
                        column: x => x.AdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wohnungen_JuristischePersonen_JuristischePersonId",
                        column: x => x.JuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnhangNatuerlichePerson",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NatuerlichePersonenNatuerlichePersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangNatuerlichePerson", x => new { x.AnhaengeAnhangId, x.NatuerlichePersonenNatuerlichePersonId });
                    table.ForeignKey(
                        name: "FK_AnhangNatuerlichePerson_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangNatuerlichePerson_NatuerlichePersonen_NatuerlichePersonenNatuerlichePersonId",
                        column: x => x.NatuerlichePersonenNatuerlichePersonId,
                        principalTable: "NatuerlichePersonen",
                        principalColumn: "NatuerlichePersonId",
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

            migrationBuilder.CreateTable(
                name: "AnhangGarage",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GaragenGarageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangGarage", x => new { x.AnhaengeAnhangId, x.GaragenGarageId });
                    table.ForeignKey(
                        name: "FK_AnhangGarage_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangGarage_Garagen_GaragenGarageId",
                        column: x => x.GaragenGarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangWohnung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WohnungenWohnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangWohnung", x => new { x.AnhaengeAnhangId, x.WohnungenWohnungId });
                    table.ForeignKey(
                        name: "FK_AnhangWohnung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangWohnung_Wohnungen_WohnungenWohnungId",
                        column: x => x.WohnungenWohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Erhaltungsaufwendungen",
                columns: table => new
                {
                    ErhaltungsaufwendungId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AusstellerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", nullable: false),
                    Betrag = table.Column<double>(type: "REAL", nullable: false),
                    WohnungId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Erhaltungsaufwendungen", x => x.ErhaltungsaufwendungId);
                    table.ForeignKey(
                        name: "FK_Erhaltungsaufwendungen_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vertraege",
                columns: table => new
                {
                    VertragId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WohnungId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnsprechpartnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true),
                    Ende = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vertraege", x => x.VertragId);
                    table.ForeignKey(
                        name: "FK_Vertraege_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZaehlerSet",
                columns: table => new
                {
                    ZaehlerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kennnummer = table.Column<string>(type: "TEXT", nullable: false),
                    AllgemeinzaehlerZaehlerId = table.Column<int>(type: "INTEGER", nullable: true),
                    WohnungId = table.Column<int>(type: "INTEGER", nullable: true),
                    Typ = table.Column<int>(type: "INTEGER", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaehlerSet", x => x.ZaehlerId);
                    table.ForeignKey(
                        name: "FK_ZaehlerSet_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZaehlerSet_ZaehlerSet_AllgemeinzaehlerZaehlerId",
                        column: x => x.AllgemeinzaehlerZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnhangErhaltungsaufwendung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ErhaltungsaufwendungenErhaltungsaufwendungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangErhaltungsaufwendung", x => new { x.AnhaengeAnhangId, x.ErhaltungsaufwendungenErhaltungsaufwendungId });
                    table.ForeignKey(
                        name: "FK_AnhangErhaltungsaufwendung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangErhaltungsaufwendung_Erhaltungsaufwendungen_ErhaltungsaufwendungenErhaltungsaufwendungId",
                        column: x => x.ErhaltungsaufwendungenErhaltungsaufwendungId,
                        principalTable: "Erhaltungsaufwendungen",
                        principalColumn: "ErhaltungsaufwendungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangVertrag",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VertraegeVertragId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangVertrag", x => new { x.AnhaengeAnhangId, x.VertraegeVertragId });
                    table.ForeignKey(
                        name: "FK_AnhangVertrag_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangVertrag_Vertraege_VertraegeVertragId",
                        column: x => x.VertraegeVertragId,
                        principalTable: "Vertraege",
                        principalColumn: "VertragId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GarageVertrag",
                columns: table => new
                {
                    GaragenGarageId = table.Column<int>(type: "INTEGER", nullable: false),
                    VertraegeVertragId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarageVertrag", x => new { x.GaragenGarageId, x.VertraegeVertragId });
                    table.ForeignKey(
                        name: "FK_GarageVertrag_Garagen_GaragenGarageId",
                        column: x => x.GaragenGarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarageVertrag_Vertraege_VertraegeVertragId",
                        column: x => x.VertraegeVertragId,
                        principalTable: "Vertraege",
                        principalColumn: "VertragId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mieten",
                columns: table => new
                {
                    MieteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<int>(type: "INTEGER", nullable: true),
                    Zahlungsdatum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BetreffenderMonat = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Betrag = table.Column<double>(type: "REAL", nullable: true),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mieten", x => x.MieteId);
                    table.ForeignKey(
                        name: "FK_Mieten_Vertraege_VertragId",
                        column: x => x.VertragId,
                        principalTable: "Vertraege",
                        principalColumn: "VertragId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MieterSet",
                columns: table => new
                {
                    MieterId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VertragId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MieterSet", x => x.MieterId);
                    table.ForeignKey(
                        name: "FK_MieterSet_Vertraege_VertragId",
                        column: x => x.VertragId,
                        principalTable: "Vertraege",
                        principalColumn: "VertragId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MietMinderungen",
                columns: table => new
                {
                    MietminderungId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<int>(type: "INTEGER", nullable: true),
                    Beginn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ende = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Minderung = table.Column<double>(type: "REAL", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MietMinderungen", x => x.MietminderungId);
                    table.ForeignKey(
                        name: "FK_MietMinderungen_Vertraege_VertragId",
                        column: x => x.VertragId,
                        principalTable: "Vertraege",
                        principalColumn: "VertragId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VertragVersionen",
                columns: table => new
                {
                    VertragVersionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Personenzahl = table.Column<int>(type: "INTEGER", nullable: false),
                    VertragId = table.Column<int>(type: "INTEGER", nullable: false),
                    Beginn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Grundmiete = table.Column<double>(type: "REAL", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VertragVersionen", x => x.VertragVersionId);
                    table.ForeignKey(
                        name: "FK_VertragVersionen_Vertraege_VertragId",
                        column: x => x.VertragId,
                        principalTable: "Vertraege",
                        principalColumn: "VertragId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangZaehler",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ZaehlerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangZaehler", x => new { x.AnhaengeAnhangId, x.ZaehlerId });
                    table.ForeignKey(
                        name: "FK_AnhangZaehler_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangZaehler_ZaehlerSet_ZaehlerId",
                        column: x => x.ZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "Zaehlerstaende",
                columns: table => new
                {
                    ZaehlerstandId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZaehlerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Stand = table.Column<double>(type: "REAL", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zaehlerstaende", x => x.ZaehlerstandId);
                    table.ForeignKey(
                        name: "FK_Zaehlerstaende_ZaehlerSet_ZaehlerId",
                        column: x => x.ZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangMiete",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MietenMieteId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangMiete", x => new { x.AnhaengeAnhangId, x.MietenMieteId });
                    table.ForeignKey(
                        name: "FK_AnhangMiete_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangMiete_Mieten_MietenMieteId",
                        column: x => x.MietenMieteId,
                        principalTable: "Mieten",
                        principalColumn: "MieteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangMietminderung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MietminderungenMietminderungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangMietminderung", x => new { x.AnhaengeAnhangId, x.MietminderungenMietminderungId });
                    table.ForeignKey(
                        name: "FK_AnhangMietminderung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangMietminderung_MietMinderungen_MietminderungenMietminderungId",
                        column: x => x.MietminderungenMietminderungId,
                        principalTable: "MietMinderungen",
                        principalColumn: "MietminderungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangVertragVersion",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VertragVersionenVertragVersionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangVertragVersion", x => new { x.AnhaengeAnhangId, x.VertragVersionenVertragVersionId });
                    table.ForeignKey(
                        name: "FK_AnhangVertragVersion_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangVertragVersion_VertragVersionen_VertragVersionenVertragVersionId",
                        column: x => x.VertragVersionenVertragVersionId,
                        principalTable: "VertragVersionen",
                        principalColumn: "VertragVersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Umlagen",
                columns: table => new
                {
                    UmlageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Typ = table.Column<int>(type: "INTEGER", nullable: false),
                    Schluessel = table.Column<int>(type: "INTEGER", nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", nullable: true),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true),
                    HKVOId = table.Column<int>(type: "INTEGER", nullable: true),
                    ZaehlerId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Umlagen", x => x.UmlageId);
                    table.ForeignKey(
                        name: "FK_Umlagen_HKVO_HKVOId",
                        column: x => x.HKVOId,
                        principalTable: "HKVO",
                        principalColumn: "HKVOId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Umlagen_ZaehlerSet_ZaehlerId",
                        column: x => x.ZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnhangZaehlerstand",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ZaehlerstaendeZaehlerstandId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangZaehlerstand", x => new { x.AnhaengeAnhangId, x.ZaehlerstaendeZaehlerstandId });
                    table.ForeignKey(
                        name: "FK_AnhangZaehlerstand_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangZaehlerstand_Zaehlerstaende_ZaehlerstaendeZaehlerstandId",
                        column: x => x.ZaehlerstaendeZaehlerstandId,
                        principalTable: "Zaehlerstaende",
                        principalColumn: "ZaehlerstandId",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_AnhangUmlage_Umlagen_UmlagenUmlageId",
                        column: x => x.UmlagenUmlageId,
                        principalTable: "Umlagen",
                        principalColumn: "UmlageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Betriebskostenrechnungen",
                columns: table => new
                {
                    BetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Betrag = table.Column<double>(type: "REAL", nullable: false),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BetreffendesJahr = table.Column<int>(type: "INTEGER", nullable: false),
                    UmlageId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notiz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Betriebskostenrechnungen", x => x.BetriebskostenrechnungId);
                    table.ForeignKey(
                        name: "FK_Betriebskostenrechnungen_Umlagen_UmlageId",
                        column: x => x.UmlageId,
                        principalTable: "Umlagen",
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
                        name: "FK_UmlageWohnung_Umlagen_UmlagenUmlageId",
                        column: x => x.UmlagenUmlageId,
                        principalTable: "Umlagen",
                        principalColumn: "UmlageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UmlageWohnung_Wohnungen_WohnungenWohnungId",
                        column: x => x.WohnungenWohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnhangBetriebskostenrechnung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BetriebskostenrechnungenBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangBetriebskostenrechnung", x => new { x.AnhaengeAnhangId, x.BetriebskostenrechnungenBetriebskostenrechnungId });
                    table.ForeignKey(
                        name: "FK_AnhangBetriebskostenrechnung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangBetriebskostenrechnung_Betriebskostenrechnungen_BetriebskostenrechnungenBetriebskostenrechnungId",
                        column: x => x.BetriebskostenrechnungenBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VertragsBetriebskostenrechnung",
                columns: table => new
                {
                    VertragsBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<int>(type: "INTEGER", nullable: true),
                    RechnungBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VertragsBetriebskostenrechnung", x => x.VertragsBetriebskostenrechnungId);
                    table.ForeignKey(
                        name: "FK_VertragsBetriebskostenrechnung_Betriebskostenrechnungen_RechnungBetriebskostenrechnungId",
                        column: x => x.RechnungBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VertragsBetriebskostenrechnung_Vertraege_VertragId",
                        column: x => x.VertragId,
                        principalTable: "Vertraege",
                        principalColumn: "VertragId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnhangVertragsBetriebskostenrechnung",
                columns: table => new
                {
                    AnhaengeAnhangId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhangVertragsBetriebskostenrechnung", x => new { x.AnhaengeAnhangId, x.VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId });
                    table.ForeignKey(
                        name: "FK_AnhangVertragsBetriebskostenrechnung_Anhaenge_AnhaengeAnhangId",
                        column: x => x.AnhaengeAnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnhangVertragsBetriebskostenrechnung_VertragsBetriebskostenrechnung_VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId",
                        column: x => x.VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId,
                        principalTable: "VertragsBetriebskostenrechnung",
                        principalColumn: "VertragsBetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdresseAnhang_AnhaengeAnhangId",
                table: "AdresseAnhang",
                column: "AnhaengeAnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangBetriebskostenrechnung_BetriebskostenrechnungenBetriebskostenrechnungId",
                table: "AnhangBetriebskostenrechnung",
                column: "BetriebskostenrechnungenBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangErhaltungsaufwendung_ErhaltungsaufwendungenErhaltungsaufwendungId",
                table: "AnhangErhaltungsaufwendung",
                column: "ErhaltungsaufwendungenErhaltungsaufwendungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangGarage_GaragenGarageId",
                table: "AnhangGarage",
                column: "GaragenGarageId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangJuristischePerson_JuristischePersonenJuristischePersonId",
                table: "AnhangJuristischePerson",
                column: "JuristischePersonenJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangKonto_KontenKontoId",
                table: "AnhangKonto",
                column: "KontenKontoId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangMiete_MietenMieteId",
                table: "AnhangMiete",
                column: "MietenMieteId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangMietminderung_MietminderungenMietminderungId",
                table: "AnhangMietminderung",
                column: "MietminderungenMietminderungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangNatuerlichePerson_NatuerlichePersonenNatuerlichePersonId",
                table: "AnhangNatuerlichePerson",
                column: "NatuerlichePersonenNatuerlichePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangUmlage_UmlagenUmlageId",
                table: "AnhangUmlage",
                column: "UmlagenUmlageId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangVertrag_VertraegeVertragId",
                table: "AnhangVertrag",
                column: "VertraegeVertragId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangVertragsBetriebskostenrechnung_VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId",
                table: "AnhangVertragsBetriebskostenrechnung",
                column: "VertragsBetriebskostenrechnungenVertragsBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangVertragVersion_VertragVersionenVertragVersionId",
                table: "AnhangVertragVersion",
                column: "VertragVersionenVertragVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangWohnung_WohnungenWohnungId",
                table: "AnhangWohnung",
                column: "WohnungenWohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangZaehler_ZaehlerId",
                table: "AnhangZaehler",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhangZaehlerstand_ZaehlerstaendeZaehlerstandId",
                table: "AnhangZaehlerstand",
                column: "ZaehlerstaendeZaehlerstandId");

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungen_UmlageId",
                table: "Betriebskostenrechnungen",
                column: "UmlageId");

            migrationBuilder.CreateIndex(
                name: "IX_Erhaltungsaufwendungen_WohnungId",
                table: "Erhaltungsaufwendungen",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Garagen_AdresseId",
                table: "Garagen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Garagen_JuristischePersonId",
                table: "Garagen",
                column: "JuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageVertrag_VertraegeVertragId",
                table: "GarageVertrag",
                column: "VertraegeVertragId");

            migrationBuilder.CreateIndex(
                name: "IX_HKVO_ZaehlerId",
                table: "HKVO",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonen_AdresseId",
                table: "JuristischePersonen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonJuristischePerson_JuristischePersonenJuristischePersonId",
                table: "JuristischePersonJuristischePerson",
                column: "JuristischePersonenJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonNatuerlichePerson_NatuerlicheMitgliederNatuerlichePersonId",
                table: "JuristischePersonNatuerlichePerson",
                column: "NatuerlicheMitgliederNatuerlichePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Mieten_VertragId",
                table: "Mieten",
                column: "VertragId");

            migrationBuilder.CreateIndex(
                name: "IX_MieterSet_VertragId",
                table: "MieterSet",
                column: "VertragId");

            migrationBuilder.CreateIndex(
                name: "IX_MietMinderungen_VertragId",
                table: "MietMinderungen",
                column: "VertragId");

            migrationBuilder.CreateIndex(
                name: "IX_NatuerlichePersonen_AdresseId",
                table: "NatuerlichePersonen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Umlagen_HKVOId",
                table: "Umlagen",
                column: "HKVOId");

            migrationBuilder.CreateIndex(
                name: "IX_Umlagen_ZaehlerId",
                table: "Umlagen",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_UmlageWohnung_WohnungenWohnungId",
                table: "UmlageWohnung",
                column: "WohnungenWohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Vertraege_WohnungId",
                table: "Vertraege",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_VertragsBetriebskostenrechnung_RechnungBetriebskostenrechnungId",
                table: "VertragsBetriebskostenrechnung",
                column: "RechnungBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_VertragsBetriebskostenrechnung_VertragId",
                table: "VertragsBetriebskostenrechnung",
                column: "VertragId");

            migrationBuilder.CreateIndex(
                name: "IX_VertragVersionen_VertragId",
                table: "VertragVersionen",
                column: "VertragId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_AdresseId",
                table: "Wohnungen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_JuristischePersonId",
                table: "Wohnungen",
                column: "JuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerSet_AllgemeinzaehlerZaehlerId",
                table: "ZaehlerSet",
                column: "AllgemeinzaehlerZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerSet_WohnungId",
                table: "ZaehlerSet",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Zaehlerstaende_ZaehlerId",
                table: "Zaehlerstaende",
                column: "ZaehlerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdresseAnhang");

            migrationBuilder.DropTable(
                name: "AnhangBetriebskostenrechnung");

            migrationBuilder.DropTable(
                name: "AnhangErhaltungsaufwendung");

            migrationBuilder.DropTable(
                name: "AnhangGarage");

            migrationBuilder.DropTable(
                name: "AnhangJuristischePerson");

            migrationBuilder.DropTable(
                name: "AnhangKonto");

            migrationBuilder.DropTable(
                name: "AnhangMiete");

            migrationBuilder.DropTable(
                name: "AnhangMietminderung");

            migrationBuilder.DropTable(
                name: "AnhangNatuerlichePerson");

            migrationBuilder.DropTable(
                name: "AnhangUmlage");

            migrationBuilder.DropTable(
                name: "AnhangVertrag");

            migrationBuilder.DropTable(
                name: "AnhangVertragsBetriebskostenrechnung");

            migrationBuilder.DropTable(
                name: "AnhangVertragVersion");

            migrationBuilder.DropTable(
                name: "AnhangWohnung");

            migrationBuilder.DropTable(
                name: "AnhangZaehler");

            migrationBuilder.DropTable(
                name: "AnhangZaehlerstand");

            migrationBuilder.DropTable(
                name: "GarageVertrag");

            migrationBuilder.DropTable(
                name: "JuristischePersonJuristischePerson");

            migrationBuilder.DropTable(
                name: "JuristischePersonNatuerlichePerson");

            migrationBuilder.DropTable(
                name: "MieterSet");

            migrationBuilder.DropTable(
                name: "UmlageWohnung");

            migrationBuilder.DropTable(
                name: "Erhaltungsaufwendungen");

            migrationBuilder.DropTable(
                name: "Kontos");

            migrationBuilder.DropTable(
                name: "Mieten");

            migrationBuilder.DropTable(
                name: "MietMinderungen");

            migrationBuilder.DropTable(
                name: "VertragsBetriebskostenrechnung");

            migrationBuilder.DropTable(
                name: "VertragVersionen");

            migrationBuilder.DropTable(
                name: "Anhaenge");

            migrationBuilder.DropTable(
                name: "Zaehlerstaende");

            migrationBuilder.DropTable(
                name: "Garagen");

            migrationBuilder.DropTable(
                name: "NatuerlichePersonen");

            migrationBuilder.DropTable(
                name: "Betriebskostenrechnungen");

            migrationBuilder.DropTable(
                name: "Vertraege");

            migrationBuilder.DropTable(
                name: "Umlagen");

            migrationBuilder.DropTable(
                name: "HKVO");

            migrationBuilder.DropTable(
                name: "ZaehlerSet");

            migrationBuilder.DropTable(
                name: "Wohnungen");

            migrationBuilder.DropTable(
                name: "JuristischePersonen");

            migrationBuilder.DropTable(
                name: "Adressen");
        }
    }
}
