using System;
using System.Collections.Generic;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
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
