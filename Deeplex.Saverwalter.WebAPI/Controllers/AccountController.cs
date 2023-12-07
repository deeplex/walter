using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize(Policy = "RequireAdmin")]
    public class AccountController : ControllerBase
    {
        public class UserAccountEntryBase
        {
            protected UserAccount? Entity { get; }

            public Guid Id { get; set; }
            public string Username { get; set; } = null!;
            public string Name { get; set; } = null!;
            public UserRole Role { get; set; }

            public UserAccountEntryBase() { }
            public UserAccountEntryBase(UserAccount entity)
            {
                Entity = entity;
                Id = Entity.Id;

                Username = Entity.Username;
                Name = Entity.Name;
                Role = Entity.Role;
            }
        }

        public class UserAccountEntry : UserAccountEntryBase
        {
            public IEnumerable<KontaktEntryBase>? Kontakte
                => Entity?.Kontakte.Select(e => new KontaktEntryBase(e));


            public UserAccountEntry() : base() { }
            public UserAccountEntry(UserAccount entity) : base(entity)
            {
            }
        }

        private readonly ILogger<AccountController> _logger;
        UserAccountDbService DbService { get; }

        public AccountController(ILogger<AccountController> logger, UserAccountDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<IActionResult> Get() => DbService.GetList();

        [HttpPost]
        public Task<IActionResult> Post([FromBody] UserAccountEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(Guid id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(Guid id, UserAccountEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(Guid id) => DbService.Delete(User!, id);
    }
}
