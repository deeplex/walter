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
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mietzahlungen")]
    public class MietzahlungController(SaverwalterContext ctx, IAuthorizationService auth) : ControllerBase
    {
        public class OffenerPostenStatus
        {
            public decimal Rechnungsbetrag { get; set; }
            public decimal SchonGezahlt { get; set; }
            public decimal VerbleibenderBetrag { get; set; }
        }

        /// <summary>Eintrag in der Zahlungsliste eines Vertrags.</summary>
        public class MietzahlungListEntry
        {
            public Guid Id { get; set; }
            public DateOnly Buchungsdatum { get; set; }
            public string BetreffenderMonat { get; set; } = string.Empty;
            public decimal KaltmieteZahlung { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public MietzahlungListEntry() { }
            public MietzahlungListEntry(Buchungssatz satz, decimal betrag, Permissions permissions)
            {
                Id = satz.BuchungssatzId;
                Buchungsdatum = satz.Buchungsdatum;
                BetreffenderMonat = new DateOnly(satz.Buchungsdatum.Year, satz.Buchungsdatum.Month, 1).ToString("yyyy-MM-01");
                KaltmieteZahlung = betrag;
                Permissions = permissions;
            }
        }

        /// <summary>
        /// Forderungsstatus für einen Monat — dient der UI zur Vorauswahl der Zahlungsbeträge.
        /// </summary>
        public class GarageForderungsstatusEntry
        {
            public int GarageVertragId { get; set; }
            public string GarageKennung { get; set; } = string.Empty;
            public decimal GaragenMiete { get; set; }
            public decimal Forderungsbetrag { get; set; }
            public decimal SchonGezahlt { get; set; }
            public decimal VerbleibendeForderung { get; set; }
            public bool SollstellungVorhanden { get; set; }
        }

        public class ForderungsstatusEntry
        {
            public DateOnly Monat { get; set; }
            public decimal Forderungsbetrag { get; set; }
            public decimal SchonGezahlt { get; set; }
            public decimal VerbleibendeForderung { get; set; }
            public decimal NkVorauszahlung { get; set; }
            public bool SollstellungVorhanden { get; set; }
            public decimal Grundmiete { get; set; }
            public DateOnly? GrundmieteSeit { get; set; }
            public List<GarageForderungsstatusEntry> Garagen { get; set; } = [];
        }

        /// <summary>
        /// Listet alle Kaltmiete-Zahlungs-Buchungssätze eines Vertrags.
        /// </summary>
        [HttpGet("{vertragId}")]
        public async Task<ActionResult<IEnumerable<MietzahlungListEntry>>> GetByVertrag(int vertragId)
        {
            var vertrag = await ctx.Vertraege.FindAsync(vertragId);
            if (vertrag is null) return NotFound();

            var authRx = await auth.AuthorizeAsync(User, vertrag, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var permissions = await GetPermissions(User, vertrag, auth);
            var kontoId = vertrag.MietBuchungskonto.BuchungskontoId;

            var saetze = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Where(s => s.Buchungszeilen.Any(z =>
                    z.SollHaben == SollHaben.Haben
                    && z.Buchungskonto.BuchungskontoId == kontoId))
                .ToListAsync();

            var result = saetze.Select(s =>
            {
                var betrag = s.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben && z.Buchungskonto.BuchungskontoId == kontoId)
                    .Sum(z => z.Betrag);
                return new MietzahlungListEntry(s, betrag, permissions);
            });

            return Ok(result);
        }

        public class BkForderungInfo
        {
            public int BetriebskostenrechnungId { get; set; }
            public decimal Betrag { get; set; }
            public DateOnly Datum { get; set; }
            public decimal SchonGezahlt { get; set; }
            public decimal Verbleibend { get; set; }
        }

        /// <summary>
        /// Gibt existierende Betriebskostenrechnungen für eine Umlage in einem Jahr zurück.
        /// </summary>
        [HttpGet("bk/check-forderung")]
        public async Task<ActionResult<IEnumerable<BkForderungInfo>>> CheckForderung([FromQuery] int umlageId, [FromQuery] int year)
        {
            var umlage = await ctx.Umlagen.FindAsync(umlageId);
            if (umlage is null) return NotFound();
            var authRx = await auth.AuthorizeAsync(User, umlage, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var rechnungen = await ctx.Betriebskostenrechnungen
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile)
                .Where(r => r.Umlage.UmlageId == umlageId && r.BetreffendesJahr == year)
                .ToListAsync();

            return Ok(rechnungen.Select(r =>
            {
                var habenZeile = r.Buchungssatz?.Buchungszeilen.FirstOrDefault(z => z.SollHaben == SollHaben.Haben);
                var schonGezahlt = habenZeile?.AlsHabenZeile.Sum(a => a.SollZeile.Betrag) ?? 0m;
                return new BkForderungInfo
                {
                    BetriebskostenrechnungId = r.BetriebskostenrechnungId,
                    Betrag = r.Betrag,
                    Datum = r.Datum,
                    SchonGezahlt = schonGezahlt,
                    Verbleibend = r.Betrag - schonGezahlt
                };
            }));
        }

        /// <summary>
        /// Offener Posten einer Betriebskostenrechnung — Rechnungsbetrag minus bereits gebuchte Zahlungen.
        /// </summary>
        [HttpGet("bk/{betriebskostenrechnungId}/offenerposten")]
        public async Task<ActionResult<OffenerPostenStatus>> GetBkOffenerPosten(int betriebskostenrechnungId)
        {
            var rechnung = await ctx.Betriebskostenrechnungen
                .Include(b => b.Umlage)
                .Include(b => b.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile)
                .FirstOrDefaultAsync(b => b.BetriebskostenrechnungId == betriebskostenrechnungId);

            if (rechnung is null) return NotFound();

            var authRx = await auth.AuthorizeAsync(User, rechnung.Umlage, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var forderungsHabenZeile = rechnung.Buchungssatz?.Buchungszeilen
                .FirstOrDefault(z => z.SollHaben == SollHaben.Haben);

            var schonGezahlt = forderungsHabenZeile?.AlsHabenZeile
                .Sum(a => a.SollZeile.Betrag) ?? 0m;

            return Ok(new OffenerPostenStatus
            {
                Rechnungsbetrag = rechnung.Betrag,
                SchonGezahlt = schonGezahlt,
                VerbleibenderBetrag = rechnung.Betrag - schonGezahlt
            });
        }

        /// <summary>
        /// Info zu einer Erhaltungsaufwendung — Rechnungsbetrag für die UI-Vorauswahl.
        /// </summary>
        [HttpGet("ea/{erhaltungsaufwendungId}/info")]
        public async Task<ActionResult<OffenerPostenStatus>> GetEaInfo(int erhaltungsaufwendungId)
        {
            var ea = await ctx.Erhaltungsaufwendungen
                .Include(e => e.Wohnung)
                .FirstOrDefaultAsync(e => e.ErhaltungsaufwendungId == erhaltungsaufwendungId);

            if (ea is null) return NotFound();

            var authRx = await auth.AuthorizeAsync(User, ea.Wohnung, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            return Ok(new OffenerPostenStatus
            {
                Rechnungsbetrag = ea.Betrag,
                SchonGezahlt = 0,
                VerbleibenderBetrag = ea.Betrag
            });
        }

        /// <summary>
        /// Gibt den Forderungsstatus für einen Monat zurück.
        /// Dient der UI zur Vorauswahl von Kaltmiete- und NK-Betrag.
        /// </summary>
        [HttpGet("{vertragId}/forderung/{monat}")]
        public async Task<ActionResult<ForderungsstatusEntry>> GetForderungsstatus(int vertragId, DateOnly monat)
        {
            var vertrag = await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.MietBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.GarageVertraege).ThenInclude(gv => gv.Versionen)
                .Include(v => v.GarageVertraege).ThenInclude(gv => gv.Garage)
                .Include(v => v.GarageVertraege).ThenInclude(gv => gv.MietBuchungskonto)
                    .ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .FirstOrDefaultAsync(v => v.VertragId == vertragId);
            if (vertrag is null) return NotFound();

            var authRx = await auth.AuthorizeAsync(User, vertrag, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var ersterDesMonats = new DateOnly(monat.Year, monat.Month, 1);

            var sollSumme = vertrag.MietBuchungskonto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll
                    && z.Buchungssatz.Buchungsdatum.Year == ersterDesMonats.Year
                    && z.Buchungssatz.Buchungsdatum.Month == ersterDesMonats.Month)
                .Sum(z => z.Betrag);

            var habenSumme = vertrag.MietBuchungskonto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben
                    && z.Buchungssatz.Buchungsdatum.Year == ersterDesMonats.Year
                    && z.Buchungssatz.Buchungsdatum.Month == ersterDesMonats.Month)
                .Sum(z => z.Betrag);

            var version = vertrag.Versionen
                .Where(v => v.Beginn <= ersterDesMonats)
                .MaxBy(v => v.Beginn);

            var garageStatuses = vertrag.GarageVertraege
                .Where(gv => gv.Beginn() <= ersterDesMonats && (gv.Ende == null || gv.Ende >= ersterDesMonats))
                .Select(gv =>
                {
                    var gvVersion = gv.Versionen
                        .Where(v => v.Beginn <= ersterDesMonats)
                        .MaxBy(v => v.Beginn);
                    if (gvVersion == null) return null;

                    var gvSoll = gv.MietBuchungskonto.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Soll
                            && z.Buchungssatz.Buchungsdatum.Year == ersterDesMonats.Year
                            && z.Buchungssatz.Buchungsdatum.Month == ersterDesMonats.Month)
                        .Sum(z => z.Betrag);
                    var gvHaben = gv.MietBuchungskonto.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Haben
                            && z.Buchungssatz.Buchungsdatum.Year == ersterDesMonats.Year
                            && z.Buchungssatz.Buchungsdatum.Month == ersterDesMonats.Month)
                        .Sum(z => z.Betrag);

                    return new GarageForderungsstatusEntry
                    {
                        GarageVertragId = gv.GarageVertragId,
                        GarageKennung = gv.Garage.Kennung,
                        GaragenMiete = gvVersion.GaragenMiete,
                        Forderungsbetrag = gvSoll,
                        SchonGezahlt = gvHaben,
                        VerbleibendeForderung = gvSoll - gvHaben,
                        SollstellungVorhanden = gvSoll > 0
                    };
                })
                .Where(g => g != null)
                .Cast<GarageForderungsstatusEntry>()
                .ToList();

            return Ok(new ForderungsstatusEntry
            {
                Monat = ersterDesMonats,
                Forderungsbetrag = sollSumme,
                SchonGezahlt = habenSumme,
                VerbleibendeForderung = sollSumme - habenSumme,
                NkVorauszahlung = version?.Nebenkostenvorauszahlung ?? 0,
                SollstellungVorhanden = sollSumme > 0,
                Grundmiete = version?.Grundmiete ?? 0,
                GrundmieteSeit = version?.Beginn,
                Garagen = garageStatuses
            });
        }
    }
}
