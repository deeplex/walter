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

using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    /// <summary>
    /// Gibt die offenen Forderungen für ein Jahr zurück. Strikt lesend: Ein Monat ohne
    /// Sollstellung gilt als fällig mit der Miete der Vertragsversion und nichts bezahlt.
    /// Angelegt werden die Sollstellungen vom <see cref="SollstellungService"/>.
    /// </summary>
    [ApiController]
    [Route("api/offene-forderungen")]
    public class OffeneForderungenController(SaverwalterContext ctx) : ControllerBase
    {
        public class OffeneGarageInfo
        {
            public int GarageVertragId { get; set; }
            public string GarageKennung { get; set; } = "";
            public decimal Offen { get; set; }
        }

        public class OffeneForderungEntry
        {
            public int VertragId { get; set; }
            public string VertragBezeichnung { get; set; } = "";
            public string Monat { get; set; } = "";
            public decimal KaltmieteOffen { get; set; }
            public decimal Offen { get; set; }
            public int? ZahlungsempfaengerId { get; set; }
            public List<OffeneGarageInfo> Garagen { get; set; } = [];
        }

        [HttpGet("{jahr}")]
        public Task<ActionResult<IEnumerable<OffeneForderungEntry>>> GetOffeneForderungen(int jahr) =>
            ErmittleOffeneForderungenAsync(jahr);

        private async Task<ActionResult<IEnumerable<OffeneForderungEntry>>> ErmittleOffeneForderungenAsync(int jahr)
        {
            var startOfYear = new DateOnly(jahr, 1, 1);
            var endOfYear = new DateOnly(jahr, 12, 31);
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Künftige Monate sind nicht fällig: ein künftiges Jahr hat keine.
            if (jahr > today.Year)
                return Ok(Array.Empty<OffeneForderungEntry>());

            var vertraege = await ctx.Vertraege
                .AsSplitQuery()
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.Wohnung).ThenInclude(w => w.MietErtragskonto)
                .Include(v => v.Mieter)
                .Include(v => v.ZahlungsKonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz)
                            .ThenInclude(s => s.Buchungszeilen).ThenInclude(g => g.Buchungskonto)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.Transaktion)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.StornoVon)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.StornoNach)
                .Include(v => v.GarageVertraege).ThenInclude(gv => gv.Versionen)
                .Include(v => v.GarageVertraege).ThenInclude(gv => gv.Garage).ThenInclude(g => g.Ertragskonto)
                .Include(v => v.GarageVertraege)
                    .ThenInclude(gv => gv.MietBuchungskonto)
                        .ThenInclude(k => k.Buchungszeilen)
                            .ThenInclude(z => z.Buchungssatz)
                .Where(v => v.Versionen.Any(ver => ver.Beginn <= endOfYear)
                         && (v.Ende == null || v.Ende >= startOfYear))
                .ToListAsync();

            var zahlungsKontoIds = vertraege
                .Select(v => v.ZahlungsKonto.BuchungskontoId)
                .ToHashSet();
            var bankkontoByZahlungsKonto = (await ctx.Bankkontos
                .Include(b => b.BuchungsKonto)
                .Where(b => zahlungsKontoIds.Contains(b.BuchungsKonto.BuchungskontoId))
                .ToListAsync())
                .GroupBy(b => b.BuchungsKonto.BuchungskontoId)
                .ToDictionary(g => g.Key, g => g.First().BankkontoId);

            // Collect Soll-Zeile IDs from loaded MietBuchungskonten for separate OPOS query
            var sollZeileIds = vertraege
                .SelectMany(v => v.MietBuchungskonto?.Buchungszeilen ?? [])
                .Where(z => z.SollHaben == SollHaben.Soll)
                .Select(z => z.BuchungszeileId)
                .ToHashSet();

            var garageSollZeileIds = vertraege
                .SelectMany(v => v.GarageVertraege)
                .SelectMany(gv => gv.MietBuchungskonto?.Buchungszeilen ?? [])
                .Where(z => z.SollHaben == SollHaben.Soll)
                .Select(z => z.BuchungszeileId)
                .ToHashSet();

            // Load OPOS separately to avoid AsSplitQuery issues with dual ThenInclude chains
            var allSollZeileIds = sollZeileIds.Concat(garageSollZeileIds).ToHashSet();
            var oposBySollZeile = (await ctx.OffenePostenAusgleiche
                .Include(o => o.HabenZeile)
                .Include(o => o.SollZeile)
                .Where(o => allSollZeileIds.Contains(o.SollZeile.BuchungszeileId))
                .ToListAsync())
                .GroupBy(o => o.SollZeile.BuchungszeileId)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.HabenZeile.Betrag));

            var result = new List<OffeneForderungEntry>();

            foreach (var vertrag in vertraege)
            {
                foreach (var monat in Sollstellung.Monate(jahr, today, vertrag.Beginn(), vertrag.Ende))
                {
                    var version = vertrag.Versionen
                        .Where(v => v.Beginn <= monat)
                        .MaxBy(v => v.Beginn);

                    if (version != null)
                    {
                        var sollZeile = vertrag.MietBuchungskonto.Buchungszeilen
                            .FirstOrDefault(z => Sollstellung.IstImMonat(z, monat, vertrag.NkBuchungskonto.BuchungskontoId));

                        // Ohne Sollstellung: die Miete der Version, nichts bezahlt.
                        var gezahlt = sollZeile != null && oposBySollZeile.TryGetValue(sollZeile.BuchungszeileId, out var g) ? g : 0m;
                        var kaltmieteOffen = (sollZeile?.Betrag ?? version.Grundmiete) - gezahlt;

                        // Collect open garage forderungen for this month
                        var offeneGaragen = new List<OffeneGarageInfo>();
                        foreach (var gv in vertrag.GarageVertraege)
                        {
                            var gvVersion = gv.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn);
                            if (gvVersion == null) continue;

                            var gvSollZeile = gv.MietBuchungskonto.Buchungszeilen
                                .FirstOrDefault(z => Sollstellung.IstImMonat(z, monat));

                            var gvGezahlt = gvSollZeile != null && oposBySollZeile.TryGetValue(gvSollZeile.BuchungszeileId, out var gg) ? gg : 0m;
                            var gvOffen = (gvSollZeile?.Betrag ?? gvVersion.GaragenMiete) - gvGezahlt;

                            if (gvOffen > 0.005m)
                                offeneGaragen.Add(new OffeneGarageInfo
                                {
                                    GarageVertragId = gv.GarageVertragId,
                                    GarageKennung = gv.Garage.Kennung,
                                    Offen = gvOffen
                                });
                        }

                        var totalOffen = kaltmieteOffen + offeneGaragen.Sum(og => og.Offen);

                        if (totalOffen > 0.005m)
                        {
                            bankkontoByZahlungsKonto.TryGetValue(vertrag.ZahlungsKonto.BuchungskontoId, out var zahlungsempfaengerId);
                            result.Add(new OffeneForderungEntry
                            {
                                VertragId = vertrag.VertragId,
                                VertragBezeichnung = BuildBezeichnung(vertrag),
                                Monat = monat.ToString("yyyy-MM-dd"),
                                KaltmieteOffen = kaltmieteOffen,
                                Offen = totalOffen,
                                ZahlungsempfaengerId = zahlungsempfaengerId == 0 ? null : zahlungsempfaengerId,
                                Garagen = offeneGaragen
                            });
                        }
                    }
                }
            }

            return Ok(result.OrderBy(r => r.Monat).ThenBy(r => r.VertragBezeichnung));
        }

        public class GuthabenEntry
        {
            public int VertragId { get; set; }
            public string VertragBezeichnung { get; set; } = "";
            public string Monat { get; set; } = "";
            public decimal Guthaben { get; set; }
        }

        /// <summary>
        /// Gibt alle Monate zurück, in denen ein Mieter mehr gezahlt hat als gefordert wurde
        /// (negativer OPOS-Saldo auf MietBuchungskonto). Erstellt keine neuen Sollstellungen.
        /// </summary>
        [HttpGet("guthaben/{jahr}")]
        public async Task<ActionResult<IEnumerable<GuthabenEntry>>> GetGuthaben(int jahr)
        {
            var startOfYear = new DateOnly(jahr, 1, 1);
            var endOfYear = new DateOnly(jahr, 12, 31);

            var vertraege = await ctx.Vertraege
                .AsSplitQuery()
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz)
                            .ThenInclude(s => s.Buchungszeilen).ThenInclude(g => g.Buchungskonto)
                .Where(v => v.Versionen.Any(ver => ver.Beginn <= endOfYear)
                         && (v.Ende == null || v.Ende >= startOfYear))
                .ToListAsync();

            var sollZeileIds = vertraege
                .SelectMany(v => v.MietBuchungskonto?.Buchungszeilen ?? [])
                .Where(z => z.SollHaben == SollHaben.Soll
                         && z.Buchungssatz.Buchungsjahr == jahr)
                .Select(z => z.BuchungszeileId)
                .ToHashSet();

            var oposBySollZeile = (await ctx.OffenePostenAusgleiche
                .Include(o => o.HabenZeile)
                .Include(o => o.SollZeile)
                .Where(o => sollZeileIds.Contains(o.SollZeile.BuchungszeileId))
                .ToListAsync())
                .GroupBy(o => o.SollZeile.BuchungszeileId)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.HabenZeile.Betrag));

            var result = new List<GuthabenEntry>();

            foreach (var vertrag in vertraege)
            {
                var sollZeilenImJahr = vertrag.MietBuchungskonto.Buchungszeilen
                    .Where(z => z.Buchungssatz.Buchungsjahr == jahr
                        && Sollstellung.Ist(z, vertrag.NkBuchungskonto.BuchungskontoId))
                    .ToList();

                foreach (var sollZeile in sollZeilenImJahr)
                {
                    var gezahlt = oposBySollZeile.TryGetValue(sollZeile.BuchungszeileId, out var g) ? g : 0m;
                    var offen = sollZeile.Betrag - gezahlt;

                    if (offen < -0.005m)
                    {
                        result.Add(new GuthabenEntry
                        {
                            VertragId = vertrag.VertragId,
                            VertragBezeichnung = BuildBezeichnung(vertrag),
                            Monat = new DateOnly(sollZeile.Buchungssatz.Buchungsdatum.Year, sollZeile.Buchungssatz.Buchungsdatum.Month, 1).ToString("yyyy-MM-dd"),
                            Guthaben = -offen
                        });
                    }
                }
            }

            return Ok(result.OrderBy(r => r.Monat).ThenBy(r => r.VertragBezeichnung));
        }

        public class OffeneBkForderungEntry
        {
            public Guid BuchungssatzId { get; set; }
            public string Bezeichnung { get; set; } = "";
            public decimal Betrag { get; set; }
            public decimal SchonGezahlt { get; set; }
            public decimal Verbleibend { get; set; }
        }

        [HttpGet("bk/{jahr}")]
        public async Task<ActionResult<IEnumerable<OffeneBkForderungEntry>>> GetOffeneBkForderungen(int jahr)
        {
            var umlagen = await ctx.Umlagen
                .AsSplitQuery()
                .Include(u => u.Typ)
                .Include(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Haben))
                    .ThenInclude(z => z.Buchungssatz)
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Haben))
                    .ThenInclude(z => z.AlsHabenZeile)
                    .ThenInclude(a => a.SollZeile)
                .Where(u => u.NkVerrechnungsKonto.Buchungszeilen.Any(z =>
                    z.SollHaben == SollHaben.Haben
                    && (z.Buchungssatz.Buchungsjahr == jahr)))
                .ToListAsync();

            var result = umlagen
                .SelectMany(u => u.NkVerrechnungsKonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben
                             && (z.Buchungssatz.Buchungsjahr == jahr))
                    .Select(z =>
                    {
                        var schonGezahlt = z.AlsHabenZeile.Sum(a => a.SollZeile.Betrag);
                        var verbleibend = z.Betrag - schonGezahlt;
                        return new { z, u, schonGezahlt, verbleibend };
                    }))
                .Where(x => x.verbleibend > 0.005m)
                .Select(x => new OffeneBkForderungEntry
                {
                    BuchungssatzId = x.z.Buchungssatz.BuchungssatzId,
                    Bezeichnung = $"{x.u.Typ.Bezeichnung} – {x.u.GetWohnungenBezeichnung()}",
                    Betrag = x.z.Betrag,
                    SchonGezahlt = x.schonGezahlt,
                    Verbleibend = x.verbleibend
                })
                .OrderBy(e => e.Bezeichnung);

            return Ok(result);
        }

        public class OffeneGarageForderungEntry
        {
            public int GarageVertragId { get; set; }
            public string Bezeichnung { get; set; } = "";
            public string Monat { get; set; } = "";
            public decimal Offen { get; set; }
            public int? ZahlungsempfaengerId { get; set; }
        }

        [HttpGet("garagen/{jahr}")]
        public Task<ActionResult<IEnumerable<OffeneGarageForderungEntry>>> GetOffeneGarageForderungen(int jahr) =>
            ErmittleOffeneGarageForderungenAsync(jahr);

        private async Task<ActionResult<IEnumerable<OffeneGarageForderungEntry>>> ErmittleOffeneGarageForderungenAsync(int jahr)
        {
            var startOfYear = new DateOnly(jahr, 1, 1);
            var endOfYear = new DateOnly(jahr, 12, 31);
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (jahr > today.Year)
                return Ok(Array.Empty<OffeneGarageForderungEntry>());

            var garageVertraege = await ctx.GarageVertraege
                .AsSplitQuery()
                .Include(gv => gv.Versionen)
                .Include(gv => gv.Garage).ThenInclude(g => g.Ertragskonto)
                .Include(gv => gv.Mieter)
                .Include(gv => gv.ZahlungsKonto)
                .Include(gv => gv.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen)
                        .ThenInclude(z => z.Buchungssatz)
                .Where(gv => gv.Vertrag == null
                    && gv.Versionen.Any(v => v.Beginn <= endOfYear)
                    && (gv.Ende == null || gv.Ende >= startOfYear))
                .ToListAsync();

            var garageSollZeileIds = garageVertraege
                .SelectMany(gv => gv.MietBuchungskonto?.Buchungszeilen ?? [])
                .Where(z => z.SollHaben == SollHaben.Soll)
                .Select(z => z.BuchungszeileId)
                .ToHashSet();

            var oposBySollZeile = (await ctx.OffenePostenAusgleiche
                .Include(o => o.HabenZeile)
                .Include(o => o.SollZeile)
                .Where(o => garageSollZeileIds.Contains(o.SollZeile.BuchungszeileId))
                .ToListAsync())
                .GroupBy(o => o.SollZeile.BuchungszeileId)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.HabenZeile.Betrag));

            var zahlungsKontoIds = garageVertraege
                .Select(gv => gv.ZahlungsKonto.BuchungskontoId)
                .ToHashSet();
            var bankkontoByZahlungsKonto = (await ctx.Bankkontos
                .Include(b => b.BuchungsKonto)
                .Where(b => zahlungsKontoIds.Contains(b.BuchungsKonto.BuchungskontoId))
                .ToListAsync())
                .GroupBy(b => b.BuchungsKonto.BuchungskontoId)
                .ToDictionary(g => g.Key, g => g.First().BankkontoId);

            var result = new List<OffeneGarageForderungEntry>();

            foreach (var gv in garageVertraege)
            {
                foreach (var monat in Sollstellung.Monate(jahr, today, gv.Beginn(), gv.Ende))
                {
                    var version = gv.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn);
                    if (version != null)
                    {
                        var sollZeile = gv.MietBuchungskonto.Buchungszeilen
                            .FirstOrDefault(z => Sollstellung.IstImMonat(z, monat));

                        // Ohne Sollstellung: die Miete der Version, nichts bezahlt.
                        var gezahlt = sollZeile != null && oposBySollZeile.TryGetValue(sollZeile.BuchungszeileId, out var g) ? g : 0m;
                        var offen = (sollZeile?.Betrag ?? version.GaragenMiete) - gezahlt;

                        if (offen > 0.005m)
                        {
                            var mieterNamen = gv.Mieter.Count > 0
                                ? string.Join(", ", gv.Mieter.Select(m => m.Bezeichnung))
                                : "Leerstand";
                            bankkontoByZahlungsKonto.TryGetValue(gv.ZahlungsKonto.BuchungskontoId, out var zahlungsempfaengerId);
                            result.Add(new OffeneGarageForderungEntry
                            {
                                GarageVertragId = gv.GarageVertragId,
                                Bezeichnung = $"{gv.Garage.Kennung} | {mieterNamen}",
                                Monat = monat.ToString("yyyy-MM-dd"),
                                Offen = offen,
                                ZahlungsempfaengerId = zahlungsempfaengerId == 0 ? null : zahlungsempfaengerId
                            });
                        }
                    }
                }
            }

            return Ok(result.OrderBy(r => r.Monat).ThenBy(r => r.Bezeichnung));
        }

        private static string BuildBezeichnung(Vertrag vertrag)
        {
            var wohnung = $"{vertrag.Wohnung.Adresse?.Anschrift ?? "?"} – {vertrag.Wohnung.Bezeichnung}";
            var mieter = vertrag.Mieter.Count > 0
                ? string.Join(", ", vertrag.Mieter.Select(m => m.Bezeichnung))
                : "Leerstand";
            return $"{wohnung} | {mieter}";
        }

        public class ZaehlerstandFaelligEntry
        {
            public int Id { get; set; }
            public string Kennnummer { get; set; } = "";
            public string TypText { get; set; } = "";
            public decimal? LastStand { get; set; }
            public string? LastEinheit { get; set; }
        }

        [HttpGet("zaehlerstaende-faellig/{jahr}")]
        public async Task<ActionResult<IEnumerable<ZaehlerstandFaelligEntry>>> GetZaehlerstaendeFaellig(int jahr)
        {
            var zaehler = await ZaehlerPermissionHandler.GetQueryable(ctx, User)
                .Include(z => z.Staende)
                .Where(z => z.Ende == null || z.Ende.Value.Year >= jahr)
                .Where(z => !z.Staende.Any(s => s.Datum.Year == jahr))
                .ToListAsync();

            return Ok(zaehler
                .OrderBy(z => z.Kennnummer)
                .Select(z =>
                {
                    var last = z.Staende.OrderBy(s => s.Datum).LastOrDefault();
                    return new ZaehlerstandFaelligEntry
                    {
                        Id = z.ZaehlerId,
                        Kennnummer = z.Kennnummer,
                        TypText = z.Typ.ToString(),
                        LastStand = last?.Stand,
                        LastEinheit = last != null ? z.Typ.ToUnitString() : null
                    };
                }));
        }
    }
}
