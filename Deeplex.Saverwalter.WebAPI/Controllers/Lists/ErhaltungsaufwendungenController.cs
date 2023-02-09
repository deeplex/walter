using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleInjector;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErhaltungsaufwendungenController : ControllerBase
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

        private readonly ILogger<ErhaltungsaufwendungenController> _logger;

        public ErhaltungsaufwendungenController(ILogger<ErhaltungsaufwendungenController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetErhaltungsaufwendungen")]
        public IEnumerable<ErhaltungsaufwendungListEntry> Get()
        {
            return Program.ctx.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungListEntry(e)).ToList();
        }
    }
}
