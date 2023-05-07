using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/adressen")]
    public class AdresseController : ControllerBase
    {
        public class AdresseEntryBase
        {
            protected Adresse? Entity { get; }

            public int Id { get; set; }
            public string Strasse { get; set; } = string.Empty;
            public string Hausnummer { get; set; } = string.Empty;
            public string Postleitzahl { get; set; } = string.Empty;
            public string Stadt { get; set; } = string.Empty;
            public string? Anschrift { get; set; }
            public string? Notiz { get; set; }

            public AdresseEntryBase() { }
            public AdresseEntryBase(Adresse entity)
            {
                Entity = entity;
                Id = Entity.AdresseId;

                Strasse = Entity.Strasse;
                Hausnummer = Entity.Hausnummer;
                Postleitzahl = Entity.Postleitzahl;
                Stadt = Entity.Stadt;
                Anschrift = Entity.Anschrift;
                Notiz = Entity.Notiz;
            }
        }

        public class AdresseEntry : AdresseEntryBase
        {
            private SaverwalterContext? Ctx { get; }

            public IEnumerable<WohnungEntryBase>? Wohnungen
                => Entity?.Wohnungen.Select(e => new WohnungEntryBase(e, Ctx!));
            public IEnumerable<PersonEntryBase>? Kontakte
                => Entity?.JuristischePersonen.Select(e => new PersonEntryBase(e))
                    .Concat(Entity.NatuerlichePersonen.Select(e => new PersonEntryBase(e)));
            public IEnumerable<ZaehlerEntryBase>? Zaehler
                => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e));


            public AdresseEntry() : base() { }
            public AdresseEntry(Adresse entity, SaverwalterContext ctx) : base(entity)
            {
                Ctx = ctx;
            }
        }

        private readonly ILogger<AdresseController> _logger;
        AdresseDbService DbService { get; }

        public AdresseController(ILogger<AdresseController> logger, AdresseDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public IActionResult Get()
            => new OkObjectResult(DbService.Ctx.Adressen
            .ToList()
            .Select(e => new AdresseEntryBase(e))
            .ToList());
        [HttpPost]
        public IActionResult Post([FromBody] AdresseEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, AdresseEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
