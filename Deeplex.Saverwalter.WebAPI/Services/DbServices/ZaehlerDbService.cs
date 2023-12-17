using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerDbService : WalterDbServiceBase<ZaehlerEntry, Zaehler>
    {
        public ZaehlerDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<ZaehlerEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await ZaehlerPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new ZaehlerEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Zaehler>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.ZaehlerSet.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<ZaehlerEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                return new ZaehlerEntry(entity, permissions);
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.ZaehlerSet.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<ZaehlerEntry>> Post(ClaimsPrincipal user, ZaehlerEntry entry)
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

        private async Task<ZaehlerEntry> Add(ZaehlerEntry entry)
        {
            var typ = (Zaehlertyp)entry.Typ.Id;
            var entity = new Zaehler(entry.Kennnummer, typ);

            await SetOptionalValues(entity, entry);
            Ctx.ZaehlerSet.Add(entity);
            Ctx.SaveChanges();

            return new ZaehlerEntry(entity, entry.Permissions);
        }
        public override async Task<ActionResult<ZaehlerEntry>> Put(ClaimsPrincipal user, int id, ZaehlerEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Kennnummer = entry.Kennnummer;
                entity.Typ = (Zaehlertyp)entry.Typ.Id;

                await SetOptionalValues(entity, entry);
                Ctx.ZaehlerSet.Update(entity);
                Ctx.SaveChanges();

                return new ZaehlerEntry(entity, entry.Permissions);
            });
        }

        private async Task SetOptionalValues(Zaehler entity, ZaehlerEntry entry)
        {
            entity.Wohnung = entry.Wohnung is SelectionEntry w ? await Ctx.Wohnungen.FindAsync(w.Id) : null;

            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
            entity.Notiz = entry.Notiz;
            entity.Ende = entry.Ende;

            if (entry.SelectedUmlagen is IEnumerable<SelectionEntry> umlagen)
            {
                // Add missing umlagen
                entity.Umlagen.AddRange(umlagen
                    .Where(umlage => !entity.Umlagen.Exists(e => umlage.Id == e.UmlageId))
                    .SelectMany(w => Ctx.Umlagen.Where(u => u.UmlageId == w.Id)));
                // Remove old umlagen
                entity.Umlagen.RemoveAll(w => !umlagen.ToList().Exists(e => e.Id == w.UmlageId));
            }
        }
    }
}
