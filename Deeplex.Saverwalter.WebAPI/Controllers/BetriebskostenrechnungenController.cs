using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BetriebskostenrechnungenController : ControllerBase
    {
        public class BetriebskostenrechungListEntry
        {
            private Betriebskostenrechnung Entity { get; }

            public int id => Entity.BetriebskostenrechnungId;
            public string Typ => Entity.Umlage.Typ.ToDescriptionString();
            public string Betrag => Entity.Betrag.Euro();
            public string Wohnung => Entity.GetWohnungenBezeichnung();
            public string BetreffendesJahr => Entity.BetreffendesJahr.ToString();
            public string Datum => Entity.Datum.Datum();

            public BetriebskostenrechungListEntry(Betriebskostenrechnung b)
            {
                Entity = b;
            }
        }

        private readonly ILogger<BetriebskostenrechnungenController> _logger;

        public BetriebskostenrechnungenController(ILogger<BetriebskostenrechnungenController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetBetriebskostenrechnungen")]
        public IEnumerable<BetriebskostenrechungListEntry> Get()
        {
            return Program.ctx.Betriebskostenrechnungen
                .Include(e => e.Umlage)
                .Select(e => new BetriebskostenrechungListEntry(e)).ToList();
        }
    }
}
