using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public interface IAdresse
    {
        public Adresse Adresse { get; set; }
    }

    // An Adresse is pointed at by a Wohnung, Garage or Kontakt.
    public class Adresse
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
        public virtual List<Wohnung> Wohnungen { get; set; } = new List<Wohnung>();
        public virtual List<Garage> Garagen { get; private set; } = new List<Garage>();
    }
}
