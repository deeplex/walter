using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

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
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public MietminderungEntryBase() { }
            public MietminderungEntryBase(Mietminderung entity, Permissions permissions)
            {
                Id = entity.MietminderungId;
                Beginn = entity.Beginn;
                Ende = entity.Ende;
                Minderung = entity.Minderung;
                Notiz = entity.Notiz;
                var anschrift = entity.Vertrag.Wohnung.Adresse is Adresse a ? a.Anschrift : "Unbekannte Anschrift";
                var vertragTitle = $"{anschrift} - {entity.Vertrag.Wohnung.Bezeichnung} - {Beginn.Datum()}";
                Vertrag = new(entity.Vertrag.VertragId, vertragTitle);

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;

                Permissions = permissions;
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
        public Task<IActionResult> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<IActionResult> Post([FromBody] MietminderungEntryBase entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, [FromBody] MietminderungEntryBase entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
