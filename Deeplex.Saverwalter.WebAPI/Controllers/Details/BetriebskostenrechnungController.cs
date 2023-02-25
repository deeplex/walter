using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
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

        public BetriebskostenrechnungController(ILogger<BetriebskostenrechnungController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public BetriebskostenrechnungEntry Get(int id)
        {
            return new BetriebskostenrechnungEntry(DbService.ctx.Betriebskostenrechnungen.Find(id), DbService);
        }
    }
}
