using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragVersionDbService : ICRUDService<VertragVersionEntryBase>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public VertragVersionDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.VertragVersionen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Vertrag.Wohnung, [Operations.Read]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {
                var entry = new VertragVersionEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.VertragVersionen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Vertrag.Wohnung, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.VertragVersionen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, VertragVersionEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = (await Ctx.ZaehlerSet.FindAsync(entry.Vertrag!.Id))?.Wohnung;
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

        private async Task<VertragVersionEntryBase> Add(VertragVersionEntryBase entry)
        {
            var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id);
            var entity = new VertragVersion(entry.Beginn, entry.Grundmiete, entry.Personenzahl)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            Ctx.VertragVersionen.Add(entity);
            Ctx.SaveChanges();

            return new VertragVersionEntryBase(entity);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, VertragVersionEntryBase entry)
        {
            var entity = await Ctx.VertragVersionen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
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

        private VertragVersionEntryBase Update(VertragVersionEntryBase entry, VertragVersion entity)
        {
            entity.Beginn = entry.Beginn;
            entity.Grundmiete = entry.Grundmiete;
            entity.Personenzahl = entry.Personenzahl;

            SetOptionalValues(entity, entry);
            Ctx.VertragVersionen.Update(entity);
            Ctx.SaveChanges();

            return new VertragVersionEntryBase(entity);
        }

        private void SetOptionalValues(VertragVersion entity, VertragVersionEntryBase entry)
        {
            if (entity.VertragVersionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
