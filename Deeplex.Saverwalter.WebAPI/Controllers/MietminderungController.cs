using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mietminderungen")]
    public class MietminderungController : ControllerBase
    {
        public class MietminderungEntryBase
        {
            private Mietminderung? Entity { get; }

            public int Id { get; set; } 
            public DateTime? Beginn { get; set; }
            public DateTime? Ende { get; set; }
            public double? Minderung { get; set; }
            public string? Notiz {get; set;}
            public SelectionEntry? Vertrag { get; set; }

            public MietminderungEntryBase() { }
            public MietminderungEntryBase(Mietminderung entity)
            {
                Entity = entity;

                Id = entity.MietminderungId;
                Beginn = entity.Beginn;
                Ende = entity.Ende;
                Minderung = entity.Minderung;
                Notiz = entity.Notiz;
                var v = entity.Vertrag;
                var a = v.Wohnung.Adresse.Anschrift;
                Vertrag = new(v.VertragId, a + " - " + v.Wohnung.Bezeichnung + " - " + Beginn.Datum());
            }
        }

        private readonly ILogger<MietminderungController> _logger;
        private MietminderungDbService DbService { get; }

        public MietminderungController(ILogger<MietminderungController> logger, MietminderungDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get() =>
            new OkObjectResult(DbService.ctx.Mietminderungen.ToList().Select(e => new MietminderungEntryBase(e)).ToList());
        [HttpPost]
        public IActionResult Post([FromBody] MietminderungEntryBase entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] MietminderungEntryBase entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
