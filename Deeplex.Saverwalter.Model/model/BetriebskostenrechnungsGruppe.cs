using System;
using System.Collections.Generic;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    // JOIN_TABLE
    // Many Wohnungen may share a Betriebskostenrechnung. The calculation is done by the
    // Umlageschluessel and then by respective calculations.
    public sealed class BetriebskostenrechnungsGruppe
    {
        public int BetriebskostenrechnungsGruppeId { get; set; }
        public int WohnungId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        public Betriebskostenrechnung Rechnung { get; set; } = null!;
    }
}
