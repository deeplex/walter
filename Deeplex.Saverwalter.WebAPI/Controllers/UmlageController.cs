using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/umlagen")]
    public class UmlageController : FileControllerBase<UmlageEntry, Umlage>
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
            public SelectionEntry? Typ { get; set; }
            public string? WohnungenBezeichnung { get; set; }

            // For Tabelle
            public IEnumerable<SelectionEntry>? SelectedWohnungen { get; set; }
            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            protected UmlageEntryBase() { }
            public UmlageEntryBase(Umlage entity, Permissions permissions)
            {
                Id = entity.UmlageId;

                Typ = new SelectionEntry(entity.Typ.UmlagetypId, entity.Typ.Bezeichnung);
                WohnungenBezeichnung = entity.GetWohnungenBezeichnung() ?? "";

                Betriebskostenrechnungen = entity.Betriebskostenrechnungen.Select(e => new BetriebskostenrechnungEntryBase(e, permissions));
                SelectedWohnungen = entity.Wohnungen.Select(e =>
                    new SelectionEntry(
                        e.WohnungId,
                        $"{e.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {e.Bezeichnung}"));

                Permissions = permissions;
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            private Umlage Entity { get; } = null!;

            public string? Notiz { get; set; }
            public string? Beschreibung { get; set; }
            public SelectionEntry? Schluessel { get; set; }
            public HKVOEntryBase? HKVO { get; set; }
            public IEnumerable<SelectionEntry>? SelectedZaehler { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<WohnungEntryBase> Wohnungen { get; set; } = [];
            public IEnumerable<ZaehlerEntryBase> Zaehler { get; set; } = [];

            public UmlageEntry() : base() { }
            public UmlageEntry(Umlage entity, Permissions permissions) : base(entity, permissions)
            {
                Entity = entity;

                Notiz = entity.Notiz;
                Beschreibung = entity.Beschreibung;
                Schluessel = new SelectionEntry((int)entity.Schluessel, entity.Schluessel.ToDescriptionString());

                SelectedZaehler = entity.Zaehler.Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer));

                if (entity.HKVO != null)
                {
                    HKVO = new HKVOEntryBase(entity.HKVO, permissions);
                }

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<UmlageController> _logger;
        protected override UmlageDbService DbService { get; }

        public UmlageController(ILogger<UmlageController> logger, UmlageDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            _logger = logger;
            DbService = dbService;
        }


        [HttpGet]
        public Task<ActionResult<IEnumerable<UmlageEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<UmlageEntry>> Post([FromBody] UmlageEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<UmlageEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<UmlageEntry>> Put(int id, UmlageEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
