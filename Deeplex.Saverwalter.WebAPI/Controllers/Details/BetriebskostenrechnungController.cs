using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/betriebskostenrechnungen/{id}")]
    public class BetriebskostenrechnungController : ControllerBase
    {
        public class BetriebskostenrechnungEntryBase
        {
            protected Betriebskostenrechnung Entity { get; } = null!;

            public int Id { get; set; }
            public double Betrag { get; set; }
            public int BetreffendesJahr { get; set; }
            public DateTime Datum { get; set; }
            public string? Notiz { get; set; }

            protected BetriebskostenrechnungEntryBase() { }
            public BetriebskostenrechnungEntryBase(Betriebskostenrechnung entity)
            {
                Entity = entity;
                Id = Entity.BetriebskostenrechnungId;

                Betrag = Entity.Betrag;
                BetreffendesJahr = Entity.BetreffendesJahr;
                Datum = Entity.Datum;
                Notiz = Entity.Notiz ?? "";
            }
        }

        public class BetriebskostenrechnungEntry : BetriebskostenrechnungEntryBase
        {
            private IWalterDbService DbService { get; } = null!;

            public UmlageEntry Umlage { get; set; } = null!;
            public IEnumerable<WohnungListEntry>? Wohnungen => Entity?.Umlage.Wohnungen.Select(e => new WohnungListEntry(e, DbService));
            public IEnumerable<AnhangListEntry>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangListEntry(e));

            public BetriebskostenrechnungEntry() : base() { }
            public BetriebskostenrechnungEntry(Betriebskostenrechnung entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;

                Umlage = new UmlageEntry(Entity.Umlage, DbService);
            }
        }

        private readonly ILogger<BetriebskostenrechnungController> _logger;
        IWalterDbService DbService { get; }
        BetriebskostenrechnungControllerService Service { get; }

        public BetriebskostenrechnungController(
            ILogger<BetriebskostenrechnungController> logger,
            IWalterDbService dbService,
            BetriebskostenrechnungControllerService service)
        {
            _logger = logger;
            DbService = dbService;
            Service = service;
        }

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(int id) => Service.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post([FromBody] BetriebskostenrechnungEntry entry) => Service.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(int id, BetriebskostenrechnungEntry entry) => Service.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(int id) => Service.Delete(id);
    }
}
