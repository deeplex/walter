using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Garage : IAdresse
    {
        public int GarageId { get; set; }
        public Adresse Adresse { get; set; } = null!;
        public string Kennung { get; set; } = null!;
        public Guid BesitzerId { get; set; }
        public string? Notiz { get; set; }
        public List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
    }
}
