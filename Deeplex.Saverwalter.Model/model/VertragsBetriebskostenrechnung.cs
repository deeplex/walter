using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    // JOIN_TABLE
    // A Betriebskostenrechnung may be issued to one Vertrag only, if e.g. extra costs and the Mieter is to blame.

    public sealed class VertragsBetriebskostenrechnung : IAnhang
    {
        public int VertragsBetriebskostenrechnungId { get; set; }
        public Guid VertragId { get; set; }
        public Betriebskostenrechnung Rechnung { get; set; } = null!;
        public List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }

}
