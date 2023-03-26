using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Wohnung
    {
        public int WohnungId { get; set; }
        [Required]
        public string Bezeichnung { get; set; }
        [Required]
        public double Wohnflaeche { get; set; }
        [Required]
        public double Nutzflaeche { get; set; }
        [Required]
        public int Nutzeinheit { get; set; } // TODO Rename to Nutzeinheiten
        public virtual Adresse? Adresse { get; set; }
        public Guid BesitzerId { get; set; }
        public string? Notiz { get; set; }

        public virtual List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
        public virtual List<Zaehler> Zaehler { get; private set; } = new List<Zaehler>();
        public virtual List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; private set; } = new List<Erhaltungsaufwendung>();
        public virtual List<Umlage> Umlagen { get; private set; } = new List<Umlage>();

        public Wohnung(string bezeichnung, double wohnflaeche, double nutzflaeche, int nutzeinheit)
        {
            Bezeichnung = bezeichnung;
            Wohnflaeche = wohnflaeche;
            Nutzflaeche = nutzflaeche;
            Nutzeinheit = nutzeinheit;
        }
    }
}
