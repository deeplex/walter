using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.JuristischePersonController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/kontakte/nat/{id}")]
    public class NatuerlichePersonController : ControllerBase
    {
        public class NatuerlichePersonEntryBase : PersonEntry
        {
            protected new NatuerlichePerson? Entity { get; }
            public int Id { get; set; }

            public Anrede? Anrede { get; set; }
            public string? Vorname { get; set; }
            public string? Nachname { get; set; }

            protected NatuerlichePersonEntryBase() : base() { }
            public NatuerlichePersonEntryBase(NatuerlichePerson entity, IWalterDbService dbService) : base(entity, dbService)
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
            public IEnumerable<AnhangListEntry>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangListEntry(e));

            public NatuerlichePersonEntry() : base() { }
            public NatuerlichePersonEntry(NatuerlichePerson entity, IWalterDbService dbService) : base(entity, dbService)
            {
            }
        }

        private readonly ILogger<NatuerlichePersonController> _logger;
        private NatuerlichePersonDbService DbService { get; }

        public NatuerlichePersonController(ILogger<NatuerlichePersonController> logger, NatuerlichePersonDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(int id) => DbService.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post([FromBody] NatuerlichePersonEntry entry) => DbService.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(int id, NatuerlichePersonEntry entry) => DbService.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
