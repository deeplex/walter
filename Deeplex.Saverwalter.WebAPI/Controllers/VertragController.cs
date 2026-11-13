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

using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using static Deeplex.Saverwalter.WebAPI.Controllers.BuchungskontoController;
using static Deeplex.Saverwalter.WebAPI.Controllers.GarageVertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietzahlungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/vertraege")]
    public class VertragController : FileControllerBase<VertragEntry, int, Vertrag>
    {
        public class VertragEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public DateOnly? Ende { get; set; }
            public SelectionEntry? Wohnung { get; set; }
            public string? MieterAuflistung { get; set; }

            public IEnumerable<VertragVersionEntryBase>? Versionen { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public VertragEntryBase() { }
            public VertragEntryBase(Vertrag entity, Permissions permissions)
            {
                Id = entity.VertragId;
                Beginn = entity.Beginn();
                Ende = entity.Ende;
                if (entity.Wohnung is Wohnung wohnung)
                {
                    var anschrift = wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                    Wohnung = new(
                        wohnung.WohnungId,
                        $"{anschrift} - {wohnung.Bezeichnung}",
                        wohnung.Eigentuemer.FirstOrDefault(e => e.Bis == null)?.Kontakt.Bezeichnung);
                }
                else
                {
                    Wohnung = new(0, "Unbekannte Wohnung");
                }

                Versionen = entity.Versionen.Select(e => new VertragVersionEntryBase(e, permissions));
                MieterAuflistung = string.Join(", ", entity.Mieter.Select(a => a.Bezeichnung));

                Permissions = permissions;
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            private Vertrag Entity { get; } = null!;

            public SelectionEntry? Ansprechpartner { get; set; }
            public string? Notiz { get; set; }
            public IEnumerable<SelectionEntry>? SelectedMieter { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<MietminderungEntryBase> Mietminderungen { get; set; } = [];
            public IEnumerable<KontaktEntryBase> Mieter { get; set; } = [];

            public IEnumerable<AbrechnungsresultatEntryBase> Abrechnungsresultate { get; set; } = [];
            public IEnumerable<GarageVertragEntryBase> GarageVertraege { get; set; } = [];
            public IEnumerable<BuchungskontoRefEntry> Konten { get; set; } = [];
            /// <summary>Gehaltene Kaution: Saldo des KautionsKontos (Haben positiv).</summary>
            public decimal Kaution { get; set; }
            public int? KautionsKontoId { get; set; }
            /// <summary>Eingänge (positiv) und Rückzahlungen (negativ) auf dem KautionsKonto.</summary>
            public IEnumerable<KautionsBuchungEntry> KautionsBuchungen { get; set; } = [];


            public VertragEntry() : base() { }
            public VertragEntry(Vertrag entity, Permissions permissions) : base(entity, permissions)
            {
                Entity = entity;

                if (entity.Ansprechpartner is Kontakt a)
                {
                    Ansprechpartner = new(a.KontaktId, a.Bezeichnung);
                }
                Notiz = entity.Notiz;

                SelectedMieter = entity.Mieter.Select(e => new SelectionEntry(e.KontaktId, e.Bezeichnung));

                Mietminderungen = entity.Mietminderungen.ToList().Select(e
                    => new MietminderungEntryBase(e, permissions));
                Mieter = entity.Mieter.Select(e => new KontaktEntryBase(e, permissions));
                Abrechnungsresultate = entity.Abrechnungsresultate.Select(e
                    => new AbrechnungsresultatEntry(e, permissions));
                GarageVertraege = entity.GarageVertraege.Select(e
                    => new GarageVertragEntryBase(e, permissions));
                Konten = BuchungskontoRefEntry.Collect(
                    (entity.MietBuchungskonto, KontoFunktion.Mietforderungen),
                    (entity.NkBuchungskonto, KontoFunktion.NkVorauszahlungen),
                    (entity.BkAbrechnungsKonto, KontoFunktion.BkAbrechnung),
                    (entity.ZahlungsKonto, KontoFunktion.Zahlungseingaenge),
                    (entity.MietminderungsKonto, KontoFunktion.Mietminderungen),
                    (entity.KautionsKonto, KontoFunktion.Kaution));
                if (entity.KautionsKonto is Buchungskonto kautionsKonto)
                {
                    KautionsKontoId = kautionsKonto.BuchungskontoId;
                    Kaution = kautionsKonto.Buchungszeilen
                        .Sum(z => z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag);
                    KautionsBuchungen = kautionsKonto.Buchungszeilen
                        .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                        .Select(z => new KautionsBuchungEntry
                        {
                            Datum = z.Buchungssatz.Buchungsdatum,
                            Betrag = z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag,
                            Beschreibung = z.Buchungssatz.Beschreibung,
                            TransaktionId = z.Buchungssatz.Transaktion?.TransaktionId,
                        })
                        .ToList();
                }

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        public class KautionsBuchungEntry
        {
            public DateOnly Datum { get; set; }
            public decimal Betrag { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
            public Guid? TransaktionId { get; set; }
        }

        private readonly ILogger<VertragController> _logger;
        private readonly SaverwalterContext _ctx;
        private readonly IAuthorizationService _auth;
        private readonly MietKorrekturService _mietKorrekturService;
        private readonly TransaktionBuchungsService _buchungsService;
        private readonly MietkontoService _mietkontoService;
        protected override VertragDbService DbService { get; }

        public VertragController(
            ILogger<VertragController> logger,
            VertragDbService dbService,
            SaverwalterContext ctx,
            IAuthorizationService auth,
            MietKorrekturService mietKorrekturService,
            TransaktionBuchungsService buchungsService,
            MietkontoService mietkontoService,
            HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _ctx = ctx;
            _auth = auth;
            _mietKorrekturService = mietKorrekturService;
            _buchungsService = buchungsService;
            _mietkontoService = mietkontoService;
            _logger = logger;
        }

        [HttpGet]
        public Task<PagedResult<VertragEntryBase>> Get([FromQuery] PagedQuery query)
            => DbService.GetList(User!, query);

        public record VertragOverlapInfo(int Id, DateOnly Beginn, DateOnly? Ende, string Mieter);

        [HttpGet("check-overlap")]
        public async Task<ActionResult<VertragOverlapInfo?>> CheckOverlap(
            [FromQuery] int wohnungId,
            [FromQuery] DateOnly beginn,
            [FromQuery] DateOnly? ende,
            [FromQuery] int excludeId = 0)
        {
            var existing = await _ctx.Vertraege
                .Where(v => v.Wohnung.WohnungId == wohnungId && v.VertragId != excludeId)
                .Select(v => new
                {
                    Id = v.VertragId,
                    Ende = v.Ende,
                    Beginn = v.Versionen.Min(vv => (DateOnly?)vv.Beginn),
                    Mieter = v.Mieter.Select(m => m.Bezeichnung)
                })
                .ToListAsync();

            var conflict = existing.FirstOrDefault(v =>
                v.Beginn.HasValue &&
                beginn <= (v.Ende ?? DateOnly.MaxValue) &&
                (ende ?? DateOnly.MaxValue) >= v.Beginn.Value);

            if (conflict is null) return Ok(null);

            return Ok(new VertragOverlapInfo(
                conflict.Id,
                conflict.Beginn!.Value,
                conflict.Ende,
                string.Join(", ", conflict.Mieter)));
        }

        [HttpPost]
        public Task<ActionResult<VertragEntry>> Post([FromBody] VertragEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<VertragEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<VertragEntry>> Put(int id, VertragEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);

        /// <summary>
        /// Returns all Transaktionen that have Buchungssätze touching this Vertrag's
        /// MietBuchungskonto, NkBuchungskonto or KautionsKonto.
        /// </summary>
        [HttpGet("{id}/transaktionen")]
        public async Task<ActionResult<IEnumerable<TransaktionEntry>>> GetTransaktionen(int id)
        {
            var vertrag = await _ctx.Vertraege.FindAsync(id);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var permissions = await GetPermissions(User!, vertrag, _auth);

            var mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            var nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;
            var kautionsKontoId = vertrag.KautionsKonto.BuchungskontoId;

            var transaktionen = await _ctx.Transaktionen
                .Include(t => t.Zahler)
                .Include(t => t.Zahlungsempfaenger)
                .Include(t => t.Buchungssaetze)
                    .ThenInclude(s => s.Buchungszeilen)
                        .ThenInclude(z => z.Buchungskonto)
                .Where(t => t.Buchungssaetze.Any(s =>
                    s.Buchungszeilen.Any(z =>
                        z.Buchungskonto.BuchungskontoId == mietKontoId ||
                        z.Buchungskonto.BuchungskontoId == nkKontoId ||
                        z.Buchungskonto.BuchungskontoId == kautionsKontoId)))
                .OrderByDescending(t => t.Zahlungsdatum)
                .ToListAsync();

            var result = transaktionen.Select(t => new TransaktionEntry(t, permissions));
            return Ok(result);
        }

        /// <summary>
        /// Mietkonto des Vertrags: Kaltmiete und Garagen je Monat, NK-Vorauszahlungen je Jahr —
        /// siehe <see cref="MietkontoService"/>.
        /// </summary>
        [HttpGet("{id}/mietkonto")]
        public async Task<ActionResult<MietkontoService.Mietkonto>> GetMietkonto(int id)
        {
            var vertrag = await _ctx.Vertraege.FindAsync(id);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            return Ok(await _mietkontoService.ErmittleAsync(id, DateOnly.FromDateTime(DateTime.Today)));
        }

        public class MietKorrekturMonatEntry
        {
            public int Monat { get; set; }
            public decimal IstKaltmiete { get; set; }
            public decimal SollKaltmiete { get; set; }
            public decimal Delta { get; set; }
        }

        public class MietKorrekturBuchungEntry
        {
            public Guid Id { get; set; }
            public DateOnly Datum { get; set; }
            public decimal Betrag { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
            public string? Notiz { get; set; }
            public bool KannLoeschen { get; set; }
            public bool KannStornieren { get; set; }
            public string? Sperrgrund { get; set; }
        }

        public class MietKorrekturJahrEntry
        {
            public int Jahr { get; set; }
            public List<MietKorrekturMonatEntry> Monate { get; set; } = [];
            public List<MietKorrekturBuchungEntry> Korrekturen { get; set; } = [];
            public decimal Abweichung { get; set; }
            public decimal Korrigiert { get; set; }
            public decimal Differenz { get; set; }
            public decimal NkVorauszahlung { get; set; }
            public string Status { get; set; } = string.Empty;

            public MietKorrekturJahrEntry() { }
            public MietKorrekturJahrEntry(MietKorrekturService.MietKorrekturJahr j)
            {
                Jahr = j.Jahr;
                Monate = j.Monate.Select(m => new MietKorrekturMonatEntry
                {
                    Monat = m.Monat,
                    IstKaltmiete = m.IstKaltmiete,
                    SollKaltmiete = m.SollKaltmiete,
                    Delta = m.Delta,
                }).ToList();
                Korrekturen = j.Korrekturen.Select(k => new MietKorrekturBuchungEntry
                {
                    Id = k.BuchungssatzId,
                    Datum = k.Buchungsdatum,
                    Betrag = k.Betrag,
                    Beschreibung = k.Beschreibung,
                    Notiz = k.Notiz,
                    KannLoeschen = k.KannLoeschen,
                    KannStornieren = k.KannStornieren,
                    Sperrgrund = k.Sperrgrund,
                }).ToList();
                Abweichung = j.Abweichung;
                Korrigiert = j.Korrigiert;
                Differenz = j.Differenz;
                NkVorauszahlung = j.NkVorauszahlung;
                Status = j.Status.ToString();
            }
        }

        /// <summary>
        /// Jahre, deren gebuchte Kaltmiete von den <see cref="VertragVersion"/>en abweicht
        /// oder die schon Korrekturen haben — siehe <see cref="MietKorrekturService"/>.
        /// </summary>
        [HttpGet("{id}/miet-korrektur")]
        public async Task<ActionResult<IEnumerable<MietKorrekturJahrEntry>>> GetMietKorrektur(int id)
        {
            var vertrag = await _ctx.Vertraege.FindAsync(id);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var jahre = await _mietKorrekturService.ErmittleAsync(id);
            return Ok(jahre.Select(j => new MietKorrekturJahrEntry(j)));
        }

        public class MietKorrekturRequest
        {
            public int Jahr { get; set; }
            public string Grund { get; set; } = string.Empty;
        }

        /// <summary>
        /// Bucht die offene Differenz eines Jahres als Korrektur-Satz — siehe
        /// <see cref="MietKorrekturService.BucheDifferenzAsync"/>. Rückgängig machen oder
        /// stornieren läuft über die gewöhnlichen Buchungssatz-Endpunkte.
        /// </summary>
        [HttpPost("{id}/miet-korrektur")]
        public async Task<ActionResult<MietKorrekturJahrEntry>> PostMietKorrektur(
            int id, [FromBody] MietKorrekturRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Grund))
            {
                return BadRequest("Für eine Miet-Korrektur muss ein Grund angegeben werden.");
            }

            var vertrag = await _ctx.Vertraege.FindAsync(id);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.Update]);
            if (!authRx.Succeeded) return Forbid();

            try
            {
                await _mietKorrekturService.BucheDifferenzAsync(id, request.Jahr, request.Grund);
            }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }

            var jahr = (await _mietKorrekturService.ErmittleAsync(id)).Single(j => j.Jahr == request.Jahr);
            return Ok(new MietKorrekturJahrEntry(jahr));
        }

        public class KautionRequest
        {
            public DateOnly Datum { get; set; }
            public decimal Betrag { get; set; }
            public bool Rueckzahlung { get; set; }
            public string? Notiz { get; set; }
        }

        /// <summary>
        /// Bucht Eingang oder Rückzahlung der Kaution als Transaktion mit einer
        /// Kautions-Position — siehe <see cref="TransaktionBuchungsService"/>. Rückgängig
        /// machen läuft über die gewöhnlichen Transaktions-Endpunkte.
        /// </summary>
        [HttpPost("{id}/kaution")]
        public async Task<ActionResult<VertragEntry>> PostKaution(int id, [FromBody] KautionRequest request)
        {
            var vertrag = await _ctx.Vertraege.FindAsync(id);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.SubCreate]);
            if (!authRx.Succeeded) return Forbid();

            try
            {
                await _buchungsService.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
                {
                    Betrag = request.Betrag,
                    Zahlungsdatum = request.Datum,
                    Verwendungszweck = request.Rueckzahlung ? "Kaution Rückzahlung" : "Kaution Eingang",
                    Notiz = request.Notiz,
                    Kautionen =
                    {
                        new TransaktionBuchungsService.KautionInput
                        {
                            VertragId = id,
                            Betrag = request.Betrag,
                            Rueckzahlung = request.Rueckzahlung,
                        }
                    }
                });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

            return await DbService.Get(User!, id);
        }
    }
}
