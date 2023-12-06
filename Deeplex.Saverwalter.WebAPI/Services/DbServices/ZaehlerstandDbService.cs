using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerstandDbService : ICRUDService<ZaehlerstandEntryBase>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public ZaehlerstandDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Zaehlerstaende.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Zaehler.Wohnung, [Operations.Read]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {
                var entry = new ZaehlerstandEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Zaehlerstaende.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Zaehler.Wohnung, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Zaehlerstaende.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, ZaehlerstandEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = (await Ctx.ZaehlerSet.FindAsync(entry.Zaehler.Id))?.Wohnung;
                var authRx = await Auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate]);
                if (!authRx.Succeeded)
                {
                    return new ForbidResult();
                }

                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<ZaehlerstandEntryBase> Add(ZaehlerstandEntryBase entry)
        {
            var zaehler = await Ctx.ZaehlerSet.FindAsync(entry.Zaehler!.Id!);
            var entity = new Zaehlerstand(entry.Datum, entry.Stand)
            {
                Zaehler = zaehler!
            };
            SetOptionalValues(entity, entry);
            Ctx.Zaehlerstaende.Add(entity);
            Ctx.SaveChanges();

            return new ZaehlerstandEntryBase(entity);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, ZaehlerstandEntryBase entry)
        {
            var entity = await Ctx.Zaehlerstaende.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Zaehler.Wohnung, [Operations.Update]);
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

        private ZaehlerstandEntryBase Update(ZaehlerstandEntryBase entry, Zaehlerstand entity)
        {
            entity.Datum = entry.Datum;
            entity.Stand = entry.Stand;

            SetOptionalValues(entity, entry);
            Ctx.Zaehlerstaende.Update(entity);
            Ctx.SaveChanges();

            return new ZaehlerstandEntryBase(entity);
        }

        private void SetOptionalValues(Zaehlerstand entity, ZaehlerstandEntryBase entry)
        {
            entity.Stand = entry.Stand;
            entity.Notiz = entry.Notiz;
        }
    }
}
