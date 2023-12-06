using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
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
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

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

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;
            }
        }

        public class AdresseEntry : AdresseEntryBase
        {
            public IEnumerable<WohnungEntryBase>? Wohnungen
                => Entity?.Wohnungen.Select(e => new WohnungEntryBase(e));
            public IEnumerable<KontaktEntryBase>? Kontakte
                => Entity?.Kontakte.Select(e => new KontaktEntryBase(e));
            public IEnumerable<ZaehlerEntryBase>? Zaehler
                => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e));


            public AdresseEntry() : base() { }
            public AdresseEntry(Adresse entity) : base(entity)
            {
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
        public Task<IActionResult> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<IActionResult> Post([FromBody] AdresseEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, AdresseEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
