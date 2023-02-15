using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public class Vertrag : IAnhang
    {
        public int VertragId { get; set; }
        public virtual Wohnung Wohnung { get; set; } = null!;
        public Guid? AnsprechpartnerId { get; set; }
        public string? Notiz { get; set; }
        public DateTime? Ende { get; set; }
        public virtual List<VertragVersion> Versionen { get; private set; } = new List<VertragVersion>();
        public virtual List<Miete> Mieten { get; private set; } = new List<Miete>();
        public virtual List<Mietminderung> Mietminderungen { get; private set; } = new List<Mietminderung>();
        public virtual List<Garage> Garagen { get; private set; } = new List<Garage>();
        public virtual List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }

    public class VertragVersion : IAnhang
    {
        public int VertragVersionId { get; set; }
        public int Personenzahl { get; set; }
        public virtual Vertrag Vertrag { get; set; } = null!;
        public DateTime Beginn { get; set; }
        public double Grundmiete { get; set; }
        public string? Notiz { get; set; }
        public virtual List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }
}
