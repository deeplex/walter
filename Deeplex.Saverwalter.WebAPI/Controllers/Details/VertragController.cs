using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.MieteListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.MietminderungListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/vertraege/{id}")]
    public class VertragController : ControllerBase
    {
        public class VertragEntryBase
        {
            protected Vertrag Entity { get; }

            public int id => Entity.VertragId;
            public DateTime Beginn => Entity.Beginn();
            public DateTime? Ende => Entity.Ende;
            public WohnungEntryBase Wohnung => new WohnungEntryBase(Entity.Wohnung);
            public Guid? AnsprechpartnerId => Entity.AnsprechpartnerId;

            public VertragEntryBase(Vertrag entity)
            {
                Entity = entity;
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            // TODO Versionen
            private IWalterDbService DbService { get; }

            public IEnumerable<KontaktListEntry> Mieter => DbService.ctx.MieterSet.Where(m => m.Vertrag.VertragId == Entity.VertragId).ToList().Select(e => new KontaktListEntry(DbService.ctx.FindPerson(e.PersonId), DbService));
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<MieteListEntry> Mieten => Entity.Mieten.Select(e => new MieteListEntry(e));
            public IEnumerable<MietminderungListEntry> Mietminderungen => Entity.Mietminderungen.Select(e => new MietminderungListEntry(e));
            // TODO Garagen

            public VertragEntry(Vertrag entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<VertragController> _logger;
        private IWalterDbService DbService { get; }

        public VertragController(ILogger<VertragController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public VertragEntry Get(int id)
        {
            return new VertragEntry(DbService.ctx.Vertraege.Find(id), DbService);
        }
    }
}
