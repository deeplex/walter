using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class WohnungDbService : WalterDbServiceBase<WohnungEntry, Wohnung>
    {
        public WohnungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<WohnungEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await WohnungPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Wohnung>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Wohnungen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<WohnungEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new WohnungEntry(entity, permissions);
                entry.Haus = await Task.WhenAll(entity.Adresse?
                    .Wohnungen.Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))) ?? []);

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Wohnungen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<WohnungEntry>> Post(ClaimsPrincipal user, WohnungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var entity = new Wohnung(entry.Bezeichnung, entry.Wohnflaeche, entry.Nutzflaeche, entry.Einheiten)
                {
                    Besitzer = await Ctx.Kontakte.FindAsync(entry.Besitzer!.Id)!
                };

                SetOptionalValues(entity, entry);
                Ctx.Wohnungen.Add(entity);
                Ctx.SaveChanges();

                return entry;
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public override async Task<ActionResult<WohnungEntry>> Put(ClaimsPrincipal user, int id, WohnungEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Bezeichnung = entry.Bezeichnung;
                entity.Wohnflaeche = entry.Wohnflaeche;
                entity.Nutzflaeche = entry.Nutzflaeche;
                entity.Nutzeinheit = entry.Einheiten;
                entity.Besitzer = await Ctx.Kontakte.FindAsync(entry.Besitzer!.Id)!;

                SetOptionalValues(entity, entry);
                Ctx.Wohnungen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new WohnungEntry(entity, entry.Permissions);
            });
        }

        private void SetOptionalValues(Wohnung entity, WohnungEntry entry)
        {
            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
            entity.Notiz = entry.Notiz;
        }
    }
}
