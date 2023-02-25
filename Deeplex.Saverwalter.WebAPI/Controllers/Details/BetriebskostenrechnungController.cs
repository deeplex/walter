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
            protected Betriebskostenrechnung Entity { get; }

            public int Id => Entity.BetriebskostenrechnungId;
            public double Betrag => Entity.Betrag;
            public int BetreffendesJahr => Entity.BetreffendesJahr;
            public DateTime Datum => Entity.Datum;
            public string Notiz => Entity.Notiz ?? "";

            public BetriebskostenrechnungEntryBase(Betriebskostenrechnung entity)
            {
                Entity = entity;
            }
        }

        public class BetriebskostenrechnungEntry : BetriebskostenrechnungEntryBase
        {
            private IWalterDbService DbService { get; }

            public UmlageEntry Umlage => new UmlageEntry(Entity.Umlage, DbService);
            public IEnumerable<WohnungListEntry> Wohnungen => Entity.Umlage.Wohnungen.Select(e => new WohnungListEntry(e, DbService));
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));

            public BetriebskostenrechnungEntry(Betriebskostenrechnung entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<BetriebskostenrechnungController> _logger;
        IWalterDbService DbService { get; }
        BetriebskostenrechnungControllerService Service { get; }

        public BetriebskostenrechnungController(ILogger<BetriebskostenrechnungController> logger, IWalterDbService dbService, BetriebskostenrechnungControllerService service)
        {
            _logger = logger;
            DbService = dbService;
            Service = service;
        }

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(int id) => Service.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post(BetriebskostenrechnungEntry entry) => Service.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(int id, BetriebskostenrechnungEntry entry) => Service.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(int id, BetriebskostenrechnungEntry entry) => Service.Delete(id, entry);
    }
}
