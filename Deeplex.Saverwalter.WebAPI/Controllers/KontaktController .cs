using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte")]
    public class KontaktController : ControllerBase
    {
        public class KontaktEntryBase
        {
            protected Kontakt? Entity { get; }

            public int Id { get; set; }
            public SelectionEntry? Anrede { get; set; }
            public SelectionEntry Rechtsform { get; set; } = null!;
            public string? Vorname { get; set; }
            public string Bezeichnung { get; } = null!;
            public string Name { get; set; } = null!;
            public string? Email { get; set; }
            public string? Fax { get; set; }
            public string? Notiz { get; set; }
            public string? Telefon { get; set; }
            public string? Mobil { get; set; }
            public AdresseEntryBase? Adresse { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            protected KontaktEntryBase() : base() { }
            public KontaktEntryBase(Kontakt entity)
            {
                Entity = entity;
                Id = entity.KontaktId;
                Anrede = new SelectionEntry((int)Entity.Anrede, Entity.Anrede.ToString());
                Rechtsform = new SelectionEntry((int)Entity.Rechtsform, Entity.Rechtsform.ToDescriptionString());
                Vorname = Entity.Vorname;
                Name = Entity.Name;
                Bezeichnung = Entity.Bezeichnung;
                Email = Entity.Email;
                Fax = Entity.Fax;
                Notiz = Entity.Notiz;
                Telefon = Entity.Telefon;
                Mobil = Entity.Mobil;

                if (Entity.Adresse is Adresse a)
                {
                    Adresse = new AdresseEntryBase(a);
                }

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;
            }
        }

        public sealed class KontaktEntry : KontaktEntryBase
        {
            public IEnumerable<SelectionEntry>? SelectedJuristischePersonen
                => Entity?.JuristischePersonen.Select(e => new SelectionEntry(e.KontaktId, e.Name));

            public IEnumerable<SelectionEntry>? SelectedMitglieder
                => Entity?.Mitglieder.Select(e => new SelectionEntry(e.KontaktId, e.Name));

            public IEnumerable<KontaktEntryBase>? JuristischePersonen
                => Entity?.JuristischePersonen.Select(e => new KontaktEntryBase(e));

            public IEnumerable<KontaktEntryBase>? Mitglieder
                => Entity?.Mitglieder.Select(e => new KontaktEntryBase(e));

            public IEnumerable<VertragEntryBase>? Vertraege
                => Entity?.Mietvertraege
                .Concat(Entity.Wohnungen.SelectMany(w => w.Vertraege))
                .Distinct()
                .Select(e => new VertragEntryBase(e));

            public IEnumerable<WohnungEntryBase>? Wohnungen
                => Entity?.Mietvertraege
                .Concat(Entity.Wohnungen.SelectMany(w => w.Vertraege))
                .Select(e => e.Wohnung)
                .Distinct()
                .Select(e => new WohnungEntryBase(e));

            public KontaktEntry() : base() { }
            public KontaktEntry(Kontakt entity) : base(entity) { }
        }

        private readonly ILogger<KontaktController> _logger;
        private KontaktDbService DbService { get; }

        public KontaktController(ILogger<KontaktController> logger, KontaktDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public IActionResult Get() => new OkObjectResult(DbService.Ctx.Kontakte.ToList().Select(e => new KontaktEntryBase(e)).ToList());
        [HttpPost]
        public IActionResult Post([FromBody] KontaktEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, KontaktEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
