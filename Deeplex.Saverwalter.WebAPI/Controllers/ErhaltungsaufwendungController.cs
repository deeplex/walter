using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/erhaltungsaufwendungen")]
    public class ErhaltungsaufwendungController : ControllerBase
    {
        public class ErhaltungsaufwendungEntryBase
        {
            protected Erhaltungsaufwendung? Entity { get; }

            public int Id { get; set; }
            public double Betrag { get; set; }
            public DateTime Datum { get; set; }
            public string? Notiz { get; set; }
            public string? Bezeichnung { get; set; }
            public PersonEntryBase? Aussteller { get; set; }
            public WohnungEntryBase? Wohnung { get; set; }

            public ErhaltungsaufwendungEntryBase(Erhaltungsaufwendung entity, IWalterDbService dbService)
            {
                Entity = entity;

                Id = Entity.ErhaltungsaufwendungId;
                Betrag = Entity.Betrag;
                Datum = Entity.Datum;
                Notiz = Entity.Notiz;
                Bezeichnung = Entity.Bezeichnung;
                Aussteller = new (dbService.ctx.FindPerson(Entity.AusstellerId));
                Wohnung = new(Entity.Wohnung, dbService);
            }
        }

        public class ErhaltungsaufwendungEntry : ErhaltungsaufwendungEntryBase
        {
            public IEnumerable<AnhangEntryBase>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangEntryBase(e));

            public ErhaltungsaufwendungEntry(Erhaltungsaufwendung entity, IWalterDbService dbService) : base(entity, dbService)
            {
            }
        }

        private readonly ILogger<ErhaltungsaufwendungController> _logger;
        private ErhaltungsaufwendungDbService DbService { get; }

        public ErhaltungsaufwendungController(ILogger<ErhaltungsaufwendungController> logger, ErhaltungsaufwendungDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get() => new OkObjectResult(DbService.ctx.Erhaltungsaufwendungen
            .ToList()
            .Select(e => new ErhaltungsaufwendungEntryBase(e, DbService.Ref))
            .ToList());
        [HttpPost]
        public IActionResult Post([FromBody] ErhaltungsaufwendungEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] ErhaltungsaufwendungEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
