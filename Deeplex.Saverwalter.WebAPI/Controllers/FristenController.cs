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
    /// Überwacht Abrechnungsfristen nach § 556 Abs. 3 BGB.
    /// Die Frist zur Zustellung einer Betriebskostenabrechnung endet 12 Monate
    /// nach Ablauf des Abrechnungszeitraums. Bei Versäumnis verliert der
    /// Vermieter den Anspruch auf Nachzahlungen.
    /// </summary>
    [ApiController]
    [Route("api/fristen")]
    public class FristenController(SaverwalterContext ctx) : ControllerBase
    {
        public class FristWarnung
        {
            public int VertragId { get; set; }
            public string VertragBezeichnung { get; set; } = string.Empty;
            public int Abrechnungsjahr { get; set; }
            /// <summary>31.12.(Abrechnungsjahr + 1) nach § 556 Abs. 3 BGB.</summary>
            public DateOnly Fristablauf { get; set; }
            public bool IstAbgelaufen { get; set; }
        }

        /// <summary>
        /// Gibt alle Verträge zurück, für die ein Abrechnungsjahr ohne gesendetes
        /// Abrechnungsresultat existiert, zusammen mit dem jeweiligen Fristablauf.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FristWarnung>>> GetFristen()
        {
            var heute = DateOnly.FromDateTime(DateTime.Today);

            var vertraege = await VertragPermissionHandler
                .GetQueryable(ctx, User!)
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Adresse)
                .Include(v => v.Abrechnungsresultate)
                    .ThenInclude(r => r.Buchungssatz)
                .ToListAsync();

            var warnungen = new List<FristWarnung>();

            foreach (var vertrag in vertraege)
            {
                if (!vertrag.Versionen.Any()) continue;

                var beginnjahr = vertrag.Versionen.Min(v => v.Beginn).Year;
                // Letztes abzurechnende Jahr: entweder das Jahr vor Vertragsende oder letztes Kalenderjahr
                var endjahr = vertrag.Ende.HasValue
                    ? vertrag.Ende.Value.Year
                    : heute.Year - 1;

                if (endjahr < beginnjahr) continue;

                var bezeichnung = (vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift")
                    + " – " + vertrag.Wohnung.Bezeichnung;

                for (var jahr = beginnjahr; jahr <= endjahr; jahr++)
                {
                    var hatAbgesendet = vertrag.Abrechnungsresultate
                        .Any(r => r.Abgesendet && r.Buchungssatz.Buchungsjahr == jahr);

                    if (!hatAbgesendet)
                    {
                        var fristablauf = new DateOnly(jahr + 1, 12, 31);
                        warnungen.Add(new FristWarnung
                        {
                            VertragId = vertrag.VertragId,
                            VertragBezeichnung = bezeichnung,
                            Abrechnungsjahr = jahr,
                            Fristablauf = fristablauf,
                            IstAbgelaufen = fristablauf < heute,
                        });
                    }
                }
            }

            return Ok(warnungen.OrderBy(w => w.IstAbgelaufen ? 0 : 1).ThenBy(w => w.Fristablauf));
        }
    }
}
