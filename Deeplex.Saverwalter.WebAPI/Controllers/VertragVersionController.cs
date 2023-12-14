using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

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
            public DateOnly Beginn { get; set; }
            public double Grundmiete { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public VertragVersionEntryBase() { }
            public VertragVersionEntryBase(VertragVersion entity, Permissions permissions)
            {
                Entity = entity;
                Id = entity.VertragVersionId;
                Personenzahl = entity.Personenzahl;
                Beginn = entity.Beginn;
                Grundmiete = entity.Grundmiete;

                Permissions = permissions;
            }
        }

        public class VertragVersionEntry : VertragVersionEntryBase
        {
            public string? Notiz { get; set; }
            public SelectionEntry? Vertrag { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public VertragVersionEntry() : base() { }
            public VertragVersionEntry(VertragVersion entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;
                Vertrag = new(entity.Vertrag.VertragId, "Name not implemented");

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
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
        public Task<ActionResult<VertragVersionEntry>> Post([FromBody] VertragVersionEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<VertragVersionEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<VertragVersionEntry>> Put(int id, VertragVersionEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
