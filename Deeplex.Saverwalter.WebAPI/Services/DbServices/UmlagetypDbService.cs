using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlagetypDbService : ICRUDService<UmlagetypEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public UmlagetypDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Umlagetypen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var wohnungen = entity.Umlagen.SelectMany(entity => entity.Wohnungen);
            var success = false;
            foreach (var wohnung in wohnungen)
            {
                var authRx = await Auth.AuthorizeAsync(user, wohnung, [Operations.Read]);
                if (authRx.Succeeded)
                {
                    success = true;
                    break;
                }
            }
            if (!success)
            {
                return new ForbidResult();
            }

            try
            {
                var entry = new UmlagetypEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Umlagetypen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var allAuthorized = entity.Umlagen
                .SelectMany(entity => entity.Wohnungen)
                .Select(async wohnung => (await Auth.AuthorizeAsync(user, wohnung, [Operations.Delete])).Succeeded);
            if (!(await Task.WhenAll(allAuthorized)).All(result => result))
            {
                return new ForbidResult();
            }

            Ctx.Umlagetypen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, UmlagetypEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var allAuthorized = entry.Umlagen?
                .SelectMany(entity => entity.SelectedWohnungen ?? [])
                .SelectMany(w => Ctx.Wohnungen.Where(u => u.WohnungId == w.Id))
                .Select(async wohnung => (await Auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate])).Succeeded);
            if (allAuthorized == null ||
                !(await Task.WhenAll(allAuthorized)).All(result => result))
            {
                return new ForbidResult();
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

        private UmlagetypEntry Add(UmlagetypEntry entry)
        {
            var entity = new Umlagetyp(entry.Bezeichnung);
            SetOptionalValues(entity, entry);
            Ctx.Umlagetypen.Add(entity);
            Ctx.SaveChanges();

            return new UmlagetypEntry(entity);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, UmlagetypEntry entry)
        {
            var entity = await Ctx.Umlagetypen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var allAuthorized = entity.Umlagen
                .SelectMany(entity => entity.Wohnungen)
                .Select(async wohnung => (await Auth.AuthorizeAsync(user, wohnung, [Operations.Update])).Succeeded);
            if (!(await Task.WhenAll(allAuthorized)).All(result => result))
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

        private UmlagetypEntry Update(UmlagetypEntry entry, Umlagetyp entity)
        {
            entity.Bezeichnung = entry.Bezeichnung;

            SetOptionalValues(entity, entry);
            Ctx.Umlagetypen.Update(entity);
            Ctx.SaveChanges();

            return new UmlagetypEntry(entity);
        }

        private void SetOptionalValues(Umlagetyp entity, UmlagetypEntry entry)
        {
            entity.Notiz = entry.Notiz;
        }
    }
}
