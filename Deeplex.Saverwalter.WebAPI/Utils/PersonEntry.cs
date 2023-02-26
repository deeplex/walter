using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.JuristischePersonController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.VertragListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;

namespace Deeplex.Saverwalter.WebAPI.Helper
{
    public class PersonEntryBase
    {
        protected IPerson Entity { get; } = null!;

        public Guid Guid { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Fax { get; set; }
        public string? Notiz { get; set; }
        public string? Telefon { get; set; }
        public AdresseEntry? Adresse { get; set; }

        public bool NatuerlichePerson { get; set; }

        protected PersonEntryBase() { }
        public PersonEntryBase(IPerson entity)
        {
            Entity = entity;

            Guid = Entity.PersonId;
            Name = Entity.Bezeichnung;
            Email = Entity.Email;
            Fax = Entity.Fax;
            Notiz = Entity.Notiz;
            Telefon = Entity.Telefon;
            Adresse = Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;

            NatuerlichePerson = Entity is NatuerlichePerson;
        }
    }

    public class PersonEntry : PersonEntryBase
    {
        IWalterDbService DbService { get; } = null!;
        public IEnumerable<KontaktListEntry>? JuristischePersonen => Entity?.JuristischePersonen.Select(e => new KontaktListEntry(e, DbService));

        protected PersonEntry() : base() { }
        public PersonEntry(IPerson entity, IWalterDbService dbService) : base(entity)
        {
            DbService = dbService;
        }
    }
}
