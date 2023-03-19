using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/vertragversionen")]
    public class VertragVersionController : ControllerBase
    {
        public class VertragVersionEntryBase
        {
            protected VertragVersion? Entity { get; }

            public int Id { get; set; }
            public int Personenzahl { get; set; }
            public string? Notiz { get; set; }
            public DateTime? Beginn { get; set; }
            public double Grundmiete { get; set; }
            public SelectionEntry? Vertrag { get; set; }

            public VertragVersionEntryBase() { }
            public VertragVersionEntryBase(VertragVersion entity)
            {
                Entity = entity;
                Id = entity.VertragVersionId;
                Personenzahl = entity.Personenzahl;
                Notiz = entity.Notiz;
                Beginn = entity.Beginn;
                Grundmiete = entity.Grundmiete;

                Vertrag = new(Entity.Vertrag.VertragId, "Name not implemented");
            }
        }

        private readonly ILogger<VertragVersionController> _logger;
        private VertragVersionDbService DbService { get; }

        public VertragVersionController(ILogger<VertragVersionController> logger, VertragVersionDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] VertragVersionEntryBase entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, VertragVersionEntryBase entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
