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
            public string? Einheit { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public ZaehlerstandEntryBase() { }
            public ZaehlerstandEntryBase(Zaehlerstand entity, Permissions permissions)
            {
                Entity = entity;

                Id = entity.ZaehlerstandId;
                Stand = entity.Stand;
                Datum = entity.Datum;
                Einheit = entity.Zaehler.Typ.ToUnitString();

                Permissions = permissions;
            }
        }

        public class ZaehlerstandEntry : ZaehlerstandEntryBase
        {
            public SelectionEntry Zaehler { get; set; } = null!;
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public ZaehlerstandEntry() : base() { }
            public ZaehlerstandEntry(Zaehlerstand entity, Permissions permissions) : base(entity, permissions)
            {
                Zaehler = new(entity.Zaehler.ZaehlerId, entity.Zaehler.Kennnummer);
                Notiz = entity.Notiz;

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
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
        public Task<IActionResult> Post([FromBody] ZaehlerstandEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, ZaehlerstandEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
