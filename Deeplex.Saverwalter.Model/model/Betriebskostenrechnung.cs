using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Betriebskostenrechnung
    {
        public Betriebskostenrechnung ShallowCopy()
        {
            return (Betriebskostenrechnung)MemberwiseClone();
        }

        public Betriebskostenrechnung NewYear()
        {
            var copy = ShallowCopy();
            copy.BetreffendesJahr += 1;
            copy.Betrag = 0;
            copy.BetriebskostenrechnungId = 0;
            copy.Gruppen = new List<BetriebskostenrechnungsGruppe>();

            return copy;
        }

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
