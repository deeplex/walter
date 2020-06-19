﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
// using Windows.Storage;

namespace Deeplex.Saverwalter.Model
{
    public sealed class SaverwalterContext : DbContext
    {
        private bool mPreconfigured = false;

        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<AdresseAnhang> AdresseAnhaenge { get; set; } = null!;
        public DbSet<Anhang> Anhaenge { get; set; } = null!;
        public DbSet<Betriebskostenrechnung> Betriebskostenrechnungen { get; set; } = null!;
        public DbSet<BetriebskostenrechnungAnhang> BetriebskostenrechnungAnhaenge { get; set; } = null!;
        public DbSet<Betriebskostenrechnungsgruppe> Betriebskostenrechnungsgruppen { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<GarageAnhang> GarageAnhaenge { get; set; } = null!;
        public DbSet<JuristischePerson> JuristischePersonen { get; set; } = null!;
        public DbSet<JuristischePersonAnhang> JuristischePersonAnhaenge { get; set; } = null!;
        public DbSet<Konto> Kontos { get; set; } = null!;
        public DbSet<KontoAnhang> KontoAnhaenge { get; set; } = null!;
        public DbSet<Miete> Mieten { get; set; } = null!;
        public DbSet<MieteAnhang> MieteAnhaenge { get; set; } = null!;
        public DbSet<Mieter> MieterSet { get; set; } = null!;
        public DbSet<MietMinderung> MietMinderungen { get; set; } = null!;
        public DbSet<MietMinderungAnhang> MietMinderungAnhaenge { get; set; } = null!;
        public DbSet<MietobjektGarage> MietobjektGaragen { get; set; } = null!;
        public DbSet<NatuerlichePerson> NatuerlichePersonen { get; set; } = null!;
        public DbSet<NatuerlichePersonAnhang> NatuerlichePersonAnhaenge { get; set; } = null!;
        public DbSet<Vertrag> Vertraege { get; set; } = null!;
        public DbSet<VertragAnhang> VertragAnhaenge { get; set; } = null!;
        public DbSet<Wohnung> Wohnungen { get; set; } = null!;
        public DbSet<WohnungAnhang> WohnungAnhaenge { get; set; } = null!;
        public DbSet<Zaehler> ZaehlerSet { get; set; } = null!;
        public DbSet<ZaehlerAnhang> ZaehlerAnhaenge { get; set; } = null!;
        public DbSet<Zaehlerstand> Zaehlerstaende { get; set; } = null!;
        public DbSet<ZaehlerstandAnhang> ZaehlerstandAnhaenge { get; set; } = null!;

        public SaverwalterContext()
            : base()
        {
        }
        public SaverwalterContext(DbContextOptions<SaverwalterContext> options)
            : base(options)
        {
            mPreconfigured = true;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!mPreconfigured)
            {
                options.UseSqlite("Data Source=walter.db");
            }
        }

        public IPerson FindPerson(Guid PersonId)
        {
            var left = JuristischePersonen.SingleOrDefault(j => PersonId == j.PersonId);
            if (left != null)
            {
                return left;
            }
            return NatuerlichePersonen.SingleOrDefault(n => PersonId == n.PersonId);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vertrag>()
                .HasKey(v => v.rowid);
            modelBuilder.Entity<Vertrag>()
                .HasAlternateKey("VertragId", "Version");
            modelBuilder.Entity<Vertrag>()
                .Property(v => v.Version);

            modelBuilder.Entity<JuristischePerson>()
                .HasAlternateKey(jp => jp.PersonId);
            modelBuilder.Entity<NatuerlichePerson>()
                .HasAlternateKey(np => np.PersonId);
        }
    }

    public sealed class Anhang
    {
        public Guid AnhangId { get; set; }
        public string FileName { get; set; } = null!;
        public byte[] Sha256Hash { get; set; } = null!;
        public string? ContentType { get; set; }
        public byte[] Content { get; set; } = null!;
        public DateTime CreationTime { get; set; }

        public Anhang()
        {
            AnhangId = Guid.NewGuid();
        }
    }

    public interface IPerson
    {
        public Guid PersonId { get; }
        public string Bezeichnung { get; }

        public bool isVermieter { get; set; }
        public bool isMieter { get; set; }
        public bool isHandwerker { get; set; }
        public Anrede Anrede { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public int? AdresseId { get; set; }
        public Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }
    }

    public sealed class NatuerlichePersonAnhang
    {
        public NatuerlichePerson Person { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
    }

    public sealed class NatuerlichePerson : IPerson
    {
        public string Bezeichnung => string.Join(" ", Vorname ?? "", Nachname);

