using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Betriebskostenrechnung
    {
        public int BetriebskostenrechnungId { get; set; }
        [Required]
        public double Betrag { get; set; }
        [Required]
        public DateOnly Datum { get; set; }
        [Required]
        public int BetreffendesJahr { get; set; }
        [Required]
        public virtual Umlage Umlage { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public string? Notiz { get; set; }

        public Betriebskostenrechnung(double betrag, DateOnly datum, int betreffendesJahr)
        {
            Betrag = betrag;
            Datum = datum;
            BetreffendesJahr = betreffendesJahr;
        }
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


    public enum Umlageschluessel
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
