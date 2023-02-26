using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte/nat")]
    public class NatuerlichePersonController : ControllerBase
    {
        public class NatuerlichePersonEntryBase : PersonEntry
        {
            protected new NatuerlichePerson? Entity { get; }

            public Anrede? Anrede { get; set; }
            public string? Vorname { get; set; }
            public string? Nachname { get; set; }

            protected NatuerlichePersonEntryBase() : base() { }
            public NatuerlichePersonEntryBase(NatuerlichePerson entity) : base(entity)
            {
                Entity = entity;
                Id = entity.NatuerlichePersonId;

                Anrede = Entity.Anrede;
                Vorname = Entity.Vorname;
                Nachname = Entity.Nachname;
            }
        }

        public sealed class NatuerlichePersonEntry : NatuerlichePersonEntryBase
        {
            public IEnumerable<AnhangEntryBase>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangEntryBase(e));

            public NatuerlichePersonEntry() : base() { }
            public NatuerlichePersonEntry(NatuerlichePerson entity) : base(entity) { }
        }

        private readonly ILogger<NatuerlichePersonController> _logger;
        private NatuerlichePersonDbService DbService { get; }

        public NatuerlichePersonController(ILogger<NatuerlichePersonController> logger, NatuerlichePersonDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpPost]
        public IActionResult Post([FromBody] NatuerlichePersonEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, NatuerlichePersonEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
