using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AccountController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AccountDbService : ICRUDServiceGuid<AccountEntryBase>
    {
        public SaverwalterContext Ctx { get; }
        private readonly UserService _userService;

        public AccountDbService(SaverwalterContext ctx, UserService userService)
        {
            Ctx = ctx;
            _userService = userService;
        }

        public async Task<IActionResult> GetList()
        {
            var list = await Ctx.UserAccounts.ToListAsync();
            return new OkObjectResult(list.Select(e => new AccountEntryBase(e)).ToList());
        }


        public async Task<IActionResult> Get(ClaimsPrincipal user, Guid id)
        {
            var entity = await Ctx.UserAccounts.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new AccountEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, Guid id)
        {
            var entity = await Ctx.UserAccounts.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.UserAccounts.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, AccountEntryBase entry)
        {
            if (entry.Id != Guid.Empty)
            {
                return new BadRequestResult();
            }

            try
            {
                return new OkObjectResult(await Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<AccountEntryBase> Add(AccountEntryBase entry)
        {

            var entity = new UserAccount()
            {
                Username = entry.Username,
                Name = entry.Name,
                Role = (UserRole)entry.Role.Id
            };
            SetOptionalValues(entity, entry);
            Ctx.UserAccounts.Add(entity);
            await Ctx.SaveChangesAsync();

            return new AccountEntryBase(entity);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, Guid id, AccountEntryBase entry)
        {
            var entity = await Ctx.UserAccounts.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                return new OkObjectResult(await Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<string> ResetCredentialsFor(Guid userId)
        {
            var user = await Ctx.UserAccounts
                .Where(user => user.Id == userId)
                .Include(user => user.UserResetCredential)
                .SingleOrDefaultAsync() ?? throw new ArgumentException("User not found");

            return await _userService.CreateResetToken(user);
        }

        private async Task<AccountEntryBase> Update(AccountEntryBase entry, UserAccount entity)
        {
            entity.Username = entry.Username;
            entity.Name = entry.Name;
            entity.Role = (UserRole)entry.Role.Id;

            SetOptionalValues(entity, entry);
            Ctx.UserAccounts.Update(entity);
            await Ctx.SaveChangesAsync();

            return new AccountEntryBase(entity);
        }

        private void SetOptionalValues(UserAccount entity, AccountEntryBase entry)
        {
            if (entity.Id != entry.Id)
            {
                throw new Exception();
            }

            if (entry.Verwalter is IEnumerable<VerwalterEntry> verwalter)
            {
                // Update existing Verwalter
                entity.Verwalter.ForEach(v =>
                {
                    var verwalterEntry = verwalter.SingleOrDefault(w => w.Wohnung.Id == v.Wohnung.WohnungId);
                    if (verwalterEntry != null)
                    {
                        v.Rolle = (VerwalterRolle)verwalterEntry.Rolle.Id;
                        v.Notiz = verwalterEntry.Notiz;
                    }
                });
                // Add missing Verwalter
                entity.Verwalter.AddRange(verwalter
                    .Where(v => !entity.Verwalter.Exists(vv => v.Wohnung.Id == vv.Wohnung.WohnungId))
                    .Select(w => new Verwalter((VerwalterRolle)w.Rolle.Id)
                    {
                        Notiz = w.Notiz,
                        VerwalterId = w.Id,
                        UserAccount = entity,
                        Wohnung = Ctx.Wohnungen.Find(w.Wohnung.Id)!
                    }));
                // Remove removed Verwalter
                entity.Verwalter.RemoveAll(v => !verwalter.ToList().Exists(verwalter => verwalter.Wohnung.Id == v.Wohnung.WohnungId));
            }
        }
    }
}
