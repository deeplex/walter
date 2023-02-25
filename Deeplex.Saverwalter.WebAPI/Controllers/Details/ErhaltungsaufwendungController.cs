using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/erhaltungsaufwendungen/{id}")]
    public class ErhaltungsaufwendungController : ControllerBase
    {
        public class ErhaltungsaufwendungEntryBase
        {
            protected Erhaltungsaufwendung Entity { get; }

            public int Id => Entity.ErhaltungsaufwendungId;
            public double Betrag => Entity.Betrag;
            public DateTime Datum => Entity.Datum;
            public string Notiz => Entity.Notiz ?? "";
            public string Bezeichnung => Entity.Bezeichnung;

            public ErhaltungsaufwendungEntryBase(Erhaltungsaufwendung entity)
            {
                Entity = entity;
            }
        }

        public class ErhaltungsaufwendungEntry : ErhaltungsaufwendungEntryBase
        {
            IWalterDbService DbService { get; }

            public PersonEntryBase Aussteller => new PersonEntryBase(DbService.ctx.FindPerson(Entity.AusstellerId));
            public WohnungEntryBase Wohnung => new WohnungEntryBase(Entity.Wohnung);
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));

            public ErhaltungsaufwendungEntry(Erhaltungsaufwendung entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<ErhaltungsaufwendungController> _logger;
        private IWalterDbService DbService { get; }

        public ErhaltungsaufwendungController(ILogger<ErhaltungsaufwendungController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "GetErhaltungsaufwendung")]
        public ErhaltungsaufwendungEntry Get(int id)
        {
            return new ErhaltungsaufwendungEntry(DbService.ctx.Erhaltungsaufwendungen.Find(id), DbService);
        }
    }
}
