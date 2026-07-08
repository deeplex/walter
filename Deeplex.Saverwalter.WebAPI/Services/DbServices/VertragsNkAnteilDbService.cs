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
using Deeplex.Saverwalter.WebAPI.Utils;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragsNkAnteilController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class VertragsNkAnteilDbService(
        SaverwalterContext ctx,
        IAuthorizationService auth,
        NkAnteilBuchungsService buchungsService)
    {
        private async Task<(Dictionary<int, Vertrag> NkMap, Dictionary<int, Umlage> UmlageMap)> BuildMapsAsync()
        {
            var vertraege = await ctx.Vertraege
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.Mieter)
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.Wohnung).ThenInclude(w => w.Verwalter).ThenInclude(vw => vw.UserAccount)
                .ToListAsync();
            var nkMap = vertraege.ToDictionary(v => v.NkBuchungskonto.BuchungskontoId, v => v);

            var umlagen = await ctx.Umlagen
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.Typ)
                .ToListAsync();
            var umlageMap = umlagen.ToDictionary(u => u.NkVerrechnungsKonto.BuchungskontoId, u => u);

            return (nkMap, umlageMap);
        }

        private static (Vertrag? Vertrag, Umlage? Umlage) FindContext(
            Buchungssatz satz,
            Dictionary<int, Vertrag> nkMap,
            Dictionary<int, Umlage> umlageMap)
        {
            var sollKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Soll && nkMap.ContainsKey(z.Buchungskonto.BuchungskontoId))
                ?.Buchungskonto.BuchungskontoId;
            var habenKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Haben && umlageMap.ContainsKey(z.Buchungskonto.BuchungskontoId))
                ?.Buchungskonto.BuchungskontoId;

            var vertrag = sollKontoId.HasValue && nkMap.TryGetValue(sollKontoId.Value, out var v) ? v : null;
            var umlage = habenKontoId.HasValue && umlageMap.TryGetValue(habenKontoId.Value, out var u) ? u : null;
            return (vertrag, umlage);
        }

        public async Task<List<VertragsNkAnteilEntry>> GetList(ClaimsPrincipal user, int? vertragId, int? umlageId)
        {
            var saetze = await ctx.Buchungssaetze
                .AsSplitQuery()
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Where(s => s.Beschreibung.StartsWith(NkAnteilBuchungsService.BeschreibungPrefix))
                .ToListAsync();

            var (nkMap, umlageMap) = await BuildMapsAsync();

            var entries = saetze
                .Select(s =>
                {
                    var (v, u) = FindContext(s, nkMap, umlageMap);
                    return (Satz: s, Vertrag: v, Umlage: u);
                })
                .Where(t => t.Vertrag != null && t.Umlage != null);

            if (vertragId.HasValue)
                entries = entries.Where(t => t.Vertrag!.VertragId == vertragId.Value);

            if (umlageId.HasValue)
                entries = entries.Where(t => t.Umlage!.UmlageId == umlageId.Value);

            var userId = user.IsInRole("Admin") ? (Guid?)null : user.GetUserId();
            if (userId.HasValue)
                entries = entries.Where(t =>
                    t.Vertrag!.Wohnung.Verwalter.Any(vw => vw.UserAccount.Id == userId.Value));

            return entries
                .OrderByDescending(t => t.Satz.Buchungsdatum)
                .Select(t =>
                {
                    var betrag = t.Satz.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Soll)
                        .Sum(z => z.Betrag);
                    return new VertragsNkAnteilEntry
                    {
                        Id = t.Satz.BuchungssatzId,
                        Betrag = betrag,
                        Datum = t.Satz.Buchungsdatum,
                        BetreffendesJahr = t.Satz.Buchungsjahr,
                        Notiz = t.Satz.Notiz,
                        Vertrag = new Controllers.SelectionListController.SelectionEntry(
                            t.Vertrag!.VertragId,
                            VertragLabel(t.Vertrag)),
                        Umlage = new Controllers.SelectionListController.SelectionEntry(
                            t.Umlage!.UmlageId,
                            t.Umlage.Typ.Bezeichnung),
                        Permissions = new Permissions { Read = true, Update = false, Remove = true }
                    };
                })
                .ToList();
        }

        public async Task<ActionResult<VertragsNkAnteilEntry>> Post(ClaimsPrincipal user, VertragsNkAnteilEntry entry)
        {
            if (entry.Vertrag == null)
                return new BadRequestObjectResult("entry.Vertrag kann nicht null sein.");
            if (entry.Umlage == null)
                return new BadRequestObjectResult("entry.Umlage kann nicht null sein.");
            if (entry.Betrag <= 0)
                return new BadRequestObjectResult("Betrag muss größer als 0 sein.");

            var vertrag = await ctx.Vertraege.FindAsync((int)entry.Vertrag.Id);
            if (vertrag is null)
                return new BadRequestObjectResult($"Vertrag {entry.Vertrag.Id} nicht gefunden.");

            var authRx = await auth.AuthorizeAsync(user, vertrag, [Operations.SubCreate]);
            if (!authRx.Succeeded) return new ForbidResult();

            var satz = await buchungsService.BucheVertragsNkAnteilAsync(
                (int)entry.Vertrag.Id,
                (int)entry.Umlage.Id,
                entry.Betrag,
                entry.BetreffendesJahr > 0 ? entry.BetreffendesJahr : DateTime.Today.Year - 1,
                entry.Datum == default ? DateOnly.FromDateTime(DateTime.Today) : entry.Datum,
                entry.Notiz);

            var (nkMap, umlageMap) = await BuildMapsAsync();
            var (v, u) = FindContext(satz, nkMap, umlageMap);

            return new VertragsNkAnteilEntry
            {
                Id = satz.BuchungssatzId,
                Betrag = entry.Betrag,
                Datum = satz.Buchungsdatum,
                BetreffendesJahr = satz.Buchungsjahr,
                Notiz = satz.Notiz,
                Vertrag = new Controllers.SelectionListController.SelectionEntry(
                    v!.VertragId, VertragLabel(v)),
                Umlage = new Controllers.SelectionListController.SelectionEntry(
                    u!.UmlageId, u.Typ.Bezeichnung),
                Permissions = new Permissions { Read = true, Update = false, Remove = true }
            };
        }

        public async Task<ActionResult> Delete(ClaimsPrincipal user, Guid id)
        {
            var satz = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == id);

            if (satz is null || !satz.Beschreibung.StartsWith(NkAnteilBuchungsService.BeschreibungPrefix))
                return new NotFoundResult();

            var nkMap = (await ctx.Vertraege
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.Wohnung).ThenInclude(w => w.Verwalter)
                .ToListAsync())
                .ToDictionary(v => v.NkBuchungskonto.BuchungskontoId, v => v);

            var sollKontoId = satz.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Soll && nkMap.ContainsKey(z.Buchungskonto.BuchungskontoId))
                ?.Buchungskonto.BuchungskontoId;

            if (!sollKontoId.HasValue || !nkMap.TryGetValue(sollKontoId.Value, out var vertrag))
                return new NotFoundResult();

            var authRx = await auth.AuthorizeAsync(user, vertrag, [Operations.Delete]);
            if (!authRx.Succeeded) return new ForbidResult();

            ctx.Buchungssaetze.Remove(satz);
            await ctx.SaveChangesAsync();
            return new OkResult();
        }

        private static string VertragLabel(Vertrag v)
        {
            var anschrift = v.Wohnung.Adresse is Adresse a ? a.Anschrift : "?";
            var mieter = v.Mieter.Count > 0
                ? string.Join(", ", v.Mieter.Select(m => m.Bezeichnung))
                : "Leerstand";
            return $"{anschrift} – {v.Wohnung.Bezeichnung} ({mieter})";
        }
    }
}
