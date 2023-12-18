using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public string? Notiz { get; set; } = null!;

            public VerwalterEntry() { }
            public VerwalterEntry(Verwalter entity)
            {
                Entity = entity;
                Id = Entity.VerwalterId;
                Rolle = new SelectionEntry((int)Entity.Rolle, Entity.Rolle.ToString());
                Wohnung = new SelectionEntry(Entity.Wohnung.WohnungId, Entity.Wohnung.Bezeichnung);
                Notiz = Entity.Notiz;

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;
            }
        }

        public class AccountEntryBase
        {
            protected UserAccount? Entity { get; }

            public Guid Id { get; set; }
            public string Username { get; set; } = null!;
            public string Name { get; set; } = null!;
            public SelectionEntry Role { get; set; } = null!;

            public string? ResetToken { get; set; } = null!;
            public DateTime? ResetTokenExpires { get; set; } = null!;

            public IEnumerable<VerwalterEntry>? Verwalter { get; set; }

            public AccountEntryBase() { }
            public AccountEntryBase(UserAccount entity)
            {
                Entity = entity;
                Id = Entity.Id;

                Username = Entity.Username;
                Name = Entity.Name;
                Role = new((int)Entity.Role, Entity.Role.ToString());

                if (Entity.UserResetCredential is UserResetCredential cred)
                {
                    ResetToken = WebEncoders.Base64UrlEncode(cred.Token);
                    ResetTokenExpires = cred.ExpiresAt;
                }

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
        public async Task<ActionResult<List<AccountEntryBase>>> Get() => await DbService.GetList();

        [HttpPost]
        public async Task<ActionResult<AccountEntryBase>> Post([FromBody] AccountEntryBase entry) => await DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountEntryBase>> Get(Guid id) => await DbService.Get(User!, id);
        [HttpPut("{id}")]
        public async Task<ActionResult<AccountEntryBase>> Put(Guid id, AccountEntryBase entry) => await DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public async Task<ActionResult<AccountEntryBase>> Delete(Guid id) => await DbService.Delete(User!, id);

        [HttpPost("{userId}/reset-credentials")]
        [ProducesErrorResponseType(typeof(void))] // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> ResetCredentials(Guid userId)
        {
            try
            {
                return await DbService.ResetCredentialsFor(userId);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
