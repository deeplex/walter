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
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragDbService : WalterDbServiceBase<VertragEntry, int, Vertrag>
    {
        public VertragDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public Task<PagedResult<VertragEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            VertragPermissionHandler.GetQueryable(Ctx, user).PagedAsync(query,
                searchPredicate: t => e =>
                    e.Wohnung.Bezeichnung.ToLower().Contains(t) ||
                    (e.Wohnung.Adresse != null && (
                        e.Wohnung.Adresse.Strasse.ToLower().Contains(t) ||
                        e.Wohnung.Adresse.Stadt.ToLower().Contains(t))) ||
                    e.Mieter.Any(m =>
                        m.Name.ToLower().Contains(t) ||
                        (m.Vorname != null && m.Vorname.ToLower().Contains(t))),
                applySort: (q, sortBy, dir) => sortBy switch
                {
                    "beginn" => q.SortBy(e => e.Versionen.Min(v => v.Beginn), dir),
                    "ende" => q.SortBy(e => e.Ende, dir),
                    _ => q.SortBy(e => e.Versionen.Min(v => v.Beginn), "desc")
                },
                toEntry: async e => new VertragEntryBase(e, await GetPermissions(user, e, Auth)));

        public override async Task<ActionResult<Vertrag>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Vertraege.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<VertragEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new VertragEntry(entity, permissions);

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Vertraege.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<VertragEntry>> Post(ClaimsPrincipal user, VertragEntry entry)
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

                var beginn = entry.Versionen?.FirstOrDefault()?.Beginn;
                if (beginn.HasValue)
                {
                    var conflict = await FindOverlappingVertrag(entry.Wohnung.Id, beginn.Value, entry.Ende);
                    if (conflict is not null) return new ConflictObjectResult(conflict);
                }

                return await Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<VertragEntry> Add(VertragEntry entry)
        {
            if (entry.Wohnung == null)
            {
                throw new ArgumentException("entry has no Wohnung");
            }
            var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id);
            var idx = $"V{entry.Id:D5}";
            var entity = new Vertrag()
            {
                Wohnung = wohnung!,
                MietBuchungskonto = new Buchungskonto($"{idx}-MB", "Mietforderungen", BuchungskontoTyp.Aktiv),
                NkBuchungskonto = new Buchungskonto($"{idx}-NK", "NK-Vorauszahlungen", BuchungskontoTyp.Passiv),
                KautionsKonto = new Buchungskonto($"{idx}-KA", "Kaution", BuchungskontoTyp.Aktiv),
                BkAbrechnungsKonto = new Buchungskonto($"{idx}-BK", "BK-Abrechnung", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto($"{idx}-ZK", "Zahlung", BuchungskontoTyp.Aktiv),
                MietminderungsKonto = new Buchungskonto($"{idx}-MM", "Mietminderung", BuchungskontoTyp.Aufwand),
            };

            await SetOptionalValues(entity, entry);
            Ctx.Vertraege.Add(entity);
            Ctx.SaveChanges();

            return new VertragEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<VertragEntry>> Put(ClaimsPrincipal user, int id, VertragEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                if (entry.Wohnung == null)
                {
                    throw new ArgumentException("entry has no Wohnung.");
                }

                var beginn = await Ctx.VertragVersionen
                    .Where(v => v.Vertrag.VertragId == id)
                    .MinAsync(v => (DateOnly?)v.Beginn);
                if (beginn.HasValue)
                {
                    var conflict = await FindOverlappingVertrag(entry.Wohnung.Id, beginn.Value, entry.Ende, excludeId: id);
                    if (conflict is not null) return new ConflictObjectResult(conflict);
                }

                entity.Wohnung = (await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id))!;

                await SetOptionalValues(entity, entry);
                Ctx.Vertraege.Update(entity);
                await Ctx.SaveChangesAsync();

                return new VertragEntry(entity, entry.Permissions);
            });
        }

        private async Task<string?> FindOverlappingVertrag(int wohnungId, DateOnly beginn, DateOnly? ende, int excludeId = 0)
        {
            var existing = await Ctx.Vertraege
                .Where(v => v.Wohnung.WohnungId == wohnungId && v.VertragId != excludeId)
                .Select(v => new
                {
                    Ende = v.Ende,
                    Beginn = v.Versionen.Min(vv => (DateOnly?)vv.Beginn),
                    Mieter = v.Mieter.Select(m => m.Bezeichnung)
                })
                .ToListAsync();

            var conflict = existing.FirstOrDefault(v =>
                v.Beginn.HasValue &&
                beginn <= (v.Ende ?? DateOnly.MaxValue) &&
                (ende ?? DateOnly.MaxValue) >= v.Beginn.Value);

            if (conflict is null) return null;

            var mieter = string.Join(", ", conflict.Mieter);
            var endeText = conflict.Ende.HasValue ? conflict.Ende.Value.ToString("dd.MM.yyyy") : "offen";
            return $"Konflikt mit bestehendem Vertrag ({mieter}) vom {conflict.Beginn!.Value:dd.MM.yyyy} bis {endeText}.";
        }

        private async Task SetOptionalValues(Vertrag entity, VertragEntry entry)
        {
            if (entity.VertragId != entry.Id)
            {
                throw new Exception();
            }

            // Create Version for initial create (if provided)
            if (entity.VertragId == 0)
            {
                var entryVersion = entry.Versionen?.FirstOrDefault();
                if (entryVersion != null)
                {
                    var entityVersion = new VertragVersion(entryVersion.Beginn, entryVersion.Grundmiete, entryVersion.Personenzahl)
                    {
                        Vertrag = entity,
                        Nebenkostenvorauszahlung = entryVersion.Nebenkostenvorauszahlung
                    };
                    entity.Versionen.Add(entityVersion);
                }
            }

            entity.Ende = entry.Ende;
            entity.Ansprechpartner = entry.Ansprechpartner?.Id is int apId
                ? await Ctx.Kontakte.FindAsync(apId)
                : null;
            entity.Notiz = entry.Notiz;

            if (entry.SelectedMieter is IEnumerable<SelectionEntry> l)
            {
                // Add missing mieter
                foreach (var selectedMieter in l)
                {
                    if (!entity.Mieter.Any(m => m.KontaktId == selectedMieter.Id))
                    {
                        entity.Mieter.Add((await Ctx.Kontakte.FindAsync(selectedMieter.Id))!);
                    }
                }
                var currentMieter = entity.Mieter.ToList();
                // Remove mieter
                foreach (var m in currentMieter)
                {
                    if (!l.Any(e => m.KontaktId == e.Id))
                    {
                        entity.Mieter.Remove(m);
                    }
                }
            }
        }
    }
}
