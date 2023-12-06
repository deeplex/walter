using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MieteDbService : ICRUDService<MieteEntryBase>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public MieteDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        private Task<List<Miete>> GetListForUser(ClaimsPrincipal user)
        {
            Guid.TryParse(user.FindAll(ClaimTypes.NameIdentifier).SingleOrDefault()?.Value, out Guid guid);
            return Ctx.Mieten
                .Where(e => e.Vertrag.Wohnung.Verwalter.Any(v => v.UserAccount.Id == guid))
                .ToListAsync();
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await (user.IsInRole("Admin")
                ? Ctx.Mieten.ToListAsync()
                : GetListForUser(user));

            return new OkObjectResult(list.Select(e => new MieteEntryBase(e)).ToList());
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Mieten.FindAsync(id);
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
                var entry = new MieteEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Mieten.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Vertrag.Wohnung, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Mieten.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, MieteEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = (await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id))?.Wohnung;
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

        private async Task<MieteEntryBase> Add(MieteEntryBase entry)
        {
            var mieten = new List<Miete>();

            // Be able to create multiple Mieten at once
            for (int i = 0; i <= entry.Repeat; ++i)
            {
                var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag.Id);
                var monat = entry.BetreffenderMonat.AddMonths(i);
                var entity = new Miete(entry.Zahlungsdatum, monat, entry.Betrag)
                {
                    Vertrag = vertrag!
                };

                SetOptionalValues(entity, entry);
                Ctx.Mieten.Add(entity);
                mieten.Add(entity);
            }

            Ctx.SaveChanges();

            return new MieteEntryBase(mieten.First(), entry.Repeat);

        }


        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, MieteEntryBase entry)
        {
            var entity = await Ctx.Mieten.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity.Vertrag.Wohnung, [Operations.Update]);
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

        private MieteEntryBase Update(MieteEntryBase entry, Miete entity)
        {
            entity.BetreffenderMonat = entry.BetreffenderMonat;
            entity.Betrag = entry.Betrag;
            entity.Zahlungsdatum = entry.Zahlungsdatum;

            SetOptionalValues(entity, entry);
            Ctx.Mieten.Update(entity);
            Ctx.SaveChanges();

            return new MieteEntryBase(entity);
        }

        private static void SetOptionalValues(Miete entity, MieteEntryBase entry)
        {
            if (entity.MieteId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
