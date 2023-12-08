using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize(Policy = "RequireAdmin")]
    public class AccountController : ControllerBase
    {
        public class VerwalterEntry
        {
            private Verwalter? Entity { get; }

            public int Id { get; set; }
            public SelectionEntry Rolle { get; set; } = null!;
            public SelectionEntry Wohnung { get; set; } = null!;

            public VerwalterEntry() { }
            public VerwalterEntry(Verwalter entity)
            {
                Entity = entity;
                Id = Entity.VerwalterId;
                Rolle = new SelectionEntry((int)Entity.Rolle, Entity.Rolle.ToString());
                Wohnung = new SelectionEntry(Entity.Wohnung.WohnungId, Entity.Wohnung.Bezeichnung);
            }
        }

        public class AccountEntryBase
        {
            protected UserAccount? Entity { get; }

            public Guid Id { get; set; }
            public string Username { get; set; } = null!;
            public string Name { get; set; } = null!;
            public UserRole Role { get; set; }

            public IEnumerable<VerwalterEntry>? Verwalter { get; set; }

            public AccountEntryBase() { }
            public AccountEntryBase(UserAccount entity)
            {
                Entity = entity;
                Id = Entity.Id;

                Username = Entity.Username;
                Name = Entity.Name;
                Role = Entity.Role;

                Verwalter = Entity.Verwalter.Select(w => new VerwalterEntry(w));
            }
        }

        private readonly ILogger<AccountController> _logger;
        AccountDbService DbService { get; }

        public AccountController(ILogger<AccountController> logger, AccountDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<IActionResult> Get() => DbService.GetList();

        [HttpPost]
        public Task<IActionResult> Post([FromBody] AccountEntryBase entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(Guid id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(Guid id, AccountEntryBase entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(Guid id) => DbService.Delete(User!, id);
    }
}
