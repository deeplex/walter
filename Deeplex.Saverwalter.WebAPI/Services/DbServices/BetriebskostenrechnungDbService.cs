using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

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

        private Task<List<Betriebskostenrechnung>> GetListForUser(ClaimsPrincipal user)
        {
            Guid.TryParse(user.FindAll(ClaimTypes.NameIdentifier).SingleOrDefault()?.Value, out Guid guid);
            return Ctx.Betriebskostenrechnungen
                .Where(e => e.Umlage.Wohnungen.Any(w => w.Verwalter.Any(v => v.UserAccount.Id == guid)))
                .ToListAsync();
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await (user.IsInRole("Admin")
                ? Ctx.Betriebskostenrechnungen.ToListAsync()
                : GetListForUser(user));

            return new OkObjectResult(list.Select(e => new BetriebskostenrechnungEntryBase(e)).ToList());
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Betriebskostenrechnungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var wohnungen = entity.Umlage.Wohnungen;
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
                var entry = new BetriebskostenrechnungEntry(entity);
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

            var allAuthorized = entity.Umlage.Wohnungen
                .Select(async wohnung => (await Auth.AuthorizeAsync(user, wohnung, [Operations.Delete])).Succeeded);
            if (!(await Task.WhenAll(allAuthorized)).All(result => result))
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

            try
            {
                var allAuthorized = (await Ctx.Umlagen.FindAsync(entry.Umlage!.Id))!
                    .Wohnungen
                    .Select(async wohnung => (await Auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate])).Succeeded);
                if (allAuthorized == null ||
                    !(await Task.WhenAll(allAuthorized)).All(result => result))
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

            return new BetriebskostenrechnungEntry(entity);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, BetriebskostenrechnungEntry entry)
        {
            var entity = await Ctx.Betriebskostenrechnungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var allAuthorized = entity.Umlage.Wohnungen
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

            return new BetriebskostenrechnungEntry(entity);
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
