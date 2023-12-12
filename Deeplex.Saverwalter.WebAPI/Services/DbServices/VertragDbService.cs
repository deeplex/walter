using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragDbService : ICRUDService<VertragEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public VertragDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await VertragPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return new OkObjectResult(await Task.WhenAll(list
                .Select(async e => new VertragEntryBase(e, await Utils.GetPermissions(user, e, Auth)))));
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Vertraege.FindAsync(id);
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
                var entry = new VertragEntry(entity, permissions);

                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Vertraege.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Vertraege.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, VertragEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung!.Id);
                var authRx = await Auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate]);
                if (!authRx.Succeeded)
                {
                    return new ForbidResult();
                }

                return new OkObjectResult(await Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<VertragEntry> Add(VertragEntry entry)
        {
            if (entry.Wohnung == null)
            {
                throw new ArgumentException("entry has no Wohnung");
            }
            var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id);
            var entity = new Vertrag() { Wohnung = wohnung! };

            await SetOptionalValues(entity, entry);
            Ctx.Vertraege.Add(entity);
            Ctx.SaveChanges();

            return new VertragEntry(entity, entry.Permissions);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, VertragEntry entry)
        {
            var entity = await Ctx.Vertraege.FindAsync(id);
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

        private async Task<VertragEntry> Update(VertragEntry entry, Vertrag entity)
        {
            if (entry.Wohnung == null)
            {
                throw new ArgumentException("entry has no Wohnung.");
            }
            entity.Wohnung = (await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id))!;

            await SetOptionalValues(entity, entry);
            Ctx.Vertraege.Update(entity);
            Ctx.SaveChanges();

            return new VertragEntry(entity, entry.Permissions);
        }

        private async Task SetOptionalValues(Vertrag entity, VertragEntry entry)
        {
            if (entity.VertragId != entry.Id)
            {
                throw new Exception();
            }

            // Create Version for initial create (if provided)
            if (entity.VertragId == 0)
            {
                var entryVersion = entry.Versionen?.FirstOrDefault();
                if (entryVersion != null)
                {
                    var entityVersion = new VertragVersion(entryVersion.Beginn, entryVersion.Grundmiete, entryVersion.Personenzahl)
                    {
                        Vertrag = entity,
                    };
                    entity.Versionen.Add(entityVersion);
                }
            }

            entity.Ende = entry.Ende;
            entity.Ansprechpartner = await Ctx.Kontakte.FindAsync(entry.Ansprechpartner.Id)!;
            entity.Notiz = entry.Notiz;

            if (entry.SelectedMieter is IEnumerable<SelectionEntry> l)
            {
                // Add missing mieter
                foreach (var selectedMieter in l)
                {
                    if (!entity.Mieter.Any(m => m.KontaktId == selectedMieter.Id))
                    {
                        entity.Mieter.Add((await Ctx.Kontakte.FindAsync(selectedMieter.Id))!);
                    }
                }
                // Remove mieter
                foreach (var m in entity.Mieter)
                {
                    if (!l.Any(e => m.KontaktId == e.Id))
                    {
                        entity.Mieter.Remove(m);
                    }
                }
            }
        }
    }
}
