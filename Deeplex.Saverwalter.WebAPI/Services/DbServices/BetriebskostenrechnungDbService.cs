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
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Deeplex.Saverwalter.WebAPI.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BetriebskostenrechnungDbService : WalterDbServiceBase<BetriebskostenrechnungEntry, int, Betriebskostenrechnung>
    {
        private readonly BetriebskostenrechnungBuchungsService _buchungsService;

        public BetriebskostenrechnungDbService(
            SaverwalterContext ctx,
            IAuthorizationService authorizationService,
            BetriebskostenrechnungBuchungsService buchungsService)
            : base(ctx, authorizationService)
        {
            _buchungsService = buchungsService;
        }

        public async Task<PagedResult<BetriebskostenrechnungEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query)
        {
            var userId = user.IsInRole("Admin") ? (Guid?)null : user.GetUserId();

            // Resolve write-access up front so the closure in toEntry is cheap.
            var adminPerms = new Utils.Permissions { Read = true, Update = true, Remove = true };
            HashSet<int>? writableIds = null;
            if (userId.HasValue)
            {
                var uid = userId.Value;
                writableIds = (await Ctx.Wohnungen
                    .Where(w => w.Verwalter.Any(v =>
                        v.UserAccount.Id == uid &&
                        (v.Rolle == VerwalterRolle.Eigentuemer || v.Rolle == VerwalterRolle.Vollmacht)))
                    .Select(w => w.WohnungId)
                    .ToListAsync())
                    .ToHashSet();
            }

            var source = Ctx.Betriebskostenrechnungen.AsQueryable();
            if (userId.HasValue)
            {
                var uid = userId.Value;
                source = source.Where(e =>
                    e.Umlage.Wohnungen.Any(w =>
                        w.Verwalter.Any(v => v.UserAccount.Id == uid)));
            }

            // Includes must be part of the queryable so EF loads navigation
            // properties when materialising the page; CountAsync ignores them.
            source = source
                .AsSplitQuery()
                .Include(e => e.Umlage).ThenInclude(u => u.Typ)
                .Include(e => e.Umlage).ThenInclude(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                .Include(e => e.Buchungssatz).ThenInclude(s => s.Buchungszeilen);

            return await source.PagedAsync(query,
                searchPredicate: t => e =>
                    e.Umlage.Typ.Bezeichnung.ToLower().Contains(t) ||
                    e.Umlage.Wohnungen.Any(w =>
                        w.Bezeichnung.ToLower().Contains(t) ||
                        (w.Adresse != null && w.Adresse.Stadt.ToLower().Contains(t))),
                applySort: (q, sortBy, dir) => sortBy switch
                {
                    "betrag" => q.SortBy(e => e.Betrag, dir),
                    "betreffendesJahr" => q.SortBy(e => e.BetreffendesJahr, dir),
                    _ => q.SortBy(e => e.Datum, dir)
                },
                toEntry: e =>
                {
                    var perms = writableIds == null
                        ? adminPerms
                        : new Utils.Permissions
                        {
                            Read = true,
                            Update = e.Umlage.Wohnungen.Any(w => writableIds.Contains(w.WohnungId)),
                            Remove = e.Umlage.Wohnungen.Any(w => writableIds.Contains(w.WohnungId))
                        };
                    return new BetriebskostenrechnungEntryBase(e, perms);
                });
        }

        public override async Task<ActionResult<Betriebskostenrechnung>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Betriebskostenrechnungen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<BetriebskostenrechnungEntry>> Get(ClaimsPrincipal user, int id)
        {

            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new BetriebskostenrechnungEntry(entity, permissions);

                entry.Betriebskostenrechnungen = await Task.WhenAll(entity.Umlage.Betriebskostenrechnungen
                        .Select(async e => new BetriebskostenrechnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Wohnungen = await Task.WhenAll(entity.Umlage.Wohnungen
                        .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Betriebskostenrechnungen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        private async Task<BetriebskostenrechnungEntry> Add(BetriebskostenrechnungEntry entry)
        {
            if (entry.Umlage == null)
                throw new ArgumentException("entry.Umlage can't be null.");

            var umlage = await Ctx.Umlagen.FindAsync(entry.Umlage.Id)
                ?? throw new ArgumentException($"Did not find Umlage with Id {entry.Umlage.Id}");

            var entity = await _buchungsService.BucheRechnungAsync(
                umlage,
                entry.Betrag,
                entry.Datum,
                entry.BetreffendesJahr,
                entry.Notiz);

            return new BetriebskostenrechnungEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<BetriebskostenrechnungEntry>> Put(ClaimsPrincipal user, int id, BetriebskostenrechnungEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                if (entry.Umlage == null)
                    throw new ArgumentException("entry has no Umlage");

                var umlage = await Ctx.Umlagen.FindAsync(entry.Umlage.Id)
                    ?? throw new ArgumentException($"entry has no Umlage with Id {entry.Umlage.Id}");

                entity.Betrag = entry.Betrag;
                entity.Datum = entry.Datum;
                entity.BetreffendesJahr = entry.BetreffendesJahr;
                entity.Umlage = umlage;
                entity.Notiz = entry.Notiz;

                await _buchungsService.AktualisiereBuchungssatzAsync(
                    entity, umlage, entry.Betrag, entry.Datum, entry.BetreffendesJahr, entry.Notiz);

                Ctx.Betriebskostenrechnungen.Update(entity);

                return new BetriebskostenrechnungEntry(entity, entry.Permissions);
            });
        }

    }
}
