using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await WohnungPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return new OkObjectResult(await Task.WhenAll(list
                .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth)))));
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

                entry.Haus = await Task.WhenAll(entity.Adresse?
                    .Wohnungen.Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))) ?? []);

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
                return new OkObjectResult(await Add(entry));
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
                return new OkObjectResult(await Update(entry, entity));
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
