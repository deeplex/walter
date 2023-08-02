using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/betriebskostenrechnungen")]
    public class BetriebskostenrechnungController : ControllerBase
    {
        public class BetriebskostenrechnungEntryBase
        {
            protected Betriebskostenrechnung? Entity { get; }

            public int Id { get; set; }
            public double Betrag { get; set; }
            public int BetreffendesJahr { get; set; }
            public DateOnly Datum { get; set; }
            public string? Notiz { get; set; }
            public SelectionEntry Typ { get; set; } = null!;
            public SelectionEntry Umlage { get; set; } = null!;

            protected BetriebskostenrechnungEntryBase() { }
            public BetriebskostenrechnungEntryBase(Betriebskostenrechnung entity)
            {
                Entity = entity;
                Id = Entity.BetriebskostenrechnungId;

                Betrag = Entity.Betrag;
                BetreffendesJahr = Entity.BetreffendesJahr;
                Datum = Entity.Datum;
                Notiz = Entity.Notiz;
                Typ = new SelectionEntry((int)Entity.Umlage.Typ, Entity.Umlage.Typ.ToDescriptionString());
                Umlage = new SelectionEntry(
                    entity.Umlage.UmlageId,
                    entity.Umlage.GetWohnungenBezeichnung(),
                    ((int)entity.Umlage.Typ).ToString());
            }
        }

        public class BetriebskostenrechnungEntry : BetriebskostenrechnungEntryBase
        {
            private SaverwalterContext? Ctx { get; }

            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen
                => Entity?.Umlage?.Betriebskostenrechnungen.Select(e => new BetriebskostenrechnungEntryBase(e));
            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Umlage.Wohnungen.Select(e => new WohnungEntryBase(e, Ctx!));

            public BetriebskostenrechnungEntry() : base() { }
            public BetriebskostenrechnungEntry(Betriebskostenrechnung entity, SaverwalterContext ctx) : base(entity)
            {
                Ctx = ctx;
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
        public IActionResult Get()
            => new OkObjectResult(DbService.Ctx.Betriebskostenrechnungen
            .ToList()
            .Select(e => new BetriebskostenrechnungEntryBase(e))
            .ToList());
        [HttpPost]
        public IActionResult Post([FromBody] BetriebskostenrechnungEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, BetriebskostenrechnungEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
