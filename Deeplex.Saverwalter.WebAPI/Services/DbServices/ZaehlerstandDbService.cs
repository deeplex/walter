using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZählerstandPermissionHandler : WohnungPermissionHandlerBase<Zaehlerstand>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Zaehlerstand entity,
            Guid userId)
        {
            var wohnung = entity.Zaehler.Wohnung!;
            if (IsBesitzer(userId, wohnung))
            {
                context.Succeed(requirement);
            }
            else if (requirement.Name == Operations.Update.Name
                && (IsVerwalter(userId, wohnung)
                    || IsMieter(userId, wohnung)))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public class ZaehlerstandDbService : ICRUDService<ZaehlerstandEntryBase>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService _authorizationService;

        public ZaehlerstandDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Zaehlerstaende.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
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
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private ZaehlerstandEntryBase Add(ZaehlerstandEntryBase entry)
        {
            var zaehler = Ctx.ZaehlerSet.Find(entry.Zaehler!.Id!);
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

            var authRx = await _authorizationService.AuthorizeAsync(user, entity, "EditZählerstand");
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
