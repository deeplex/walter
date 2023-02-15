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
            private Erhaltungsaufwendung Entity { get; }

            public int id => Entity.ErhaltungsaufwendungId;
            public string Betrag => Entity.Betrag.Euro();
            public string Bezeichnung => Entity.Bezeichnung;
            public string Aussteller => Program.FindPerson(Entity.AusstellerId) is IPerson p ? p.Bezeichnung : "Unbekannt";
            public string Datum => Entity.Datum.Datum();

            public ErhaltungsaufwendungListEntry(Erhaltungsaufwendung e)
            {
                Entity = e;
            }
        }

        private readonly ILogger<ErhaltungsaufwendungListController> _logger;

        public ErhaltungsaufwendungListController(ILogger<ErhaltungsaufwendungListController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetErhaltungsaufwendungList")]
        public IEnumerable<ErhaltungsaufwendungListEntry> Get()
        {
            return Program.ctx.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungListEntry(e)).ToList();
        }
    }
}
