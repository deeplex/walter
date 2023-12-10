using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/zaehlerstaende")]
    public class ZaehlerstandController : ControllerBase
    {
        public class ZaehlerstandEntryBase
        {
            private Zaehlerstand? Entity { get; }
            public int Id { get; set; }
            public double Stand { get; set; }
            public DateOnly Datum { get; set; }
            public SelectionEntry Zaehler { get; set; } = null!;
            public string? Einheit { get; set; }
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public ZaehlerstandEntryBase() { }
            public ZaehlerstandEntryBase(Zaehlerstand entity, Permissions permissions)
            {
                Entity = entity;

                Id = Entity.ZaehlerstandId;
                Stand = Entity.Stand;
                Datum = Entity.Datum;
                Zaehler = new(Entity.Zaehler.ZaehlerId, Entity.Zaehler.Kennnummer);
                Einheit = Entity.Zaehler.Typ.ToUnitString();
                Notiz = Entity.Notiz;

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;

                Permissions = permissions;
            }
        }

        private readonly ILogger<ZaehlerstandController> _logger;
        private ZaehlerstandDbService DbService { get; }

        public ZaehlerstandController(ILogger<ZaehlerstandController> logger, ZaehlerstandDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpPost]
        public Task<IActionResult> Post([FromBody] ZaehlerstandEntryBase entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, ZaehlerstandEntryBase entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
