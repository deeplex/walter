using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

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

        public async Task<ActionResult<IEnumerable<AdresseEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await AdressePermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new AdresseEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public async Task<ActionResult<AdresseEntry>> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Adressen.FindAsync(id);
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
                var entry = new AdresseEntry(entity, permissions);

                entry.Wohnungen = await Task.WhenAll(entity.Wohnungen
                    .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Kontakte = await Task.WhenAll(entity.Kontakte
                    .Select(async e => new KontaktEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Zaehler = await Task.WhenAll(entity.Zaehler
                    .Select(async e => new ZaehlerEntryBase(e, await Utils.GetPermissions(user, e, Auth))));

                return entry;
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Adressen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Adressen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<ActionResult<AdresseEntry>> Post(ClaimsPrincipal user, AdresseEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var wohnungen = entry.Wohnungen!.SelectMany(wohnung => Ctx.Wohnungen.Where(w => w.WohnungId == wohnung.Id));
            var authRx = await Auth.AuthorizeAsync(user, wohnungen, [Operations.Delete]);
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

        private AdresseEntry Add(AdresseEntry entry)
        {
            var entity = new Adresse(entry.Strasse, entry.Hausnummer, entry.Postleitzahl, entry.Stadt);
            SetOptionalValues(entity, entry);
            Ctx.Adressen.Add(entity);
            Ctx.SaveChanges();

            return new AdresseEntry(entity, entry.Permissions);
        }

        public async Task<ActionResult<AdresseEntry>> Put(ClaimsPrincipal user, int id, AdresseEntry entry)
        {
            var entity = await Ctx.Adressen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
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

        private AdresseEntry Update(AdresseEntry entry, Adresse entity)
        {
            entity.Strasse = entry.Strasse;
            entity.Hausnummer = entry.Hausnummer;
            entity.Postleitzahl = entry.Postleitzahl;
            entity.Stadt = entry.Stadt;

            SetOptionalValues(entity, entry);
            Ctx.Adressen.Update(entity);
            Ctx.SaveChanges();

            return new AdresseEntry(entity, entry.Permissions);
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
