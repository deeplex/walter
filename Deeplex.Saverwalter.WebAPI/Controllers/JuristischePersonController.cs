using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte/jur")]
    public class JuristischePersonController : ControllerBase
    {
        public class JuristischePersonEntry : PersonEntry
        {
            protected new JuristischePerson? Entity { get; }

            public IEnumerable<PersonEntryBase>? Mitglieder => Entity?.Mitglieder.Select(e => new PersonEntryBase(e));
            public IEnumerable<SelectionEntry>? SelectedMitglieder { get; set; }

            public JuristischePersonEntry() : base() { }
            public JuristischePersonEntry(JuristischePerson entity, WalterDbService.WalterDb dbService) : base(entity, dbService)
            {
                Entity = entity;
                SelectedMitglieder = Entity!.Mitglieder.Select(e => new SelectionEntry(e.PersonId, dbService.ctx.FindPerson(e.PersonId).Bezeichnung));
            }
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
