using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.NatuerlichePersonController;
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
            protected Vertrag? Entity { get; }

            public int? Id { get; set; }
            public DateTime? Beginn { get; set; }
            public DateTime? Ende { get; set; }
            public WohnungEntryBase? Wohnung { get; set; }
            public Guid? AnsprechpartnerId { get; set; }
            public string? Notiz { get; set; }

            public VertragEntryBase() { }
            public VertragEntryBase(Vertrag entity)
            {
                Entity = entity;

                Id = Entity.VertragId;
                Beginn = Entity.Beginn();
                Ende = Entity.Ende;
                Wohnung = new WohnungEntryBase(Entity.Wohnung);
                AnsprechpartnerId = Entity.AnsprechpartnerId;
                Notiz= Entity.Notiz;
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            // TODO Versionen
            private IWalterDbService? DbService { get; }

            public IEnumerable<AnhangListEntry>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<MieteListEntry>? Mieten => Entity?.Mieten.Select(e => new MieteListEntry(e));
            public IEnumerable<MietminderungListEntry>? Mietminderungen => Entity?.Mietminderungen.Select(e => new MietminderungListEntry(e));
            public IEnumerable<KontaktListEntry>? Mieter => DbService?.ctx.MieterSet
                .Where(m => Entity != null && m.Vertrag.VertragId == Entity.VertragId)
                .ToList()
                .Select(e => new KontaktListEntry(DbService.ctx.FindPerson(e.PersonId), DbService));
            // TODO Garagen

            public VertragEntry() : base() { }
            public VertragEntry(Vertrag entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<VertragController> _logger;
        private VertragDbService DbService { get; }

        public VertragController(ILogger<VertragController> logger, VertragDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(int id) => DbService.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post([FromBody] VertragEntry entry) => DbService.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(int id, VertragEntry entry) => DbService.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
