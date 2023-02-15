using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/betriebskostenrechnungen")]
    public class BetriebskostenrechnungListController : ControllerBase
    {
        public class BetriebskostenrechungListEntry
        {
            private Betriebskostenrechnung Entity { get; }

            public int id => Entity.BetriebskostenrechnungId;
            public string Typ => Entity.Umlage.Typ.ToDescriptionString();
            public string Betrag => Entity.Betrag.Euro();
            public string WohnungenBezeichnung => Entity.GetWohnungenBezeichnung();
            public string BetreffendesJahr => Entity.BetreffendesJahr.ToString();
            public string Datum => Entity.Datum.Datum();

            public BetriebskostenrechungListEntry(Betriebskostenrechnung b)
            {
                Entity = b;
            }
        }

        private readonly ILogger<BetriebskostenrechnungListController> _logger;

        public BetriebskostenrechnungListController(ILogger<BetriebskostenrechnungListController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetBetriebskostenrechnungList")]
        public IEnumerable<BetriebskostenrechungListEntry> Get()
        {
            return Program.ctx.Betriebskostenrechnungen
                .Include(e => e.Umlage)
                .ThenInclude(e => e.Wohnungen)
                .ThenInclude(e => e.Adresse)
                .Select(e => new BetriebskostenrechungListEntry(e)).ToList();
        }
    }
}
