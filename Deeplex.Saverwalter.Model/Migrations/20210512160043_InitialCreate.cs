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
                    AdresseId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hausnummer = table.Column<string>(nullable: false),
                    Strasse = table.Column<string>(nullable: false),
                    Postleitzahl = table.Column<string>(nullable: false),
                    Stadt = table.Column<string>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adressen", x => x.AdresseId);
                });

            migrationBuilder.CreateTable(
                name: "Anhaenge",
                columns: table => new
                {
                    AnhangId = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(nullable: false),
                    Sha256Hash = table.Column<byte[]>(nullable: false),
                    ContentType = table.Column<string>(nullable: true),
                    Content = table.Column<byte[]>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anhaenge", x => x.AnhangId);
                });

            migrationBuilder.CreateTable(
                name: "Kontos",
                columns: table => new
                {
                    KontoId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bank = table.Column<string>(nullable: false),
                    Iban = table.Column<string>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kontos", x => x.KontoId);
                });

            migrationBuilder.CreateTable(
                name: "Mieten",
                columns: table => new
                {
                    MieteId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(nullable: false),
                    Zahlungsdatum = table.Column<DateTime>(nullable: false),
                    BetreffenderMonat = table.Column<DateTime>(nullable: false),
                    Betrag = table.Column<double>(nullable: true),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mieten", x => x.MieteId);
                });

            migrationBuilder.CreateTable(
                name: "MieterSet",
                columns: table => new
                {
                    MieterId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<Guid>(nullable: false),
                    VertragId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MieterSet", x => x.MieterId);
                });

            migrationBuilder.CreateTable(
                name: "MietMinderungen",
                columns: table => new
                {
                    MietMinderungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(nullable: false),
                    Beginn = table.Column<DateTime>(nullable: false),
                    Ende = table.Column<DateTime>(nullable: true),
                    Minderung = table.Column<double>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MietMinderungen", x => x.MietMinderungId);
                });

            migrationBuilder.CreateTable(
                name: "JuristischePersonen",
                columns: table => new
                {
                    JuristischePersonId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<Guid>(nullable: false),
                    Bezeichnung = table.Column<string>(nullable: false),
                    isVermieter = table.Column<bool>(nullable: false),
                    isMieter = table.Column<bool>(nullable: false),
                    isHandwerker = table.Column<bool>(nullable: false),
                    Telefon = table.Column<string>(nullable: true),
                    Mobil = table.Column<string>(nullable: true),
                    Fax = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    AdresseId = table.Column<int>(nullable: true),
                    Notiz = table.Column<string>(nullable: true),
                    Anrede = table.Column<int>(nullable: false)
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
                    NatuerlichePersonId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<Guid>(nullable: false),
                    Vorname = table.Column<string>(nullable: true),
                    Nachname = table.Column<string>(nullable: false),
                    Titel = table.Column<int>(nullable: false),
                    isVermieter = table.Column<bool>(nullable: false),
                    isMieter = table.Column<bool>(nullable: false),
                    isHandwerker = table.Column<bool>(nullable: false),
                    Anrede = table.Column<int>(nullable: false),
                    Telefon = table.Column<string>(nullable: true),
                    Mobil = table.Column<string>(nullable: true),
                    Fax = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    AdresseId = table.Column<int>(nullable: true),
                    Notiz = table.Column<string>(nullable: true)
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
                name: "AdresseAnhaenge",
                columns: table => new
                {
                    AdresseAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetAdresseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdresseAnhaenge", x => x.AdresseAnhangId);
                    table.ForeignKey(
                        name: "FK_AdresseAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdresseAnhaenge_Adressen_TargetAdresseId",
                        column: x => x.TargetAdresseId,
                        principalTable: "Adressen",
                        principalColumn: "AdresseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VertragAnhaenge",
                columns: table => new
                {
                    VertragAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    Target = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VertragAnhaenge", x => x.VertragAnhangId);
                    table.ForeignKey(
                        name: "FK_VertragAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KontoAnhaenge",
                columns: table => new
                {
                    KontoAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetKontoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KontoAnhaenge", x => x.KontoAnhangId);
                    table.ForeignKey(
                        name: "FK_KontoAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KontoAnhaenge_Kontos_TargetKontoId",
                        column: x => x.TargetKontoId,
                        principalTable: "Kontos",
                        principalColumn: "KontoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MieteAnhaenge",
                columns: table => new
                {
                    MieteAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetMieteId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MieteAnhaenge", x => x.MieteAnhangId);
                    table.ForeignKey(
                        name: "FK_MieteAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MieteAnhaenge_Mieten_TargetMieteId",
                        column: x => x.TargetMieteId,
                        principalTable: "Mieten",
                        principalColumn: "MieteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MietMinderungAnhaenge",
                columns: table => new
                {
                    MietMinderungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetMietMinderungId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MietMinderungAnhaenge", x => x.MietMinderungAnhangId);
                    table.ForeignKey(
                        name: "FK_MietMinderungAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MietMinderungAnhaenge_MietMinderungen_TargetMietMinderungId",
                        column: x => x.TargetMietMinderungId,
                        principalTable: "MietMinderungen",
                        principalColumn: "MietMinderungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Garagen",
                columns: table => new
                {
                    GarageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdresseId = table.Column<int>(nullable: false),
                    Kennung = table.Column<string>(nullable: false),
                    BesitzerId = table.Column<Guid>(nullable: false),
                    Notiz = table.Column<string>(nullable: true),
                    JuristischePersonId = table.Column<int>(nullable: true)
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
                name: "JuristischePersonAnhaenge",
                columns: table => new
                {
                    JuristischePersonAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetJuristischePersonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuristischePersonAnhaenge", x => x.JuristischePersonAnhangId);
                    table.ForeignKey(
                        name: "FK_JuristischePersonAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JuristischePersonAnhaenge_JuristischePersonen_TargetJuristischePersonId",
                        column: x => x.TargetJuristischePersonId,
                        principalTable: "JuristischePersonen",
                        principalColumn: "JuristischePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wohnungen",
                columns: table => new
                {
                    WohnungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdresseId = table.Column<int>(nullable: false),
                    Bezeichnung = table.Column<string>(nullable: false),
                    BesitzerId = table.Column<Guid>(nullable: false),
                    Wohnflaeche = table.Column<double>(nullable: false),
                    Nutzflaeche = table.Column<double>(nullable: false),
                    Nutzeinheit = table.Column<int>(nullable: false),
                    Notiz = table.Column<string>(nullable: true),
                    JuristischePersonId = table.Column<int>(nullable: true)
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
                name: "JuristischePersonenMitglieder",
                columns: table => new
                {
                    JuristischePersonenMitgliedId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<Guid>(nullable: false),
                    JuristischePersonId = table.Column<int>(nullable: false),
                    NatuerlichePersonId = table.Column<int>(nullable: true)
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

            migrationBuilder.CreateTable(
                name: "NatuerlichePersonAnhaenge",
                columns: table => new
                {
                    NatuerlichePersonAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetNatuerlichePersonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NatuerlichePersonAnhaenge", x => x.NatuerlichePersonAnhangId);
                    table.ForeignKey(
                        name: "FK_NatuerlichePersonAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NatuerlichePersonAnhaenge_NatuerlichePersonen_TargetNatuerlichePersonId",
                        column: x => x.TargetNatuerlichePersonId,
                        principalTable: "NatuerlichePersonen",
                        principalColumn: "NatuerlichePersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GarageAnhaenge",
                columns: table => new
                {
                    GarageAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetGarageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarageAnhaenge", x => x.GarageAnhangId);
                    table.ForeignKey(
                        name: "FK_GarageAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarageAnhaenge_Garagen_TargetGarageId",
                        column: x => x.TargetGarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MietobjektGaragen",
                columns: table => new
                {
                    MietobjektGarageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(nullable: false),
                    GarageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MietobjektGaragen", x => x.MietobjektGarageId);
                    table.ForeignKey(
                        name: "FK_MietobjektGaragen_Garagen_GarageId",
                        column: x => x.GarageId,
                        principalTable: "Garagen",
                        principalColumn: "GarageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Erhaltungsaufwendungen",
                columns: table => new
                {
                    ErhaltungsaufwendungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Datum = table.Column<DateTime>(nullable: false),
                    AusstellerId = table.Column<Guid>(nullable: false),
                    Bezeichnung = table.Column<string>(nullable: false),
                    Betrag = table.Column<double>(nullable: false),
                    WohnungId = table.Column<int>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
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
                    rowid = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VertragId = table.Column<Guid>(nullable: false),
                    Version = table.Column<int>(nullable: false),
                    WohnungId = table.Column<int>(nullable: false),
                    Personenzahl = table.Column<int>(nullable: false),
                    KaltMiete = table.Column<double>(nullable: false),
                    Beginn = table.Column<DateTime>(nullable: false),
                    Ende = table.Column<DateTime>(nullable: true),
                    AnsprechpartnerId = table.Column<Guid>(nullable: true),
                    VersionsNotiz = table.Column<string>(nullable: true),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vertraege", x => x.rowid);
                    table.UniqueConstraint("AK_Vertraege_VertragId_Version", x => new { x.VertragId, x.Version });
                    table.ForeignKey(
                        name: "FK_Vertraege_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WohnungAnhaenge",
                columns: table => new
                {
                    WohnungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetWohnungId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WohnungAnhaenge", x => x.WohnungAnhangId);
                    table.ForeignKey(
                        name: "FK_WohnungAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WohnungAnhaenge_Wohnungen_TargetWohnungId",
                        column: x => x.TargetWohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZaehlerSet",
                columns: table => new
                {
                    ZaehlerId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kennnummer = table.Column<string>(nullable: false),
                    AllgemeinZaehlerZaehlerId = table.Column<int>(nullable: true),
                    WohnungId = table.Column<int>(nullable: true),
                    Typ = table.Column<int>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaehlerSet", x => x.ZaehlerId);
                    table.ForeignKey(
                        name: "FK_ZaehlerSet_ZaehlerSet_AllgemeinZaehlerZaehlerId",
                        column: x => x.AllgemeinZaehlerZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZaehlerSet_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ErhaltungsaufwendungAnhaenge",
                columns: table => new
                {
                    ErhaltungsaufwendungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetErhaltungsaufwendungId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErhaltungsaufwendungAnhaenge", x => x.ErhaltungsaufwendungAnhangId);
                    table.ForeignKey(
                        name: "FK_ErhaltungsaufwendungAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ErhaltungsaufwendungAnhaenge_Erhaltungsaufwendungen_TargetErhaltungsaufwendungId",
                        column: x => x.TargetErhaltungsaufwendungId,
                        principalTable: "Erhaltungsaufwendungen",
                        principalColumn: "ErhaltungsaufwendungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Betriebskostenrechnungen",
                columns: table => new
                {
                    BetriebskostenrechnungId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Typ = table.Column<int>(nullable: false),
                    Betrag = table.Column<double>(nullable: false),
                    Datum = table.Column<DateTime>(nullable: false),
                    BetreffendesJahr = table.Column<int>(nullable: false),
                    Schluessel = table.Column<int>(nullable: false),
                    Beschreibung = table.Column<string>(nullable: true),
                    HKVO_P7 = table.Column<double>(nullable: true),
                    HKVO_P8 = table.Column<double>(nullable: true),
                    HKVO_P9 = table.Column<int>(nullable: true),
                    ZaehlerId = table.Column<int>(nullable: true),
                    Notiz = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Betriebskostenrechnungen", x => x.BetriebskostenrechnungId);
                    table.ForeignKey(
                        name: "FK_Betriebskostenrechnungen_ZaehlerSet_ZaehlerId",
                        column: x => x.ZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZaehlerAnhaenge",
                columns: table => new
                {
                    ZaehlerAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetZaehlerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaehlerAnhaenge", x => x.ZaehlerAnhangId);
                    table.ForeignKey(
                        name: "FK_ZaehlerAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZaehlerAnhaenge_ZaehlerSet_TargetZaehlerId",
                        column: x => x.TargetZaehlerId,
                        principalTable: "ZaehlerSet",
                        principalColumn: "ZaehlerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zaehlerstaende",
                columns: table => new
                {
                    ZaehlerstandId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZaehlerId = table.Column<int>(nullable: false),
                    Datum = table.Column<DateTime>(nullable: false),
                    Stand = table.Column<double>(nullable: false),
                    Notiz = table.Column<string>(nullable: true)
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
                name: "BetriebskostenrechnungAnhaenge",
                columns: table => new
                {
                    BetriebskostenrechnungAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetBetriebskostenrechnungId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetriebskostenrechnungAnhaenge", x => x.BetriebskostenrechnungAnhangId);
                    table.ForeignKey(
                        name: "FK_BetriebskostenrechnungAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BetriebskostenrechnungAnhaenge_Betriebskostenrechnungen_TargetBetriebskostenrechnungId",
                        column: x => x.TargetBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Betriebskostenrechnungsgruppen",
                columns: table => new
                {
                    BetriebskostenrechnungsGruppeId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WohnungId = table.Column<int>(nullable: false),
                    RechnungBetriebskostenrechnungId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Betriebskostenrechnungsgruppen", x => x.BetriebskostenrechnungsGruppeId);
                    table.ForeignKey(
                        name: "FK_Betriebskostenrechnungsgruppen_Betriebskostenrechnungen_RechnungBetriebskostenrechnungId",
                        column: x => x.RechnungBetriebskostenrechnungId,
                        principalTable: "Betriebskostenrechnungen",
                        principalColumn: "BetriebskostenrechnungId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Betriebskostenrechnungsgruppen_Wohnungen_WohnungId",
                        column: x => x.WohnungId,
                        principalTable: "Wohnungen",
                        principalColumn: "WohnungId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZaehlerstandAnhaenge",
                columns: table => new
                {
                    ZaehlerstandAnhangId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnhangId = table.Column<Guid>(nullable: false),
                    TargetZaehlerstandId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaehlerstandAnhaenge", x => x.ZaehlerstandAnhangId);
                    table.ForeignKey(
                        name: "FK_ZaehlerstandAnhaenge_Anhaenge_AnhangId",
                        column: x => x.AnhangId,
                        principalTable: "Anhaenge",
                        principalColumn: "AnhangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZaehlerstandAnhaenge_Zaehlerstaende_TargetZaehlerstandId",
                        column: x => x.TargetZaehlerstandId,
                        principalTable: "Zaehlerstaende",
                        principalColumn: "ZaehlerstandId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdresseAnhaenge_AnhangId",
                table: "AdresseAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_AdresseAnhaenge_TargetAdresseId",
                table: "AdresseAnhaenge",
                column: "TargetAdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_BetriebskostenrechnungAnhaenge_AnhangId",
                table: "BetriebskostenrechnungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_BetriebskostenrechnungAnhaenge_TargetBetriebskostenrechnungId",
                table: "BetriebskostenrechnungAnhaenge",
                column: "TargetBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungen_ZaehlerId",
                table: "Betriebskostenrechnungen",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungsgruppen_RechnungBetriebskostenrechnungId",
                table: "Betriebskostenrechnungsgruppen",
                column: "RechnungBetriebskostenrechnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Betriebskostenrechnungsgruppen_WohnungId",
                table: "Betriebskostenrechnungsgruppen",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_ErhaltungsaufwendungAnhaenge_AnhangId",
                table: "ErhaltungsaufwendungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_ErhaltungsaufwendungAnhaenge_TargetErhaltungsaufwendungId",
                table: "ErhaltungsaufwendungAnhaenge",
                column: "TargetErhaltungsaufwendungId");

            migrationBuilder.CreateIndex(
                name: "IX_Erhaltungsaufwendungen_WohnungId",
                table: "Erhaltungsaufwendungen",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageAnhaenge_AnhangId",
                table: "GarageAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageAnhaenge_TargetGarageId",
                table: "GarageAnhaenge",
                column: "TargetGarageId");

            migrationBuilder.CreateIndex(
                name: "IX_Garagen_AdresseId",
                table: "Garagen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Garagen_JuristischePersonId",
                table: "Garagen",
                column: "JuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonAnhaenge_AnhangId",
                table: "JuristischePersonAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonAnhaenge_TargetJuristischePersonId",
                table: "JuristischePersonAnhaenge",
                column: "TargetJuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonen_AdresseId",
                table: "JuristischePersonen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonenMitglieder_JuristischePersonId",
                table: "JuristischePersonenMitglieder",
                column: "JuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_JuristischePersonenMitglieder_NatuerlichePersonId",
                table: "JuristischePersonenMitglieder",
                column: "NatuerlichePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_KontoAnhaenge_AnhangId",
                table: "KontoAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_KontoAnhaenge_TargetKontoId",
                table: "KontoAnhaenge",
                column: "TargetKontoId");

            migrationBuilder.CreateIndex(
                name: "IX_MieteAnhaenge_AnhangId",
                table: "MieteAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_MieteAnhaenge_TargetMieteId",
                table: "MieteAnhaenge",
                column: "TargetMieteId");

            migrationBuilder.CreateIndex(
                name: "IX_MietMinderungAnhaenge_AnhangId",
                table: "MietMinderungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_MietMinderungAnhaenge_TargetMietMinderungId",
                table: "MietMinderungAnhaenge",
                column: "TargetMietMinderungId");

            migrationBuilder.CreateIndex(
                name: "IX_MietobjektGaragen_GarageId",
                table: "MietobjektGaragen",
                column: "GarageId");

            migrationBuilder.CreateIndex(
                name: "IX_NatuerlichePersonAnhaenge_AnhangId",
                table: "NatuerlichePersonAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_NatuerlichePersonAnhaenge_TargetNatuerlichePersonId",
                table: "NatuerlichePersonAnhaenge",
                column: "TargetNatuerlichePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_NatuerlichePersonen_AdresseId",
                table: "NatuerlichePersonen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Vertraege_WohnungId",
                table: "Vertraege",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_VertragAnhaenge_AnhangId",
                table: "VertragAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_WohnungAnhaenge_AnhangId",
                table: "WohnungAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_WohnungAnhaenge_TargetWohnungId",
                table: "WohnungAnhaenge",
                column: "TargetWohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_AdresseId",
                table: "Wohnungen",
                column: "AdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Wohnungen_JuristischePersonId",
                table: "Wohnungen",
                column: "JuristischePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerAnhaenge_AnhangId",
                table: "ZaehlerAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerAnhaenge_TargetZaehlerId",
                table: "ZaehlerAnhaenge",
                column: "TargetZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerSet_AllgemeinZaehlerZaehlerId",
                table: "ZaehlerSet",
                column: "AllgemeinZaehlerZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerSet_WohnungId",
                table: "ZaehlerSet",
                column: "WohnungId");

            migrationBuilder.CreateIndex(
                name: "IX_Zaehlerstaende_ZaehlerId",
                table: "Zaehlerstaende",
                column: "ZaehlerId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerstandAnhaenge_AnhangId",
                table: "ZaehlerstandAnhaenge",
                column: "AnhangId");

            migrationBuilder.CreateIndex(
                name: "IX_ZaehlerstandAnhaenge_TargetZaehlerstandId",
                table: "ZaehlerstandAnhaenge",
                column: "TargetZaehlerstandId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdresseAnhaenge");

            migrationBuilder.DropTable(
                name: "BetriebskostenrechnungAnhaenge");

            migrationBuilder.DropTable(
                name: "Betriebskostenrechnungsgruppen");

            migrationBuilder.DropTable(
                name: "ErhaltungsaufwendungAnhaenge");

            migrationBuilder.DropTable(
                name: "GarageAnhaenge");

            migrationBuilder.DropTable(
                name: "JuristischePersonAnhaenge");

            migrationBuilder.DropTable(
                name: "JuristischePersonenMitglieder");

            migrationBuilder.DropTable(
                name: "KontoAnhaenge");

            migrationBuilder.DropTable(
                name: "MieteAnhaenge");

            migrationBuilder.DropTable(
                name: "MieterSet");

            migrationBuilder.DropTable(
                name: "MietMinderungAnhaenge");

            migrationBuilder.DropTable(
                name: "MietobjektGaragen");

            migrationBuilder.DropTable(
                name: "NatuerlichePersonAnhaenge");

            migrationBuilder.DropTable(
                name: "Vertraege");

            migrationBuilder.DropTable(
                name: "VertragAnhaenge");

            migrationBuilder.DropTable(
                name: "WohnungAnhaenge");

            migrationBuilder.DropTable(
                name: "ZaehlerAnhaenge");

            migrationBuilder.DropTable(
                name: "ZaehlerstandAnhaenge");

            migrationBuilder.DropTable(
                name: "Betriebskostenrechnungen");

            migrationBuilder.DropTable(
                name: "Erhaltungsaufwendungen");

            migrationBuilder.DropTable(
                name: "Kontos");

            migrationBuilder.DropTable(
                name: "Mieten");

            migrationBuilder.DropTable(
                name: "MietMinderungen");

            migrationBuilder.DropTable(
                name: "Garagen");

            migrationBuilder.DropTable(
                name: "NatuerlichePersonen");

            migrationBuilder.DropTable(
                name: "Anhaenge");

            migrationBuilder.DropTable(
                name: "Zaehlerstaende");

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
