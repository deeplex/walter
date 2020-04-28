using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Deeplex.Saverwalter.Model
{
    public class SaverwalterContext : DbContext
    {
        public DbSet<Adresse> Adressen { get; set; }
        public DbSet<Strasse> Strassen { get; set; }
        public DbSet<Postleitzahl> Postleitzahlen { get; set; }
        public DbSet<Stadt> Staedte { get; set; }
        public DbSet<Wohnung> Wohnungen { get; set; }
        public DbSet<Garage> Garagen { get; set; }
        public DbSet<Zaehler> ZaehlerSet { get; set; }
        public DbSet<Vertrag> Vertraege { get; set; }
        public DbSet<MietobjektWohnung> MietobjektWohnungen { get; set; }
        public DbSet<MietobjektGarage> MietobjektGaragen { get; set; }
        public DbSet<JuristischePerson> JuristischePersonen { get; set; }
        public DbSet<Mieter> MieterSet { get; set; }
        public DbSet<Kontakt> Kontakte { get; set; }
        public DbSet<Konto> Kontos { get; set; }
        public DbSet<KalteBetriebskostenpunkt> KalteBetriebskosten { get; set; }

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
        public string Hausnummer { get; set; }
        public Strasse Strasse { get; set; }
        public List<Wohnung> Wohnungen { get; } = new List<Wohnung>();
    }

    public class Strasse
    {
        public int StrasseId { get; set; }
        public string Name { get; set; }
        public Postleitzahl Postleitzahl { get; set; }
    }

    public class Postleitzahl
    {
        public int PostleitzahlId { get; set; }
        public string Bezeichnung { get; set; }
        public Stadt Stadt { get; set; }
    }
    public class Stadt
    {
        public int StadtId { get; set; }
        public string Name { get; set; }
    }

    public class Wohnung
    {
        public int WohnungId { get; set; }
        public string Bezeichnung { get; set; }
        public double Wohnflaeche { get; set; }
        public double Nutzflaeche { get; set; }
        public Adresse Adresse { get; set; }
    }

    public class Garage
    {
        public int GarageId { get; set; }
    }

    public class Zaehler
    {
        public int ZaehlerId { get; set; }
        public Wohnung Wohnung { get; set; }
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
        public List<MietobjektWohnung> Wohnungen { get; } = new List<MietobjektWohnung>();
        public List<MietobjektGarage> Garagen { get; } = new List<MietobjektGarage>();
        public JuristischePerson Vermieter { get; set; }
        public int Personenzahl { get; set; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public Kontakt Ansprechpartner { get; set; }

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

            Wohnungen = alt.Wohnungen.Select(w => new MietobjektWohnung
            {
                Vertrag = this,
                WohnungId = w.WohnungId,
                Wohnung = w.Wohnung,
            }).ToList();
            Garagen = alt.Garagen.Select(g => new MietobjektGarage
            {
                Vertrag = this,
                GarageId = g.GarageId,
                Garage = g.Garage,
            }).ToList();
            Vermieter = alt.Vermieter;
            Ansprechpartner = alt.Ansprechpartner;
            alt.Ende = Datum.AddDays(-1);
            Beginn = Datum;
        }
    }

    public class MietobjektWohnung
    {
        public int MietobjektWohnungId { get; set; }
        public int VertragId { get; set; }
        public Vertrag Vertrag { get; set; }
        public int WohnungId { get; set; }
        public Wohnung Wohnung { get; set; }
    }

    public class MietobjektGarage
    {
        public int MietobjektGarageId { get; set; }
        public int VertragId { get; set; }
        public Vertrag Vertrag { get; set; }
        public int GarageId { get; set; }
        public Garage Garage { get; set; }
    }

    public class JuristischePerson // TODO Angehörige Personen verlinken.
    {
        public int JuristischePersonId { get; set; }
        public string Bezeichnung { get; set; }
    }

    public class Mieter
    {
        public int MieterId { get; set; }
        public int KontaktId { get; set; }
        public Kontakt Kontakt { get; set; }
        public int VertragId { get; set; }
        public Vertrag Vertrag { get; set; }
    }

    public class Kontakt
    {
        public int KontaktId { get; set; }
        public string? Vorname { get; set; }
        public string Nachname { get; set; }
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
        public string Bank { get; set; }
        public string Iban { get; set; }
    }
    public class KalteBetriebskostenpunkt
    {
        public int KalteBetriebskostenpunktId { get; set; }
        public string Bezeichnung { get; set; }
        public string? Beschreibung { get; set; }
        public UmlageSchluessel Schluessel { get; set; }
    }
    public enum UmlageSchluessel
    {
        NachWohnflaeche,
        NachNutzeinheit,
        NachPersonenzahl,
        NachVerbrauch,
    }
}