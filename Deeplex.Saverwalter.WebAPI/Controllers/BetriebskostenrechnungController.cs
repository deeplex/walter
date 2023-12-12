using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/betriebskostenrechnungen")]
    public class BetriebskostenrechnungController : ControllerBase
    {
        public class BetriebskostenrechnungEntryBase
        {
            public int Id { get; set; }
            public double Betrag { get; set; }
            public int BetreffendesJahr { get; set; }
            public DateOnly Datum { get; set; }
            public SelectionEntry? Typ { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public BetriebskostenrechnungEntryBase() { }
            public BetriebskostenrechnungEntryBase(Betriebskostenrechnung entity, Permissions permissions)
            {
                Id = entity.BetriebskostenrechnungId;

                Betrag = entity.Betrag;
                BetreffendesJahr = entity.BetreffendesJahr;
                Datum = entity.Datum;
                Typ = new SelectionEntry(entity.Umlage.Typ.UmlagetypId, entity.Umlage.Typ.Bezeichnung);

                Permissions = permissions;
            }
        }

        public class BetriebskostenrechnungEntry : BetriebskostenrechnungEntryBase
        {
            public SelectionEntry? Umlage { get; set; }
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            private Betriebskostenrechnung? Entity { get; }

            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen
                => Entity?.Umlage?.Betriebskostenrechnungen.Select(e => new BetriebskostenrechnungEntryBase(e, new()));
            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Umlage.Wohnungen.Select(e => new WohnungEntryBase(e, new()));

            public BetriebskostenrechnungEntry() : base() { }
            public BetriebskostenrechnungEntry(Betriebskostenrechnung entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;
                Umlage = new SelectionEntry(
                    entity.Umlage.UmlageId,
                    entity.Umlage.GetWohnungenBezeichnung(),
                    entity.Umlage.Typ.UmlagetypId.ToString());

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;

                Entity = entity;
            }
        }

        private readonly ILogger<BetriebskostenrechnungController> _logger;
        BetriebskostenrechnungDbService DbService { get; }

        public BetriebskostenrechnungController(ILogger<BetriebskostenrechnungController> logger, BetriebskostenrechnungDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<IActionResult> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<IActionResult> Post([FromBody] BetriebskostenrechnungEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, BetriebskostenrechnungEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
