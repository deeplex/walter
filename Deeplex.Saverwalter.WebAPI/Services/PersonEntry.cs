using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.JuristischePersonController;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public class PersonEntryBase
    {
        protected IPerson Entity { get; }

        public Guid Guid => Entity.PersonId;
        public string Name => Entity.Bezeichnung;
        public string Email => Entity.Email ?? "";
        public string Fax => Entity.Fax ?? "";
        public string Notiz => Entity.Notiz ?? "";
        public bool natuerlichePerson => Entity is NatuerlichePerson;
        public string Telefon => Entity.Telefon ?? "";
        public AdresseEntry? Adresse => Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;

        public PersonEntryBase(IPerson entity)
        {
            Entity = entity;
        }
    }

    public class PersonEntry : PersonEntryBase
    {
        public IEnumerable<PersonEntryBase> JuristischePersonen => Entity.JuristischePersonen.Select(e => new PersonEntryBase(e));

        public PersonEntry(IPerson entity) : base(entity)
        {
        }
    }
}
