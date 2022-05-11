using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Erhaltungsaufwendung : IAnhang
    {
        public int ErhaltungsaufwendungId { get; set; }
        public DateTime Datum { get; set; }
        public Guid AusstellerId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public double Betrag { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public string? Notiz { get; set; }
        public List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }
}
