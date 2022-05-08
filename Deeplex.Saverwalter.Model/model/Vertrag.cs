using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Vertrag
    {
        public int rowid { get; set; }
        public Guid VertragId { get; set; }
        public int Version { get; set; } = 1;
        public int WohnungId { get; set; }
        public Wohnung Wohnung { get; set; } = null!;
        // Personenzahl is not inherently a property of a Vertrag.
        // But it is best tracked in as Vertrag(version). 
        public int Personenzahl { get; set; }
        // The KaltMiete may change without the Vertrag to be changed.
        // It has to be tracked by Versions.
        public double KaltMiete { get; set; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public Guid? AnsprechpartnerId { get; set; }
        public string? VersionsNotiz { get; set; }
        public string? Notiz { get; set; }
        public List<Garage> Garagen { get; private set; } = new List<Garage>();

        public Vertrag()
        {
            VertragId = Guid.NewGuid();
        }

        public Vertrag(Vertrag alt, DateTime Datum)
        {
            VertragId = alt.VertragId;
            Version = alt.Version + 1;
            Wohnung = alt.Wohnung;
            Notiz = alt.Notiz;
            AnsprechpartnerId = alt.AnsprechpartnerId;
            alt.Ende = Datum.AddDays(-1);
            Beginn = Datum;
        }
    }
}
