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
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BuchungssaetzeController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;
using Operations = Deeplex.Saverwalter.WebAPI.Services.Operations;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/buchungssaetze")]
    public class BuchungssaetzeController : FileControllerBase<BuchungssatzEntry, Guid, Buchungssatz>
    {
        private readonly StornoBuchungsService stornoService;
        private readonly BuchungssatzSchutzService schutzService;
        private readonly SaverwalterContext ctx;
        private readonly IAuthorizationService auth;
        protected override BuchungssatzDbService DbService { get; }

        public BuchungssaetzeController(
            ILogger<BuchungssaetzeController> logger,
            BuchungssatzDbService dbService,
            StornoBuchungsService stornoService,
            BuchungssatzSchutzService schutzService,
            SaverwalterContext ctx,
            IAuthorizationService auth,
            HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            this.stornoService = stornoService;
            this.schutzService = schutzService;
            this.ctx = ctx;
            this.auth = auth;
        }

        public class StornoRequest
        {
            public string Grund { get; set; } = string.Empty;
        }

        public class StornoBuchungssatzInfo
        {
            public Guid BuchungssatzId { get; set; }
            public int Buchungsnummer { get; set; }
            public int Buchungsjahr { get; set; }
            public DateOnly Buchungsdatum { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
        }

        public class BuchungssatzEntryBase
        {
            public Guid Id { get; set; }
            public int Buchungsnummer { get; set; }
            public int Buchungsjahr { get; set; }
            public DateOnly Buchungsdatum { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
            /// <summary>Summe der Soll-Seite — bei einem ausgeglichenen Satz der Buchungsbetrag.</summary>
            public decimal Betrag { get; set; }
            public string SollKonten { get; set; } = string.Empty;
            public string HabenKonten { get; set; } = string.Empty;
            public bool IstStorno { get; set; }
            public bool IstStorniert { get; set; }

            /// <summary>
            /// Soll-/Haben-Anteil eines bestimmten Kontos an diesem Satz —
            /// nur gefüllt, wenn die Liste nach kontoId gefiltert ist (Kontoblatt).
            /// </summary>
            public decimal? KontoSoll { get; set; }
            public decimal? KontoHaben { get; set; }
            /// <summary>Nicht durch OPOS-Ausgleiche gedeckter Rest der Zeilen dieses Kontos.</summary>
            public decimal? KontoOffen { get; set; }

            public BuchungssatzEntryBase() { }
            public BuchungssatzEntryBase(Buchungssatz entity)
            {
                Id = entity.BuchungssatzId;
                Buchungsnummer = entity.Buchungsnummer;
                Buchungsjahr = entity.Buchungsjahr;
                Buchungsdatum = entity.Buchungsdatum;
                Beschreibung = entity.Beschreibung;
                var sollSum = entity.Buchungszeilen
                    .Where(z => z.SollHaben == Model.SollHaben.Soll)
                    .Sum(z => z.Betrag);
                var habenSum = entity.Buchungszeilen
                    .Where(z => z.SollHaben == Model.SollHaben.Haben)
                    .Sum(z => z.Betrag);
                Betrag = Math.Max(sollSum, habenSum);
                SollKonten = KontenText(entity, Model.SollHaben.Soll);
                HabenKonten = KontenText(entity, Model.SollHaben.Haben);
                IstStorno = entity.StornoVon != null;
                IstStorniert = entity.StornoNach != null;
            }

            private static string KontenText(Buchungssatz entity, SollHaben seite) =>
                string.Join(", ", entity.Buchungszeilen
                    .Where(z => z.SollHaben == seite)
                    .Select(z => $"{z.Buchungskonto.Kontonummer} {z.Buchungskonto.Bezeichnung}")
                    .Distinct());
        }

        /// <summary>
        /// Verweis auf den Buchungssatz der Gegenzeile eines OPOS-Ausgleichs —
        /// von der Forderung zur ausgleichenden Zahlung navigieren und umgekehrt.
        /// </summary>
        public class AusgleichEntry
        {
            public Guid BuchungssatzId { get; set; }
            public int Buchungsnummer { get; set; }
            public int Buchungsjahr { get; set; }
            public DateOnly Buchungsdatum { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
            public decimal Betrag { get; set; }
        }

        public class BuchungszeileEntry
        {
            public Guid Id { get; set; }
            public int KontoId { get; set; }
            public string Kontonummer { get; set; } = string.Empty;
            public string Kontobezeichnung { get; set; } = string.Empty;
            public string SollHaben { get; set; } = string.Empty;
            public decimal Betrag { get; set; }

            /// <summary>Über OPOS-Ausgleiche gedeckter Anteil dieser Zeile.</summary>
            public decimal Ausgeglichen { get; set; }
            public decimal Offen { get; set; }
            /// <summary>Ob das Konto der Zeile ein Ausgleichskonto ist — nur dann ist "Offen" ein offener Posten.</summary>
            public bool Ausgleichbar { get; set; }
            public List<AusgleichEntry> Ausgleiche { get; set; } = [];
        }

        public class BuchungssatzLink
        {
            public Guid Id { get; set; }
            public int Buchungsnummer { get; set; }
            public int Buchungsjahr { get; set; }
        }

        public class TransaktionLink
        {
            public Guid Id { get; set; }
            public string Text { get; set; } = string.Empty;
        }

        public class BuchungssatzEntry : BuchungssatzEntryBase
        {
            public string? Notiz { get; set; }
            public string? Belegpfad { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public List<BuchungszeileEntry> Zeilen { get; set; } = [];
            public TransaktionLink? Transaktion { get; set; }
            public BuchungssatzLink? StornoVon { get; set; }
            public BuchungssatzLink? StornoNach { get; set; }
            public List<KontoVerknuepfungEntry> Verknuepfungen { get; set; } = [];

            /// <summary>Schutzstatus: Korrekturen nur im Rahmen der GoB-Regeln.</summary>
            public bool KannStornieren { get; set; }
            public bool KannLoeschen { get; set; }
            public string? Sperrgrund { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public BuchungssatzEntry() : base() { }
            public BuchungssatzEntry(Buchungssatz entity) : base(entity)
            {
                Notiz = entity.Notiz;
                Belegpfad = entity.Belegpfad;
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
                Zeilen = entity.Buchungszeilen
                    .OrderBy(z => z.SollHaben)
                    .ThenBy(z => z.Buchungskonto.Kontonummer)
                    .Select(ToZeileEntry)
                    .ToList();
                Transaktion = entity.Transaktion is Transaktion t
                    ? new TransaktionLink
                    {
                        Id = t.TransaktionId,
                        Text = $"{t.Zahlungsdatum:dd.MM.yyyy} | {t.Betrag:0.00} € | {t.Verwendungszweck}"
                    }
                    : null;
                StornoVon = ToLink(entity.StornoVon);
                StornoNach = ToLink(entity.StornoNach);
            }

            private static BuchungszeileEntry ToZeileEntry(Buchungszeile z)
            {
                // Gegenzeilen aller OPOS-Ausgleiche dieser Zeile — egal ob sie
                // selbst die Soll- oder die Haben-Seite des Ausgleichs ist.
                var gegenzeilen = z.AlsSollZeile.Select(a => a.HabenZeile)
                    .Concat(z.AlsHabenZeile.Select(a => a.SollZeile))
                    .ToList();
                var ausgeglichen = gegenzeilen.Sum(g => g.Betrag);

                return new BuchungszeileEntry
                {
                    Id = z.BuchungszeileId,
                    KontoId = z.Buchungskonto.BuchungskontoId,
                    Kontonummer = z.Buchungskonto.Kontonummer,
                    Kontobezeichnung = z.Buchungskonto.Bezeichnung,
                    SollHaben = z.SollHaben == Model.SollHaben.Soll ? "Soll" : "Haben",
                    Betrag = z.Betrag,
                    Ausgeglichen = ausgeglichen,
                    Offen = Math.Max(0, z.Betrag - ausgeglichen),
                    Ausgleiche = gegenzeilen
                        .Select(g => new AusgleichEntry
                        {
                            BuchungssatzId = g.Buchungssatz.BuchungssatzId,
                            Buchungsnummer = g.Buchungssatz.Buchungsnummer,
                            Buchungsjahr = g.Buchungssatz.Buchungsjahr,
                            Buchungsdatum = g.Buchungssatz.Buchungsdatum,
                            Beschreibung = g.Buchungssatz.Beschreibung,
                            Betrag = g.Betrag
                        })
                        .ToList()
                };
            }

            private static BuchungssatzLink? ToLink(Buchungssatz? satz) =>
                satz is null ? null : new BuchungssatzLink
                {
                    Id = satz.BuchungssatzId,
                    Buchungsnummer = satz.Buchungsnummer,
                    Buchungsjahr = satz.Buchungsjahr
                };
        }

        /// <summary>
        /// Buchungssätze, die der Nutzer sehen darf — Admin alle, sonst nur Sätze
        /// mit mindestens einer Zeile auf einem Konto einer verwalteten Wohnung
        /// (analog zur Sichtbarkeit der Buchungskonten).
        /// </summary>
        private IQueryable<Buchungssatz> ScopedSaetze() =>
            BuchungssatzDbService.Scoped(ctx, User!);

        [HttpGet]
        public async Task<ActionResult<PagedResult<BuchungssatzEntryBase>>> GetList(
            [FromQuery] PagedQuery query, [FromQuery] int? kontoId)
        {
            var saetze = ScopedSaetze();
            if (kontoId is int konto)
            {
                saetze = saetze
                    .Where(s =>
                        s.Buchungszeilen.Any(z => z.Buchungskonto.BuchungskontoId == konto))
                    // Für die OPOS-Spalte des Kontoblatts werden die Ausgleiche gebraucht
                    .Include(s => s.Buchungszeilen)
                        .ThenInclude(z => z.AlsSollZeile)
                            .ThenInclude(a => a.HabenZeile)
                    .Include(s => s.Buchungszeilen)
                        .ThenInclude(z => z.AlsHabenZeile)
                            .ThenInclude(a => a.SollZeile);
            }

            var result = await saetze
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.Buchungskonto)
                .PagedAsync(query,
                    searchPredicate: t => s =>
                        s.Beschreibung.ToLower().Contains(t) ||
                        (s.Notiz != null && s.Notiz.ToLower().Contains(t)) ||
                        s.Buchungszeilen.Any(z =>
                            z.Buchungskonto.Kontonummer.ToLower().Contains(t) ||
                            z.Buchungskonto.Bezeichnung.ToLower().Contains(t)),
                    applySort: (q, sortBy, dir) => sortBy switch
                    {
                        "buchungsnummer" => q.SortBy(s => s.Buchungsjahr, dir).ThenSortBy(s => s.Buchungsnummer, dir),
                        "beschreibung" => q.SortBy(s => s.Beschreibung, dir),
                        "betrag" => q.SortBy(s => s.Buchungszeilen
                            .Where(z => z.SollHaben == Model.SollHaben.Soll)
                            .Sum(z => z.Betrag), dir),
                        _ => q.SortBy(s => s.Buchungsdatum, dir).ThenSortBy(s => s.Buchungsnummer, dir)
                    },
                    toEntry: s =>
                    {
                        var entry = new BuchungssatzEntryBase(s);
                        if (kontoId is int kId)
                        {
                            entry.KontoSoll = KontoSeite(s, kId, Model.SollHaben.Soll);
                            entry.KontoHaben = KontoSeite(s, kId, Model.SollHaben.Haben);
                            entry.KontoOffen = s.Buchungszeilen
                                .Where(z => z.Buchungskonto.BuchungskontoId == kId)
                                .Sum(z => Math.Max(0, z.Betrag -
                                    z.AlsSollZeile.Sum(a => a.HabenZeile.Betrag) -
                                    z.AlsHabenZeile.Sum(a => a.SollZeile.Betrag)));
                        }
                        return entry;
                    });

            return Ok(result);
        }

        private static decimal? KontoSeite(Buchungssatz satz, int kontoId, SollHaben seite)
        {
            var summe = satz.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == kontoId && z.SollHaben == seite)
                .Sum(z => z.Betrag);
            return summe == 0 ? null : summe;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BuchungssatzEntry>> Get(Guid id)
        {
            var satz = await ScopedSaetze()
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.Buchungskonto)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsSollZeile)
                        .ThenInclude(a => a.HabenZeile)
                            .ThenInclude(z => z.Buchungssatz)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile)
                        .ThenInclude(a => a.SollZeile)
                            .ThenInclude(z => z.Buchungssatz)
                .Include(s => s.Transaktion)
                .Include(s => s.StornoVon)
                .Include(s => s.StornoNach)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == id);

            if (satz is null)
            {
                // Existiert der Satz, ist aber außerhalb des Sichtbereichs? -> 403 statt 404
                return await ctx.Buchungssaetze.AnyAsync(s => s.BuchungssatzId == id)
                    ? Forbid()
                    : NotFound();
            }

            var entry = new BuchungssatzEntry(satz);
            var kontoIds = satz.Buchungszeilen
                .Select(z => z.Buchungskonto.BuchungskontoId)
                .Distinct()
                .ToList();
            entry.Verknuepfungen = await KontoVerknuepfungService.ForKontenAsync(ctx, kontoIds);

            // Offene Posten gibt es nur auf Ausgleichskonten — auf Summenkonten
            // (Erträge, Zahlungseingänge, ...) ist "Offen" bedeutungslos.
            foreach (var zeile in entry.Zeilen)
            {
                zeile.Ausgleichbar = entry.Verknuepfungen
                    .Any(v => v.KontoId == zeile.KontoId && v.Ausgleichbar);
            }

            var schutz = await schutzService.PruefeAsync(satz);
            entry.KannStornieren = schutz.KannStornieren;
            entry.KannLoeschen = schutz.KannLoeschen;
            entry.Sperrgrund = schutz.Sperrgrund;
            entry.Permissions = new Permissions(read: true)
            {
                Update = true,
                Remove = false
            };

            return Ok(entry);
        }

        /// <summary>
        /// Autorisierung für Korrekturen: Admin oder Vollzugriff auf die Wohnung
        /// des ersten Buchungskontos des Satzes.
        /// </summary>
        private async Task<ActionResult?> AuthorizeKorrektur(Guid id)
        {
            if (User!.IsInRole("Admin"))
            {
                return null;
            }

            var wohnung = await ctx.Buchungssaetze
                .Where(s => s.BuchungssatzId == id)
                .SelectMany(s => s.Buchungszeilen)
                .Select(z => z.Buchungskonto)
                .SelectMany(k =>
                    ctx.Vertraege
                        .Where(v =>
                            v.MietBuchungskonto.BuchungskontoId == k.BuchungskontoId ||
                            v.NkBuchungskonto.BuchungskontoId == k.BuchungskontoId ||
                            v.ZahlungsKonto.BuchungskontoId == k.BuchungskontoId)
                        .Select(v => v.Wohnung))
                .FirstOrDefaultAsync();

            if (wohnung == null) return Forbid();

            var authRx = await auth.AuthorizeAsync(User!, wohnung, [Operations.Delete]);
            return authRx.Succeeded ? null : Forbid();
        }

        private async Task<Buchungssatz?> LadeSatzMitZeilen(Guid id) =>
            await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.Buchungskonto)
                .Include(s => s.StornoVon)
                .Include(s => s.StornoNach)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == id);

        /// <summary>
        /// Erstellt eine Stornobuchung für den angegebenen Buchungssatz.
        /// Alle Buchungszeilen werden mit umgekehrten Soll/Haben-Seiten gebucht,
        /// bestehende OPOS-Ausgleiche der Originalzeilen werden gelöscht.
        /// Der Grund ist Pflicht und wird als Notiz am Storno-Satz festgehalten.
        /// </summary>
        [HttpPost("{id}/storno")]
        public async Task<ActionResult<StornoBuchungssatzInfo>> Stornieren(
            Guid id, [FromBody] StornoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Grund))
            {
                return BadRequest("Für eine Stornobuchung muss ein Grund angegeben werden.");
            }

            var satz = await LadeSatzMitZeilen(id);
            if (satz == null) return NotFound();

            if (await AuthorizeKorrektur(id) is ActionResult verboten) return verboten;

            var schutz = await schutzService.PruefeAsync(satz);
            if (!schutz.KannStornieren)
            {
                return Conflict(schutz.Sperrgrund);
            }

            try
            {
                var storno = await stornoService.StornierenAsync(id, request.Grund);
                return Ok(new StornoBuchungssatzInfo
                {
                    BuchungssatzId = storno.BuchungssatzId,
                    Buchungsnummer = storno.Buchungsnummer,
                    Buchungsjahr = storno.Buchungsjahr,
                    Buchungsdatum = storno.Buchungsdatum,
                    Beschreibung = storno.Beschreibung,
                });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        public class BuchungssatzPatchEntry
        {
            public int Buchungsjahr { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
            public string? Notiz { get; set; }
        }

        /// <summary>
        /// Aktualisiert Metadaten eines Buchungssatzes (Buchungsjahr, Beschreibung, Notiz).
        /// Die Buchungszeilen selbst bleiben unverändert — wer den Satz sehen darf,
        /// darf diese Felder pflegen (analog zum Speichern-Button im Frontend).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<BuchungssatzEntry>> Put(Guid id, [FromBody] BuchungssatzPatchEntry patch)
        {
            if (await ScopedSaetze().AllAsync(s => s.BuchungssatzId != id))
            {
                return await ctx.Buchungssaetze.AnyAsync(s => s.BuchungssatzId == id)
                    ? Forbid()
                    : NotFound();
            }

            var satz = await LadeSatzMitZeilen(id);
            if (satz is null) return NotFound();

            satz.Buchungsjahr = patch.Buchungsjahr;
            satz.Beschreibung = patch.Beschreibung;
            satz.Notiz = patch.Notiz;
            await ctx.SaveChangesAsync();

            var entry = new BuchungssatzEntry(satz);
            var schutz = await schutzService.PruefeAsync(satz);
            entry.KannStornieren = schutz.KannStornieren;
            entry.KannLoeschen = schutz.KannLoeschen;
            entry.Sperrgrund = schutz.Sperrgrund;
            entry.Permissions = new Permissions(read: true) { Update = true };
            return Ok(entry);
        }

        /// <summary>
        /// Löscht einen Buchungssatz endgültig — nur für "freie" Sätze erlaubt
        /// (keine Abrechnung betroffen, keine OPOS-Verknüpfung, kein Storno-Paar).
        /// Für alles andere ist die Stornobuchung der richtige Weg.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var satz = await LadeSatzMitZeilen(id);
            if (satz == null) return NotFound();

            if (await AuthorizeKorrektur(id) is ActionResult verboten) return verboten;

            var schutz = await schutzService.PruefeAsync(satz);
            if (!schutz.KannLoeschen)
            {
                return Conflict(schutz.Sperrgrund);
            }

            ctx.Buchungssaetze.Remove(satz);
            await ctx.SaveChangesAsync();

            return Ok();
        }
    }
}
