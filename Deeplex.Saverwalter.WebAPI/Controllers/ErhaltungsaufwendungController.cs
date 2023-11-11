using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

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
            public DateOnly Datum { get; set; }
            public string? Notiz { get; set; }
            public string Bezeichnung { get; set; } = null!;
            public SelectionEntry Aussteller { get; set; } = null!;
            public SelectionEntry Wohnung { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public ErhaltungsaufwendungEntryBase() { }
            public ErhaltungsaufwendungEntryBase(Erhaltungsaufwendung entity, SaverwalterContext ctx)
            {
                Entity = entity;

                Id = Entity.ErhaltungsaufwendungId;
                Betrag = Entity.Betrag;
                Datum = Entity.Datum;
                Notiz = Entity.Notiz;
                Bezeichnung = Entity.Bezeichnung;
                Aussteller = new(Entity.AusstellerId, ctx.FindPerson(Entity.AusstellerId).Bezeichnung);
                var anschrift = Entity.Wohnung.Adresse is Adresse a ? a.Anschrift : "Unbekannte Anschrift";
                Wohnung = new(Entity.Wohnung.WohnungId, $"{anschrift} - {Entity.Wohnung.Bezeichnung}");

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;
            }
        }

        public class ErhaltungsaufwendungEntry : ErhaltungsaufwendungEntryBase
        {
            public ErhaltungsaufwendungEntry() { }
            public ErhaltungsaufwendungEntry(Erhaltungsaufwendung entity, SaverwalterContext ctx) : base(entity, ctx)
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
        public IActionResult Get() => new OkObjectResult(DbService.Ctx.Erhaltungsaufwendungen
            .ToList()
            .Select(e => new ErhaltungsaufwendungEntryBase(e, DbService.Ctx))
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
