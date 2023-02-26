using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.BetriebskostenrechnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.ErhaltungsaufwendungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.UmlageListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.VertragListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.ZaehlerListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/anhaenge/{id}")]
    public class AnhangController : ControllerBase
    {
        public class AnhangEntryBase
        {
            protected Anhang? Entity { get; }

            public Guid Id { get; set; }
            public string? FileName { get; set; }
            public DateTime CreationTime { get; set; }
            //public string Notiz => Entity.Notiz;

            public AnhangEntryBase() { }
            public AnhangEntryBase(Anhang entity)
            {
                Entity = entity;
             
                Id = Entity.AnhangId;
                FileName = Entity.FileName;
                CreationTime = Entity.CreationTime;
            }
        }

        public class AnhangEntry : AnhangEntryBase
        {
            private IWalterDbService? DbService { get; }

            public IEnumerable<BetriebskostenrechungListEntry>? Betriebskostenrechnungen => Entity?.Betriebskostenrechnungen.Select(e => new BetriebskostenrechungListEntry(e));
            public IEnumerable<ErhaltungsaufwendungListEntry>? Erhaltungsaufwendungen => Entity?.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungListEntry(e, DbService!));
            public IEnumerable<KontaktListEntry>? NatuerlichePersonen => Entity?.NatuerlichePersonen.Select(e => new KontaktListEntry(e, DbService!));
            public IEnumerable<KontaktListEntry>? JuristischePersonen => Entity?.JuristischePersonen.Select(e => new KontaktListEntry(e, DbService!));
            public IEnumerable<UmlageListEntry>? Umlagen => Entity?.Umlagen.Select(e => new UmlageListEntry(e));
            public IEnumerable<VertragListEntry>? Vertraege => Entity?.Vertraege.Select(e => new VertragListEntry(e, DbService!));
            public IEnumerable<WohnungListEntry>? Wohnungen => Entity?.Wohnungen.Select(e => new WohnungListEntry(e, DbService!)).ToList();
            public IEnumerable<ZaehlerListEntry>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerListEntry(e));

            public AnhangEntry() : base() { }
            public AnhangEntry(Anhang entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<AnhangController> _logger;
        private AnhangDbService DbService { get; }

        public AnhangController(ILogger<AnhangController> logger, AnhangDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(Guid id) => DbService.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post([FromBody] AnhangEntry entry) => DbService.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(Guid id, [FromBody] AnhangEntry entry) => DbService.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(Guid id) => DbService.Delete(id);
    }
}
