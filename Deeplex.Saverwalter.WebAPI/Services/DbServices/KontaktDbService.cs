// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.BankkontoController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktMitgliedschaftController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class KontaktDbService : WalterDbServiceBase<KontaktEntry, int, Kontakt>
    {
        public KontaktDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public Task<PagedResult<KontaktEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            KontaktPermissionHandler.GetQueryable(Ctx, user).PagedAsync(query,
                searchPredicate: t => e =>
                    e.Name.ToLower().Contains(t) ||
                    (e.Vorname != null && e.Vorname.ToLower().Contains(t)) ||
                    (e.Email != null && e.Email.ToLower().Contains(t)) ||
                    (e.Telefon != null && e.Telefon.ToLower().Contains(t)) ||
                    (e.Mobil != null && e.Mobil.ToLower().Contains(t)) ||
                    (e.Adresse != null && (
                        e.Adresse.Strasse.ToLower().Contains(t) ||
                        e.Adresse.Stadt.ToLower().Contains(t))),
                applySort: (q, sortBy, dir) => sortBy switch
                {
                    "email" => q.SortBy(e => e.Email, dir),
                    _ => q.SortBy(e => e.Name, dir).ThenSortBy(e => e.Vorname, dir)
                },
                toEntry: async e => new KontaktEntryBase(e, await GetPermissions(user, e, Auth)));

        public override async Task<ActionResult<Kontakt>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Kontakte.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<KontaktEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                // TODO: Who can manage contacts?
                var permissions = new Permissions()
                {
                    Read = true,
                    Update = true,
                    Remove = true
                };
                var entry = new KontaktEntry(entity, permissions);

                entry.JuristischePersonen = await Task.WhenAll(entity.AlsMitglied
                    .Where(m => m.Bis == null)
                    .Select(m => m.JuristischePerson)
                    .Select(async e => new KontaktEntryBase(e, await GetPermissions(user, e, Auth))));
                entry.Mitglieder = await Task.WhenAll(entity.AlsJuristischePerson
                    .Where(m => m.Bis == null)
                    .Select(m => m.Mitglied)
                    .Select(async e => new KontaktEntryBase(e, await GetPermissions(user, e, Auth))));
                entry.MitgliedschaftenAlsMitglied = entity.AlsMitglied
                    .Select(m => new KontaktMitgliedschaftEntry(m))
                    .ToList();
                entry.MitgliedschaftenAlsJuristischePerson = entity.AlsJuristischePerson
                    .Select(m => new KontaktMitgliedschaftEntry(m))
                    .ToList();
                var eigentuemerWohnungen = entity.EigentuemerIn.Select(e => e.Wohnung);
                entry.Vertraege = await Task.WhenAll(entity.Mietvertraege
                    .Concat(eigentuemerWohnungen.SelectMany(w => w.Vertraege))
                    .Distinct()
                    .Select(async e => new VertragEntryBase(e, await GetPermissions(user, e, Auth))));
                entry.Wohnungen = await Task.WhenAll(entity.Mietvertraege
                    .Concat(eigentuemerWohnungen.SelectMany(w => w.Vertraege))
                    .Select(e => e.Wohnung)
                    .Distinct()
                    .Select(async e => new WohnungEntryBase(e, await GetPermissions(user, e, Auth))));
                var myBankkontoIds = Ctx.Bankkontos
                    .Where(b => b.Besitzer.Any(k => k.KontaktId == entity.KontaktId))
                    .Select(b => b.BankkontoId)
                    .ToList();

                entry.Bankkontos = Ctx.Bankkontos
                    .Where(b => myBankkontoIds.Contains(b.BankkontoId))
                    .AsEnumerable()
                    .Select(b => new BankkontoEntryBase(b, new Permissions { Read = true, Update = true, Remove = true }))
                    .ToList();

                entry.Transaktionen = myBankkontoIds.Count == 0 ? [] : Ctx.Transaktionen
                    .Where(t =>
                        (t.Zahler != null && myBankkontoIds.Contains(t.Zahler.BankkontoId)) ||
                        (t.Zahlungsempfaenger != null && myBankkontoIds.Contains(t.Zahlungsempfaenger.BankkontoId)))
                    .Include(t => t.Zahler)
                    .Include(t => t.Zahlungsempfaenger)
                    .AsEnumerable()
                    .Select(e => new TransaktionEntryBase(e, new(true)))
                    .ToList();

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
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
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
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
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (entry.SelectedJuristischePersonen is IEnumerable<SelectionEntry> l)
            {
                var lIds = l.Select(x => x.Id).ToHashSet();
                // Close memberships no longer selected
                foreach (var m in entity.AlsMitglied.Where(m => m.Bis == null && !lIds.Contains(m.JuristischePerson.KontaktId)))
                    m.Bis = today;
                // Add new active memberships
                var existingActiveIds = entity.AlsMitglied.Where(m => m.Bis == null).Select(m => m.JuristischePerson.KontaktId).ToHashSet();
                foreach (var selected in l.Where(x => !existingActiveIds.Contains(x.Id)))
                {
                    var jp = Ctx.Kontakte.Find(selected.Id);
                    if (jp != null)
                        Ctx.KontaktMitgliedschaften.Add(new KontaktMitgliedschaft(today) { JuristischePerson = jp, Mitglied = entity });
                }
            }

            if (entry.SelectedMitglieder is IEnumerable<SelectionEntry> sel)
            {
                var selIds = sel.Select(x => x.Id).ToHashSet();
                // Close memberships no longer selected
                foreach (var m in entity.AlsJuristischePerson.Where(m => m.Bis == null && !selIds.Contains(m.Mitglied.KontaktId)))
                    m.Bis = today;
                // Add new active memberships
                var existingActiveIds = entity.AlsJuristischePerson.Where(m => m.Bis == null).Select(m => m.Mitglied.KontaktId).ToHashSet();
                foreach (var selected in sel.Where(x => !existingActiveIds.Contains(x.Id)))
                {
                    var mitglied = Ctx.Kontakte.Find(selected.Id);
                    if (mitglied != null)
                        Ctx.KontaktMitgliedschaften.Add(new KontaktMitgliedschaft(today) { JuristischePerson = entity, Mitglied = mitglied });
                }
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
