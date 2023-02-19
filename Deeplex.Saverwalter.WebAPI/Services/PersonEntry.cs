using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.JuristischePersonController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.VertragListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;

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
        public bool NatuerlichePerson => Entity is NatuerlichePerson;
        public string Telefon => Entity.Telefon ?? "";
        public AdresseEntry? Adresse => Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;

        public PersonEntryBase(IPerson entity)
        {
            Entity = entity;
        }
    }

    public class PersonEntry : PersonEntryBase
    {
        public IEnumerable<KontaktListEntry> JuristischePersonen => Entity.JuristischePersonen.Select(e => new KontaktListEntry(e));

        public PersonEntry(IPerson entity) : base(entity)
        {
        }
    }
}
