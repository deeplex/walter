using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/umlagen")]
    public class UmlageController : ControllerBase
    {
        public class UmlageEntryBase
        {
            public int Id { get; set; }
            public string? Notiz { get; set; }
            public string? Beschreibung { get; set; }
            public SelectionEntry? Schluessel { get; set; }
            public SelectionEntry? Typ { get; set; }
            public IEnumerable<SelectionEntry>? SelectedWohnungen { get; set; }
            public string? WohnungenBezeichnung { get; set; }
            public IEnumerable<SelectionEntry>? SelectedZaehler { get; set; }
            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            protected UmlageEntryBase() { }
            public UmlageEntryBase(Umlage entity)
            {
                Id = entity.UmlageId;

                Notiz = entity.Notiz;
                Beschreibung = entity.Beschreibung;
                Schluessel = new SelectionEntry((int)entity.Schluessel, entity.Schluessel.ToDescriptionString());
                Typ = new SelectionEntry((int)entity.Typ, entity.Typ.ToDescriptionString());
                WohnungenBezeichnung = entity.GetWohnungenBezeichnung() ?? "";

                SelectedWohnungen = entity.Wohnungen.Select(e =>
                    new SelectionEntry(
                        e.WohnungId,
                        $"{e.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {e.Bezeichnung}"));

                SelectedZaehler = entity.Zaehler.Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer));

                Betriebskostenrechnungen = entity.Betriebskostenrechnungen.Select(e => new BetriebskostenrechnungEntryBase(e));

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            private SaverwalterContext? Ctx { get; } = null!;
            private Umlage Entity { get; } = null!;

            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Wohnungen.Select(e => new WohnungEntryBase(e, Ctx!));
            public IEnumerable<ZaehlerEntryBase>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e));
            // TODO HKVO

            public UmlageEntry() : base() { }
            public UmlageEntry(Umlage entity, SaverwalterContext ctx) : base(entity)
            {
                Entity = entity;
                Ctx = ctx;
            }
        }

        private readonly ILogger<UmlageController> _logger;
        private UmlageDbService DbService { get; }

        public UmlageController(ILogger<UmlageController> logger, UmlageDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public IActionResult Get() => new OkObjectResult(DbService.Ctx.Umlagen.ToList().Select(e => new UmlageEntryBase(e)).ToList());
        [HttpPost]
        public IActionResult Post([FromBody] UmlageEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, UmlageEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
