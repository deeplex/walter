using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AnhangController;

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
            public Zaehlertyp Typ { get; set; }
            public AdresseEntry? Adresse { get; set; }
            public string? Notiz { get; set; }
            public string? Wohnung { get; set; }

            public ZaehlerEntryBase() { }
            public ZaehlerEntryBase(Zaehler entity)
            {
                Entity = entity;

                Id = Entity.ZaehlerId;
                Kennnummer = Entity.Kennnummer;
                Typ = Entity.Typ;
                Adresse = Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;
                Wohnung = Entity.Wohnung is Wohnung w ? w.Adresse.Anschrift + ", " + w.Bezeichnung : null;
                Notiz = Entity.Notiz;
            }
        }

        public class ZaehlerEntry : ZaehlerEntryBase
        {
            public ZaehlerEntryBase? AllgemeinZaehler { get; set; }

            public IEnumerable<ZaehlerEntryBase>? Einzelzaehler => Entity?.EinzelZaehler.ToList().Select(e => new ZaehlerEntryBase(e));
            public IEnumerable<ZaehlerstandEntryBase>? Staende => Entity?.Staende.ToList().Select(e => new ZaehlerstandEntryBase(e));
            public IEnumerable<AnhangEntryBase>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangEntryBase(e));

            public ZaehlerEntry() : base() { }
            public ZaehlerEntry(Zaehler entity) : base(entity)
            {
                AllgemeinZaehler = Entity?.Allgemeinzaehler is Zaehler z ? new ZaehlerEntryBase(z) : null;
            }
        }

        public class ZaehlerstandEntryBase
        {
            private Zaehlerstand? Entity { get; }
            public int Id { get; set; }
            public string Stand { get; set; }
            public string Datum { get; set; }

            public ZaehlerstandEntryBase(Zaehlerstand entity)
            {
                Entity = entity;

                Id = Entity.ZaehlerstandId;
                Stand = Entity.Stand.ToString();
                Datum = Entity.Datum.Datum();
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
            => new OkObjectResult(DbService.ctx.ZaehlerSet.ToList().Select(e => new ZaehlerEntryBase(e)).ToList());
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
