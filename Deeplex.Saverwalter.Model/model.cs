﻿using System;
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
        public DbSet<Wohnung> Wohnungen { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<Zaehler> ZaehlerSet { get; set; } = null!;
        public DbSet<Zaehlergemeinschaft> Zaehlergemeinschaften { get; set; } = null!;
        public DbSet<Zaehlerstand> Zaehlerstaende { get; set; } = null!;
        public DbSet<WarmeBetriebskostenRechnung> WarmeBetriebskostenRechnungen { get; set; } = null!;
        public DbSet<Vertrag> Vertraege { get; set; } = null!;
        public DbSet<MietobjektGarage> MietobjektGaragen { get; set; } = null!;
        public DbSet<JuristischePerson> JuristischePersonen { get; set; } = null!;
        public DbSet<Mieter> MieterSet { get; set; } = null!;
        public DbSet<Kontakt> Kontakte { get; set; } = null!;
        public DbSet<Konto> Kontos { get; set; } = null!;
        public DbSet<KalteBetriebskostenpunkt> KalteBetriebskosten { get; set; } = null!;
        public DbSet<KalteBetriebskostenRechnung> KalteBetriebskostenRechnungen { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            // TODO adjust this...
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
        public List<KalteBetriebskostenpunkt> KalteBetriebskosten { get; private set; } = new List<KalteBetriebskostenpunkt>();
        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
    }

    public class Wohnung
    {
        public int WohnungId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public double Wohnflaeche { get; set; }
        public double Nutzflaeche { get; set; }
        public List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
        public List<Zaehler> Zaehler { get; private set; } = new List<Zaehler>();
        public JuristischePerson Besitzer { get; set; } = null!;
        public List<Zaehlergemeinschaft> Zaehlergemeinschaften { get; private set; } = new List<Zaehlergemeinschaft>();
        public int AdresseId { get; set; }
        public Adresse Adresse { get; set; } = null!;
    }

    public class Garage
    {
        public int GarageId { get; set; }
        public string Kennung { get; set; } = null!;
        public JuristischePerson Besitzer { get; set; } = null!;
    }


    public class Zaehlergemeinschaft
    {
        public int ZaehlergemeinschaftId { get; set; }
        public Allgemeinzaehler Allgemeinzaehler { get; set; } = null!;
        public Wohnung Wohnung { get; set; } = null!;
        public Zaehlertyp Typ { get; set; }
    }

    public class Allgemeinzaehler
    {
        public int AllgemeinzaehlerId { get; set; }
        public List<Zaehlergemeinschaft> Zaehlergemeinschaften { get; private set; } = new List<Zaehlergemeinschaft>();
        public List<WarmeBetriebskostenRechnung> Rechnungen { get; private set; } = new List<WarmeBetriebskostenRechnung>();
        public string? Beschreibung { get; set; }
    }

    public class Zaehler
    {
        public int ZaehlerId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public int WohnungId { get; set; }
        public Zaehlertyp Typ { get; set; }
        public List<Zaehlerstand> Staende { get; private set; } = new List<Zaehlerstand>();
    }

    public enum Zaehlertyp
    {
        Warmwasser,
        Kaltwasser,
        Strom,
        Gas,
    }

    public class WarmeBetriebskostenRechnung
    {
        public int WarmeBetriebskostenRechnungId { get; set; }
        public Allgemeinzaehler Allgemeinzaehler { get; set; } = null!;
        public int Jahr { get; set; }
        public double Betrag { get; set; }
    }

    public class Zaehlerstand
    {
        public int ZaehlerstandId { get; set; }
        public Zaehler Zaehler { get; set; } = null!;
        public DateTime Datum { get; set; }
        public double Stand { get; set; }
    }

    public class Vertrag
    {
        public int rowid { get; set; }
        public Guid VertragId { get; set; }
        public int Version { get; set; }
        public List<Mieter> Mieter { get; private set; } = new List<Mieter>();
        public List<MietobjektGarage> Garagen { get; private set; } = new List<MietobjektGarage>();
        public int? WohnungId { get; set; }
        public Wohnung? Wohnung { get; set; }
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
        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
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
        public int? AdresseId { get; set; }
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
        public List<KalteBetriebskostenRechnung> Rechnungen { get; private set; } = new List<KalteBetriebskostenRechnung>();
        public Adresse Adresse { get; set; } = null!;
        public string? Beschreibung { get; set; }
        public UmlageSchluessel Schluessel { get; set; }
    }

    public enum KalteBetriebskosten
    {
        [Description("Allgemeinstrom/Hausbeleuchtung")]
        AllgemeinstromHausbeleuchtung,
        [Description("Breitbandkabelanschluss")]
        Breitbandkabelanschluss,
        [Description("Dachrinnenreinigung")]
        Dachrinnenreinigung,
        [Description("Entwässerung/Niederschlagswasser")]
        EntwaesserungNiederschlagswasser,
        [Description("Entwässerung/Schmutzwasser")]
        EntwaesserungSchmutzwasser,
        [Description("Gartenpflege")]
        Gartenpflege,
        [Description("Ungezieferbekämpfung")]
        Ungezieferbekaempfung,
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
        [Description("n. WF")]
        NachWohnflaeche,
        [Description("n. NE")]
        NachNutzeinheit,
        [Description("n. Pers.")]
        NachPersonenzahl,
        [Description("n. Verb.")]
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