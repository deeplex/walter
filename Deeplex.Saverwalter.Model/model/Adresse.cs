using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Adresse
    {
        public string Anschrift => string.Join(", ",
            string.Join(" ", Strasse, Hausnummer),
            string.Join(" ", Postleitzahl, Stadt));

        public int AdresseId { get; set; }
        [Required]
        public string Hausnummer { get; set; }
        [Required]
        public string Strasse { get; set; }
        [Required]
        public string Postleitzahl { get; set; }
        [Required]
        public string Stadt { get; set; }
        public string? Notiz { get; set; }

        public virtual List<Wohnung> Wohnungen { get; set; } = new List<Wohnung>();
        public virtual List<Garage> Garagen { get; private set; } = new List<Garage>();
        public virtual List<JuristischePerson> JuristischePersonen { get; private set; } = new List<JuristischePerson>();
        public virtual List<NatuerlichePerson> NatuerlichePersonen { get; private set; } = new List<NatuerlichePerson>();
        public virtual List<Zaehler> Zaehler { get; private set; } = new List<Zaehler>();

        public Adresse(string strasse, string hausnummer, string postleitzahl, string stadt)
        {
            Strasse= strasse;
            Hausnummer= hausnummer;
            Postleitzahl = postleitzahl;
            Stadt = stadt;
        }
    }
}
