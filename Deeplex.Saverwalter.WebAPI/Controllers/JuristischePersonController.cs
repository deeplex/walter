using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte/jur")]
    public class JuristischePersonController : ControllerBase
    {
        public class JuristischePersonEntryBase : PersonEntry
        {
            protected new JuristischePerson? Entity { get; }

            public JuristischePersonEntryBase() : base() { }
            public JuristischePersonEntryBase(JuristischePerson entity) : base(entity)
            {
                Entity = entity;
            }
        }

        public sealed class JuristischePersonEntry : JuristischePersonEntryBase
        {
            public IEnumerable<AnhangEntryBase>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangEntryBase(e));
            public IEnumerable<PersonEntryBase>? Mitglieder => Entity?.Mitglieder.Select(e => new PersonEntryBase(e));

            public JuristischePersonEntry() : base() { }
            public JuristischePersonEntry(JuristischePerson entity) : base(entity) { }
        }

        private readonly ILogger<JuristischePersonController> _logger;
        private JuristischePersonDbService DbService { get; }

        public JuristischePersonController(ILogger<JuristischePersonController> logger, JuristischePersonDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] JuristischePersonEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] JuristischePersonEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
