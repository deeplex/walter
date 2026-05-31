// Copyright (c) 2023-2026 Kai Lawrence
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
using Deeplex.Saverwalter.WebAPI.Utils;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class BetriebskostenrechnungDbService(
        SaverwalterContext ctx,
        IAuthorizationService auth,
        BetriebskostenrechnungBuchungsService buchungsService)
    {
        private async Task<Dictionary<int, Umlage>> GetNkKontoUmlageMapAsync()
            => await ctx.Umlagen
                .Include(u => u.Typ)
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                .Include(u => u.Wohnungen).ThenInclude(w => w.Verwalter).ThenInclude(v => v.UserAccount)
                .ToDictionaryAsync(u => u.NkVerrechnungsKonto.BuchungskontoId, u => u);

        private async Task<List<Buchungssatz>> LoadBkSaetzeAsync(HashSet<int> nkKontoIds)
            => await ctx.Buchungssaetze
                .AsSplitQuery()
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.AlsHabenZeile).ThenInclude(opa => opa.SollZeile)
                .Where(s => s.Buchungszeilen.Any(z =>
                    z.SollHaben == SollHaben.Haben &&
                    nkKontoIds.Contains(z.Buchungskonto.BuchungskontoId)))
                .ToListAsync();

        private static Umlage? FindUmlage(Buchungssatz satz, Dictionary<int, Umlage> map)
        {
            var habenKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Haben)
                ?.Buchungskonto.BuchungskontoId;
            return habenKontoId.HasValue && map.TryGetValue(habenKontoId.Value, out var u) ? u : null;
        }

        public async Task<PagedResult<BetriebskostenrechnungEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query)
        {
            var userId = user.IsInRole("Admin") ? (Guid?)null : user.GetUserId();
            var adminPerms = new Permissions { Read = true, Update = false, Remove = true };

            HashSet<int>? writableWohnungIds = null;
            if (userId.HasValue)
            {
                var uid = userId.Value;
                writableWohnungIds = (await ctx.Wohnungen
                    .Where(w => w.Verwalter.Any(v =>
                        v.UserAccount.Id == uid &&
                        (v.Rolle == VerwalterRolle.Eigentuemer || v.Rolle == VerwalterRolle.Vollmacht)))
                    .Select(w => w.WohnungId)
                    .ToListAsync()).ToHashSet();
            }

            var umlageMap = await GetNkKontoUmlageMapAsync();
            var nkKontoIds = umlageMap.Keys.ToHashSet();
            var allSaetze = await LoadBkSaetzeAsync(nkKontoIds);

            // Filter by authorization
            IEnumerable<(Buchungssatz Satz, Umlage Umlage)> pairs = allSaetze
                .Select(s => (Satz: s, Umlage: FindUmlage(s, umlageMap)!))
                .Where(p => p.Umlage != null);

            if (userId.HasValue)
            {
                pairs = pairs.Where(p =>
                    p.Umlage.Wohnungen.Any(w =>
                        w.Verwalter.Any(v => v.UserAccount.Id == userId.Value)));
            }

            // Apply search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var t = query.Search.ToLower();
                pairs = pairs.Where(p =>
                    p.Umlage.Typ.Bezeichnung.ToLower().Contains(t) ||
                    p.Umlage.Wohnungen.Any(w =>
                        w.Bezeichnung.ToLower().Contains(t) ||
                        (w.Adresse != null && w.Adresse.Stadt.ToLower().Contains(t))));
            }

            var list = pairs.ToList();
            var totalCount = list.Count;

            // Sort
            list = (query.SortBy switch
            {
                "betrag" => query.SortDir == "asc"
                    ? list.OrderBy(p => p.Satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag))
                    : list.OrderByDescending(p => p.Satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag)),
                "betreffendesJahr" => query.SortDir == "asc"
                    ? list.OrderBy(p => p.Satz.Buchungsjahr)
                    : list.OrderByDescending(p => p.Satz.Buchungsjahr),
                _ => query.SortDir == "asc"
                    ? list.OrderBy(p => p.Satz.Buchungsdatum)
                    : list.OrderByDescending(p => p.Satz.Buchungsdatum)
            }).ToList();

            var items = list.Skip(query.Skip).Take(query.Take).Select(p =>
            {
                var umlage = p.Umlage;
                var perms = writableWohnungIds == null
                    ? adminPerms
                    : new Permissions
                    {
                        Read = true,
                        Update = false,
                        Remove = umlage.Wohnungen.Any(w => writableWohnungIds.Contains(w.WohnungId))
                    };
                return (BetriebskostenrechnungEntryBase)new BetriebskostenrechnungEntryBase(p.Satz, umlage, perms);
            });

            return new PagedResult<BetriebskostenrechnungEntryBase>(items, totalCount);
        }

        public async Task<ActionResult<BetriebskostenrechnungEntry>> Get(ClaimsPrincipal user, Guid id)
        {
            var satz = await ctx.Buchungssaetze
                .AsSplitQuery()
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.AlsHabenZeile).ThenInclude(opa => opa.SollZeile)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == id);

            if (satz is null) return new NotFoundResult();

            var habenKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Haben)
                ?.Buchungskonto.BuchungskontoId;
            if (!habenKontoId.HasValue) return new NotFoundResult();

            var umlage = await ctx.Umlagen
                .Include(u => u.Typ)
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                .Include(u => u.Wohnungen).ThenInclude(w => w.Verwalter).ThenInclude(v => v.UserAccount)
                .FirstOrDefaultAsync(u => u.NkVerrechnungsKonto.BuchungskontoId == habenKontoId.Value);

            if (umlage is null) return new NotFoundResult();

            var authRx = await auth.AuthorizeAsync(user, umlage, [Operations.Read]);
            if (!authRx.Succeeded) return new ForbidResult();

            var permissions = await GetPermissions(user, umlage, auth);
            var entry = new BetriebskostenrechnungEntry(satz, umlage, permissions);

            var relatedSaetze = await ctx.Buchungssaetze
                .AsSplitQuery()
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.AlsHabenZeile).ThenInclude(opa => opa.SollZeile)
                .Where(s => s.BuchungssatzId != id &&
                    s.Buchungszeilen.Any(z =>
                        z.SollHaben == SollHaben.Haben &&
                        z.Buchungskonto.BuchungskontoId == habenKontoId.Value))
                .ToListAsync();
            entry.Betriebskostenrechnungen = relatedSaetze
                .Select(s => new BetriebskostenrechnungEntryBase(s, umlage, permissions))
                .ToList();

            entry.Wohnungen = await Task.WhenAll(umlage.Wohnungen
                .Select(async w => new WohnungEntryBase(w, await GetPermissions(user, w, auth))));

            return entry;
        }

        public async Task<ActionResult> Delete(ClaimsPrincipal user, Guid id)
        {
            var satz = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == id);

            if (satz is null) return new NotFoundResult();

            var habenKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Haben)
                ?.Buchungskonto.BuchungskontoId;
            if (!habenKontoId.HasValue) return new NotFoundResult();

            var umlage = await ctx.Umlagen
                .Include(u => u.Wohnungen)
                .FirstOrDefaultAsync(u => u.NkVerrechnungsKonto.BuchungskontoId == habenKontoId.Value);
            if (umlage is null) return new NotFoundResult();

            var authRx = await auth.AuthorizeAsync(user, umlage, [Operations.Delete]);
            if (!authRx.Succeeded) return new ForbidResult();

            ctx.Buchungssaetze.Remove(satz);
            await ctx.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<ActionResult<BetriebskostenrechnungEntry>> Post(ClaimsPrincipal user, BetriebskostenrechnungEntry entry)
        {
            if (entry.Umlage == null)
                return new BadRequestObjectResult("entry.Umlage can't be null.");

            var umlage = await ctx.Umlagen
                .Include(u => u.Typ)
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.Wohnungen)
                .FirstOrDefaultAsync(u => u.UmlageId == entry.Umlage.Id);
            if (umlage is null)
                return new BadRequestObjectResult($"Did not find Umlage with Id {entry.Umlage.Id}");

            var authRx = await auth.AuthorizeAsync(user, umlage, [Operations.Update]);
            if (!authRx.Succeeded) return new ForbidResult();

            var satz = await buchungsService.BucheRechnungAsync(
                umlage, entry.Betrag, entry.Datum, entry.BetreffendesJahr, entry.Notiz);

            // Reload with full navigation for response
            satz = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .FirstAsync(s => s.BuchungssatzId == satz.BuchungssatzId);

            var permissions = await GetPermissions(user, umlage, auth);
            return new BetriebskostenrechnungEntry(satz, umlage, permissions);
        }

        public async Task<ActionResult<BetriebskostenrechnungEntry>> Put(ClaimsPrincipal user, Guid id, BetriebskostenrechnungEntry entry)
        {
            var satz = await ctx.Buchungssaetze
                .AsSplitQuery()
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.AlsHabenZeile).ThenInclude(opa => opa.SollZeile)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == id);

            if (satz is null) return new NotFoundResult();

            var habenKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Haben)
                ?.Buchungskonto.BuchungskontoId;
            if (!habenKontoId.HasValue) return new NotFoundResult();

            var umlage = await ctx.Umlagen
                .Include(u => u.Typ)
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.Wohnungen)
                .FirstOrDefaultAsync(u => u.NkVerrechnungsKonto.BuchungskontoId == habenKontoId.Value);
            if (umlage is null) return new NotFoundResult();

            var authRx = await auth.AuthorizeAsync(user, umlage, [Operations.Update]);
            if (!authRx.Succeeded) return new ForbidResult();

            var neueUmlage = entry.Umlage != null
                ? await ctx.Umlagen.Include(u => u.Typ).Include(u => u.NkVerrechnungsKonto)
                    .FirstOrDefaultAsync(u => u.UmlageId == entry.Umlage.Id) ?? umlage
                : umlage;

            await buchungsService.AktualisiereBuchungssatzAsync(
                satz, neueUmlage, entry.Betrag, entry.Datum, entry.BetreffendesJahr, entry.Notiz);

            var permissions = await GetPermissions(user, neueUmlage, auth);
            return new BetriebskostenrechnungEntry(satz, neueUmlage, permissions);
        }
    }
}
