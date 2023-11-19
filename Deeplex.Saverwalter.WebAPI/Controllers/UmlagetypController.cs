using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/umlagetypen")]
    public class UmlagetypController : ControllerBase
    {
        public class UmlagetypEntryBase
        {
            protected Umlagetyp? Entity { get; }
            public int Id { get; set; }
            public string Bezeichnung { get; set; } = null!;
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public UmlagetypEntryBase() { }
            public UmlagetypEntryBase(Umlagetyp entity)
            {
                Entity = entity;

                Id = Entity.UmlagetypId;
                Bezeichnung = Entity.Bezeichnung;
                Notiz = Entity.Notiz;

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;
            }
        }

        public class UmlagetypEntry : UmlagetypEntryBase
        {
            public IEnumerable<UmlageEntryBase>? Umlagen => Entity?.Umlagen.ToList().Select(e => new UmlageEntryBase(e));

            public UmlagetypEntry() : base() { }
            public UmlagetypEntry(Umlagetyp entity) : base(entity) { }
        }

        private readonly ILogger<UmlagetypController> _logger;
        private UmlagetypDbService DbService { get; }

        public UmlagetypController(ILogger<UmlagetypController> logger, UmlagetypDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }


        [HttpGet]
        public IActionResult Get()
            => new OkObjectResult(DbService.Ctx.Umlagetypen.ToList().Select(e
                => new UmlagetypEntryBase(e)).ToList());
        [HttpPost]
        public IActionResult Post([FromBody] UmlagetypEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, UmlagetypEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
