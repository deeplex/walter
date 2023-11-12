using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Zaehler
    {
        public int ZaehlerId { get; set; }
        [Required]
        public string Kennnummer { get; set; }
        [Required]
        public Zaehlertyp Typ { get; set; }
        public virtual Wohnung? Wohnung { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public virtual DateOnly? Ende { get; set; }
        public string? Notiz { get; set; }

        public virtual List<Zaehlerstand> Staende { get; private set; } = new List<Zaehlerstand>();
        public virtual List<Umlage> Umlagen { get; private set; } = new List<Umlage>();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Zaehler(string kennnummer, Zaehlertyp typ)
        {
            Kennnummer = kennnummer;
            Typ = typ;
        }
    }

    public enum Zaehlertyp
    {
        [Unit(Zaehlereinheit.Kubikmeter)]
        Warmwasser,
        [Unit(Zaehlereinheit.Kubikmeter)]
        Kaltwasser,
        [Unit(Zaehlereinheit.Kilowattstunden)]
        Strom,
        [Unit(Zaehlereinheit.Kilowattstunden)]
        Gas,
    }

    public enum Zaehlereinheit
    {
        [UnitString("")]
        Dimensionslos,
        [UnitString("m³")]
        Kubikmeter,
        [UnitString("kWh")]
        Kilowattstunden
    }
}
