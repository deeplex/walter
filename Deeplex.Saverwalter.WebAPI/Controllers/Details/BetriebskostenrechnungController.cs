using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/betriebskostenrechnungen/{id}")]
    public class BetriebskostenrechnungController
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
            public IEnumerable<AnhangEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangEntry(e));

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
