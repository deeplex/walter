using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte")]
    public class KontaktListController : ControllerBase
    {
        public class PersonEntryBase
        {
            protected IPerson? Entity { get; }

            public int Id { get; set; }
            public Guid Guid { get; set; }
            public string Name { get; set; } = null!;
            public string? Email { get; set; }
            public string? Fax { get; set; }
            public string? Notiz { get; set; }
            public string? Telefon { get; set; }
            public string? Mobil { get; set; }
            public AdresseEntryBase? Adresse { get; set; }

            public bool NatuerlichePerson { get; set; }

            protected PersonEntryBase() { }
            public PersonEntryBase(IPerson p)
            {
                Entity = p;
                if (p is NatuerlichePerson n)
                {
                    Id = n.NatuerlichePersonId;
                    NatuerlichePerson = true;
                }
                else if (p is JuristischePerson j)
                {
                    Id = -j.JuristischePersonId;
                    NatuerlichePerson = false;
                }
                else
                {
                    throw new EntryPointNotFoundException();
                }

                Guid = p.PersonId;
                Name = p.Bezeichnung;
                Email = p.Email;
                Fax = p.Fax;
                Notiz = p.Notiz;
                Telefon = p.Telefon;
                Mobil = p.Mobil;

                if (Entity.Adresse is Adresse a)
                {
                    Adresse = new AdresseEntryBase(a);
                }
            }
        }

        public class PersonEntry : PersonEntryBase
        {
            private IEnumerable<Vertrag>? GetVertraege()
            {
                if (DbService == null)
                {
                    return null;
                }
                var Person = DbService!.ctx.FindPerson(Guid).JuristischePersonen.Select(e => e.PersonId).ToList();
                var asMieter = DbService!.ctx.MieterSet.Where(e => e.PersonId == Guid || Person!.Contains(e.PersonId)).Select(e => e.Vertrag).ToList();
                var asOther = DbService!.ctx.Vertraege.Where(e =>
                    Guid == e.Wohnung.BesitzerId ||
                    Guid == e.AnsprechpartnerId ||
                    Person.Contains(e.Wohnung.BesitzerId) ||
                    (e.AnsprechpartnerId.HasValue ? Person.Contains(e.AnsprechpartnerId ?? Guid.Empty) : false)).ToList();

                asOther.AddRange(asMieter);

                return asOther.AsQueryable().DistinctBy(e => e.VertragId);
            }

            private WalterDbService.WalterDb? DbService { get; set; }

            public IEnumerable<SelectionEntry>? SelectedJuristischePersonen { get; set; }
            public IEnumerable<PersonEntryBase>? JuristischePersonen
                => Entity?.JuristischePersonen.Select(e => new PersonEntryBase(e));
            public IEnumerable<VertragEntryBase>? Vertraege
                => GetVertraege()?.Select(e => new VertragEntryBase(e, DbService!));
            public IEnumerable<WohnungEntryBase>? Wohnungen
                => GetVertraege()?.Select(e => e.Wohnung).Distinct().Select(e => new WohnungEntryBase(e, DbService!));

            protected PersonEntry() : base() { }
            public PersonEntry(IPerson entity, WalterDbService.WalterDb dbService) : base(entity)
            {
                DbService = dbService;
                SelectedJuristischePersonen = Entity!.JuristischePersonen.Select(e => new SelectionEntry(e.PersonId, dbService.ctx.FindPerson(e.PersonId).Bezeichnung));
            }
        }

        private readonly ILogger<KontaktListController> _logger;
        private WalterDbService.WalterDb DbService { get; }

        public KontaktListController(ILogger<KontaktListController> logger, WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var np = DbService.ctx.NatuerlichePersonen.ToList().Select(e => new PersonEntryBase(e)).ToList();
            var jp = DbService.ctx.JuristischePersonen.ToList().Select(e => new PersonEntryBase(e)).ToList();
            return new OkObjectResult(np.Concat(jp));
        }
    }
}
