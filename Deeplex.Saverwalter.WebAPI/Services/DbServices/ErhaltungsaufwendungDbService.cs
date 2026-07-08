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
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class ErhaltungsaufwendungDbService(
        SaverwalterContext ctx,
        IAuthorizationService auth,
        ErhaltungsaufwendungBuchungsService buchungsService)
    {
        private async Task<(Dictionary<int, Wohnung> AufwandsMap, Dictionary<int, Kontakt> VerbindlichkeitsMap)> BuildMapsAsync()
        {
            var wohnungen = await ctx.Wohnungen
                .Include(w => w.AufwandsKonto)
                .Include(w => w.Adresse)
                .Include(w => w.Verwalter).ThenInclude(v => v.UserAccount)
                .ToListAsync();
            var aufwandsMap = wohnungen.ToDictionary(w => w.AufwandsKonto.BuchungskontoId, w => w);

            var kontakte = await ctx.Kontakte
                .Where(k => k.VerbindlichkeitsKonto != null)
                .Include(k => k.VerbindlichkeitsKonto)
                .ToListAsync();
            var verbindlichkeitsMap = kontakte
                .Where(k => k.VerbindlichkeitsKonto != null)
                .ToDictionary(k => k.VerbindlichkeitsKonto!.BuchungskontoId, k => k);

            return (aufwandsMap, verbindlichkeitsMap);
        }

        private async Task<List<Buchungssatz>> LoadEaSaetzeAsync(HashSet<int> aufwandsKontoIds)
            => await ctx.Buchungssaetze
                .AsSplitQuery()
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Where(s => s.Buchungszeilen.Any(z =>
                    z.SollHaben == SollHaben.Soll &&
                    aufwandsKontoIds.Contains(z.Buchungskonto.BuchungskontoId)))
                .ToListAsync();

        private static (Wohnung? Wohnung, Kontakt? Aussteller) FindContext(
            Buchungssatz satz,
            Dictionary<int, Wohnung> aufwandsMap,
            Dictionary<int, Kontakt> verbindlichkeitsMap)
        {
            // Find the Soll-Zeile that is on an AufwandsKonto — a Buchungssatz from the
            // Abrechnung may contain multiple Soll-Zeilen (for Mieter NK-Konten + Leerstand
            // AufwandsKonto), so checking only FirstOrDefault would pick the wrong one.
            var sollKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Soll && aufwandsMap.ContainsKey(z.Buchungskonto.BuchungskontoId))
                ?.Buchungskonto.BuchungskontoId;
            var habenKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Haben)
                ?.Buchungskonto.BuchungskontoId;

            var wohnung = sollKontoId.HasValue && aufwandsMap.TryGetValue(sollKontoId.Value, out var w) ? w : null;
            var aussteller = habenKontoId.HasValue && verbindlichkeitsMap.TryGetValue(habenKontoId.Value, out var k) ? k : null;
            return (wohnung, aussteller);
        }

        public async Task<PagedResult<ErhaltungsaufwendungEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query)
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

            var (aufwandsMap, verbindlichkeitsMap) = await BuildMapsAsync();
            var aufwandsKontoIds = aufwandsMap.Keys.ToHashSet();
            var allSaetze = await LoadEaSaetzeAsync(aufwandsKontoIds);

            IEnumerable<(Buchungssatz Satz, Wohnung? Wohnung, Kontakt? Aussteller)> triples = allSaetze
                .Select(s =>
                {
                    var (wohnung, aussteller) = FindContext(s, aufwandsMap, verbindlichkeitsMap);
                    return (Satz: s, Wohnung: wohnung, Aussteller: aussteller);
                })
                .Where(t => t.Wohnung != null);

            if (userId.HasValue)
            {
                triples = triples.Where(t =>
                    t.Wohnung!.Verwalter.Any(v => v.UserAccount.Id == userId.Value));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var t = query.Search.ToLower();
                triples = triples.Where(p =>
                    p.Satz.Beschreibung.ToLower().Contains(t) ||
                    (p.Aussteller?.Name.ToLower().Contains(t) ?? false) ||
                    (p.Aussteller?.Vorname?.ToLower().Contains(t) ?? false) ||
                    (p.Wohnung?.Bezeichnung.ToLower().Contains(t) ?? false) ||
                    (p.Wohnung?.Adresse?.Stadt.ToLower().Contains(t) ?? false));
            }

            var list = triples.ToList();
            var totalCount = list.Count;

            list = (query.SortBy switch
            {
                "betrag" => query.SortDir == "asc"
                    ? list.OrderBy(p => p.Satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag))
                    : list.OrderByDescending(p => p.Satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag)),
                "bezeichnung" => query.SortDir == "asc"
                    ? list.OrderBy(p => p.Satz.Beschreibung)
                    : list.OrderByDescending(p => p.Satz.Beschreibung),
                _ => query.SortDir == "asc"
                    ? list.OrderBy(p => p.Satz.Buchungsdatum)
                    : list.OrderByDescending(p => p.Satz.Buchungsdatum)
            }).ToList();

            var items = list.Skip(query.Skip).Take(query.Take).Select(p =>
            {
                var perms = writableWohnungIds == null
                    ? adminPerms
                    : new Permissions
                    {
                        Read = true,
                        Update = false,
                        Remove = p.Wohnung != null && writableWohnungIds.Contains(p.Wohnung.WohnungId)
                    };
                return (ErhaltungsaufwendungEntryBase)new ErhaltungsaufwendungEntryBase(p.Satz, new SelectionEntry(p.Wohnung!.WohnungId, p.Wohnung.Bezeichnung), p.Aussteller, perms, p.Wohnung.AufwandsKonto.BuchungskontoId);
            });

            return new PagedResult<ErhaltungsaufwendungEntryBase>(items, totalCount);
        }

        public async Task<ActionResult<ErhaltungsaufwendungEntry>> Get(ClaimsPrincipal user, Guid id)
        {
            var satz = await ctx.Buchungssaetze
                .AsSplitQuery()
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == id);

            if (satz is null) return new NotFoundResult();

            var (aufwandsMap, verbindlichkeitsMap) = await BuildMapsAsync();
            var (wohnung, aussteller) = FindContext(satz, aufwandsMap, verbindlichkeitsMap);

            if (wohnung is null) return new NotFoundResult();

            var authRx = await auth.AuthorizeAsync(user, wohnung, [Operations.Read]);
            if (!authRx.Succeeded) return new ForbidResult();

            var permissions = await GetPermissions(user, wohnung, auth);
            permissions.Update = false;

            return new ErhaltungsaufwendungEntry(satz, wohnung, aussteller, permissions);
        }

        public async Task<ActionResult> Delete(ClaimsPrincipal user, Guid id)
        {
            var satz = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == id);

            if (satz is null) return new NotFoundResult();

            var sollKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Soll)
                ?.Buchungskonto.BuchungskontoId;
            if (!sollKontoId.HasValue) return new NotFoundResult();

            var wohnung = await ctx.Wohnungen
                .Include(w => w.Verwalter)
                .FirstOrDefaultAsync(w => w.AufwandsKonto.BuchungskontoId == sollKontoId.Value);
            if (wohnung is null) return new NotFoundResult();

            var authRx = await auth.AuthorizeAsync(user, wohnung, [Operations.Delete]);
            if (!authRx.Succeeded) return new ForbidResult();

            ctx.Buchungssaetze.Remove(satz);
            await ctx.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<ActionResult<ErhaltungsaufwendungEntry>> Post(ClaimsPrincipal user, ErhaltungsaufwendungEntry entry)
        {
            if (entry.Wohnung == null)
                return new BadRequestObjectResult("entry.Wohnung can't be null.");
            if (entry.Aussteller == null)
                return new BadRequestObjectResult("entry.Aussteller can't be null.");

            var wohnung = await ctx.Wohnungen
                .Include(w => w.AufwandsKonto)
                .Include(w => w.Adresse)
                .Include(w => w.Verwalter)
                .FirstOrDefaultAsync(w => w.WohnungId == entry.Wohnung.Id);
            if (wohnung is null)
                return new BadRequestObjectResult($"Did not find Wohnung with Id {entry.Wohnung.Id}");

            var authRx = await auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate]);
            if (!authRx.Succeeded) return new ForbidResult();

            var aussteller = await ctx.Kontakte
                .Include(k => k.VerbindlichkeitsKonto)
                .FirstOrDefaultAsync(k => k.KontaktId == entry.Aussteller.Id);
            if (aussteller is null)
                return new BadRequestObjectResult($"Did not find Kontakt with Id {entry.Aussteller.Id}");

            var satz = await buchungsService.BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, entry.Betrag, entry.Datum, entry.Bezeichnung, entry.Notiz);

            // Reload with full navigation for response
            satz = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .FirstAsync(s => s.BuchungssatzId == satz.BuchungssatzId);

            // Refresh kontakt with potentially newly created VerbindlichkeitsKonto
            aussteller = await ctx.Kontakte
                .Include(k => k.VerbindlichkeitsKonto)
                .FirstAsync(k => k.KontaktId == aussteller.KontaktId);

            var permissions = await GetPermissions(user, wohnung, auth);
            permissions.Update = false;
            return new ErhaltungsaufwendungEntry(satz, wohnung, aussteller, permissions);
        }
    }
}
