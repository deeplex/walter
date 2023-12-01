using Deeplex.Saverwalter.Model.Auth;
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
        public virtual Kontakt? Besitzer { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }

        public virtual List<Vertrag> Vertraege { get; private set; } = [];
        public virtual List<Zaehler> Zaehler { get; private set; } = [];
        public virtual List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; private set; } = [];
        public virtual List<Umlage> Umlagen { get; private set; } = [];
        public virtual List<Verwalter> Verwalter { get; private set; } = [];

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Wohnung(string bezeichnung, double wohnflaeche, double nutzflaeche, int nutzeinheit)
        {
            Bezeichnung = bezeichnung;
            Wohnflaeche = wohnflaeche;
            Nutzflaeche = nutzflaeche;
            Nutzeinheit = nutzeinheit;
        }

        public List<Guid> AssembleReadUserGuids()
        {
            var accounts = new List<UserAccount>();
            if (Besitzer != null)
            {
                accounts.AddRange(Besitzer.Accounts);
            }
            foreach (var verwalter in Verwalter)
            {
                accounts.AddRange(Verwalter.SelectMany(verwalter => verwalter.Kontakt.Accounts));
            }

            var guids = new List<Guid>();
            foreach (var account in accounts)
            {
                guids.Add(account.Id);
            }

            return guids;
        }
    }
}
