using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public class Miete : IAnhang
    {
        public int MieteId { get; set; }
        public virtual Vertrag Vertrag { get; set; } = null!;

        // Zahlungsdatum may be used to determine if the last Zahlung is more than a month ago (+ tolerance).
        public DateTime Zahlungsdatum { get; set; }
        // BetreffenderMonat to be able to track single Mietsausfälle in specific months.
        public DateTime BetreffenderMonat { get; set; }
        public double? Betrag { get; set; }
        public string? Notiz { get; set; }
        public virtual List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }
}
