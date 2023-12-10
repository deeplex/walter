using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MietminderungDbService : ICRUDService<MietminderungEntryBase>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public MietminderungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        private Task<List<Mietminderung>> GetListForUser(ClaimsPrincipal user)
        {
            Guid.TryParse(user.FindAll(ClaimTypes.NameIdentifier).SingleOrDefault()?.Value, out Guid guid);
            return Ctx.Mietminderungen
                .Where(e => e.Vertrag.Wohnung.Verwalter.Any(v => v.UserAccount.Id == guid))
                .ToListAsync();
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await (user.IsInRole("Admin")
                ? Ctx.Mietminderungen.ToListAsync()
                : GetListForUser(user));

            return new OkObjectResult(list.Select(e => new MietminderungEntryBase(e, new(true))).ToList());
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Mietminderungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var permissions = await Utils.GetPermissions(user, entity, Auth);
            if (!permissions.Read)
            {
                return new ForbidResult();
            }

            try
            {
                var entry = new MietminderungEntryBase(entity, permissions);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Mietminderungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Mietminderungen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, MietminderungEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var vertrag = (await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id));
                var authRx = await Auth.AuthorizeAsync(user, vertrag, [Operations.SubCreate]);
                if (!authRx.Succeeded)
                {
                    return new ForbidResult();
                }

                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<MietminderungEntryBase> Add(MietminderungEntryBase entry)
        {
            var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag.Id);
            var entity = new Mietminderung(entry.Beginn, entry.Minderung)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            Ctx.Mietminderungen.Add(entity);
            Ctx.SaveChanges();

            return new MietminderungEntryBase(entity, entry.Permissions);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, MietminderungEntryBase entry)
        {
            var entity = await Ctx.Mietminderungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Update]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
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

        private MietminderungEntryBase Update(MietminderungEntryBase entry, Mietminderung entity)
        {
            entity.Ende = entry.Ende;
            entity.Beginn = entry.Beginn;
            entity.Minderung = entry.Minderung;

            SetOptionalValues(entity, entry);
            Ctx.Mietminderungen.Update(entity);
            Ctx.SaveChanges();

            return new MietminderungEntryBase(entity, entry.Permissions);
        }

        private void SetOptionalValues(Mietminderung entity, MietminderungEntryBase entry)
        {
            if (entity.MietminderungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
