namespace Deeplex.Saverwalter.Model
{
    public class Zaehler
    {
        public int ZaehlerId { get; set; }
        public string Kennnummer { get; set; } = null!;
        public virtual Wohnung? Wohnung { get; set; } = null!;
        public virtual Adresse? Adresse { get; set; } = null!;
        public virtual Zaehler? Allgemeinzaehler { get; set; } = null!;
        public virtual List<Zaehler> EinzelZaehler { get; private set; } = new List<Zaehler>();
        public Zaehlertyp Typ { get; set; }
        public virtual List<Zaehlerstand> Staende { get; private set; } = new List<Zaehlerstand>();
        public string? Notiz { get; set; }
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
}
