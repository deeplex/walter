using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/anhaenge")]
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

            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen => Entity?.Betriebskostenrechnungen.Select(e => new BetriebskostenrechnungEntryBase(e));
            public IEnumerable<ErhaltungsaufwendungEntryBase>? Erhaltungsaufwendungen => Entity?.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungEntryBase(e, DbService!));
            public IEnumerable<PersonEntryBase>? NatuerlichePersonen => Entity?.NatuerlichePersonen.Select(e => new PersonEntryBase(e));
            public IEnumerable<PersonEntryBase>? JuristischePersonen => Entity?.JuristischePersonen.Select(e => new PersonEntryBase(e));
            public IEnumerable<UmlageEntryBase>? Umlagen => Entity?.Umlagen.Select(e => new UmlageEntryBase(e));
            public IEnumerable<VertragEntryBase>? Vertraege => Entity?.Vertraege.Select(e => new VertragEntryBase(e, DbService!));
            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Wohnungen.Select(e => new WohnungEntryBase(e, DbService!)).ToList();
            public IEnumerable<ZaehlerEntryBase>? Zaehler => Entity?.Zaehler.Select(e => new ZaehlerEntryBase(e));

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

        [HttpGet]
        public IActionResult Get()
            => new OkObjectResult(DbService.ctx.Anhaenge.ToList().Select(e => new AnhangEntryBase(e)).ToList());
        [HttpPost]
        public IActionResult Post([FromBody] AnhangEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(Guid id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] AnhangEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id) => DbService.Delete(id);
    }
}
