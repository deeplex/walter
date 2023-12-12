using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BetriebskostenrechnungDbService : ICRUDService<BetriebskostenrechnungEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public BetriebskostenrechnungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await BetriebskostenrechnungPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return new OkObjectResult(await Task.WhenAll(list
                .Select(async e => new BetriebskostenrechnungEntryBase(e, await Utils.GetPermissions(user, e, Auth)))));
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Betriebskostenrechnungen.FindAsync(id);
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
                var entry = new BetriebskostenrechnungEntry(entity, permissions);

                entry.Betriebskostenrechnungen = await Task.WhenAll(entity.Umlage.Betriebskostenrechnungen
                    .Select(async e => new BetriebskostenrechnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Wohnungen = await Task.WhenAll(entity.Umlage.Wohnungen
                    .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));

                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Betriebskostenrechnungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Betriebskostenrechnungen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, BetriebskostenrechnungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var umlage = Ctx.Umlagen.FindAsync(entry.Umlage!.Id);
            var authRx = await Auth.AuthorizeAsync(user, umlage, [Operations.SubCreate]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {

                return new OkObjectResult(await Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<BetriebskostenrechnungEntry> Add(BetriebskostenrechnungEntry entry)
        {
            if (entry.Umlage == null)
            {
                throw new ArgumentException("entry.Umlage can't be null.");
            }
            var umlage = await Ctx.Umlagen.FindAsync(entry.Umlage.Id);
            if (umlage == null)
            {
                throw new ArgumentException($"Did not find Umlage with Id {entry.Umlage.Id}");
            }
            var entity = new Betriebskostenrechnung(entry.Betrag, entry.Datum, entry.BetreffendesJahr)
            {
                Umlage = umlage!
            };

            SetOptionalValues(entity, entry);
            Ctx.Betriebskostenrechnungen.Add(entity);
            Ctx.SaveChanges();

            return new BetriebskostenrechnungEntry(entity, entry.Permissions);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, BetriebskostenrechnungEntry entry)
        {
            var entity = await Ctx.Betriebskostenrechnungen.FindAsync(id);
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
                return new OkObjectResult(await Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<BetriebskostenrechnungEntry> Update(BetriebskostenrechnungEntry entry, Betriebskostenrechnung entity)
        {
            entity.Betrag = entry.Betrag;
            entity.Datum = entry.Datum;
            entity.BetreffendesJahr = entry.BetreffendesJahr;
            if (entry.Umlage == null)
            {
                throw new ArgumentException("entry has no Umlage");
            }
            var umlage = await Ctx.Umlagen.FindAsync(entry.Umlage.Id);
            if (umlage == null)
            {
                throw new ArgumentException($"entry has no Umlage with Id {entry.Umlage.Id}");
            }

            entity.Umlage = umlage;

            SetOptionalValues(entity, entry);
            Ctx.Betriebskostenrechnungen.Update(entity);
            Ctx.SaveChanges();

            return new BetriebskostenrechnungEntry(entity, entry.Permissions);
        }

        private void SetOptionalValues(Betriebskostenrechnung entity, BetriebskostenrechnungEntry entry)
        {
            if (entity.BetriebskostenrechnungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
