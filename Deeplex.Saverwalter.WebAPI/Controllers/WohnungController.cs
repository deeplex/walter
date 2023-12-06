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
            public double Wohnflaeche { get; set; }
            public double Nutzflaeche { get; set; }
            public int Einheiten { get; set; }
            public string? Notiz { get; set; }
            public AdresseEntryBase? Adresse { get; set; }
            public string? Bewohner { get; set; }
            public SelectionEntry? Besitzer { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public WohnungEntryBase() { }
            public WohnungEntryBase(Wohnung entity)
            {
                Entity = entity;

                Id = Entity.WohnungId;
                Bezeichnung = Entity.Bezeichnung;
                Wohnflaeche = Entity.Wohnflaeche;
                Nutzflaeche = Entity.Nutzflaeche;
                Einheiten = Entity.Nutzeinheit;
                Notiz = Entity.Notiz;
                Adresse = Entity.Adresse is Adresse a ? new AdresseEntryBase(a) : null;
                if (Entity.Besitzer is Kontakt k)
                {
                    Besitzer = new(k.KontaktId, k.Bezeichnung);
                }

                var v = Entity.Vertraege.FirstOrDefault(e => e.Ende == null || e.Ende < DateOnly.FromDateTime(DateTime.Now));
                Bewohner = v != null ?
                    string.Join(", ", v.Mieter.Select(m => m.Bezeichnung)) :
                    null;

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;
            }
        }

        public class WohnungEntry : WohnungEntryBase
        {
            public IEnumerable<WohnungEntryBase>? Haus => Entity?.Adresse?.Wohnungen.Select(e => new WohnungEntryBase(e));
            public IEnumerable<ZaehlerEntryBase>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e));
            public IEnumerable<VertragEntryBase>? Vertraege => Entity?.Vertraege.Select(e => new VertragEntryBase(e));
            public IEnumerable<ErhaltungsaufwendungEntryBase>? Erhaltungsaufwendungen
                => Entity?.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungEntryBase(e));
            public IEnumerable<UmlageEntryBase>? Umlagen => Entity?.Umlagen.Select(e => new UmlageEntryBase(e));
            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen
                => Entity?.Umlagen.SelectMany(e => e.Betriebskostenrechnungen.Select(f => new BetriebskostenrechnungEntryBase(f)));

            public WohnungEntry() : base() { }
            public WohnungEntry(Wohnung entity) : base(entity)
            {
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
