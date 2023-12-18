using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragDbService : WalterDbServiceBase<VertragEntry, Vertrag>
    {
        public VertragDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<VertragEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await VertragPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new VertragEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Vertrag>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Vertraege.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<VertragEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new VertragEntry(entity, permissions);

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Vertraege.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<VertragEntry>> Post(ClaimsPrincipal user, VertragEntry entry)
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

                return await Add(entry);
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

        public override async Task<ActionResult<VertragEntry>> Put(ClaimsPrincipal user, int id, VertragEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                if (entry.Wohnung == null)
                {
                    throw new ArgumentException("entry has no Wohnung.");
                }
                entity.Wohnung = (await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id))!;

                await SetOptionalValues(entity, entry);
                Ctx.Vertraege.Update(entity);
                await Ctx.SaveChangesAsync();

                return new VertragEntry(entity, entry.Permissions);
            });
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
