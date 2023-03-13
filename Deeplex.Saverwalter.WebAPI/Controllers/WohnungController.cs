using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
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
            public string? Bezeichnung { get; set; }
            public double Wohnflaeche { get; set; }
            public double Nutzflaeche { get; set; }
            public int Einheiten { get; set; }
            public string? Notiz { get; set; }
            public AdresseEntryBase? Adresse { get; set; }
            public string? Bewohner { get; set; }
            public SelectionEntry? Besitzer { get; set; }

            public WohnungEntryBase() { }
            public WohnungEntryBase(Wohnung entity, IWalterDbService dbService)
            {
                Entity = entity;

                Id = Entity.WohnungId;
                Bezeichnung = Entity.Bezeichnung;
                Wohnflaeche = Entity.Wohnflaeche;
                Nutzflaeche = Entity.Nutzflaeche;
                Einheiten = Entity.Nutzeinheit;
                Notiz = Entity.Notiz;
                Adresse = Entity.Adresse is Adresse a ? new AdresseEntryBase(a) : null;
                Besitzer = Entity.BesitzerId is Guid id && id != Guid.Empty ? new(id, dbService.ctx.FindPerson(id).Bezeichnung) : null;
             
                var v = Entity.Vertraege.FirstOrDefault(e => e.Ende == null || e.Ende < DateTime.Now);
                Bewohner = v != null ?
                    string.Join(", ", dbService.ctx.MieterSet
                        .Where(m => m.Vertrag.VertragId == v.VertragId)
                        .ToList()
                        .Select(a => dbService.ctx.FindPerson(a.PersonId).Bezeichnung)) :
                    null;
            }
        }

        public class WohnungEntry : WohnungEntryBase
        {
            private IWalterDbService? DbService { get; }

            public IEnumerable<WohnungEntryBase>? Haus => Entity?.Adresse?.Wohnungen.Select(e => new WohnungEntryBase(e, DbService!));
            public IEnumerable<ZaehlerEntryBase>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e));
            public IEnumerable<VertragEntryBase>? Vertraege => Entity?.Vertraege.Select(e => new VertragEntryBase(e, DbService!));
            public IEnumerable<ErhaltungsaufwendungEntryBase>? Erhaltungsaufwendungen
                => Entity?.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungEntryBase(e, DbService!));
            public IEnumerable<UmlageEntryBase>? Umlagen => Entity?.Umlagen.Select(e => new UmlageEntryBase(e));
            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen
                => Entity?.Umlagen.SelectMany(e => e.Betriebskostenrechnungen.Select(f => new BetriebskostenrechnungEntryBase(f)));

            public WohnungEntry() : base() { }
            public WohnungEntry(Wohnung entity, IWalterDbService dbService) : base(entity, dbService)
            {
                DbService = dbService;
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
        public IActionResult Get() => new OkObjectResult(DbService.ctx.Wohnungen
            .ToList()
            .Select(e => new WohnungEntryBase(e, DbService.DbService))
            .ToList());
        [HttpPost]
        public IActionResult Post([FromBody] WohnungEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, WohnungEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}