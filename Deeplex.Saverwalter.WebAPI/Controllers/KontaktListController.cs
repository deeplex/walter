using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.JuristischePersonController;

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
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Fax { get; set; }
            public string? Notiz { get; set; }
            public string? Telefon { get; set; }
            public string? Mobil { get; set; }
            public AdresseEntry? Adresse { get; set; }

            protected PersonEntryBase() { }
            public PersonEntryBase(IPerson p)
            {
                if (p is NatuerlichePerson n)
                {
                    Entity = n;
                    Id = n.NatuerlichePersonId;
                }
                else if (p is JuristischePerson j)
                {
                    Entity = j;
                    Id = -j.JuristischePersonId;
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
                    Adresse = new AdresseEntry(a);
                }

            }
        }

        public class PersonEntry : PersonEntryBase
        {
            public IEnumerable<JuristischePersonEntryBase>? JuristischePersonen
                => Entity!.JuristischePersonen.Select(e => new JuristischePersonEntryBase(e))
                .ToList();

            protected PersonEntry() : base() { }
            public PersonEntry(IPerson entity) : base(entity) { }
        }

        private readonly ILogger<KontaktListController> _logger;
        private IWalterDbService DbService { get; }

        public KontaktListController(ILogger<KontaktListController> logger, IWalterDbService dbService)
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