        public Guid PersonId { get; set; }
        public int NatuerlichePersonId { get; set; }
        public string? Vorname { get; set; }
        public string Nachname { get; set; } = null!;
        // public Titel Titel { get; set; } TODO
        public bool isVermieter { get; set; }
        public bool isMieter { get; set; }
        public bool isHandwerker { get; set; }
        public Anrede Anrede { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public int? AdresseId { get; set; }
        public Adresse? Adresse { get; set; }
        public List<JuristischePersonenMitglied> JuristischePersonen { get; private set; } = new List<JuristischePersonenMitglied>();
        public string? Notiz { get; set; }

        public NatuerlichePerson()
        {
            PersonId = Guid.NewGuid();
        }
    }

    // JuristischePerson is a Name. Kontakte may subscribe to this and is used for dashboards and stuff... nothing wild really.
    public sealed class JuristischePerson : IPerson
    {
        public Guid PersonId { get; set; }
        public int JuristischePersonId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public bool isVermieter { get; set; }
        public bool isMieter { get; set; }
        public bool isHandwerker { get; set; }
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
        public Anrede Anrede { get; set; }

        public JuristischePerson()
        {
            PersonId = Guid.NewGuid();
        }
    }

    public sealed class JuristischePersonAnhang
    {
        public JuristischePerson Person { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
    }

    public sealed class JuristischePersonenMitglied
    {
        public int JuristischePersonenMitgliedId { get; set; }
        public Guid PersonId { get; set; }
        public int JuristischePersonId { get; set; }
        public JuristischePerson JuristischePerson { get; set; } = null!;
    }

    public sealed class Wohnung
    {
        public int WohnungId { get; set; }
        public int AdresseId { get; set; }
        public Adresse Adresse { get; set; } = null!;
        public string Bezeichnung { get; set; } = null!;
        public Guid BesitzerId { get; set; }
        public double Wohnflaeche { get; set; }
        public double Nutzflaeche { get; set; }
        // Nutzeinheit is always 1, but dummies may have more... Or really big Wohnungen, who knows.
        public int Nutzeinheit { get; set; } = 1;
        public string? Notiz { get; set; }
        public List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
        public List<Zaehler> Zaehler { get; private set; } = new List<Zaehler>();
        public List<Betriebskostenrechnungsgruppe> Betriebskostenrechnungsgruppen { get; private set; } = new List<Betriebskostenrechnungsgruppe>();
    }

    public sealed class WohnungAnhang
    {
        public Wohnung Wohnung { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
    }

    public sealed class Garage
    {
        public int GarageId { get; set; }
        public Adresse Adresse { get; set; } = null!;
        public string Kennung { get; set; } = null!;
        public Guid BesitzerId { get; set; }
        public string? Notiz { get; set; }
    }

    public sealed class GarageAnhang
    {
        public Garage Garage { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
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
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
    }

    public sealed class AdresseAnhang
    {
        public Adresse Adresse { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
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
        public Guid? AnsprechpartnerId { get; set; }
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
            AnsprechpartnerId = alt.AnsprechpartnerId;
            alt.Ende = Datum.AddDays(-1);
            Beginn = Datum;
        }
    }

    public sealed class VertragAnhang
    {
        public Vertrag Vertrag { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
    }

    // JoinTable between a Kontakt and a Vertrag.
    public sealed class Mieter
    {
        public int MieterId { get; set; }
        public Guid PersonId { get; set; }
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

    public sealed class MieteAnhang
    {
        public Miete Miete { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
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

    public sealed class MietMinderungAnhang
    {
        public MietMinderung MietMinderung { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
    }

    public sealed class MietobjektGarage
    {
        public int MietobjektGarageId { get; set; }
        public Guid VertragId { get; set; }
        public int GarageId { get; set; }
        public Garage Garage { get; set; } = null!;
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

    public sealed class KontoAnhang
    {
        public Konto Konto { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
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

    public sealed class BetriebskostenrechnungAnhang
    {
        public Betriebskostenrechnung Betriebskostenrechnung { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
    }

    // A Betriebskostenrechnung may be issued to one Vertrag only, if e.g. extra costs and the Mieter is to blame.
    public sealed class VertragsBetriebskostenrechnung
    {
        public int VertragsBetriebskostenrechnungId { get; set; }
        public Guid VertragId { get; set; }
        public Betriebskostenrechnung Rechnung { get; set; } = null!;
    }

    public sealed class VertragsBetriebskostenrechnungAnhang
    {
        public VertragsBetriebskostenrechnung VertragsBetriebskostenrechnung { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
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

    public sealed class ZaehlerAnhang
    {
        public Zaehler Zaehler { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
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

    public sealed class ZaehlerstandAnhang
    {
        public Zaehlerstand Zaehlerstand { get; set; } = null!;
        public Anhang Anhang { get; set; } = null!;
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
        [Description("Heizkosten")]
        Heizkosten = 1,
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