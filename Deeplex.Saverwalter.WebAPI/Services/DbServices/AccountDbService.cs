using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AccountController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AccountDbService : ICRUDServiceGuid<AccountEntryBase>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public AccountDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
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
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private AccountEntryBase Add(AccountEntryBase entry)
        {

            var entity = new UserAccount()
            {
                Username = entry.Username,
                Name = entry.Name,
                Role = entry.Role
            };
            SetOptionalValues(entity, entry);
            Ctx.UserAccounts.Add(entity);
            Ctx.SaveChanges();

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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private AccountEntryBase Update(AccountEntryBase entry, UserAccount entity)
        {
            entity.Username = entry.Username;
            entity.Name = entry.Name;
            entity.Role = entry.Role;

            SetOptionalValues(entity, entry);
            Ctx.UserAccounts.Update(entity);
            Ctx.SaveChanges();

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
                // Add missing Verwalter
                entity.Verwalter.AddRange(verwalter
                    .Where(v => !entity.Verwalter.Exists(vv => v.Id == vv.VerwalterId))
                    .SelectMany(w => Ctx.VerwalterSet.Where(v => v.VerwalterId == w.Id)));
                // Remove old Verwalter
                entity.Verwalter.RemoveAll(v => !verwalter.ToList().Exists(verwalter => verwalter.Id == v.VerwalterId));
            }
        }
    }
}
