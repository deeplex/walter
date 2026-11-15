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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    /// <summary>
    /// Abrechnungsstand je Vertrag und Jahr: welche Abrechnungen gesendet sind, für welche
    /// Wohnungen im Jahr eine Abrechnung nötig ist und bis wann abgerechnet werden soll
    /// (Fristablauf ist der 31.12. des Folgejahres).
    /// </summary>
    [ApiController]
    [Route("api/fristen")]
    public class FristenController(SaverwalterContext ctx) : ControllerBase
    {
        public class AbrechnungsMatrixJahr
        {
            public int Jahr { get; set; }
            /// <summary>Für das Jahr liegt eine gesendete, nicht stornierte Abrechnung vor.</summary>
            public bool Gesendet { get; set; }
            public DateOnly Fristablauf { get; set; }
            public bool IstAbgelaufen { get; set; }
        }

        public class AbrechnungsMatrixZeile
        {
            public int VertragId { get; set; }
            public string VertragBezeichnung { get; set; } = string.Empty;
            public string Adresse { get; set; } = string.Empty;
            public string Wohnung { get; set; } = string.Empty;
            public string Mieter { get; set; } = string.Empty;
            /// <summary>
            /// Alle Abrechnungsjahre des Vertrags, auch die schon gesendeten. Jahre, in denen
            /// weder Miete noch NK-Vorauszahlung vereinbart war oder ein Abrechnungsverzicht
            /// hinterlegt ist, fehlen: dort gibt es nichts abzurechnen.
            /// </summary>
            public List<AbrechnungsMatrixJahr> Jahre { get; set; } = [];
        }

        /// <summary>
        /// Je Vertrag alle Abrechnungsjahre mit dem Stand „gesendet oder nicht“, damit sich
        /// erledigte und offene Jahre nebeneinander darstellen lassen.
        /// </summary>
        [HttpGet("matrix")]
        public async Task<ActionResult<IEnumerable<AbrechnungsMatrixZeile>>> GetMatrix()
        {
            var heute = DateOnly.FromDateTime(DateTime.Today);

            var vertraege = await VertragPermissionHandler
                .GetQueryable(ctx, User!)
                .Include(v => v.Versionen)
                .Include(v => v.Mieter)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Adresse)
                .Include(v => v.Abrechnungsresultate)
                    .ThenInclude(r => r.Buchungssatz)
                .ToListAsync();

            var verzichte = (await ctx.Abrechnungsverzichte
                .Select(v => new { v.Vertrag.VertragId, v.Jahr })
                .ToListAsync())
                .Select(v => (v.VertragId, v.Jahr))
                .ToHashSet();

            var zeilen = new List<AbrechnungsMatrixZeile>();

            foreach (var vertrag in vertraege)
            {
                if (!vertrag.Versionen.Any()) continue;

                var beginnjahr = vertrag.Versionen.Min(v => v.Beginn).Year;
                var endjahr = vertrag.Ende.HasValue
                    ? vertrag.Ende.Value.Year
                    : heute.Year - 1;

                if (endjahr < beginnjahr) continue;

                var anschrift = vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                var mieter = vertrag.Mieter.Count > 0
                    ? string.Join(", ", vertrag.Mieter.Select(m => m.Bezeichnung))
                    : "Leerstand";

                var zeile = new AbrechnungsMatrixZeile
                {
                    VertragId = vertrag.VertragId,
                    VertragBezeichnung = $"{anschrift} – {vertrag.Wohnung.Bezeichnung} | {mieter}",
                    Adresse = anschrift,
                    Wohnung = vertrag.Wohnung.Bezeichnung,
                    Mieter = mieter,
                };

                var versionen = vertrag.Versionen.OrderBy(v => v.Beginn).ToList();

                for (var jahr = beginnjahr; jahr <= endjahr; jahr++)
                {
                    if (!ZahltImJahr(versionen, jahr)) continue;
                    if (verzichte.Contains((vertrag.VertragId, jahr))) continue;

                    var fristablauf = new DateOnly(jahr + 1, 12, 31);
                    zeile.Jahre.Add(new AbrechnungsMatrixJahr
                    {
                        Jahr = jahr,
                        Gesendet = vertrag.Abrechnungsresultate
                            .Any(r => r.Abgesendet && r.Buchungssatz.Buchungsjahr == jahr && r.Buchungssatz.StornoNach == null),
                        Fristablauf = fristablauf,
                        IstAbgelaufen = fristablauf < heute,
                    });
                }

                if (zeile.Jahre.Count > 0) zeilen.Add(zeile);
            }

            return Ok(zeilen.OrderBy(z => z.Adresse).ThenBy(z => z.Wohnung).ThenBy(z => z.Mieter));
        }

        public class WohnungMitAbrechnung
        {
            public int WohnungId { get; set; }
            /// <summary>
            /// Alle nötigen Abrechnungen der Wohnung sind für das Jahr gesendet; Rechnungen und
            /// Zählerstände lassen sich dann nicht mehr ergänzen und gelten als erledigt.
            /// </summary>
            public bool Abgeschlossen { get; set; }
        }

        /// <summary>
        /// Die Wohnungen, für die im Jahr eine Abrechnung nötig ist: mindestens ein Vertrag
        /// läuft im Jahr, hat dort Miete oder NK-Vorauszahlung und keinen Abrechnungsverzicht.
        /// Für alle anderen Wohnungen sind Rechnungen und Zählerstände des Jahres nicht nötig.
        /// Dazu, ob die Abrechnungen der Wohnung schon alle gesendet sind.
        /// </summary>
        [HttpGet("benoetigt/{jahr}")]
        public async Task<ActionResult<IEnumerable<WohnungMitAbrechnung>>> GetWohnungenMitAbrechnung(int jahr)
        {
            var vertraege = await VertragPermissionHandler
                .GetQueryable(ctx, User!)
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung)
                .Include(v => v.Abrechnungsresultate)
                    .ThenInclude(r => r.Buchungssatz)
                .ToListAsync();

            var verzichte = (await ctx.Abrechnungsverzichte
                .Where(v => v.Jahr == jahr)
                .Select(v => v.Vertrag.VertragId)
                .ToListAsync())
                .ToHashSet();

            var noetig = vertraege
                .Where(v => v.Versionen.Any())
                .Where(v => v.Versionen.Min(x => x.Beginn).Year <= jahr
                    && (!v.Ende.HasValue || v.Ende.Value.Year >= jahr))
                .Where(v => ZahltImJahr(v.Versionen.OrderBy(x => x.Beginn).ToList(), jahr))
                .Where(v => !verzichte.Contains(v.VertragId));

            var result = noetig
                .GroupBy(v => v.Wohnung.WohnungId)
                .Select(g => new WohnungMitAbrechnung
                {
                    WohnungId = g.Key,
                    Abgeschlossen = g.All(v => v.Abrechnungsresultate
                        .Any(r => r.Abgesendet && r.Buchungssatz.Buchungsjahr == jahr && r.Buchungssatz.StornoNach == null))
                })
                .OrderBy(w => w.WohnungId);

            return Ok(result);
        }

        /// <summary>
        /// Gilt im Jahr eine Version, in der Miete oder NK-Vorauszahlung vereinbart ist?
        /// Eine Version gilt von ihrem Beginn bis zum Tag vor der nächsten.
        /// </summary>
        private static bool ZahltImJahr(IReadOnlyList<VertragVersion> versionen, int jahr)
        {
            var anfang = new DateOnly(jahr, 1, 1);
            var ende = new DateOnly(jahr, 12, 31);
            for (var i = 0; i < versionen.Count; i++)
            {
                var von = versionen[i].Beginn;
                var bis = i + 1 < versionen.Count ? versionen[i + 1].Beginn.AddDays(-1) : DateOnly.MaxValue;
                if (von <= ende && bis >= anfang
                    && (versionen[i].Grundmiete > 0 || versionen[i].Nebenkostenvorauszahlung > 0))
                    return true;
            }
            return false;
        }
    }
}
