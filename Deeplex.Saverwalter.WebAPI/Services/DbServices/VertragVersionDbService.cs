using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragVersionDbService : ICRUDService<VertragVersionEntry>
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

            var permissions = await Utils.GetPermissions(user, entity, Auth);
            if (!permissions.Read)
            {
                return new ForbidResult();
            }

            try
            {
                var entry = new VertragVersionEntry(entity, permissions);
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

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.VertragVersionen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, VertragVersionEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id);
                var authRx = await Auth.AuthorizeAsync(user, vertrag, [Operations.SubCreate]);
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

        private async Task<VertragVersionEntryBase> Add(VertragVersionEntry entry)
        {
            var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id);
            var entity = new VertragVersion(entry.Beginn, entry.Grundmiete, entry.Personenzahl)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            Ctx.VertragVersionen.Add(entity);
            Ctx.SaveChanges();

            return new VertragVersionEntry(entity, entry.Permissions);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, VertragVersionEntry entry)
        {
            var entity = await Ctx.VertragVersionen.FindAsync(id);
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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private VertragVersionEntry Update(VertragVersionEntry entry, VertragVersion entity)
        {
            entity.Beginn = entry.Beginn;
            entity.Grundmiete = entry.Grundmiete;
            entity.Personenzahl = entry.Personenzahl;

            SetOptionalValues(entity, entry);
            Ctx.VertragVersionen.Update(entity);
            Ctx.SaveChanges();

            return new VertragVersionEntry(entity, entry.Permissions);
        }

        private void SetOptionalValues(VertragVersion entity, VertragVersionEntry entry)
        {
            if (entity.VertragVersionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
