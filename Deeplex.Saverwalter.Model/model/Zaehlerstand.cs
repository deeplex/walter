using System;

namespace Deeplex.Saverwalter.Model
{
    public class Zaehlerstand
    {
        public int ZaehlerstandId { get; set; }
        public virtual Zaehler Zaehler { get; set; } = null!;
        public DateTime Datum { get; set; }
        public double Stand { get; set; }
        public string? Notiz { get; set; }
    }
}