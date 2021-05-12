using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
        public DbSet<BetriebskostenrechnungsGruppe> Betriebskostenrechnungsgruppen { get; set; } = null!;
        public DbSet<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; set; } = null!;
        public DbSet<ErhaltungsaufwendungAnhang> ErhaltungsaufwendungAnhaenge { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<GarageAnhang> GarageAnhaenge { get; set; } = null!;
        public DbSet<JuristischePerson> JuristischePersonen { get; set; } = null!;
        public DbSet<JuristischePersonenMitglied> JuristischePersonenMitglieder { get; set; } = null!;
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

    public abstract class AnhangRef : IAnhang
    {
        public Anhang Anhang { get; set; } = null!;
        public Guid AnhangId { get; set; }
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
    public interface IAnhang
    {
        public Guid AnhangId { get; set; }
        public Anhang Anhang { get; set; }
    }
    public interface IAnhang<T> : IAnhang
    {
        public T Target { get; set; }
    }


    public interface IPerson : IAdresse
    {
        public Guid PersonId { get; set; }
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
        public string? Notiz { get; set; }
    }

    public sealed class NatuerlichePersonAnhang : AnhangRef, IAnhang<NatuerlichePerson>
    {
        public int NatuerlichePersonAnhangId { get; set; }
        public NatuerlichePerson Target { get; set; } = null!;
    }

    public sealed class NatuerlichePerson : IPerson
    {
        public string Bezeichnung => string.Join(" ", Vorname ?? "", Nachname);

        public Guid PersonId { get; set; }
        public int NatuerlichePersonId { get; set; }
        public string? Vorname { get; set; }
        public string Nachname { get; set; } = null!;
        public Titel Titel { get; set; }
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

    public enum Anrede
    {
        Herr,
        Frau,
        Keine,
    }

    public enum Titel
    {
        Kein,
        Doktor,
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

    public sealed class JuristischePersonAnhang : AnhangRef, IAnhang<JuristischePerson>
    {
        public int JuristischePersonAnhangId { get; set; }
        public JuristischePerson Target { get; set; } = null!;
    }

    public sealed class JuristischePersonenMitglied
    {
        public int JuristischePersonenMitgliedId { get; set; }
        public Guid PersonId { get; set; }
        public int JuristischePersonId { get; set; }
        public JuristischePerson JuristischePerson { get; set; } = null!;
    }

    public sealed class Wohnung : IAdresse
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
        public List<BetriebskostenrechnungsGruppe> Betriebskostenrechnungsgruppen { get; private set; } = new List<BetriebskostenrechnungsGruppe>();
    }

    public sealed class WohnungAnhang : AnhangRef, IAnhang<Wohnung>
    {
        public int WohnungAnhangId { get; set; }
        public Wohnung Target { get; set; } = null!;
    }

    public sealed class Garage : IAdresse
    {
        public int GarageId { get; set; }
        public Adresse Adresse { get; set; } = null!;
        public string Kennung { get; set; } = null!;
        public Guid BesitzerId { get; set; }
        public string? Notiz { get; set; }
    }

    public sealed class GarageAnhang : AnhangRef, IAnhang<Garage>
    {
        public int GarageAnhangId { get; set; }
        public Garage Target { get; set; } = null!;
    }

    public interface IAdresse
    {
        public Adresse Adresse { get; set; }
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

    public sealed class AdresseAnhang : AnhangRef, IAnhang<Adresse>
    {
        public int AdresseAnhangId { get; set; }
        public Adresse Target { get; set; } = null!;
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
        // It has to be tracked by Versions.
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

    public sealed class VertragAnhang : AnhangRef, IAnhang<Guid>
    {
        public int VertragAnhangId { get; set; }
        public Guid Target { get; set; }
        //public VertragAnhangTyp Typ { get; set; }
    }
    public enum VertragAnhangTyp
    {
        Sonstiges,
        Mietvertrag,
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

    public sealed class MieteAnhang : AnhangRef, IAnhang<Miete>
    {
        public int MieteAnhangId { get; set; }
        public Miete Target { get; set; } = null!;
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

    public sealed class MietMinderungAnhang : AnhangRef, IAnhang<MietMinderung>
    {
        public int MietMinderungAnhangId { get; set; }
        public MietMinderung Target { get; set; } = null!;
    }

    public sealed class MietobjektGarage
    {
        public int MietobjektGarageId { get; set; }
        public Guid VertragId { get; set; }
        public int GarageId { get; set; }
        public Garage Garage { get; set; } = null!;
    }

    public sealed class Konto
    {
        public int KontoId { get; set; }
        public string Bank { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public string? Notiz { get; set; }
    }

    public sealed class KontoAnhang : AnhangRef, IAnhang<Konto>
    {
        public int KontoAnhangId { get; set; }
        public Konto Target { get; set; } = null!;
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

        public double? HKVO_P7 { get; set; }
        public double? HKVO_P8 { get; set; }
        public HKVO_P9A2? HKVO_P9 { get; set; }
        public Zaehler? Zaehler { get; set; }

        public string? Notiz { get; set; }

        public List<BetriebskostenrechnungsGruppe> Gruppen { get; private set; } = new List<BetriebskostenrechnungsGruppe>();
    }

    public enum HKVO_P9A2
    {
        Satz_1 = 1,
        Satz_2 = 2,
        Satz_4 = 4,
    }

    public sealed class BetriebskostenrechnungAnhang : AnhangRef, IAnhang<Betriebskostenrechnung>
    {
        public int BetriebskostenrechnungAnhangId { get; set; }
        public Betriebskostenrechnung Target { get; set; } = null!;
    }

    // A Betriebskostenrechnung may be issued to one Vertrag only, if e.g. extra costs and the Mieter is to blame.
    public sealed class VertragsBetriebskostenrechnung
    {
        public int VertragsBetriebskostenrechnungId { get; set; }
        public Guid VertragId { get; set; }
        public Betriebskostenrechnung Rechnung { get; set; } = null!;
    }

    public sealed class VertragsBetriebskostenrechnungAnhang : AnhangRef, IAnhang<VertragsBetriebskostenrechnung>
    {
        public int VertragsBetriebskostenrechnungId { get; set; }
        public VertragsBetriebskostenrechnung Target { get; set; } = null!;
    }

    // Many Wohnungen may share a Betriebskostenrechnung. The calculation is done by the
    // Umlageschluessel and then by respective calculations.
    public sealed class BetriebskostenrechnungsGruppe
    {
        public int BetriebskostenrechnungsGruppeId { get; set; }
        public int WohnungId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public Betriebskostenrechnung Rechnung { get; set; } = null!;
    }

    public sealed class Erhaltungsaufwendung
    {
        public int ErhaltungsaufwendungId { get; set; }
        public DateTime Datum { get; set; }
        public Guid AusstellerId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public double Betrag { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public string? Notiz { get; set; }
    }

    public sealed class ErhaltungsaufwendungAnhang : AnhangRef, IAnhang<Erhaltungsaufwendung>
    {
        public int ErhaltungsaufwendungAnhangId { get; set; }
        public Erhaltungsaufwendung Target { get; set; } = null!;
    }

    public sealed class Zaehler
    {
        public int ZaehlerId { get; set; }
        public string Kennnummer { get; set; } = null!;
        public Wohnung? Wohnung { get; set; } = null!;
        public Zaehler? AllgemeinZaehler { get; set; } = null!;
        public List<Zaehler> EinzelZaehler { get; private set; } = new List<Zaehler>();
        public int? WohnungId { get; set; }
        public Zaehlertyp Typ { get; set; }
        public List<Zaehlerstand> Staende { get; private set; } = new List<Zaehlerstand>();
        public string? Notiz { get; set; }
    }

    public sealed class ZaehlerAnhang : AnhangRef, IAnhang<Zaehler>
    {
        public int ZaehlerAnhangId { get; set; }
        public Zaehler Target { get; set; } = null!;
    }

    public enum Zaehlertyp
    {
        [Unit("m³")]
        Warmwasser,
        [Unit("m³")]
        Kaltwasser,
        [Unit("kWh")]
        Strom,
        [Unit("kWh")]
        Gas,
    }

    public sealed class Zaehlerstand
    {
        public int ZaehlerstandId { get; set; }
        public Zaehler Zaehler { get; set; } = null!;
        public DateTime Datum { get; set; }
        public double Stand { get; set; }
        public string? Notiz { get; set; }
    }

    public sealed class ZaehlerstandAnhang : AnhangRef, IAnhang<Zaehlerstand>
    {
        public int ZaehlerstandAnhangId { get; set; }
        public Zaehlerstand Target { get; set; } = null!;
    }

    // Even is Kalte Betriebskosten
    // Odd is Warme Betriebskosten
    public enum Betriebskostentyp
    {
        [Description("Allgemeinstrom")]
        AllgemeinstromHausbeleuchtung = 0,
        [Description("Breitbandkabelanschluss")]
        Breitbandkabelanschluss = 2,
        [Description("Dachrinnenreinigung")]
        Dachrinnenreinigung = 4,
        [Description("Entwässerung Niederschlag")]
        EntwaesserungNiederschlagswasser = 6,
        [Description("Entwässerung Schmutzwasser")]
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
        SchornsteinfegerarbeitenKalt = 24,
        [Description("Schornsteinfegerarbeiten")]
        SchornsteinfegerarbeitenWarm = 25,
        [Description("Straßenreinigung")]
        Strassenreinigung = 26,
        [Description("Wartung Therme, Speicher")]
        WartungThermenSpeicher = 28,
        [Description("Wasserversorgung")]
        WasserversorgungKalt = 30,
        [Description("Wasserversorgung")]
        WasserversorgungWarm = 31,
        [Description("Sonstige Nebenkosten")]
        WeitereSonstigeNebenkosten = 32,
        [Description("Heizkosten")]
        Heizkosten = 35,
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
        [Description("n. NF")]
        NachNutzflaeche,
    }
}