using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleInjector;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/erhaltungsaufwendungen")]
    public class ErhaltungsaufwendungListController : ControllerBase
    {
        public class ErhaltungsaufwendungListEntry
        {
            private IWalterDbService DbService { get; }
            private Erhaltungsaufwendung Entity { get; }

            public int id => Entity.ErhaltungsaufwendungId;
            public string Betrag => Entity.Betrag.Euro();
            public string Bezeichnung => Entity.Bezeichnung;
            public string Aussteller => DbService.ctx.FindPerson(Entity.AusstellerId) is IPerson p ? p.Bezeichnung : "Unbekannt";
            public string Datum => Entity.Datum.Datum();
            public string Wohnung => Entity.Wohnung.Adresse.Anschrift + " - " + Entity.Wohnung.Bezeichnung;

            public ErhaltungsaufwendungListEntry(Erhaltungsaufwendung e, IWalterDbService dbService)
            {
                Entity = e;
                DbService = dbService;
            }
        }

        private readonly ILogger<ErhaltungsaufwendungListController> _logger;
        private IWalterDbService DbService { get; }

        public ErhaltungsaufwendungListController(ILogger<ErhaltungsaufwendungListController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "GetKontaktList")]
        public IEnumerable<ErhaltungsaufwendungListEntry> Get()
        {
            return DbService.ctx.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungListEntry(e, DbService)).ToList();
        }
    }
}
