using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/zaehler")]
    public class ZaehlerController : ControllerBase
    {
        public class ZaehlerEntryBase
        {
            protected Zaehler? Entity { get; }

            public int Id { get; set; }
            public string? Kennnummer { get; set; }
            public AdresseEntry? Adresse { get; set; }
            public string? Notiz { get; set; }
            public SelectionEntry? Typ { get; set; }
            public SelectionEntry? Wohnung { get; set; }
            public SelectionEntry? AllgemeinZaehler { get; set; }

            public ZaehlerEntryBase() { }
            public ZaehlerEntryBase(Zaehler entity, IWalterDbService dbService)
            {
                Entity = entity;

                Id = Entity.ZaehlerId;
                Kennnummer = Entity.Kennnummer;
                Typ = new((int)Entity.Typ, Entity.Typ.ToString());
                Adresse = Entity.Adresse is Adresse a ? new AdresseEntry(a, dbService) : null;
                Wohnung = Entity.Wohnung is Wohnung w ? new (w.WohnungId, w.Adresse.Anschrift + ", " + w.Bezeichnung) : null;
                Notiz = Entity.Notiz;
                AllgemeinZaehler = Entity?.Allgemeinzaehler is Zaehler z ? new(z.ZaehlerId, z.Kennnummer) : null;
            }
        }

        public class ZaehlerEntry : ZaehlerEntryBase
        {
            private IWalterDbService? DbService { get; }
            public IEnumerable<ZaehlerEntryBase>? Einzelzaehler => Entity?.EinzelZaehler.ToList().Select(e => new ZaehlerEntryBase(e, DbService));
            public IEnumerable<ZaehlerstandEntryBase>? Staende => Entity?.Staende.ToList().Select(e => new ZaehlerstandEntryBase(e));
            public IEnumerable<AnhangEntryBase>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangEntryBase(e));

            public ZaehlerEntry() : base() { }
            public ZaehlerEntry(Zaehler entity, IWalterDbService dbService) : base(entity, dbService)
            {
                DbService = dbService;
            }
        }

        public class ZaehlerstandEntryBase
        {
            private Zaehlerstand? Entity { get; }
            public int Id { get; set; }
            public double Stand { get; set; }
            public DateTime Datum { get; set; }
            public string Einheit { get; set; }

            public ZaehlerstandEntryBase(Zaehlerstand entity)
            {
                Entity = entity;

                Id = Entity.ZaehlerstandId;
                Stand = Entity.Stand;
                Datum = Entity.Datum;
                Einheit = Entity.Zaehler.Typ.ToUnitString();
            }
        }

        private readonly ILogger<ZaehlerController> _logger;
        private ZaehlerDbService DbService { get; }

        public ZaehlerController(ILogger<ZaehlerController> logger, ZaehlerDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public IActionResult Get()
            => new OkObjectResult(DbService.ctx.ZaehlerSet.ToList().Select(e => new ZaehlerEntryBase(e, DbService.DbService)).ToList());
        [HttpPost]
        public IActionResult Post([FromBody] ZaehlerEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) =>  DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, ZaehlerEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
