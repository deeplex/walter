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
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mietzahlungen")]
    public class MietzahlungController : ControllerBase
    {
        /// <summary>Eingabe für eine neue Mietzahlung.</summary>
        public class MietzahlungEntry
        {
            public SelectionEntry Vertrag { get; set; } = null!;
            public DateOnly BetreffenderMonat { get; set; }
            public DateOnly Zahlungsdatum { get; set; }
            public decimal KaltmieteZahlung { get; set; }
            public decimal NkZahlung { get; set; }
            public string? Notiz { get; set; }
        }

        /// <summary>
        /// Ergebnis nach erfolgreicher Buchung.
        /// Id = KaltmieteBuchungssatzId — dient als Tabellenzeilen-Key im Frontend.
        /// </summary>
        public class MietzahlungErgebnisEntry
        {
            public Guid Id { get; set; }
            public DateOnly Buchungsdatum { get; set; }
            public string BetreffenderMonat { get; set; } = string.Empty;
            public decimal KaltmieteZahlung { get; set; }
            public Guid SollstellungBuchungssatzId { get; set; }
            public decimal SollstellungBetrag { get; set; }
            public decimal VerbleibendeForderung { get; set; }
            public Guid? NkBuchungssatzId { get; set; }
            public decimal NkZahlung { get; set; }
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
            public MietzahlungListEntry(Buchungssatz satz, decimal betrag, DateOnly betreffenderMonat, Permissions permissions)
            {
                Id = satz.BuchungssatzId;
                Buchungsdatum = satz.Buchungsdatum;
                BetreffenderMonat = betreffenderMonat.ToString("yyyy-MM-01");
                KaltmieteZahlung = betrag;
                Permissions = permissions;
            }
        }

        /// <summary>
        /// Forderungsstatus für einen Monat — dient der UI zur Vorauswahl der Zahlungsbeträge.
        /// </summary>
        public class ForderungsstatusEntry
        {
            public DateOnly Monat { get; set; }
            public decimal Forderungsbetrag { get; set; }
            public decimal SchonGezahlt { get; set; }
            public decimal VerbleibendeForderung { get; set; }
            public decimal NkVorauszahlung { get; set; }
            public bool SollstellungVorhanden { get; set; }
        }

        private readonly ILogger<MietzahlungController> _logger;
        private readonly MietzahlungBuchungsService _buchungsService;
        private readonly SaverwalterContext _ctx;
        private readonly IAuthorizationService _auth;

        public MietzahlungController(
            ILogger<MietzahlungController> logger,
            MietzahlungBuchungsService buchungsService,
            SaverwalterContext ctx,
            IAuthorizationService auth)
        {
            _logger = logger;
            _buchungsService = buchungsService;
            _ctx = ctx;
            _auth = auth;
        }

        /// <summary>
        /// Erstellt eine Mietzahlung (Kaltmiete + optional NK-VZ) für einen Vertrag.
        /// Die Sollstellung wird automatisch angelegt wenn sie für den Monat fehlt.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MietzahlungErgebnisEntry>> Post([FromBody] MietzahlungEntry entry)
        {
            var vertrag = await _ctx.Vertraege.FindAsync(entry.Vertrag.Id);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.SubCreate]);
            if (!authRx.Succeeded) return Forbid();

            if (entry.KaltmieteZahlung <= 0 && entry.NkZahlung <= 0)
                return BadRequest("Mindestens ein Betrag (Kaltmiete oder NK) muss größer als 0 sein.");

            if (entry.KaltmieteZahlung < 0 || entry.NkZahlung < 0)
                return BadRequest("Zahlungsbeträge dürfen nicht negativ sein.");

            try
            {
                var input = new MietzahlungBuchungsService.MietzahlungInput(
                    vertrag,
                    entry.BetreffenderMonat,
                    entry.Zahlungsdatum,
                    entry.KaltmieteZahlung,
                    entry.NkZahlung,
                    entry.Notiz);

                var ergebnis = await _buchungsService.BucheMietzahlungAsync(input);

                var monat = new DateOnly(entry.BetreffenderMonat.Year, entry.BetreffenderMonat.Month, 1);
                return Ok(new MietzahlungErgebnisEntry
                {
                    Id = ergebnis.KaltmieteBuchungssatz.BuchungssatzId,
                    Buchungsdatum = entry.Zahlungsdatum,
                    BetreffenderMonat = monat.ToString("yyyy-MM-dd"),
                    KaltmieteZahlung = entry.KaltmieteZahlung,
                    SollstellungBuchungssatzId = ergebnis.SollstellungBuchungssatz.BuchungssatzId,
                    SollstellungBetrag = ergebnis.SollstellungBetrag,
                    VerbleibendeForderung = ergebnis.VerbleibendeForderung,
                    NkBuchungssatzId = ergebnis.NkBuchungssatz?.BuchungssatzId,
                    NkZahlung = entry.NkZahlung
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Listet alle Kaltmiete-Zahlungs-Buchungssätze eines Vertrags.
        /// </summary>
        [HttpGet("{vertragId}")]
        public async Task<ActionResult<IEnumerable<MietzahlungListEntry>>> GetByVertrag(int vertragId)
        {
            var vertrag = await _ctx.Vertraege.FindAsync(vertragId);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var permissions = await GetPermissions(User!, vertrag, _auth);
            var saetze = await _buchungsService.GetKaltmieteZahlungenAsync(vertrag);
            var kontoId = vertrag.MietBuchungskonto.BuchungskontoId;

            var result = saetze.Select(s =>
            {
                var betrag = s.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben && z.Buchungskonto.BuchungskontoId == kontoId)
                    .Sum(z => z.Betrag);
                return new MietzahlungListEntry(s, betrag, s.Buchungsdatum, permissions);
            });

            return Ok(result);
        }

        /// <summary>
        /// Gibt den Forderungsstatus für einen Monat zurück.
        /// Dient der UI zur Vorauswahl von Kaltmiete- und NK-Betrag.
        /// </summary>
        [HttpGet("{vertragId}/forderung/{monat}")]
        public async Task<ActionResult<ForderungsstatusEntry>> GetForderungsstatus(int vertragId, DateOnly monat)
        {
            var vertrag = await _ctx.Vertraege.FindAsync(vertragId);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var ersterDesMonats = new DateOnly(monat.Year, monat.Month, 1);
            var kontoId = vertrag.MietBuchungskonto.BuchungskontoId;

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

            return Ok(new ForderungsstatusEntry
            {
                Monat = ersterDesMonats,
                Forderungsbetrag = sollSumme,
                SchonGezahlt = habenSumme,
                VerbleibendeForderung = sollSumme - habenSumme,
                NkVorauszahlung = version?.Nebenkostenvorauszahlung ?? 0,
                SollstellungVorhanden = sollSumme > 0
            });
        }
    }
}
