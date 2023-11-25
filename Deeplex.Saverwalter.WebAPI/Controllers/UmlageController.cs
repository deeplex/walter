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
        public class HKVOEntryBase
        {
            public int Id { get; set; }

            public int HKVO_P7 { get; set; }
            public int HKVO_P8 { get; set; }
            public SelectionEntry HKVO_P9 { get; set; } = null!;
            public int Strompauschale { get; set; }
            public SelectionEntry Stromrechnung { get; set; } = null!;

            public HKVOEntryBase() { }
            public HKVOEntryBase(HKVO entity)
            {
                Id = entity.HKVOId;

                HKVO_P7 = (int)(entity.HKVO_P7 * 100);
                HKVO_P8 = (int)(entity.HKVO_P8 * 100);
                HKVO_P9 = new((int)entity.HKVO_P9, entity.HKVO_P9.ToDescriptionString());
                Strompauschale = (int)(entity.Strompauschale * 100);
                Stromrechnung = new SelectionEntry(entity.Betriebsstrom.UmlageId, entity.Betriebsstrom.Typ.Bezeichnung);
            }
        }

        public class UmlageEntryBase
        {
            public int Id { get; set; }
            public string? Notiz { get; set; }
            public string? Beschreibung { get; set; }
            public SelectionEntry? Schluessel { get; set; }
            public SelectionEntry? Typ { get; set; }
            public HKVOEntryBase? HKVO { get; set; }
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
                Typ = new SelectionEntry(entity.Typ.UmlagetypId, entity.Typ.Bezeichnung);
                WohnungenBezeichnung = entity.GetWohnungenBezeichnung() ?? "";

                SelectedWohnungen = entity.Wohnungen.Select(e =>
                    new SelectionEntry(
                        e.WohnungId,
                        $"{e.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {e.Bezeichnung}"));

                SelectedZaehler = entity.Zaehler.Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer));

                Betriebskostenrechnungen = entity.Betriebskostenrechnungen.Select(e => new BetriebskostenrechnungEntryBase(e));

                if (entity.HKVO != null)
                {
                    HKVO = new HKVOEntryBase(entity.HKVO);
                }

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            private Umlage Entity { get; } = null!;

            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Wohnungen.Select(e => new WohnungEntryBase(e));
            public IEnumerable<ZaehlerEntryBase>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e));
            // TODO HKVO

            public UmlageEntry() : base() { }
            public UmlageEntry(Umlage entity) : base(entity)
            {
                Entity = entity;
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
