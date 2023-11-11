using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class JuristischePerson : IPerson
    {
        public int JuristischePersonId { get; set; }
        public Guid PersonId { get; set; }
        [Required]
        public string Bezeichnung { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }
        // Remove Wohnungen and Garagen (deprecated)
        public virtual List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
        public virtual List<Garage> Garagen { get; private set; } = new List<Garage>();
        public virtual List<JuristischePerson> JuristischeMitglieder { get; private set; } = new List<JuristischePerson>();
        public virtual List<NatuerlichePerson> NatuerlicheMitglieder { get; private set; } = new List<NatuerlichePerson>();
        public virtual List<JuristischePerson> JuristischePersonen { get; set; } = new List<JuristischePerson>();

        public List<IPerson> Mitglieder => JuristischeMitglieder.Select(w => (IPerson)w)
            .Concat(NatuerlicheMitglieder.Select(w => (IPerson)w)).ToList();

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public JuristischePerson(string bezeichnung)
        {
            Bezeichnung = bezeichnung;

            PersonId = Guid.NewGuid();
        }
    }
}
