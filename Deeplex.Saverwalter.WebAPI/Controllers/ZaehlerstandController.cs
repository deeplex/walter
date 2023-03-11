using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

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
            public DateTime? Datum { get; set; }
            public SelectionEntry? Zaehler { get; set; }
            public string? Einheit { get; set; }
            public string? Notiz { get; set; }

            public ZaehlerstandEntryBase() { }
            public ZaehlerstandEntryBase(Zaehlerstand entity)
            {
                Entity = entity;

                Id = Entity.ZaehlerstandId;
                Stand = Entity.Stand;
                Datum = Entity.Datum;
                Zaehler = new(Entity.Zaehler.ZaehlerId, Entity.Zaehler.Kennnummer);
                Einheit = Entity.Zaehler.Typ.ToUnitString();
                Notiz = Entity.Notiz;
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
        public IActionResult Post([FromBody] ZaehlerstandEntryBase entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) =>  DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, ZaehlerstandEntryBase entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
