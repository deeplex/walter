using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Windows.Storage;

namespace Deeplex.Saverwalter.Model
{
    public class SaverwalterContext : DbContext
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
    public class Adresse
    {
        public int AdresseId { get; set; }
        public string Hausnummer { get; set; } = null!;
        public string Strasse { get; set; } = null!;
        public string Postleitzahl { get; set; } = null!;
        public string Stadt { get; set; } = null!;
        public string? Notiz { get; set; }
        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
    }

    public class Wohnung
    {
        public int WohnungId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public double Wohnflaeche { get; set; }
        public double Nutzflaeche { get; set; }
        public string? Notiz { get; set; }
        public List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
        public List<Zaehler> Zaehler { get; private set; } = new List<Zaehler>();
        public List<Betriebskostenrechnungsgruppe> Betriebskostenrechnungsgruppen { get; private set; } = new List<Betriebskostenrechnungsgruppe>();
        public JuristischePerson Besitzer { get; set; } = null!;
        public int AdresseId { get; set; }
        public Adresse Adresse { get; set; } = null!;
    }

    public class Miete
    {
        public int MieteId { get; set; }
        public Guid VertragId { get; set; }
        public DateTime Datum { get; set; }
        public double? WarmMiete { get; set; }
        public double? KaltMiete { get; set; }
        public string? Notiz { get; set; }
    }

    public class Garage
    {
        public int GarageId { get; set; }
        public Adresse Adresse { get; set; } = null!;
        public string Kennung { get; set; } = null!;
        public JuristischePerson Besitzer { get; set; } = null!;
        public string? Notiz { get; set; }
    }

    public class Zaehler
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

    public class Zaehlerstand
    {
        public int ZaehlerstandId { get; set; }
        public Zaehler Zaehler { get; set; } = null!;
        public DateTime Datum { get; set; }
        public double Stand { get; set; }
        public string? Notiz { get; set; }
    }

    public class Vertrag
    {
        public int rowid { get; set; }
        public Guid VertragId { get; set; }
        public int Version { get; set; } = 1;
        public int? WohnungId { get; set; }
        public Wohnung? Wohnung { get; set; }
        public int Personenzahl { get; set; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public Kontakt Ansprechpartner { get; set; } = null!;
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

    public class MietobjektGarage
    {
        public int MietobjektGarageId { get; set; }
        public Guid VertragId { get; set; }
        public int GarageId { get; set; }
        public Garage Garage { get; set; } = null!;
    }

    public class JuristischePerson
    {
        public int JuristischePersonId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
        public List<JuristischePersonenMitglied> Mitglieder { get; private set; } = new List<JuristischePersonenMitglied>();
        public string? Notiz { get; set; }
    }

    public class JuristischePersonenMitglied
    {
        public int JuristischePersonenMitgliedId { get; set; }
        public int KontaktId { get; set; }
        public Kontakt Kontakt { get; set; } = null!;
        public int JuristischePersonId { get; set; }
        public JuristischePerson JuristischePerson { get; set; } = null!;
    }

    public class Mieter
    {
        public int MieterId { get; set; }
        public int KontaktId { get; set; }
        public Kontakt Kontakt { get; set; } = null!;
        public Guid VertragId { get; set; }
    }

    public class Kontakt
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

    public enum Anrede
    {
        Herr,
        Frau,
        Divers,
    }

    public class Konto
    {
        public int KontoId { get; set; }
        public string Bank { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public string? Notiz { get; set; }
    }


    public class Betriebskostenrechnung
    {
        public int BetriebskostenrechnungId { get; set; }
        public Betriebskostentyp Typ { get; set; }
        public double Betrag { get; set; }
        public DateTime Datum { get; set; }
        public UmlageSchluessel Schluessel { get; set; }
        public string? Beschreibung { get; set; }

        public List<Betriebskostenrechnungsgruppe> Gruppen { get; private set; } = new List<Betriebskostenrechnungsgruppe>();
    }

    public class Betriebskostenrechnungsgruppe
    {
        public int BetriebskostenrechnungsgruppeId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public Betriebskostenrechnung Rechnung { get; set; } = null!;
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
        [Description("Entwässerung/Niederschlagswasser")]
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