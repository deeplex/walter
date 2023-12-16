using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class KontaktDbService : WalterDbServiceBase<KontaktEntry, Kontakt>
    {
        public KontaktDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<KontaktEntryBase>>> GetList()
        {
            var list = await Ctx.Kontakte.ToListAsync();
            return list.Select(e => new KontaktEntryBase(e, new(true))).ToList();
        }

        public override async Task<ActionResult<Kontakt>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Kontakte.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<KontaktEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                // TODO: Who can manage contacts?
                var permissions = new Permissions()
                {
                    Read = true,
                    Update = true,
                    Remove = true
                };
                var entry = new KontaktEntry(entity, permissions);

                entry.JuristischePersonen = await Task.WhenAll(entity.JuristischePersonen
                    .Select(async e => new KontaktEntryBase(e, await GetPermissions(user, e, Auth))));
                entry.Mitglieder = await Task.WhenAll(entity.Mitglieder
                    .Select(async e => new KontaktEntryBase(e, await GetPermissions(user, e, Auth))));
                entry.Vertraege = await Task.WhenAll(entity.Mietvertraege
                    .Concat(entity.Wohnungen.SelectMany(w => w.Vertraege))
                    .Distinct()
                    .Select(async e => new VertragEntryBase(e, await GetPermissions(user, e, Auth))));
                entry.Wohnungen = await Task.WhenAll(entity.Mietvertraege
                    .Concat(entity.Wohnungen.SelectMany(w => w.Vertraege))
                    .Select(e => e.Wohnung)
                    .Distinct()
                    .Select(async e => new WohnungEntryBase(e, await GetPermissions(user, e, Auth))));

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                Ctx.Kontakte.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<KontaktEntry>> Post(ClaimsPrincipal user, KontaktEntry entry)
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

                return await Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<KontaktEntry> Add(KontaktEntry entry)
        {
            var entity = new Kontakt(entry.Name, (Rechtsform)entry.Rechtsform.Id);

            SetOptionalValues(entity, entry);
            Ctx.Kontakte.Add(entity);
            await Ctx.SaveChangesAsync();

            return new KontaktEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<KontaktEntry>> Put(ClaimsPrincipal user, int id, KontaktEntry entry)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                entity.Name = entry.Name;
                entity.Rechtsform = (Rechtsform)entry.Rechtsform.Id;

                SetOptionalValues(entity, entry);
                Ctx.Kontakte.Update(entity);
                await Ctx.SaveChangesAsync();

                return new KontaktEntry(entity, entry.Permissions);
            });
        }

        private void SetOptionalValues(Kontakt entity, KontaktEntry entry)
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
