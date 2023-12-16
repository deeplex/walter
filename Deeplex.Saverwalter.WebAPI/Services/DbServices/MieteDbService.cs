using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MieteDbService : WalterDbServiceBase<MieteEntry, Miete>
    {
        public MieteDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<MieteEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await MietePermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new MieteEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Miete>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Mieten.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<MieteEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new MieteEntry(entity, permissions);
                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                Ctx.Mieten.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<MieteEntry>> Post(ClaimsPrincipal user, MieteEntry entry)
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

                return await Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<MieteEntry> Add(MieteEntry entry)
        {
            var mieten = new List<Miete>();

            // Be able to create multiple Mieten at once
            for (var i = 0; i <= entry.Repeat; ++i)
            {
                var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag.Id);
                var monat = entry.BetreffenderMonat.AddMonths(i);
                var entity = new Miete(entry.Zahlungsdatum, monat, entry.Betrag)
                {
                    Vertrag = vertrag!
                };

                SetOptionalValues(entity, entry);
                Ctx.Mieten.Add(entity);
                mieten.Add(entity);
            }

            Ctx.SaveChanges();

            return new MieteEntry(mieten.First(), entry.Permissions, entry.Repeat);

        }


        public override async Task<ActionResult<MieteEntry>> Put(ClaimsPrincipal user, int id, MieteEntry entry)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                entity.BetreffenderMonat = entry.BetreffenderMonat;
                entity.Betrag = entry.Betrag;
                entity.Zahlungsdatum = entry.Zahlungsdatum;

                SetOptionalValues(entity, entry);
                Ctx.Mieten.Update(entity);
                await Ctx.SaveChangesAsync();

                return new MieteEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(Miete entity, MieteEntry entry)
        {
            if (entity.MieteId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
