using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Zaehlerstand : IAnhang
    {
        public int ZaehlerstandId { get; set; }
        public Zaehler Zaehler { get; set; } = null!;
        public DateTime Datum { get; set; }
        public double Stand { get; set; }
        public string? Notiz { get; set; }
        public List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }
}