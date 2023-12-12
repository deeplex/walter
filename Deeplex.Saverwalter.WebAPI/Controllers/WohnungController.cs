using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/wohnungen")]
    public class WohnungController : ControllerBase
    {
        public class WohnungEntryBase
        {
            protected Wohnung? Entity { get; }

            public int Id { get; set; }
            public string Bezeichnung { get; set; } = null!;
            public AdresseEntryBase? Adresse { get; set; }
            public SelectionEntry? Besitzer { get; set; }
            public string? Bewohner { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public WohnungEntryBase() { }
            public WohnungEntryBase(Wohnung entity, Permissions permissions)
            {
                Entity = entity;

                Id = entity.WohnungId;
                Bezeichnung = entity.Bezeichnung;
                Adresse = entity.Adresse is Adresse a ? new AdresseEntryBase(a, permissions) : null;
                if (entity.Besitzer is Kontakt k)
                {
                    Besitzer = new(k.KontaktId, k.Bezeichnung);
                }

                var v = entity.Vertraege.FirstOrDefault(e => e.Ende == null || e.Ende < DateOnly.FromDateTime(DateTime.Now));
                Bewohner = v != null ?
                    string.Join(", ", v.Mieter.Select(m => m.Bezeichnung)) :
                    null;

                Permissions = permissions;
            }
        }

        public class WohnungEntry : WohnungEntryBase
        {
            public double Wohnflaeche { get; set; }
            public double Nutzflaeche { get; set; }
            public int Einheiten { get; set; }
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<WohnungEntryBase>? Haus => Entity?.Adresse?.Wohnungen.Select(e => new WohnungEntryBase(e, new()));
            public IEnumerable<ZaehlerEntryBase>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e, new()));
            public IEnumerable<VertragEntryBase>? Vertraege => Entity?.Vertraege.Select(e => new VertragEntryBase(e, new()));
            public IEnumerable<ErhaltungsaufwendungEntryBase>? Erhaltungsaufwendungen
                => Entity?.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungEntryBase(e, new()));
            public IEnumerable<UmlageEntryBase>? Umlagen => Entity?.Umlagen.Select(e => new UmlageEntryBase(e, new()));
            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen
                => Entity?.Umlagen.SelectMany(e => e.Betriebskostenrechnungen.Select(f => new BetriebskostenrechnungEntryBase(f, new())));

            public WohnungEntry() : base() { }
            public WohnungEntry(Wohnung entity, Permissions permissions) : base(entity, permissions)
            {
                Wohnflaeche = entity.Wohnflaeche;
                Nutzflaeche = entity.Nutzflaeche;
                Einheiten = entity.Nutzeinheit;
                Notiz = entity.Notiz;

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<WohnungController> _logger;
        private WohnungDbService DbService { get; }

        public WohnungController(ILogger<WohnungController> logger, WohnungDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public Task<IActionResult> Get() => DbService.GetList(User!);

        [HttpPost]
        [Authorize(Policy = "RequireOwner")]
        public Task<IActionResult> Post([FromBody] WohnungEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, WohnungEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
