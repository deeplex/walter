using System;

namespace Deeplex.Saverwalter.Model
{
    public sealed class PersonenZeitIntervall
    {
        public DateTime Beginn { get; }
        public DateTime Ende { get; }
        public int Tage => (Ende - Beginn).Days + 1;
        public int GesamtTage => (new DateTime(Ende.Year, 12, 31) - new DateTime(Ende.Year, 1, 1)).Days + 1;
        public int Personenzahl { get; }
        public Rechnungsgruppe Parent { get; }

        public PersonenZeitIntervall((DateTime b, DateTime e, int p) i, Rechnungsgruppe parent)
        {
            Beginn = i.b;
            Ende = i.e;
            Personenzahl = i.p;
            Parent = parent;
        }
    }

}
