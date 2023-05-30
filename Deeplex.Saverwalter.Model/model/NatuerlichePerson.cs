using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class NatuerlichePerson : IPerson
    {
        public string Bezeichnung => string.IsNullOrEmpty(Vorname) ? Nachname : $"{Vorname} {Nachname}";

        public int NatuerlichePersonId { get; set; }
        public Guid PersonId { get; set; }
        [Required]
        public string Nachname { get; set; }
        public string? Vorname { get; set; }
        public Titel Titel { get; set; }
        public Anrede Anrede { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }

        public virtual List<JuristischePerson> JuristischePersonen { get; set; } = new List<JuristischePerson>();

        public NatuerlichePerson(string nachname)
        {
            Nachname = nachname;

            PersonId = Guid.NewGuid();
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
}
