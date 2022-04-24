using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public interface IAdresse
    {
        public Adresse Adresse { get; set; }
    }

    // An Adresse is pointed at by a Wohnung, Garage or Kontakt.
    public sealed class Adresse
    {
        public string Anschrift => string.Join(", ",
            string.Join(" ", Strasse, Hausnummer),
            string.Join(" ", Postleitzahl, Stadt));

        public int AdresseId { get; set; }
        public string Hausnummer { get; set; } = null!;
        public string Strasse { get; set; } = null!;
        public string Postleitzahl { get; set; } = null!;
        public string Stadt { get; set; } = null!;
        public string? Notiz { get; set; }
        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
    }
}
