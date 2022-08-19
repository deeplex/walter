using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Vertrag : IAnhang
    {
        public int VertragId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public Guid? AnsprechpartnerId { get; set; }
        public string? Notiz { get; set; }
        public DateTime? Ende { get; set; }
        public List<VertragVersion> Versionen { get; private set; } = new List<VertragVersion>();
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
        public List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }

    public sealed class VertragVersion : IAnhang
    {
        public int VertragVersionId { get; set; }
        public int Personenzahl { get; set; }
        public Vertrag Vertrag { get; set; } = null!;
        public DateTime Beginn { get; set; }
        public double Grundmiete { get; set; }
        public string? Notiz { get; set; }
        public List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }
}
