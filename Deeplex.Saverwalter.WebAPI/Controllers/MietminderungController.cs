using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mietminderungen")]
    public class MietminderungController : ControllerBase
    {
        public class MietminderungEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public DateOnly? Ende { get; set; }
            public double Minderung { get; set; }
            public string? Notiz { get; set; }
            public SelectionEntry Vertrag { get; set; } = null!;

            public MietminderungEntryBase() { }
            public MietminderungEntryBase(Mietminderung entity)
            {
                Id = entity.MietminderungId;
                Beginn = entity.Beginn;
                Ende = entity.Ende;
                Minderung = entity.Minderung;
                Notiz = entity.Notiz;
                var anschrift = entity.Vertrag.Wohnung.Adresse is Adresse a ? a.Anschrift : "Unbekannte Anschrift";
                var vertragTitle = $"{anschrift} - {entity.Vertrag.Wohnung.Bezeichnung} - {Beginn.Datum()}";
                Vertrag = new(entity.Vertrag.VertragId, vertragTitle);
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
