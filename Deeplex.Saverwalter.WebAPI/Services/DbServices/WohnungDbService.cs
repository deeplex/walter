using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class WohnungDbService : ICRUDService<WohnungEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public WohnungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        private Task<List<Wohnung>> GetListForUser(ClaimsPrincipal user)
        {
            Guid.TryParse(user.FindAll(ClaimTypes.NameIdentifier).SingleOrDefault()?.Value, out Guid guid);
            return Ctx.Wohnungen
                .Where(e => e.Verwalter.Any(v => v.UserAccount.Id == guid))
                        .ToListAsync();
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await (user.IsInRole("Admin")
                ? Ctx.Wohnungen.ToListAsync()
                : GetListForUser(user));


            return new OkObjectResult(list.Select(e => new WohnungEntryBase(e, new(true))).ToList());
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Wohnungen.FindAsync(id);
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
                var entry = new WohnungEntry(entity, permissions);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Wohnungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Wohnungen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, WohnungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var newEntry = await Add(entry);
                return new OkObjectResult(newEntry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<WohnungEntry> Add(WohnungEntry entry)
        {
            var entity = new Wohnung(entry.Bezeichnung, entry.Wohnflaeche, entry.Nutzflaeche, entry.Einheiten)
            {
                Besitzer = await Ctx.Kontakte.FindAsync(entry.Besitzer!.Id)!
            };

            SetOptionalValues(entity, entry);
            Ctx.Wohnungen.Add(entity);
            Ctx.SaveChanges();

            return new WohnungEntry(entity, entry.Permissions);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, WohnungEntry entry)
        {
            var entity = await Ctx.Wohnungen.FindAsync(id);
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

        private async Task<WohnungEntry> Update(WohnungEntry entry, Wohnung entity)
        {
            entity.Bezeichnung = entry.Bezeichnung;
            entity.Wohnflaeche = entry.Wohnflaeche;
            entity.Nutzflaeche = entry.Nutzflaeche;
            entity.Nutzeinheit = entry.Einheiten;
            entity.Besitzer = await Ctx.Kontakte.FindAsync(entry.Besitzer!.Id)!;

            SetOptionalValues(entity, entry);
            Ctx.Wohnungen.Update(entity);
            Ctx.SaveChanges();

            return new WohnungEntry(entity, entry.Permissions);
        }

        private void SetOptionalValues(Wohnung entity, WohnungEntry entry)
        {
            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
            entity.Notiz = entry.Notiz;
        }
    }
}
