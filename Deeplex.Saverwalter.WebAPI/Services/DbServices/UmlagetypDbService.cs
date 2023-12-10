using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlagetypDbService : ICRUDService<UmlagetypEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public UmlagetypDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        private Task<List<Umlagetyp>> GetListForUser(ClaimsPrincipal user)
        {
            Guid.TryParse(user.FindAll(ClaimTypes.NameIdentifier).SingleOrDefault()?.Value, out Guid guid);
            return Ctx.Umlagetypen
                .Where(e => e.Umlagen.SelectMany(u => u.Wohnungen).Any(w => w.Verwalter.Any(v => v.UserAccount.Id == guid)))
                .ToListAsync();
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await (user.IsInRole("Admin")
                ? Ctx.Umlagetypen.ToListAsync()
                : GetListForUser(user));

            return new OkObjectResult(list.Select(e => new UmlagetypEntryBase(e, new(true))).ToList());
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Umlagetypen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var wohnungen = entity.Umlagen.SelectMany(entity => entity.Wohnungen);
            var permissions = await Utils.GetPermissions(user, wohnungen, Auth);
            if (!permissions.Read)
            {
                return new ForbidResult();
            }

            try
            {
                var entry = new UmlagetypEntry(entity, permissions);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Umlagetypen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Umlagetypen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, UmlagetypEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var authRx = await Auth.AuthorizeAsync(
                user,
                entry.Umlagen!.Select(e => Ctx.Umlagen.Where(u => u.UmlageId == e.Id)),
                [Operations.SubCreate]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
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

        private UmlagetypEntry Add(UmlagetypEntry entry)
        {
            var entity = new Umlagetyp(entry.Bezeichnung);
            SetOptionalValues(entity, entry);
            Ctx.Umlagetypen.Add(entity);
            Ctx.SaveChanges();

            return new UmlagetypEntry(entity, entry.Permissions);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, UmlagetypEntry entry)
        {
            var entity = await Ctx.Umlagetypen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.SubCreate]);
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

        private UmlagetypEntry Update(UmlagetypEntry entry, Umlagetyp entity)
        {
            entity.Bezeichnung = entry.Bezeichnung;

            SetOptionalValues(entity, entry);
            Ctx.Umlagetypen.Update(entity);
            Ctx.SaveChanges();

            return new UmlagetypEntry(entity, entry.Permissions);
        }

        private void SetOptionalValues(Umlagetyp entity, UmlagetypEntry entry)
        {
            entity.Notiz = entry.Notiz;
        }
    }
}
