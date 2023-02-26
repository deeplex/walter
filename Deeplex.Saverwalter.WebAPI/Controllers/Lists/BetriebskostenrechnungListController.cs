using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.BetriebskostenrechnungController;

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
        private IWalterDbService DbService { get; }

        public BetriebskostenrechnungListController(ILogger<BetriebskostenrechnungListController> logger, IWalterDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet(Name = "[Controller]")]
        public IEnumerable<BetriebskostenrechungListEntry> Get()
        {
            return DbService.ctx.Betriebskostenrechnungen.Select(e => new BetriebskostenrechungListEntry(e)).ToList();
        }
    }
}
