using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Wohnung : IAdresse, IAnhang
    {
        public int WohnungId { get; set; }
        public int AdresseId { get; set; }
        public Adresse Adresse { get; set; } = null!;
        public string Bezeichnung { get; set; } = null!;
        public Guid BesitzerId { get; set; }
        public double Wohnflaeche { get; set; }
        public double Nutzflaeche { get; set; }
        // Nutzeinheit is always 1, but dummies may have more... Or really big Wohnungen, who knows.
        public int Nutzeinheit { get; set; } = 1;
        public string? Notiz { get; set; }
        public List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
        public List<Zaehler> Zaehler { get; private set; } = new List<Zaehler>();
        public List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; private set; } = new List<Erhaltungsaufwendung>();
        public List<Umlage> Umlagen { get; private set; } = new List<Umlage>();
        public List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }
}
