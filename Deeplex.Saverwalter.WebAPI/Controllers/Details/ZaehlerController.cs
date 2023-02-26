using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/zaehler/{id}")]
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

            public ZaehlerEntryBase() { }
            public ZaehlerEntryBase(Zaehler entity)
            {
                Entity = entity;

                Id = Entity.ZaehlerId;
                Kennnummer = Entity.Kennnummer;
                Typ = Entity.Typ;
                Adresse = Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;
                Notiz = Entity.Notiz;
            }
        }

        public class ZaehlerEntry : ZaehlerEntryBase
        {
            public IWalterDbService? DbService { get; }

            public ZaehlerEntryBase? AllgemeinZaehler => Entity?.Allgemeinzaehler is Zaehler z ? new ZaehlerEntryBase(z) : null;

            public IEnumerable<ZaehlerEntryBase>? Einzelzaehler => Entity?.EinzelZaehler.Select(e => new ZaehlerEntryBase(e));
            public IEnumerable<ZaehlerstandListEntry>? Staende => Entity?.Staende.Select(e => new ZaehlerstandListEntry(e));
            public IEnumerable<AnhangListEntry>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangListEntry(e));

            public ZaehlerEntry() : base() { }
            public ZaehlerEntry(Zaehler entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        public class ZaehlerstandListEntry
        {
            private Zaehlerstand Entity { get; }
            public int Id => Entity.ZaehlerstandId;
            public string Stand => Entity.Stand.ToString();
            public string Datum => Entity.Datum.Datum();
            //public ZaehlerEntryBase Zaehler => new ZaehlerEntryBase(Entity.Zaehler);

            public ZaehlerstandListEntry(Zaehlerstand entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<ZaehlerController> _logger;
        private ZaehlerDbService DbService { get; }

        public ZaehlerController(ILogger<ZaehlerController> logger, ZaehlerDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(int id) => DbService.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post([FromBody] ZaehlerEntry entry) => DbService.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(int id, ZaehlerEntry entry) => DbService.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
