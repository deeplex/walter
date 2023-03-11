using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    // JOIN_TABLE
    // A Betriebskostenrechnung may be issued to one Vertrag only, if e.g. extra costs and the Mieter is to blame.

    public class VertragsBetriebskostenrechnung
    {
        public int VertragsBetriebskostenrechnungId { get; set; }
        public virtual Vertrag Vertrag { get; set; } = null!;
        public virtual Betriebskostenrechnung Rechnung { get; set; } = null!;
    }

}
