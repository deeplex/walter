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
    /// Status von Mieten, Garagen, Zählerständen, offenen Betriebskostenrechnungen und
    /// Guthaben für ein Jahr, für die Startseite. Strikt lesend: Ein Monat ohne
    /// Sollstellung gilt als fällig mit der Miete der Vertragsversion und nichts bezahlt.
    /// Angelegt werden die Sollstellungen vom <see cref="SollstellungService"/>.
    /// </summary>
    [ApiController]
    [Route("api/offene-forderungen")]
    public class OffeneForderungenController(SaverwalterContext ctx) : ControllerBase
    {
        public class MietMatrixMonat
        {
            public string Monat { get; set; } = "";
            /// <summary>Kaltmiete und Garagen des Vertrags für diesen Monat.</summary>
            public decimal Soll { get; set; }
            public decimal Gezahlt { get; set; }
            public decimal Offen { get; set; }
        }

        public class MietMatrixZeile
        {
            public int VertragId { get; set; }
            public string VertragBezeichnung { get; set; } = "";
            public string Adresse { get; set; } = "";
            public string Wohnung { get; set; } = "";
            public string Mieter { get; set; } = "";
            public int? ZahlungsempfaengerId { get; set; }
            /// <summary>Nur die fälligen Monate; künftige Monate fehlen.</summary>
            public List<MietMatrixMonat> Monate { get; set; } = [];
        }

        /// <summary>
        /// Alle fälligen Monate je Vertrag – auch die bezahlten –, damit sich
        /// erledigte und offene Monate nebeneinander darstellen lassen.
        /// </summary>
        [HttpGet("matrix/{jahr}")]
        public async Task<ActionResult<IEnumerable<MietMatrixZeile>>> GetMietMatrix(int jahr)
        {
            var monate = await ErmittleMietMonateAsync(jahr);
            var result = monate
                .GroupBy(m => m.VertragId)
                .Select(g => new MietMatrixZeile
                {
                    VertragId = g.Key,
                    VertragBezeichnung = g.First().VertragBezeichnung,
                    Adresse = g.First().Adresse,
                    Wohnung = g.First().Wohnung,
                    Mieter = g.First().Mieter,
                    ZahlungsempfaengerId = g.First().ZahlungsempfaengerId,
                    Monate = g.OrderBy(m => m.Monat)
                        .Select(m => new MietMatrixMonat
                        {
                            Monat = m.Monat,
                            Soll = m.Soll,
                            Gezahlt = m.Gezahlt,
                            Offen = m.Offen
                        })
                        .ToList()
                })
                .OrderBy(z => z.VertragBezeichnung);
            return Ok(result);
        }

        private sealed class MietMonat
        {
            public int VertragId { get; init; }
            public string VertragBezeichnung { get; init; } = "";
            public string Adresse { get; init; } = "";
            public string Wohnung { get; init; } = "";
            public string Mieter { get; init; } = "";
            public string Monat { get; init; } = "";
            public decimal Soll { get; init; }
            public decimal Gezahlt { get; init; }
            public decimal Offen { get; init; }
            public int? ZahlungsempfaengerId { get; init; }
        }

        private async Task<List<MietMonat>> ErmittleMietMonateAsync(int jahr)
        {
            var startOfYear = new DateOnly(jahr, 1, 1);
            var endOfYear = new DateOnly(jahr, 12, 31);
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Künftige Monate sind nicht fällig: ein künftiges Jahr hat keine.
            if (jahr > today.Year)
                return [];

            var vertraege = await VertragPermissionHandler.GetQueryable(ctx, User)
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

            var result = new List<MietMonat>();

            foreach (var vertrag in vertraege)
            {
                bankkontoByZahlungsKonto.TryGetValue(vertrag.ZahlungsKonto.BuchungskontoId, out var zahlungsempfaengerId);

                foreach (var monat in Sollstellung.Monate(jahr, today, vertrag.Beginn(), vertrag.Ende))
                {
                    var version = vertrag.Versionen
                        .Where(v => v.Beginn <= monat)
                        .MaxBy(v => v.Beginn);

                    if (version == null) continue;

                    var sollZeile = vertrag.MietBuchungskonto.Buchungszeilen
                        .FirstOrDefault(z => Sollstellung.IstImMonat(z, monat, vertrag.NkBuchungskonto.BuchungskontoId));

                    // Ohne Sollstellung: die Miete der Version, nichts bezahlt.
                    var gezahlt = sollZeile != null && oposBySollZeile.TryGetValue(sollZeile.BuchungszeileId, out var g) ? g : 0m;
                    var kaltmieteSoll = sollZeile?.Betrag ?? version.Grundmiete;
                    var kaltmieteOffen = kaltmieteSoll - gezahlt;

                    var soll = kaltmieteSoll;
                    var gesamtGezahlt = gezahlt;

                    // Garagen des Vertrags in diesem Monat
                    var garageOffen = 0m;
                    foreach (var gv in vertrag.GarageVertraege)
                    {
                        var gvVersion = gv.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn);
                        if (gvVersion == null) continue;

                        var gvSollZeile = gv.MietBuchungskonto.Buchungszeilen
                            .FirstOrDefault(z => Sollstellung.IstImMonat(z, monat));

                        var gvGezahlt = gvSollZeile != null && oposBySollZeile.TryGetValue(gvSollZeile.BuchungszeileId, out var gg) ? gg : 0m;
                        var gvSoll = gvSollZeile?.Betrag ?? gvVersion.GaragenMiete;
                        var gvOffen = gvSoll - gvGezahlt;

                        soll += gvSoll;
                        gesamtGezahlt += gvGezahlt;

                        if (gvOffen > 0.005m) garageOffen += gvOffen;
                    }

                    result.Add(new MietMonat
                    {
                        VertragId = vertrag.VertragId,
                        VertragBezeichnung = BuildBezeichnung(vertrag),
                        Adresse = vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift",
                        Wohnung = vertrag.Wohnung.Bezeichnung,
                        Mieter = BuildMieter(vertrag),
                        Monat = monat.ToString("yyyy-MM-dd"),
                        Soll = soll,
                        Gezahlt = gesamtGezahlt,
                        Offen = kaltmieteOffen + garageOffen,
                        ZahlungsempfaengerId = zahlungsempfaengerId == 0 ? null : zahlungsempfaengerId
                    });
                }
            }

            return result;
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

            var vertraege = await VertragPermissionHandler.GetQueryable(ctx, User)
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

        public class GarageMatrixZeile
        {
            public int GarageVertragId { get; set; }
            public string Bezeichnung { get; set; } = "";
            public string Adresse { get; set; } = "";
            public string Garage { get; set; } = "";
            public string Mieter { get; set; } = "";
            public int? ZahlungsempfaengerId { get; set; }
            /// <summary>Nur die fälligen Monate; künftige Monate fehlen.</summary>
            public List<MietMatrixMonat> Monate { get; set; } = [];
        }

        /// <summary>
        /// Alle fälligen Monate je eigenständigem Garagenvertrag – auch die bezahlten.
        /// Garagen, die zu einem Wohnungsvertrag gehören, stehen in der Mietmatrix.
        /// </summary>
        [HttpGet("matrix/garagen/{jahr}")]
        public async Task<ActionResult<IEnumerable<GarageMatrixZeile>>> GetGarageMatrix(int jahr)
        {
            var monate = await ErmittleGarageMonateAsync(jahr);
            var result = monate
                .GroupBy(m => m.GarageVertragId)
                .Select(g => new GarageMatrixZeile
                {
                    GarageVertragId = g.Key,
                    Bezeichnung = $"{g.First().Kennung} | {g.First().Mieter}",
                    Adresse = g.First().Adresse,
                    Garage = g.First().Kennung,
                    Mieter = g.First().Mieter,
                    ZahlungsempfaengerId = g.First().ZahlungsempfaengerId,
                    Monate = g.OrderBy(m => m.Monat)
                        .Select(m => new MietMatrixMonat
                        {
                            Monat = m.Monat,
                            Soll = m.Soll,
                            Gezahlt = m.Gezahlt,
                            Offen = m.Offen
                        })
                        .ToList()
                })
                .OrderBy(z => z.Adresse).ThenBy(z => z.Garage);
            return Ok(result);
        }

        private sealed class GarageMonat
        {
            public int GarageVertragId { get; init; }
            public string Kennung { get; init; } = "";
            public string Adresse { get; init; } = "";
            public string Mieter { get; init; } = "";
            public string Monat { get; init; } = "";
            public decimal Soll { get; init; }
            public decimal Gezahlt { get; init; }
            public decimal Offen { get; init; }
            public int? ZahlungsempfaengerId { get; init; }
        }

        private async Task<List<GarageMonat>> ErmittleGarageMonateAsync(int jahr)
        {
            var startOfYear = new DateOnly(jahr, 1, 1);
            var endOfYear = new DateOnly(jahr, 12, 31);
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (jahr > today.Year)
                return [];

            var garageVertraege = await GarageVertragPermissionHandler.GetQueryable(ctx, User)
                .AsSplitQuery()
                .Include(gv => gv.Versionen)
                .Include(gv => gv.Garage).ThenInclude(g => g.Ertragskonto)
                .Include(gv => gv.Garage).ThenInclude(g => g.Adresse)
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

            var result = new List<GarageMonat>();

            foreach (var gv in garageVertraege)
            {
                var mieterNamen = gv.Mieter.Count > 0
                    ? string.Join(", ", gv.Mieter.Select(m => m.Bezeichnung))
                    : "Leerstand";
                bankkontoByZahlungsKonto.TryGetValue(gv.ZahlungsKonto.BuchungskontoId, out var zahlungsempfaengerId);

                foreach (var monat in Sollstellung.Monate(jahr, today, gv.Beginn(), gv.Ende))
                {
                    var version = gv.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn);
                    if (version == null) continue;

                    var sollZeile = gv.MietBuchungskonto.Buchungszeilen
                        .FirstOrDefault(z => Sollstellung.IstImMonat(z, monat));

                    // Ohne Sollstellung: die Miete der Version, nichts bezahlt.
                    var gezahlt = sollZeile != null && oposBySollZeile.TryGetValue(sollZeile.BuchungszeileId, out var g) ? g : 0m;
                    var soll = sollZeile?.Betrag ?? version.GaragenMiete;

                    result.Add(new GarageMonat
                    {
                        GarageVertragId = gv.GarageVertragId,
                        Kennung = gv.Garage.Kennung,
                        Adresse = gv.Garage.Adresse?.Anschrift ?? "Unbekannte Anschrift",
                        Mieter = mieterNamen,
                        Monat = monat.ToString("yyyy-MM-dd"),
                        Soll = soll,
                        Gezahlt = gezahlt,
                        Offen = soll - gezahlt,
                        ZahlungsempfaengerId = zahlungsempfaengerId == 0 ? null : zahlungsempfaengerId
                    });
                }
            }

            return result;
        }

        private static string BuildBezeichnung(Vertrag vertrag)
        {
            var wohnung = $"{vertrag.Wohnung.Adresse?.Anschrift ?? "?"} – {vertrag.Wohnung.Bezeichnung}";
            return $"{wohnung} | {BuildMieter(vertrag)}";
        }

        private static string BuildMieter(Vertrag vertrag) =>
            vertrag.Mieter.Count > 0
                ? string.Join(", ", vertrag.Mieter.Select(m => m.Bezeichnung))
                : "Leerstand";

        public class ZaehlerMatrixStand
        {
            public string Datum { get; set; } = "";
            public decimal Stand { get; set; }
            public string Einheit { get; set; } = "";
        }

        public class ZaehlerMatrixZeile
        {
            public int Id { get; set; }
            public string Kennnummer { get; set; } = "";
            public string TypText { get; set; } = "";
            public string Adresse { get; set; } = "";
            public string Wohnung { get; set; } = "";
            /// <summary>Die Wohnung des Zählers; null bei einem allgemeinen Zähler der Adresse.</summary>
            public int? WohnungId { get; set; }
            public decimal? LastStand { get; set; }
            public string? LastEinheit { get; set; }
            /// <summary>Datum des letzten bekannten Standes, auch aus früheren Jahren.</summary>
            public string? LastDatum { get; set; }
            /// <summary>Die Stände im gewählten Jahr; leer, wenn noch keiner eingetragen ist.</summary>
            public List<ZaehlerMatrixStand> Staende { get; set; } = [];
        }

        /// <summary>
        /// Alle Zähler, die im Jahr noch laufen, mit ihren Ständen im Jahr – auch die,
        /// bei denen schon ein Stand eingetragen ist.
        /// </summary>
        [HttpGet("matrix/zaehler/{jahr}")]
        public async Task<ActionResult<IEnumerable<ZaehlerMatrixZeile>>> GetZaehlerMatrix(int jahr)
        {
            var zaehler = await ZaehlerPermissionHandler.GetQueryable(ctx, User)
                .Include(z => z.Staende)
                .Include(z => z.Wohnung).ThenInclude(w => w!.Adresse)
                .Include(z => z.Adresse)
                .Where(z => z.Ende == null || z.Ende.Value.Year >= jahr)
                .ToListAsync();

            return Ok(zaehler
                .Select(z =>
                {
                    var last = z.Staende.OrderBy(s => s.Datum).LastOrDefault();
                    var einheit = z.Typ.ToUnitString();
                    return new ZaehlerMatrixZeile
                    {
                        Id = z.ZaehlerId,
                        Kennnummer = z.Kennnummer,
                        TypText = z.Typ.ToString(),
                        Adresse = z.Wohnung?.Adresse?.Anschrift ?? z.Adresse?.Anschrift ?? "Ohne Adresse",
                        Wohnung = z.Wohnung?.Bezeichnung ?? "",
                        WohnungId = z.Wohnung?.WohnungId,
                        LastStand = last?.Stand,
                        LastEinheit = last != null ? einheit : null,
                        LastDatum = last?.Datum.ToString("yyyy-MM-dd"),
                        Staende = z.Staende
                            .Where(s => s.Datum.Year == jahr)
                            .OrderBy(s => s.Datum)
                            .Select(s => new ZaehlerMatrixStand
                            {
                                Datum = s.Datum.ToString("yyyy-MM-dd"),
                                Stand = s.Stand,
                                Einheit = einheit
                            })
                            .ToList()
                    };
                })
                .OrderBy(z => z.Adresse).ThenBy(z => z.Wohnung).ThenBy(z => z.Kennnummer));
        }
    }
}
