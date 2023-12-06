using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AdresseDbService : ICRUDService<AdresseEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public AdresseDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Adressen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var success = false;
            foreach (var wohnung in entity.Wohnungen)
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
                var entry = new AdresseEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Adressen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var allAuthorized = entity.Wohnungen
                .Select(async wohnung => (await Auth.AuthorizeAsync(user, wohnung, [Operations.Delete])).Succeeded);
            if (!(await Task.WhenAll(allAuthorized)).All(result => result))
            {
                return new ForbidResult();
            }

            Ctx.Adressen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, AdresseEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var allAuthorized = entry.Wohnungen?
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

        private AdresseEntry Add(AdresseEntry entry)
        {
            var entity = new Adresse(entry.Strasse, entry.Hausnummer, entry.Postleitzahl, entry.Stadt);
            SetOptionalValues(entity, entry);
            Ctx.Adressen.Add(entity);
            Ctx.SaveChanges();

            return new AdresseEntry(entity);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, AdresseEntry entry)
        {
            var entity = await Ctx.Adressen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var allAuthorized = entity.Wohnungen
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

        private AdresseEntry Update(AdresseEntry entry, Adresse entity)
        {
            entity.Strasse = entry.Strasse;
            entity.Hausnummer = entry.Hausnummer;
            entity.Postleitzahl = entry.Postleitzahl;
            entity.Stadt = entry.Stadt;

            SetOptionalValues(entity, entry);
            Ctx.Adressen.Update(entity);
            Ctx.SaveChanges();

            return new AdresseEntry(entity);
        }

        private void SetOptionalValues(Adresse entity, AdresseEntry entry)
        {
            if (entity.AdresseId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
