using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/zaehler")]
    public class ZaehlerController : FileControllerBase<ZaehlerEntry, Zaehler>
    {
        public class ZaehlerEntryBase
        {
            protected Zaehler? Entity { get; }

            public int Id { get; set; }
            public string Kennnummer { get; set; } = null!;
            public SelectionEntry Typ { get; set; } = null!;
            public SelectionEntry? Wohnung { get; set; }
            public ZaehlerstandEntryBase? LastZaehlerstand { get; set; }
            public DateOnly? Ende { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public ZaehlerEntryBase() { }
            public ZaehlerEntryBase(Zaehler entity, Permissions permissions)
            {
                Entity = entity;

                Id = entity.ZaehlerId;
                Kennnummer = entity.Kennnummer;
                Typ = new((int)entity.Typ, entity.Typ.ToString());
                Wohnung = entity.Wohnung is Wohnung w ? new(w.WohnungId, $"{w.Adresse?.Anschrift ?? "Unbekannte Anschrift"}, {w.Bezeichnung}") : null;
                var letzterStand = entity.Staende?.OrderBy(s => s.Datum).ToList().LastOrDefault();
                Ende = entity.Ende;
                if (letzterStand is Zaehlerstand stand)
                {
                    LastZaehlerstand = new ZaehlerstandEntryBase(letzterStand, permissions);
                }

                Permissions = permissions;
            }
        }

        public class ZaehlerEntry : ZaehlerEntryBase
        {
            public string? Notiz { get; set; }
            public IEnumerable<SelectionEntry>? SelectedUmlagen { get; set; }
            public AdresseEntryBase? Adresse { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<ZaehlerstandEntryBase> Staende { get; } = [];
            public IEnumerable<UmlageEntryBase>? Umlagen { get; } = [];

            public ZaehlerEntry() : base() { }
            public ZaehlerEntry(Zaehler entity, Permissions permissions) : base(entity, permissions)
            {
                Adresse = entity.Adresse is Adresse a ? new AdresseEntryBase(a, permissions) : null;
                SelectedUmlagen = entity.Umlagen.ToList()
                   .Select(e => new SelectionEntry(e.UmlageId, e.Typ.Bezeichnung + " - " + e.GetWohnungenBezeichnung()));

                Notiz = entity.Notiz;
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;

                Staende = entity.Staende.ToList().Select(e => new ZaehlerstandEntryBase(e, permissions));
                Umlagen = entity.Umlagen.ToList().Select(e => new UmlageEntryBase(e, permissions));
            }
        }

        private readonly ILogger<ZaehlerController> _logger;
        protected override ZaehlerDbService DbService { get; }

        public ZaehlerController(ILogger<ZaehlerController> logger, ZaehlerDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<ZaehlerEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<ZaehlerEntry>> Post([FromBody] ZaehlerEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<ZaehlerEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<ZaehlerEntry>> Put(int id, ZaehlerEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
