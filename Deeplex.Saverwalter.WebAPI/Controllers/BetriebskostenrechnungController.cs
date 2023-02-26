using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
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
            public DateTime Datum { get; set; }
            public string? Notiz { get; set; }
            public Betriebskostentyp? Typ { get; set; }
            public string? WohnungenBezeichnung { get; set; }

            protected BetriebskostenrechnungEntryBase() { }
            public BetriebskostenrechnungEntryBase(Betriebskostenrechnung entity)
            {
                Entity = entity;
                Id = Entity.BetriebskostenrechnungId;

                Betrag = Entity.Betrag;
                BetreffendesJahr = Entity.BetreffendesJahr;
                Datum = Entity.Datum;
                Typ = Entity.Umlage?.Typ;
                WohnungenBezeichnung = Entity.Umlage?.GetWohnungenBezeichnung();
                Notiz = Entity.Notiz;
            }
        }

        public class BetriebskostenrechnungEntry : BetriebskostenrechnungEntryBase
        {
            private IWalterDbService? DbService { get; }

            public UmlageEntry Umlage { get; set; } = null!;
            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Umlage.Wohnungen.Select(e => new WohnungEntryBase(e, DbService!));
            public IEnumerable<AnhangEntryBase>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangEntryBase(e));

            public BetriebskostenrechnungEntry() : base() { }
            public BetriebskostenrechnungEntry(Betriebskostenrechnung entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;

                Umlage = new UmlageEntry(Entity!.Umlage, DbService);
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
            => new OkObjectResult(DbService.ctx.Betriebskostenrechnungen
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
