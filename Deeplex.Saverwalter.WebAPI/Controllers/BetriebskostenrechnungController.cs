using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/betriebskostenrechnungen")]
    public class BetriebskostenrechnungController : ControllerBase
    {
        public class BetriebskostenrechnungEntryBase
        {
            public int Id { get; set; }
            public double Betrag { get; set; }
            public int BetreffendesJahr { get; set; }
            public DateOnly Datum { get; set; }
            public SelectionEntry? Typ { get; set; }
            public SelectionEntry? Umlage { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public BetriebskostenrechnungEntryBase() { }
            public BetriebskostenrechnungEntryBase(Betriebskostenrechnung entity, Permissions permissions)
            {
                Id = entity.BetriebskostenrechnungId;

                Betrag = entity.Betrag;
                BetreffendesJahr = entity.BetreffendesJahr;
                Datum = entity.Datum;
                Typ = new SelectionEntry(entity.Umlage.Typ.UmlagetypId, entity.Umlage.Typ.Bezeichnung);

                Umlage = new SelectionEntry(
                    entity.Umlage.UmlageId,
                    entity.Umlage.GetWohnungenBezeichnung(),
                    entity.Umlage.Typ.UmlagetypId.ToString());

                Permissions = permissions;
            }
        }

        public class BetriebskostenrechnungEntry : BetriebskostenrechnungEntryBase
        {
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            private Betriebskostenrechnung? Entity { get; }

            public IEnumerable<BetriebskostenrechnungEntryBase> Betriebskostenrechnungen { get; set; } = [];
            public IEnumerable<WohnungEntryBase>? Wohnungen { get; set; } = [];

            public BetriebskostenrechnungEntry() : base() { }
            public BetriebskostenrechnungEntry(Betriebskostenrechnung entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;

                Entity = entity;
            }
        }

        private readonly ILogger<BetriebskostenrechnungController> _logger;
        BetriebskostenrechnungDbService DbService { get; }

        public BetriebskostenrechnungController(ILogger<BetriebskostenrechnungController> logger, BetriebskostenrechnungDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<BetriebskostenrechnungEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<BetriebskostenrechnungEntry>> Post([FromBody] BetriebskostenrechnungEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<BetriebskostenrechnungEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<BetriebskostenrechnungEntry>> Put(int id, BetriebskostenrechnungEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
