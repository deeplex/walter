using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlageDbService : ICRUDService<UmlageEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public UmlageDbService(SaverwalterContext dbService, IAuthorizationService authorizationService)
        {
            Ctx = dbService;
            Auth = authorizationService;
        }

        private Task<List<Umlage>> GetListForUser(ClaimsPrincipal user)
        {
            Guid.TryParse(user.FindAll(ClaimTypes.NameIdentifier).SingleOrDefault()?.Value, out Guid guid);
            return Ctx.Umlagen
                    .Where(e => e.Wohnungen.Any(w => w.Verwalter.Any(v => v.UserAccount.Id == guid)))
                    .ToListAsync();
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            List<Umlage> list = await (user.IsInRole("Admin")
                ? Ctx.Umlagen.ToListAsync()
                : GetListForUser(user));

            return new OkObjectResult(list.Select(e => new UmlageEntryBase(e)).ToList());
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Umlagen.FindAsync(id);
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
                var entry = new UmlageEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Umlagen.FindAsync(id);
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

            Ctx.Umlagen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, UmlageEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var allAuthorized = entry.SelectedWohnungen?
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

        private UmlageEntry Add(UmlageEntry entry)
        {
            if (entry.Typ == null)
            {
                throw new ArgumentException("entry has no Typ.");
            }

            if (entry.Schluessel == null)
            {
                throw new ArgumentException("entry has no Schluessel.");
            }
            var schluessel = (Umlageschluessel)entry.Schluessel.Id;
            var typ = Ctx.Umlagetypen.First(typ => typ.UmlagetypId == entry.Typ.Id);
            var entity = new Umlage(schluessel)
            {
                Typ = typ
            };

            SetOptionalValues(entity, entry);
            Ctx.Umlagen.Add(entity);
            Ctx.SaveChanges();

            return new UmlageEntry(entity);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, UmlageEntry entry)
        {
            var entity = await Ctx.Umlagen.FindAsync(id);
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

        private UmlageEntry Update(UmlageEntry entry, Umlage entity)
        {
            if (entry.Typ == null)
            {
                throw new ArgumentException("entry has no Typ.");
            }
            if (entry.Schluessel == null)
            {
                throw new ArgumentException("entry has no Schluessel.");
            }

            entity.Typ = Ctx.Umlagetypen.First(typ => typ.UmlagetypId == entry.Typ.Id);
            entity.Schluessel = (Umlageschluessel)entry.Schluessel.Id;

            SetOptionalValues(entity, entry);
            Ctx.Umlagen.Update(entity);
            Ctx.SaveChanges();

            return new UmlageEntry(entity);
        }

        private void SetOptionalValues(Umlage entity, UmlageEntry entry)
        {
            if (entity.UmlageId != entry.Id)
            {
                throw new Exception();
            }

            entity.Beschreibung = entry.Beschreibung;

            if (entry.SelectedWohnungen is IEnumerable<SelectionEntry> l)
            {
                // Add missing Wohnungen
                entity.Wohnungen.AddRange(l
                    .Where(w => !entity.Wohnungen.Exists(e => w.Id == e.WohnungId))
                    .SelectMany(w => Ctx.Wohnungen.Where(u => u.WohnungId == w.Id)));
                // Remove old Wohnungen
                entity.Wohnungen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.WohnungId));
            }

            if (entry.SelectedZaehler is IEnumerable<SelectionEntry> zaehler)
            {
                // Add missing zaehler
                entity.Zaehler.AddRange(zaehler
                    .Where(z => !entity.Zaehler.Exists(e => z.Id == e.ZaehlerId))
                    .SelectMany(w => Ctx.ZaehlerSet.Where(u => u.ZaehlerId == w.Id)!));
                // Remove old zaehler
                entity.Zaehler.RemoveAll(w => !zaehler.ToList().Exists(e => e.Id == w.ZaehlerId));
            }

            if (entry.Schluessel != null &&
                (Umlageschluessel)entry.Schluessel.Id == Umlageschluessel.NachVerbrauch &&
                entry.HKVO is HKVOEntryBase hkvo)
            {
                if (hkvo.Id == 0)
                {
                    var newHKVO = new HKVO(hkvo.HKVO_P7 / 100, hkvo.HKVO_P8 / 100, (HKVO_P9A2)hkvo.HKVO_P9.Id, hkvo.Strompauschale / 100)
                    {
                        Betriebsstrom = Ctx.Umlagen.Single(e => e.UmlageId == hkvo.Stromrechnung.Id)
                    };
                    entity.HKVO = newHKVO;
                    Ctx.HKVO.Add(newHKVO);
                }
                else
                {
                    var oldHKVO = Ctx.HKVO.Single(e => e.HKVOId == hkvo.Id);
                    oldHKVO.HKVO_P7 = (double)hkvo.HKVO_P7 / 100;
                    oldHKVO.HKVO_P8 = (double)hkvo.HKVO_P8 / 100;
                    oldHKVO.HKVO_P9 = (HKVO_P9A2)hkvo.HKVO_P9.Id;
                    oldHKVO.Strompauschale = (double)hkvo.Strompauschale / 100;
                    oldHKVO.Betriebsstrom = Ctx.Umlagen.Single(e => e.UmlageId == hkvo.Stromrechnung.Id);
                    Ctx.HKVO.Update(oldHKVO);
                }
            }
            else
            {
                entity.HKVO = null;
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
