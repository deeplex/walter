using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public class Wohnung : IAdresse
    {
        public int WohnungId { get; set; }
        public int AdresseId { get; set; }
        public virtual Adresse Adresse { get; set; } = null!;
        public string Bezeichnung { get; set; } = null!;
        public Guid BesitzerId { get; set; }
        public double Wohnflaeche { get; set; }
        public double Nutzflaeche { get; set; }
        // Nutzeinheit is always 1, but dummies may have more... Or really big Wohnungen, who knows.
        public int Nutzeinheit { get; set; } = 1;
        public string? Notiz { get; set; }
        public virtual List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
        public virtual List<Zaehler> Zaehler { get; private set; } = new List<Zaehler>();
        public virtual List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; private set; } = new List<Erhaltungsaufwendung>();
        public virtual List<Umlage> Umlagen { get; private set; } = new List<Umlage>();
    }
}
