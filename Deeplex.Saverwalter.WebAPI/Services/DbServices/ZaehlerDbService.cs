using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerDbService : ICRUDService<ZaehlerEntry>
    {
        public SaverwalterContext Ctx { get; }
        public IAuthorizationService Auth { get; }

        public ZaehlerDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.ZaehlerSet.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Wohnung, [Operations.Read]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {
                var entry = new ZaehlerEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.ZaehlerSet.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Wohnung, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }


            Ctx.ZaehlerSet.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, ZaehlerEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung!.Id);
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

        private async Task<ZaehlerEntry> Add(ZaehlerEntry entry)
        {
            var typ = (Zaehlertyp)entry.Typ.Id;
            var entity = new Zaehler(entry.Kennnummer, typ);

            await SetOptionalValues(entity, entry);
            Ctx.ZaehlerSet.Add(entity);
            Ctx.SaveChanges();

            return new ZaehlerEntry(entity);
        }
        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, ZaehlerEntry entry)
        {
            var entity = await Ctx.ZaehlerSet.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Wohnung, [Operations.Update]);
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

        private async Task<ZaehlerEntry> Update(ZaehlerEntry entry, Zaehler entity)
        {
            entity.Kennnummer = entry.Kennnummer;
            entity.Typ = (Zaehlertyp)entry.Typ.Id;

            await SetOptionalValues(entity, entry);
            Ctx.ZaehlerSet.Update(entity);
            Ctx.SaveChanges();

            return new ZaehlerEntry(entity);
        }

        private async Task SetOptionalValues(Zaehler entity, ZaehlerEntry entry)
        {
            entity.Wohnung = entry.Wohnung is SelectionEntry w ? await Ctx.Wohnungen.FindAsync(w.Id) : null;

            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
            entity.Notiz = entry.Notiz;
            entity.Ende = entry.Ende;

            if (entry.SelectedUmlagen is IEnumerable<SelectionEntry> umlagen)
            {
                // Add missing umlagen
                entity.Umlagen.AddRange(umlagen
                    .Where(umlage => !entity.Umlagen.Exists(e => umlage.Id == e.UmlageId))
                    .SelectMany(w => Ctx.Umlagen.Where(u => u.UmlageId == w.Id)));
                // Remove old umlagen
                entity.Umlagen.RemoveAll(w => !umlagen.ToList().Exists(e => e.Id == w.UmlageId));
            }
        }
    }
}
