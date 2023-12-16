using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlagetypDbService : WalterDbServiceBase<UmlagetypEntry, Umlagetyp>
    {
        public UmlagetypDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<UmlagetypEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await UmlagetypPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new UmlagetypEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Umlagetyp>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Umlagetypen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<UmlagetypEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new UmlagetypEntry(entity, permissions);
                entry.Umlagen = await Task.WhenAll(entity.Umlagen.Where(u => u.Wohnungen
                    .Any(w => w.Verwalter.Count > 0 && w.Verwalter.AsQueryable()
                    .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, user.GetUserId()))))
                    .Select(async e => new UmlageEntryBase(e, await Utils.GetPermissions(user, e, Auth))));

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                Ctx.Umlagetypen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<UmlagetypEntry>> Post(ClaimsPrincipal user, UmlagetypEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var umlagen = entry.Umlagen!.Select(e => Ctx.Umlagen.Where(u => u.UmlageId == e.Id));
            var authRx = await Auth.AuthorizeAsync(user, umlagen, [Operations.SubCreate]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {
                return Add(entry);
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

        public override async Task<ActionResult<UmlagetypEntry>> Put(ClaimsPrincipal user, int id, UmlagetypEntry entry)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                entity.Bezeichnung = entry.Bezeichnung;

                SetOptionalValues(entity, entry);
                Ctx.Umlagetypen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new UmlagetypEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(Umlagetyp entity, UmlagetypEntry entry)
        {
            entity.Notiz = entry.Notiz;
        }
    }
}
