using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

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

            public Permissions Permissions { get; set; } = new Permissions();

            public HKVOEntryBase() { }
            public HKVOEntryBase(HKVO entity, Permissions permissions)
            {
                Id = entity.HKVOId;

                HKVO_P7 = (int)(entity.HKVO_P7 * 100);
                HKVO_P8 = (int)(entity.HKVO_P8 * 100);
                HKVO_P9 = new((int)entity.HKVO_P9, entity.HKVO_P9.ToDescriptionString());
                Strompauschale = (int)(entity.Strompauschale * 100);
                Stromrechnung = new SelectionEntry(entity.Betriebsstrom.UmlageId, entity.Betriebsstrom.Typ.Bezeichnung);

                Permissions = permissions;
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

            public Permissions Permissions { get; set; } = new Permissions();

            protected UmlageEntryBase() { }
            public UmlageEntryBase(Umlage entity, Permissions permissions)
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

                Betriebskostenrechnungen = entity.Betriebskostenrechnungen.Select(e => new BetriebskostenrechnungEntryBase(e, permissions));

                if (entity.HKVO != null)
                {
                    HKVO = new HKVOEntryBase(entity.HKVO, permissions);
                }

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;

                Permissions = permissions;
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            private Umlage Entity { get; } = null!;

            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Wohnungen.Select(e => new WohnungEntryBase(e, Permissions));
            public IEnumerable<ZaehlerEntryBase>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e, Permissions));
            // TODO HKVO

            public UmlageEntry() : base() { }
            public UmlageEntry(Umlage entity, Permissions permissions) : base(entity, permissions)
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
        public Task<IActionResult> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<IActionResult> Post([FromBody] UmlageEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, UmlageEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
