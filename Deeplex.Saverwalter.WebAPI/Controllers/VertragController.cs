using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/vertraege")]
    public class VertragController : ControllerBase
    {
        public class VertragEntryBase
        {
            protected Vertrag? Entity { get; }

            public int Id { get; set; }
            public DateTime? Beginn { get; set; }
            public DateTime? Ende { get; set; }
            public SelectionEntry? Wohnung { get; set; }
            public SelectionEntry? Ansprechpartner { get; set; }
            public string? Notiz { get; set; }
            public string? MieterAuflistung { get; set; }
            public IEnumerable<SelectionEntry>? SelectedMieter { get; set; }

            public VertragEntryBase() { }
            public VertragEntryBase(Vertrag entity, IWalterDbService dbService)
            {
                Entity = entity;

                Id = Entity.VertragId;
                Beginn = Entity.Beginn();
                Ende = Entity.Ende;
                Wohnung = new(
                    Entity.Wohnung.WohnungId,
                    Entity.Wohnung.Adresse.Anschrift + " - " + Entity.Wohnung.Bezeichnung,
                    Entity.Wohnung.BesitzerId.ToString());
                Ansprechpartner = Entity.AnsprechpartnerId is Guid id && id != Guid.Empty ? new(id, dbService.ctx.FindPerson(id).Bezeichnung) : null;
                Notiz = Entity.Notiz;

                var Mieter = dbService.ctx.MieterSet
                    .Where(m => m.Vertrag.VertragId == Entity.VertragId)
                    .ToList();
                MieterAuflistung = string.Join(", ", Mieter
                    .Select(a => dbService.ctx.FindPerson(a.PersonId).Bezeichnung));
                SelectedMieter = Mieter.Select(e => new SelectionEntry(e.PersonId, dbService.ctx.FindPerson(e.PersonId).Bezeichnung));
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            // TODO Versionen
            private IWalterDbService? DbService { get; }

            public IEnumerable<AnhangEntryBase>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangEntryBase(e));
            public IEnumerable<MieteEntryBase>? Mieten => Entity?.Mieten.Select(e => new MieteEntryBase(e));
            public IEnumerable<MietminderungEntryBase>? Mietminderungen => Entity?.Mietminderungen.Select(e => new MietminderungEntryBase(e));
            public IEnumerable<PersonEntryBase>? Mieter => DbService?.ctx.MieterSet
                .Where(m => Entity != null && m.Vertrag.VertragId == Entity.VertragId)
                .ToList()
                .Select(e => new PersonEntryBase(DbService.ctx.FindPerson(e.PersonId), DbService));
            public IEnumerable<VertragVersionEntryBase>? Versionen => Entity?.Versionen.Select(e => new VertragVersionEntryBase(e));
            // TODO Garagen

            public VertragEntry() : base() { }
            public VertragEntry(Vertrag entity, IWalterDbService dbService) : base(entity, dbService)
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

        [HttpGet]
        public IActionResult Get() => new OkObjectResult(DbService.ctx.Vertraege
            .ToList()
            .Select(e => new VertragEntryBase(e, DbService.DbService))
            .ToList());
        [HttpPost]
        public IActionResult Post([FromBody] VertragEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, VertragEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
