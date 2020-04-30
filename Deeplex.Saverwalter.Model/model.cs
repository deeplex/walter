using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Deeplex.Saverwalter.Model
{
    public class SaverwalterContext : DbContext
    {
        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<Wohnung> Wohnungen { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<Zaehler> ZaehlerSet { get; set; } = null!;
        public DbSet<Vertrag> Vertraege { get; set; } = null!;
        public DbSet<MietobjektGarage> MietobjektGaragen { get; set; } = null!;
        public DbSet<JuristischePerson> JuristischePersonen { get; set; } = null!;
        public DbSet<Mieter> MieterSet { get; set; } = null!;
        public DbSet<Kontakt> Kontakte { get; set; } = null!;
        public DbSet<Konto> Kontos { get; set; } = null!;
        public DbSet<KalteBetriebskostenpunkt> KalteBetriebskosten { get; set; } = null!;
        public DbSet<KalteBetriebskostenRechnung> KalteBetriebskostenRechnungen { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=walter.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vertrag>()
                .HasKey(v => v.rowid);
            modelBuilder.Entity<Vertrag>()
                .HasAlternateKey("VertragId", "Version");
            modelBuilder.Entity<Vertrag>()
                .Property(v => v.Version)
                .HasDefaultValue(0);
        }
    }
    public class Adresse
    {
        public int AdresseId { get; set; }
        public string Hausnummer { get; set; } = null!;
        public string Strasse { get; set; } = null!;
        public string Postleitzahl { get; set; } = null!;
        public string Stadt { get; set; } = null!;
        public List<KalteBetriebskostenpunkt> KalteBetriebskosten { get; } = new List<KalteBetriebskostenpunkt>();
        public List<Wohnung> Wohnungen { get; } = new List<Wohnung>();
    }

    public class Wohnung
    {
        public int WohnungId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public double Wohnflaeche { get; set; }
        public double Nutzflaeche { get; set; }
        public Adresse Adresse { get; set; } = null!;
    }

    public class Garage
    {
        public int GarageId { get; set; }
    }

    public class Zaehler
    {
        public int ZaehlerId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public Zaehlertyp Typ { get; set; }
    }

    public enum Zaehlertyp
    {
        WarmWasser,
        Kaltwasser,
        Strom,
        Gas,
    }

    public class Vertrag
    {
        public int rowid { get; set; }
        public Guid VertragId { get; set; }
        public int Version { get; set; }
        public List<Mieter> Mieter { get; } = new List<Mieter>();
        public List<MietobjektGarage> Garagen { get; } = new List<MietobjektGarage>();
        public Wohnung? Wohnung { get; set; }
        public JuristischePerson Vermieter { get; set; } = null!;
        public int Personenzahl { get; set; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public Kontakt Ansprechpartner { get; set; } = null!;

        public Vertrag()
        {
            VertragId = Guid.NewGuid();
        }

        public Vertrag(Vertrag alt, DateTime Datum)
        {
            VertragId = alt.VertragId;
            Version = alt.Version + 1;
            Mieter = alt.Mieter.Select(m => new Mieter
            {
                Vertrag = this,
                KontaktId = m.KontaktId,
                Kontakt = m.Kontakt,
            }).ToList();

            Garagen = alt.Garagen.Select(g => new MietobjektGarage
            {
                Vertrag = this,
                GarageId = g.GarageId,
                Garage = g.Garage,
            }).ToList();
            Wohnung = alt.Wohnung;
            Vermieter = alt.Vermieter;
            Ansprechpartner = alt.Ansprechpartner;
            alt.Ende = Datum.AddDays(-1);
            Beginn = Datum;
        }
    }

    public class MietobjektGarage
    {
        public int MietobjektGarageId { get; set; }
        public int VertragId { get; set; }
        public Vertrag Vertrag { get; set; } = null!;
        public int GarageId { get; set; }
        public Garage Garage { get; set; } = null!;
    }

    public class JuristischePerson // TODO Angehörige Personen verlinken.
    {
        public int JuristischePersonId { get; set; }
        public string Bezeichnung { get; set; } = null!;
    }

    public class Mieter
    {
        public int MieterId { get; set; }
        public int KontaktId { get; set; }
        public Kontakt Kontakt { get; set; } = null!;
        public int VertragId { get; set; }
        public Vertrag Vertrag { get; set; } = null!;
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
        public Adresse? Adresse { get; set; }
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
    }
    public class KalteBetriebskostenpunkt
    {
        public int KalteBetriebskostenpunktId { get; set; }
        public KalteBetriebskosten Bezeichnung { get; set; }
        public List<KalteBetriebskostenRechnung> Rechnungen { get; } = new List<KalteBetriebskostenRechnung>();
        public Adresse Adresse { get; set; } = null!;
        public string? Beschreibung { get; set; }
        public UmlageSchluessel Schluessel { get; set; }
    }
    public enum KalteBetriebskosten
    {
        [Description("AllgemeinstromHausbeleuchtung")]
        AllgemeinstromHausbeleuchtung,
        [Description("Breinbandkabelanschluss")]
        Breinbandkabelanschluss,
        [Description("Dachrinnenreinigung")]
        Dachrinnenreinigung,
        [Description("Entwässerung/Niederschlagswasser")]
        EntwaesserungNiederschlagswasser,
        [Description("Entwässerung/Schmutzwasser")]
        EntwaesserungSchmutzwasser,
        [Description("Gartenpflege")]
        Gartenpflege,
        [Description("Gebäudereinigung/Ungezieverbekämpfung")]
        GebaeudereinigungUngezieverbekaempfung,
        [Description("Grundsteuer")]
        Grundsteuer,
        [Description("Haftpflichtversicherung")]
        Haftpflichtversicherung,
        [Description("Hauswartarbeiten")]
        Hauswartarbeiten,
        [Description("Müllbeseitigung")]
        Muellbeseitigung,
        [Description("Sachversicherung")]
        Sachversicherung,
        [Description("Schornsteinfegerarbeiten")]
        Schornsteinfegerarbeiten,
        [Description("Straßenreinigung")]
        Strassenreinigung,
        [Description("Wartung Thermen/Speicher")]
        WartungThermenSpeicher,
        [Description("Wasserversorgung")]
        Wasserversorgung,
        [Description("Weitere/Sonstige Nebenkosten")]
        WeitereSonstigeNebenkosten,
    }

    public enum UmlageSchluessel
    {
        NachWohnflaeche,
        NachNutzeinheit,
        NachPersonenzahl,
        NachVerbrauch,
    }

    public class KalteBetriebskostenRechnung
    {
        public int KalteBetriebskostenRechnungId { get; set; }
        public KalteBetriebskostenpunkt KalteBetriebskostenpunkt { get; set; } = null!;
        public int Jahr { get; set; }
        public double Betrag { get; set; }
    }
}