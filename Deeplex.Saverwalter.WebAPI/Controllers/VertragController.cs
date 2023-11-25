using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
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
            public SelectionEntry? Wohnung { get; set; }
            public SelectionEntry Ansprechpartner { get; set; } = null!;
            public string? Notiz { get; set; }
            public string? MieterAuflistung { get; set; }
            public IEnumerable<SelectionEntry>? SelectedMieter { get; set; }
            public IEnumerable<MieteEntryBase>? Mieten { get; set; }
            public IEnumerable<VertragVersionEntryBase>? Versionen { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public VertragEntryBase() { }
            public VertragEntryBase(Vertrag entity)
            {
                Id = entity.VertragId;
                Beginn = entity.Beginn();
                Ende = entity.Ende;
                var anschrift = entity.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                Wohnung = new(
                    entity.Wohnung.WohnungId,
                    $"{anschrift} - {entity.Wohnung.Bezeichnung}",
                    entity.Wohnung.Besitzer?.Bezeichnung);
                if (entity.Ansprechpartner is Kontakt a)
                {
                    Ansprechpartner = new(a.KontaktId, a.Bezeichnung);
                }
                Notiz = entity.Notiz;

                var Mieter = entity.Mieter;

                MieterAuflistung = string.Join(", ", Mieter.Select(a => a.Bezeichnung));
                SelectedMieter = Mieter.Select(e => new SelectionEntry(e.KontaktId, e.Bezeichnung));

                Mieten = entity.Mieten.ToList().Select(e => new MieteEntryBase(e));
                Versionen = entity.Versionen.Select(e => new VertragVersionEntryBase(e));

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            private Vertrag Entity { get; } = null!;

            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen => Entity?.Wohnung.Umlagen
                .SelectMany(e => e.Betriebskostenrechnungen)
                .Where(e => e.BetreffendesJahr >= Entity.Beginn().Year && (Entity.Ende == null || Entity.Ende.Value.Year >= e.BetreffendesJahr))
                .Select(e => new BetriebskostenrechnungEntryBase(e));
            public IEnumerable<MietminderungEntryBase>? Mietminderungen => Entity?.Mietminderungen.ToList().Select(e => new MietminderungEntryBase(e));
            public IEnumerable<KontaktEntryBase>? Mieter => Entity?.Mieter
                .Select(e => new KontaktEntryBase(e));
            // TODO Garagen

            public VertragEntry() : base() { }
            public VertragEntry(Vertrag entity) : base(entity)
            {
                Entity = entity;
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
            .Select(e => new VertragEntryBase(e))
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
