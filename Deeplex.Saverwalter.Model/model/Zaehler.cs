namespace Deeplex.Saverwalter.Model
{
    public class Zaehler
    {
        public int ZaehlerId { get; set; }
        public string Kennnummer { get; set; } = null!;
        public virtual Wohnung? Wohnung { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public virtual Zaehler? Allgemeinzaehler { get; set; }
        public Zaehlertyp Typ { get; set; }
        public string? Notiz { get; set; }

        public virtual List<Zaehlerstand> Staende { get; private set; } = new List<Zaehlerstand>();
        public virtual List<Zaehler> EinzelZaehler { get; private set; } = new List<Zaehler>();

        public Zaehler(string kennnummer, Zaehlertyp typ)
        {
            Kennnummer = kennnummer;
            Typ = typ;
        }
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
