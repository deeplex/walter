using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerstandDbService : ICRUDService<ZaehlerstandEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public ZaehlerstandDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<ActionResult<ZaehlerstandEntry>> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Zaehlerstaende.FindAsync(id);
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
                return new ZaehlerstandEntry(entity, permissions);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Zaehlerstaende.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Zaehlerstaende.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<ActionResult<ZaehlerstandEntry>> Post(ClaimsPrincipal user, ZaehlerstandEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var zaehler = await Ctx.ZaehlerSet.FindAsync(entry.Zaehler.Id);
                var authRx = await Auth.AuthorizeAsync(user, zaehler, [Operations.SubCreate]);
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

        private async Task<ZaehlerstandEntry> Add(ZaehlerstandEntry entry)
        {
            var zaehler = await Ctx.ZaehlerSet.FindAsync(entry.Zaehler!.Id!);
            var entity = new Zaehlerstand(entry.Datum, entry.Stand)
            {
                Zaehler = zaehler!
            };
            SetOptionalValues(entity, entry);
            Ctx.Zaehlerstaende.Add(entity);
            Ctx.SaveChanges();

            return new ZaehlerstandEntry(entity, entry.Permissions);
        }

        public async Task<ActionResult<ZaehlerstandEntry>> Put(ClaimsPrincipal user, int id, ZaehlerstandEntry entry)
        {
            var entity = await Ctx.Zaehlerstaende.FindAsync(id);
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
                return Update(entry, entity);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private ZaehlerstandEntry Update(ZaehlerstandEntry entry, Zaehlerstand entity)
        {
            entity.Datum = entry.Datum;
            entity.Stand = entry.Stand;

            SetOptionalValues(entity, entry);
            Ctx.Zaehlerstaende.Update(entity);
            Ctx.SaveChanges();

            return new ZaehlerstandEntry(entity, entry.Permissions);
        }

        private void SetOptionalValues(Zaehlerstand entity, ZaehlerstandEntry entry)
        {
            entity.Stand = entry.Stand;
            entity.Notiz = entry.Notiz;
        }
    }
}
