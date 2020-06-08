using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Windows.Storage;

namespace Deeplex.Saverwalter.Model
{
    public sealed class SaverwalterContext : DbContext
    {
        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<Betriebskostenrechnung> Betriebskostenrechnungen { get; set; } = null!;
        public DbSet<Betriebskostenrechnungsgruppe> Betriebskostenrechnungsgruppen { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<JuristischePerson> JuristischePersonen { get; set; } = null!;
        public DbSet<Kontakt> Kontakte { get; set; } = null!;
        public DbSet<Konto> Kontos { get; set; } = null!;
        public DbSet<Miete> Mieten { get; set; } = null!;
        public DbSet<Mieter> MieterSet { get; set; } = null!;
        public DbSet<MietMinderung> MietMinderungen { get; set; } = null!;
        public DbSet<MietobjektGarage> MietobjektGaragen { get; set; } = null!;
        public DbSet<Vertrag> Vertraege { get; set; } = null!;
        public DbSet<Wohnung> Wohnungen { get; set; } = null!;
        public DbSet<Zaehler> ZaehlerSet { get; set; } = null!;
        public DbSet<Zaehlerstand> Zaehlerstaende { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            // TODO adjust this...
            //=> options.UseSqlite("Data Source=walter.db");
            => options.UseSqlite("Data Source=" + ApplicationData.Current.LocalFolder.Path + @"\walter.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vertrag>()
                .HasKey(v => v.rowid);
            modelBuilder.Entity<Vertrag>()
                .HasAlternateKey("VertragId", "Version");
            modelBuilder.Entity<Vertrag>()
                .Property(v => v.Version);
        }
    }

    public sealed class Kontakt
    {
        public int KontaktId { get; set; }
        public string? Vorname { get; set; }
        public string Nachname { get; set; } = null!;
        public Anrede Anrede { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public int? AdresseId { get; set; }
        public Adresse? Adresse { get; set; }
        public List<JuristischePersonenMitglied> JuristischePersonen { get; private set; } = new List<JuristischePersonenMitglied>();
        public string? Notiz { get; set; }
    }

    public sealed class Wohnung
    {
        public int WohnungId { get; set; }
        public int AdresseId { get; set; }
        public Adresse Adresse { get; set; } = null!;
        public string Bezeichnung { get; set; } = null!;
        public int? BesitzerId { get; set; }
        public JuristischePerson? Besitzer { get; set; }
        public double Wohnflaeche { get; set; }
        public double Nutzflaeche { get; set; }
        // Nutzeinheit is always 1, but dummies may have more... Or really big Wohnungen, who knows.
        public int Nutzeinheit { get; set; } = 1;
        public string? Notiz { get; set; }
        public List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
        public List<Zaehler> Zaehler { get; private set; } = new List<Zaehler>();
        public List<Betriebskostenrechnungsgruppe> Betriebskostenrechnungsgruppen { get; private set; } = new List<Betriebskostenrechnungsgruppe>();
    }

    public sealed class Garage
    {
        public int GarageId { get; set; }
        public Adresse Adresse { get; set; } = null!;
        public string Kennung { get; set; } = null!;
        public JuristischePerson Besitzer { get; set; } = null!;
        public string? Notiz { get; set; }
    }

    // An Adresse is pointed at by a Wohnung, Garage or Kontakt.
    public sealed class Adresse
    {
        public int AdresseId { get; set; }
        public string Hausnummer { get; set; } = null!;
        public string Strasse { get; set; } = null!;
        public string Postleitzahl { get; set; } = null!;
        public string Stadt { get; set; } = null!;
        public string? Notiz { get; set; }
        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
        public List<Kontakt> Kontakte { get; private set; } = new List<Kontakt>();
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
    }

    public sealed class Vertrag
    {
        public int rowid { get; set; }
        public Guid VertragId { get; set; }
        public int Version { get; set; } = 1;
        public int WohnungId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        // Personenzahl is not inherently a property of a Vertrag.
        // But it is best tracked in as Vertrag(version). 
        public int Personenzahl { get; set; }
        // The KaltMiete may change without the Vertrag to be changed.
        // It has to be tracked by Verions.
        public double KaltMiete { get; set; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public int? AnsprechpartnerId { get; set; }
        public Kontakt? Ansprechpartner { get; set; }
        public string? VersionsNotiz { get; set; }
        public string? Notiz { get; set; }

        public Vertrag()
        {
            VertragId = Guid.NewGuid();
        }

        public Vertrag(Vertrag alt, DateTime Datum)
        {
            VertragId = alt.VertragId;
            Version = alt.Version + 1;
            Wohnung = alt.Wohnung;
            Notiz = alt.Notiz;
            Ansprechpartner = alt.Ansprechpartner;
            alt.Ende = Datum.AddDays(-1);
            Beginn = Datum;
        }
    }

    // JoinTable between a Kontakt and a Vertrag.
    public sealed class Mieter
    {
        public int MieterId { get; set; }
        public int KontaktId { get; set; }
        public Kontakt Kontakt { get; set; } = null!;
        public Guid VertragId { get; set; }
    }

    public sealed class Miete
    {
        public int MieteId { get; set; }
        public Guid VertragId { get; set; }
        // Zahlungsdatum may be used to determine if the last Zahlung is more than a month ago (+ tolerance).
        public DateTime Zahlungsdatum { get; set; }
        // BetreffenderMonat to be able to track single Mietsausfälle in specific months.
        public DateTime BetreffenderMonat { get; set; }
        public double? Betrag { get; set; }
        public string? Notiz { get; set; }
    }

    // Mietminderung is later taken away from the result of the Betriebskostenabrechnug.
    public sealed class MietMinderung
    {
        public int MietMinderungId { get; set; }
        public Guid VertragId { get; set; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; } = null!;
        public double Minderung { get; set; }
        public string? Notiz { get; set; }
    }

    public sealed class MietobjektGarage
    {
        public int MietobjektGarageId { get; set; }
        public Guid VertragId { get; set; }
        public int GarageId { get; set; }
        public Garage Garage { get; set; } = null!;
    }

    // JuristischePerson is a Name. Kontakte may subscribe to this and is used for dashboards and stuff... nothing wild really.
    public sealed class JuristischePerson
    {
        public int JuristischePersonId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public int? AdresseId { get; set; }
        public Adresse? Adresse { get; set; }
        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
        public List<JuristischePersonenMitglied> Mitglieder { get; private set; } = new List<JuristischePersonenMitglied>();
        public string? Notiz { get; set; }
    }

    public sealed class JuristischePersonenMitglied
    {
        public int JuristischePersonenMitgliedId { get; set; }
        public int KontaktId { get; set; }
        public Kontakt Kontakt { get; set; } = null!;
        public int JuristischePersonId { get; set; }
        public JuristischePerson JuristischePerson { get; set; } = null!;
    }

    public enum Anrede
    {
        Herr,
        Frau,
        Divers,
    }

    public sealed class Konto
    {
        public int KontoId { get; set; }
        public string Bank { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public string? Notiz { get; set; }
    }

    public sealed class Betriebskostenrechnung
    {
        public int BetriebskostenrechnungId { get; set; }
        public Betriebskostentyp Typ { get; set; }
        public double Betrag { get; set; }
        public DateTime Datum { get; set; }
        public int BetreffendesJahr { get; set; }
        public UmlageSchluessel Schluessel { get; set; }
        public string? Beschreibung { get; set; }
        public string? Notiz { get; set; }

        public List<Betriebskostenrechnungsgruppe> Gruppen { get; private set; } = new List<Betriebskostenrechnungsgruppe>();
    }

    // A Betriebskostenrechnung may be issued to one Vertrag only, if e.g. extra costs and the Mieter is to blame.
    public sealed class VertragsBetriebskostenrechnung
    {
        public int VertragsBetriebskostenrechnungId { get; set; }
        public Guid VertragId { get; set; }
        public Betriebskostenrechnung Rechnung { get; set; } = null!;
    }

    // Many Wohnungen may share a Betriebskostenrechnung. The calculation is done by the
    // Umlageschluessel and then by respective calculations.
    public sealed class Betriebskostenrechnungsgruppe
    {
        public int BetriebskostenrechnungsgruppeId { get; set; }
        public int WohnungId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public Betriebskostenrechnung Rechnung { get; set; } = null!;
    }

    public sealed class Zaehler
    {
        public int ZaehlerId { get; set; }
        public string Kennnummer { get; set; } = null!;
        public Wohnung Wohnung { get; set; } = null!;
        public int WohnungId { get; set; }
        public Zaehlertyp Typ { get; set; }
        public List<Zaehlerstand> Staende { get; private set; } = new List<Zaehlerstand>();
        public string? Notiz { get; set; }
    }

    public enum Zaehlertyp
    {
        Warmwasser,
        Kaltwasser,
        Strom,
        Gas,
    }

    public sealed class Zaehlerstand
    {
        public int ZaehlerstandId { get; set; }
        public Zaehler Zaehler { get; set; } = null!;
        public DateTime Datum { get; set; }
        public bool Abgelesen { get; set; }
        public double Stand { get; set; }
        public string? Notiz { get; set; }
    }

    // Even is Kalte Betriebskosten
    // Uneven is Warme Betriebskosten
    public enum Betriebskostentyp
    {
        [Description("Allgemeinstrom/Hausbeleuchtung")]
        AllgemeinstromHausbeleuchtung = 0,
        [Description("Breitbandkabelanschluss")]
        Breitbandkabelanschluss = 2,
        [Description("Dachrinnenreinigung")]
        Dachrinnenreinigung = 4,
        [Description("Entwässerung/Niederschlag")]
        EntwaesserungNiederschlagswasser = 6,
        [Description("Entwässerung/Schmutzwasser")]
        EntwaesserungSchmutzwasser = 8,
        [Description("Gartenpflege")]
        Gartenpflege = 10,
        [Description("Ungezieferbekämpfung")]
        Ungezieferbekaempfung = 12,
        [Description("Grundsteuer")]
        Grundsteuer = 14,
        [Description("Haftpflichtversicherung")]
        Haftpflichtversicherung = 16,
        [Description("Hauswartarbeiten")]
        Hauswartarbeiten = 18,
        [Description("Müllbeseitigung")]
        Muellbeseitigung = 20,
        [Description("Sachversicherung")]
        Sachversicherung = 22,
        [Description("Schornsteinfegerarbeiten")]
        Schornsteinfegerarbeiten = 24,
        [Description("Straßenreinigung")]
        Strassenreinigung = 26,
        [Description("Wartung Thermen/Speicher")]
        WartungThermenSpeicher = 28,
        [Description("Wasserversorgung")]
        Wasserversorgung = 30,
        [Description("Weitere/Sonstige Nebenkosten")]
        WeitereSonstigeNebenkosten = 32,
    }

    public enum UmlageSchluessel
    {
        [Description("n. WF")]
        NachWohnflaeche,
        [Description("n. NE")]
        NachNutzeinheit,
        [Description("n. Pers.")]
        NachPersonenzahl,
        [Description("n. Verb.")]
        NachVerbrauch,
    }
}