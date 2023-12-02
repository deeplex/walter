using Deeplex.Saverwalter.Model.Auth;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Kontakt
    {
        public string Bezeichnung => string.IsNullOrEmpty(Vorname) ? Name : $"{Vorname} {Name}";

        public int KontaktId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Rechtsform Rechtsform { get; set; }
        public string? Vorname { get; set; }
        public Anrede Anrede { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }

        // Only valid if Rechtsform != natuerlich
        public virtual List<Kontakt> JuristischePersonen { get; set; } = [];
        public virtual List<Kontakt> Mitglieder { get; set; } = [];

        public virtual List<Vertrag> VerwaltetVertraege { get; private set; } = [];
        public virtual List<Vertrag> Mietvertraege { get; private set; } = [];
        public virtual List<Wohnung> Wohnungen { get; private set; } = [];
        public virtual List<Garage> Garagen { get; private set; } = [];
        public virtual List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; private set; } = [];
        public virtual List<UserAccount> Accounts { get; private set; } = [];

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Kontakt(string name, Rechtsform rechtsform)
        {
            Name = name;
            Rechtsform = rechtsform;
        }
    }

    public enum Anrede
    {
        Herr,
        Frau,
        Keine,
    }

    public enum Titel
    {
        Kein,
        Doktor,
    }

    public enum Rechtsform
    {
        [Description("Natürliche Person")]
        natuerlich,
        [Description("GmbH")]
        gmbh,
        [Description("GbR")]
        gbr,
        [Description("AG")]
        ag
    }
}
