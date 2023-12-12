using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

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

            public AdresseEntryBase? Adresse { get; set; }
            public string? Bezeichnung { get; } = null!;
            public string? Email { get; set; }
            public string? Fax { get; set; }
            public string? Telefon { get; set; }
            public string? Mobil { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            protected KontaktEntryBase() : base() { }
            public KontaktEntryBase(Kontakt entity, Permissions permissions)
            {
                Entity = entity;
                Id = entity.KontaktId;

                if (entity.Adresse is Adresse a)
                {
                    Adresse = new AdresseEntryBase(a, permissions);
                }

                Bezeichnung = entity.Bezeichnung;
                Email = entity.Email;
                Fax = entity.Fax;
                Telefon = entity.Telefon;
                Mobil = entity.Mobil;

                Permissions = permissions;
            }
        }

        public sealed class KontaktEntry : KontaktEntryBase
        {
            public string? Vorname { get; set; }
            public string Name { get; set; } = null!;
            public SelectionEntry? Anrede { get; set; }
            public SelectionEntry Rechtsform { get; set; } = null!;
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<SelectionEntry>? SelectedJuristischePersonen
                => Entity?.JuristischePersonen.Select(e => new SelectionEntry(e.KontaktId, e.Name));

            public IEnumerable<SelectionEntry>? SelectedMitglieder
                => Entity?.Mitglieder.Select(e => new SelectionEntry(e.KontaktId, e.Name));

            public IEnumerable<KontaktEntryBase> JuristischePersonen { get; set; } = [];
            public IEnumerable<KontaktEntryBase> Mitglieder { get; set; } = [];
            public IEnumerable<VertragEntryBase> Vertraege { get; set; } = [];
            public IEnumerable<WohnungEntryBase> Wohnungen { get; set; } = [];

            public KontaktEntry() : base() { }
            public KontaktEntry(Kontakt entity, Permissions permissions) : base(entity, permissions)
            {
                Anrede = new SelectionEntry((int)entity.Anrede, entity.Anrede.ToString());
                Rechtsform = new SelectionEntry((int)entity.Rechtsform, entity.Rechtsform.ToDescriptionString());
                Vorname = entity.Vorname;
                Name = entity.Name;
                Notiz = entity.Notiz;

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<KontaktController> _logger;
        private KontaktDbService DbService { get; }

        public KontaktController(ILogger<KontaktController> logger, KontaktDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<IActionResult> Get() => DbService.GetList();
        [HttpPost]
        public Task<IActionResult> Post([FromBody] KontaktEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, KontaktEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
