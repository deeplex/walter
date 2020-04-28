using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.SaverWalter
{
    public class SaverwalterContext : DbContext
    {
        public DbSet<Adresse> Adressen { get; set; }
        public DbSet<Wohnung> Wohnungen { get; set; }
        public DbSet<Garage> Garagen { get; set; }
        public DbSet<Zaehler> ZaehlerSet { get; set; }
        public DbSet<Vertrag> Vertraege { get; set; }
        public DbSet<MietobjektWohnung> MietobjektWohnungen { get; set; }
        public DbSet<MietobjektGarage> MietobjektGaragen { get; set; }
        public DbSet<JuristischePerson> JuristischenPersonen { get; set; }
        public DbSet<Vertragsunterzeichner> VertragsunterzeichnerSet { get; set; }
        public DbSet<Kontakt> Kontakte { get; set; }
        public DbSet<Konto> Kontos { get; set; }
        public DbSet<KalteBetriebskostenpunkt> KalteBetriebskosten { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=walter.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vertrag>()
                .HasKey(v => new { v.VertragId, v.Version });
        }
    }

    public class Adresse
    {
        public int AdresseId { get; set; }
        public string Hausnummer { get; set; }
        public string Strasse { get; set; }
        public string Stadt { get; set; }
        public string Postleitzahl { get; set; }
        public List<Wohnung> Wohnungen { get; } = new List<Wohnung>();
    }

    public class Wohnung
    {
        public int WohnungId { get; set; }
        public string Bezeichnung { get; set; }
        public int Wohnflaeche { get; set; }
        public string Nutzflaeche { get; set; }
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
        public Zaehlertyp Typ {get; set; }
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
        public int VertragId { get; set; }
        public int Version { get; set; }
        public List<Vertragsunterzeichner> Mieter { get; } = new List<Vertragsunterzeichner>();
        public List<Vertragsunterzeichner> Vermieter { get; } = new List<Vertragsunterzeichner>();
        public List<MietobjektWohnung> Wohnungen { get; } = new List<MietobjektWohnung>();
        public List<MietobjektGarage> Garagen { get; } = new List<MietobjektGarage>();
        public int Personenzahl { get; set; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public Kontakt Ansprechpartner { get; set; }
        public JuristischePerson Gesellschaft { get; set; }
    }

    public class MietobjektWohnung
    {
        public int VertragId { get; set; }
        public Vertrag Vertrag { get; set; }
        public int WohnungId { get; set; }
        public Wohnung Wohnung { get; set; }
    }

    public class MietobjektGarage
    {
        public int VertragId { get; set; }
        public Vertrag Vertrag { get; set; }
        public int GarageId { get; set; }
        public Garage Garage { get; set; }
    }

    public class JuristischePerson
    {
        public int GesellschaftId { get; set; }
        public string Bezeichnung { get; set; }
    }

    public class Vertragsunterzeichner
    {
        public int KontaktId { get; set; }
        public Kontakt Kontakt { get; set; }
        public int VertagId { get; set; }
        public Vertrag Vertrag { get; set; }
    }

    public class Kontakt
    {
        public int KontaktId { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public Anrede Anrede { get; set; }
        public string Telefon { get; set; }
        public string Mobil { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
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
        public int KalteBetriebskostenId { get; set; }
        public string Bezeichnung { get; set; }
        public string Beschreibung { get; set; }
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