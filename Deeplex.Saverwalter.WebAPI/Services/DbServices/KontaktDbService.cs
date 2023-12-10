using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class KontaktDbService : ICRUDService<KontaktEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public KontaktDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> GetList()
        {
            var list = await Ctx.Kontakte.ToListAsync();
            return new OkObjectResult(list.Select(e => new KontaktEntryBase(e, new(true))).ToList());
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Kontakte.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var permissions = new Permissions()
                {
                    Read = true,
                    Update = true,
                    Remove = true
                };
                var entry = new KontaktEntry(entity, permissions);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Kontakte.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            // TODO: Who can delete the contact?
            //var authRx = await Auth.AuthorizeAsync(user, entity.Vertrag.Wohnung, [Operations.Delete]);
            //if (!authRx.Succeeded)
            //{
            //    return new ForbidResult();
            //}

            Ctx.Kontakte.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, KontaktEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                // TODO: Who can post new contacts?
                //var authRx = await Auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate]);
                //if (!authRx.Succeeded)
                //{
                //    return new ForbidResult();
                //}

                return new OkObjectResult(await Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<KontaktEntry> Add(KontaktEntry entry)
        {
            var entity = new Kontakt(entry.Name, (Rechtsform)entry.Rechtsform.Id);

            await SetOptionalValues(entity, entry);
            Ctx.Kontakte.Add(entity);
            Ctx.SaveChanges();

            return new KontaktEntry(entity, entry.Permissions);
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, KontaktEntry entry)
        {
            var entity = await Ctx.Kontakte.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            // TODO: Who can update entries?
            //var authRx = await Auth.AuthorizeAsync(user, entity.Vertrag.Wohnung, [Operations.Update]);
            //if (!authRx.Succeeded)
            //{
            //    return new ForbidResult();
            //}

            try
            {
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<KontaktEntry> Update(KontaktEntry entry, Kontakt entity)
        {
            entity.Name = entry.Name;
            entity.Rechtsform = (Rechtsform)entry.Rechtsform.Id;

            await SetOptionalValues(entity, entry);
            Ctx.Kontakte.Update(entity);
            Ctx.SaveChanges();

            return new KontaktEntry(entity, entry.Permissions);
        }

        private async Task SetOptionalValues(Kontakt entity, KontaktEntry entry)
        {
            if (entity.KontaktId != entry.Id)
            {
                throw new Exception();
            }

            entity.Vorname = entry.Vorname;
            entity.Email = entry.Email;
            entity.Fax = entry.Fax;
            entity.Notiz = entry.Notiz;
            entity.Telefon = entry.Telefon;
            entity.Mobil = entry.Mobil;


            if (entry.Adresse is AdresseEntryBase a)
            {
                entity.Adresse = GetAdresse(a, Ctx);
            }
            if (entry.SelectedJuristischePersonen is IEnumerable<SelectionEntry> l)
            {
                // Add new
                entity.JuristischePersonen
                    .AddRange(l.Where(w => !entity.JuristischePersonen.Exists(e => w.Id == e.KontaktId))
                    .SelectMany(w => Ctx.Kontakte.Where(u => u.KontaktId == w.Id)));
                // Remove old
                entity.JuristischePersonen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.KontaktId));
            }

            if (entry.SelectedMitglieder is IEnumerable<SelectionEntry> m)
            {
                // Add new
                entity.Mitglieder
                    .AddRange(m.Where(w => !entity.Mitglieder.Exists(e => w.Id == e.KontaktId))
                    .SelectMany(w => Ctx.Kontakte.Where(u => u.KontaktId == w.Id)));

                // Remove old
                entity.Mitglieder.RemoveAll((w) => !m.ToList().Exists(e => e.Id == w.KontaktId));
            }

            if ((Rechtsform)entry.Rechtsform.Id == Rechtsform.natuerlich)
            {
                if (entry.Anrede is SelectionEntry anrede)
                {
                    entity.Anrede = (Anrede)anrede.Id;
                }
                else
                {
                    throw new ArgumentException("Anrede is required when Rechtsform is Natürlich");
                }
                //entity.Titel = (Titel)(entry.Titel?.Id ?? int.Parse(Titel.Kein));
            }
            else
            {
                entity.Anrede = Anrede.Keine;
            }
        }
    }
}
