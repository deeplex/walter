using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public class PersonEntry
    {
        private IPerson Entity { get; }

        public Guid Guid => Entity.PersonId;
        public string Name => Entity.Bezeichnung;
        public string Email => Entity.Email ?? "";
        public string Fax => Entity.Fax ?? "";
        public string Notiz => Entity.Notiz ?? "";
        public bool natuerlichePerson => Entity is NatuerlichePerson;
        //public IEnumerable<KontaktListEntry> JuristischePersonen => Entity.JuristischePersonen.Select(e => new KontaktListEntry(e));
        public string Telefon => Entity.Telefon ?? "";
        public AdresseEntry? Adresse => Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;

        public PersonEntry(IPerson entity)
        {
            Entity = entity;
        }
    }
}
