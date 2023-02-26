using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.BetriebskostenrechnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.ErhaltungsaufwendungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.UmlageListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.VertragListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.ZaehlerListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/wohnungen/{id}")]
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
            public AdresseEntry? Adresse { get; set; }
            public string? Anschrift { get; set; }

            public Guid BesitzerId { get; set; }

            public WohnungEntryBase() { }
            public WohnungEntryBase(Wohnung entity)
            {
                Entity = entity;

                Id = Entity.WohnungId;
                Bezeichnung = Entity.Bezeichnung;
                Wohnflaeche =   Entity.Wohnflaeche;
                Nutzflaeche = Entity.Nutzflaeche;
                Einheiten = Entity.Nutzeinheit;
                Notiz = Entity.Notiz;
                Adresse = Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;
                Anschrift = Entity.Adresse.Anschrift + ", " + Entity.Bezeichnung;
                BesitzerId = Entity.BesitzerId;
            }
        }

        public class WohnungEntry : WohnungEntryBase
        {
            private IWalterDbService? DbService { get; }

            public IEnumerable<WohnungListEntry>? Haus => Entity?.Adresse.Wohnungen.Select(e => new WohnungListEntry(e, DbService!));
            public IEnumerable<AnhangListEntry>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<ZaehlerListEntry>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerListEntry(e));
            public IEnumerable<VertragListEntry>? Vertraege => Entity?.Vertraege.Select(e => new VertragListEntry(e, DbService!));
            public IEnumerable<ErhaltungsaufwendungListEntry>? Erhaltungsaufwendungen => Entity?.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungListEntry(e, DbService!));
            public IEnumerable<UmlageListEntry>? Umlagen => Entity?.Umlagen.Select(e => new UmlageListEntry(e));
            public IEnumerable<BetriebskostenrechungListEntry>? Betriebskostenrechnungen => Entity?.Umlagen.SelectMany(e => e.Betriebskostenrechnungen.Select(f => new BetriebskostenrechungListEntry(f)));

            public WohnungEntry() : base() { }
            public WohnungEntry(Wohnung entity, IWalterDbService dbService) : base(entity)
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

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(int id) => DbService.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post([FromBody] WohnungEntry entry) => DbService.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(int id, WohnungEntry entry) => DbService.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}