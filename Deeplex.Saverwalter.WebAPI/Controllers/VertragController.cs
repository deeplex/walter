using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;
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
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public DateOnly? Ende { get; set; }
            public SelectionEntry Wohnung { get; set; } = null!;
            public SelectionEntry? Ansprechpartner { get; set; }
            public string? Notiz { get; set; }
            public string? MieterAuflistung { get; set; }
            public IEnumerable<SelectionEntry>? SelectedMieter { get; set; }

            public VertragEntryBase() { }
            public VertragEntryBase(Vertrag entity, SaverwalterContext ctx)
            {
                Id = entity.VertragId;
                Beginn = entity.Beginn();
                Ende = entity.Ende;
                var anschrift = entity.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                Wohnung = new(
                    entity.Wohnung.WohnungId,
                    $"{anschrift} - {entity.Wohnung.Bezeichnung}",
                    entity.Wohnung.BesitzerId.ToString());
                Ansprechpartner = entity.AnsprechpartnerId is Guid id && id != Guid.Empty
                    ? new(id, ctx.FindPerson(id).Bezeichnung)
                    : null;
                Notiz = entity.Notiz;

                var Mieter = ctx.MieterSet
                    .Where(m => m.Vertrag.VertragId == entity.VertragId)
                    .ToList();
                MieterAuflistung = string.Join(", ", Mieter
                    .Select(a => ctx.FindPerson(a.PersonId).Bezeichnung));
                SelectedMieter = Mieter.Select(e
                    => new SelectionEntry(e.PersonId, ctx.FindPerson(e.PersonId).Bezeichnung));
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            private SaverwalterContext Ctx { get; } = null!;
            private Vertrag Entity { get; } = null!;

            public IEnumerable<MieteEntryBase>? Mieten => Entity?.Mieten.Select(e => new MieteEntryBase(e));
            public IEnumerable<MietminderungEntryBase>? Mietminderungen => Entity?.Mietminderungen.Select(e => new MietminderungEntryBase(e));
            public IEnumerable<PersonEntryBase>? Mieter => Ctx?.MieterSet
                .Where(m => Entity != null && m.Vertrag.VertragId == Entity.VertragId)
                .ToList()
                .Select(e => new PersonEntryBase(Ctx.FindPerson(e.PersonId)));
            public IEnumerable<VertragVersionEntryBase>? Versionen { get; set; }
            // TODO Garagen

            public VertragEntry() : base() { }
            public VertragEntry(Vertrag entity, SaverwalterContext ctx) : base(entity, ctx)
            {
                Entity = entity;
                Ctx = ctx;
                Versionen = Entity?.Versionen.Select(e => new VertragVersionEntryBase(e));
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
        public IActionResult Get() => new OkObjectResult(DbService.Ctx.Vertraege
            .ToList()
            .Select(e => new VertragEntryBase(e, DbService.Ctx))
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
