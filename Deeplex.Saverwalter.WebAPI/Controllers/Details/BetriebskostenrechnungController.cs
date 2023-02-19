using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.AnhangController;
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
            public UmlageEntry Umlage => new UmlageEntry(Entity.Umlage);
            public IEnumerable<WohnungListEntry> Wohnungen => Entity.Umlage.Wohnungen.Select(e => new WohnungListEntry(e));
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));

            public BetriebskostenrechnungEntry(Betriebskostenrechnung entity) : base(entity)
            {
            }
        }

        private readonly ILogger<BetriebskostenrechnungController> _logger;

        public BetriebskostenrechnungController(ILogger<BetriebskostenrechnungController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetBetriebskostenrechnung")]
        public BetriebskostenrechnungEntry Get(int id)
        {
            return new BetriebskostenrechnungEntry(Program.ctx.Betriebskostenrechnungen.Find(id));
        }
    }
}
