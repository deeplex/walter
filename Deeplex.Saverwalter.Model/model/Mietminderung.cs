using System;

namespace Deeplex.Saverwalter.Model
{
    // Mietminderung is later taken away from the result of the Betriebskostenabrechnug.
    public class Mietminderung
    {
        public int MietminderungId { get; set; }
        public virtual Vertrag Vertrag { get; set; } = null!;
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; } = null!;
        public double Minderung { get; set; }
        public string? Notiz { get; set; }
    }
}
