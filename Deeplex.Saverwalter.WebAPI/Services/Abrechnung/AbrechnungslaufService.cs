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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.NkGruppenAbrechnungsService;

namespace Deeplex.Saverwalter.WebAPI.Services.Abrechnung
{
    /// <summary>
    /// Orchestriert den gruppenbasierten Abrechnungslauf (Preview + Book).
    ///
    /// Fachlicher Ablauf:
    ///
    ///   Input: wohnungIds + jahr
    ///
    ///   A – Gruppenauflösung:
    ///       wohnungIds → validierte bestehende Abrechnungsgruppe
    ///
    ///   B – PreviewAsync:
    ///       Umlagen im Jahr laden (inkl. aller nötiger Navigations für Vertraege)
    ///       NkGruppenAbrechnungsService.ComputeEinheiten → NkParteien mit Beträgen
    ///       AggregateBetraege → ByVertragId + ByWohnungId
    ///       Vorschau-Modell (Gruppen → Einheiten → NK-Zeilen) zurückgeben
    ///
    ///   C – BookAsync:
    ///       Erneut laden + berechnen
    ///       Abrechnungsresultate je Vertrag persistieren
    ///       NK-Anteile je Rechnung buchen
    ///       Aggregat zurückgeben
    ///
    /// Hinweis: NkAnteileService wird weiterhin für die physische NK-Anteil-Buchung
    /// genutzt (BookAsync). Die Berechnung selbst findet im NkGruppenAbrechnungsService statt.
    /// </summary>
    public class AbrechnungslaufService
    {
        private readonly SaverwalterContext _ctx;
        private readonly NkAnteilBuchungsService _nkAnteilBuchungsService;
        private readonly AbrechnungsresultatBuchungsService _buchungsService;
        private readonly AbrechnungsgruppenService _gruppenService;
        private readonly StornoBuchungsService _stornoService;

        public AbrechnungslaufService(
            SaverwalterContext ctx,
            NkAnteilBuchungsService nkAnteilBuchungsService,
            AbrechnungsresultatBuchungsService buchungsService,
            AbrechnungsgruppenService gruppenService,
            StornoBuchungsService stornoService)
        {
            _ctx = ctx;
            _nkAnteilBuchungsService = nkAnteilBuchungsService;
            _buchungsService = buchungsService;
            _gruppenService = gruppenService;
            _stornoService = stornoService;
        }

        public Task<List<AbrechnungsGruppe>> GetGruppenAsync() =>
            _gruppenService.GetGruppenAsync();

        // ── Preview ───────────────────────────────────────────────────────────

        public async Task<AbrechnungslaufGruppeResult> PreviewAsync(List<int> wohnungIds, int jahr)
        {
            var (preview, _) = await ComputeAsync(wohnungIds, jahr);
            return preview;
        }

        /// <summary>
        /// Wie <see cref="PreviewAsync"/>, gibt aber zusätzlich die rohen <see cref="NkEinheit"/>-Daten
        /// zurück — wird vom <c>AbrechnungslaufPrintService</c> genutzt, um Druckdaten direkt aus den
        /// berechneten Parteien zu bauen (kein separater DB-Trip, keine zweite Berechnung).
        /// </summary>
        public async Task<(AbrechnungslaufGruppeResult Preview, IReadOnlyList<NkEinheit> Einheiten)>
            PreviewWithEinheitenAsync(List<int> wohnungIds, int jahr)
        {
            var (preview, einheiten) = await ComputeAsync(wohnungIds, jahr);
            return (preview, einheiten);
        }

        /// <summary>
        /// Lädt + berechnet Einheiten einmal und gibt sowohl das DTO-Aggregat als auch die
        /// rohen <see cref="NkEinheit"/>-Daten zurück. Letztere werden in <see cref="BookAsync"/>
        /// genutzt, um die NK-Anteile direkt aus den bereits berechneten Rechnungsplänen zu buchen
        /// – ohne erneute Berechnung.
        /// </summary>
        private async Task<(AbrechnungslaufGruppeResult Preview, List<NkEinheit> Einheiten)> ComputeAsync(
            List<int> wohnungIds, int jahr)
        {
            var alleGruppen = await GetGruppenAsync();
            var selectedGruppe = ResolveSelectedGruppe(wohnungIds, alleGruppen);

            var laufUmlagen = await LadeJahresdaten(selectedGruppe.WohnungIds, jahr);

            var einheiten = ComputeEinheiten(laufUmlagen, jahr);

            var umlageIds = laufUmlagen.Select(u => u.UmlageId).ToList();
            var prevYearBetragByUmlage = await LadePrevYearBetraege(umlageIds, jahr);

            var warnungen = new List<string>();
            var einheitenInfos = BuildAbrechnungseinheitenInfos(einheiten, warnungen, prevYearBetragByUmlage, jahr);

            var resultate = BuildResultate(einheiten, jahr);

            var preview = new AbrechnungslaufGruppeResult
            {
                GruppenBezeichnung = selectedGruppe.Bezeichnung,
                WohnungIds = selectedGruppe.WohnungIds,
                Resultate = resultate,
                Abrechnungseinheiten = einheitenInfos,
                Warnungen = warnungen
            };

            return (preview, einheiten);
        }

        private async Task<Dictionary<int, decimal>> LadePrevYearBetraege(List<int> umlageIds, int jahr)
        {
            return await _ctx.Umlagen
                .Where(u => umlageIds.Contains(u.UmlageId))
                .Select(u => new
                {
                    u.UmlageId,
                    Betrag = u.NkVerrechnungsKonto.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Haben
                                 && z.Buchungssatz.Buchungsjahr == jahr - 1)
                        .Sum(z => z.Betrag)
                })
                .ToDictionaryAsync(x => x.UmlageId, x => x.Betrag);
        }

        // ── Book ──────────────────────────────────────────────────────────────

        public async Task<AbrechnungslaufGruppeResult> BookAsync(List<int> wohnungIds, int jahr)
        {
            var (preview, einheiten) = await ComputeAsync(wohnungIds, jahr);

            // Nicht zuordenbarer Verbrauch (fehlender Zählerstand) verhindert das Buchen
            // komplett — erst den Zählerstand ergänzen. Andere Hinweise blockieren nicht.
            var fehler = preview.Warnungen
                .Where(w => w.Contains(ZaehlerstandFehltMarker, StringComparison.Ordinal))
                .Distinct()
                .ToList();
            if (fehler.Count > 0)
                throw new InvalidOperationException(
                    "Abrechnung kann nicht gebucht werden:\n" + string.Join("\n", fehler));

            // Abbrechen wenn irgend ein bereits gebuchter Betrag vom berechneten abweicht —
            // das gilt für Abrechnungsresultate und NK-Anteile gleichermaßen.
            // In dem Fall: gar nichts buchen, erst stornieren.
            var resultatKonflikt = preview.Resultate.Any(r =>
                r.VertragId.HasValue
                && r.GebuchterSaldo.HasValue
                && Math.Abs(r.GebuchterSaldo.Value - r.Saldo) > 0.005m);

            if (resultatKonflikt)
                throw new InvalidOperationException(
                    "Bereits gebuchte Abrechnungsergebnisse widersprechen der aktuellen Berechnung. Bitte erst stornieren.");

            var anteilKonflikt = preview.Abrechnungseinheiten
                .SelectMany(e => e.NkZeilen)
                .SelectMany(z => z.Anteile)
                .Any(a => a.GebuchterBetrag.HasValue
                          && a.GeplanterBetrag.HasValue
                          && Math.Abs(a.GebuchterBetrag.Value - a.GeplanterBetrag.Value) > 0.005m);

            if (anteilKonflikt)
                throw new InvalidOperationException(
                    "Bereits gebuchte NK-Anteile widersprechen der aktuellen Berechnung.");

            // Verträge mit dokumentiertem Abrechnungsverzicht für dieses Jahr: nicht buchen.
            var verzichtVertragIds = (await _ctx.Abrechnungsverzichte
                .Where(v => v.Jahr == jahr && wohnungIds.Contains(v.Vertrag.Wohnung.WohnungId))
                .Select(v => v.Vertrag.VertragId)
                .ToListAsync())
                .ToHashSet();

            // Verträge mit bereits abgesendetem Resultat: explizit warnen und überspringen.
            foreach (var partei in einheiten
                .SelectMany(e => e.Parteien)
                .Where(p => p.Vertrag is not null && p.VertragInfo?.ExistingResultat?.Abgesendet == true))
            {
                var bezeichnung = partei.Vertrag!.Mieter.Any()
                    ? string.Join(", ", partei.Vertrag.Mieter.Select(m => m.Bezeichnung))
                    : $"Vertrag {partei.Vertrag.VertragId}";
                preview.Warnungen.Add(
                    $"Vertrag {partei.Vertrag.VertragId} ({bezeichnung}): Abrechnung wurde bereits versendet und wird übersprungen.");
            }

            // Verträge ohne bestehendes Resultat: Abrechnungsresultat buchen.
            // Quelle: NkPartei (Vertrag + VertragInfo) aus den berechneten Einheiten —
            // kein erneuter DB-Trip notwendig.
            var parteiByVertragId = einheiten
                .SelectMany(e => e.Parteien)
                .Where(p => p.Vertrag is not null && p.VertragInfo?.ExistingResultat is null)
                .GroupBy(p => p.Vertrag!.VertragId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var resultat in preview.Resultate
                .Where(r => r.VertragId.HasValue && !r.GebuchterSaldo.HasValue))
            {
                if (verzichtVertragIds.Contains(resultat.VertragId!.Value))
                {
                    preview.Warnungen.Add(
                        $"Vertrag {resultat.VertragId}: Abrechnungsverzicht für {jahr} hinterlegt — wird nicht gebucht.");
                    continue;
                }

                if (!parteiByVertragId.TryGetValue(resultat.VertragId!.Value, out var partei))
                    continue;

                try
                {
                    BucheAbrechnungsresultat(partei.Vertrag!, jahr, resultat.Saldo);
                    resultat.GebuchterSaldo = resultat.Saldo;
                }
                catch (Exception ex)
                {
                    preview.Warnungen.Add(
                        $"Abrechnung Vertrag {resultat.VertragId}: {ex.GetBaseException().Message}");
                }
            }

            await _ctx.SaveChangesAsync();

            // NK-Anteile direkt aus den bereits berechneten Rechnungsplänen buchen —
            // keine Doppelberechnung mehr (war im Legacy-NkAnteileService).
            foreach (var plan in einheiten.SelectMany(e => e.Rechnungsplaene))
            {
                try
                {
                    // Der Strompauschale-Plan trägt eine transiente Umbuchung
                    // (Soll Betriebsstrom-NkVK / Haben Heizkosten-NkVK). Diese erst
                    // persistieren (idempotent) und dann die Anteile darauf buchen.
                    var satz = plan.IstStrompauschale
                        ? await EnsureStrompauschaleUmbuchungAsync(plan, jahr, preview.Warnungen)
                        : plan.Buchungssatz;
                    if (satz is null) continue;

                    var nkResult = await _nkAnteilBuchungsService.BucheAnteileAsync(satz, plan.Anteile);
                    preview.Warnungen.AddRange(nkResult.Warnungen);
                }
                catch (Exception ex)
                {
                    preview.Warnungen.Add(
                        $"NK-Anteile BS {plan.Buchungssatz.BuchungssatzId}: {ex.GetBaseException().Message}");
                }
            }

            return preview;
        }

        /// <summary>
        /// Stellt die Strompauschale-Umbuchung des Plans in der DB sicher (idempotent):
        /// findet eine vorhandene gleichartige Umbuchung (gleiche Konten + Jahr) wieder,
        /// prüft auf abweichenden Betrag (Konflikt → null) oder legt sie sonst neu an.
        /// </summary>
        private async Task<Buchungssatz?> EnsureStrompauschaleUmbuchungAsync(
            NkGruppenAbrechnungsService.NkRechnungsplan plan, int jahr, List<string> warnungen)
        {
            var transient = plan.Buchungssatz;
            var habenKontoId = transient.Buchungszeilen.First(z => z.SollHaben == SollHaben.Haben).Buchungskonto.BuchungskontoId;
            var sollKontoId = transient.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll).Buchungskonto.BuchungskontoId;

            var vorhanden = await _ctx.Buchungssaetze
                .Include(b => b.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Where(b => b.Buchungsjahr == jahr && b.Beschreibung.StartsWith(NkGruppenAbrechnungsService.StrompauschaleMarker))
                .Where(b => b.StornoNach == null) // bereits stornierte Umbuchungen ignorieren
                .Where(b => b.Buchungszeilen.Any(z => z.SollHaben == SollHaben.Haben && z.Buchungskonto.BuchungskontoId == habenKontoId)
                         && b.Buchungszeilen.Any(z => z.SollHaben == SollHaben.Soll && z.Buchungskonto.BuchungskontoId == sollKontoId))
                .FirstOrDefaultAsync();

            if (vorhanden is not null)
            {
                var gebucht = vorhanden.Buchungszeilen
                    .First(z => z.SollHaben == SollHaben.Haben && z.Buchungskonto.BuchungskontoId == habenKontoId).Betrag;
                if (Math.Abs(gebucht - plan.Betrag) > 0.005m)
                {
                    warnungen.Add(
                        $"Strompauschale-Umbuchung weicht vom gebuchten Betrag ab (gebucht {gebucht:N2} €, berechnet {plan.Betrag:N2} €). Bitte erst stornieren.");
                    return null;
                }
                return vorhanden;
            }

            _ctx.Buchungssaetze.Add(transient);
            return transient;
        }

        // ── Rückabwicklung ────────────────────────────────────────────────────

        public sealed class RueckabwicklungResult
        {
            public int Resultate { get; init; }
            public int Umbuchungen { get; init; }
            public int BereinigteVerteilungen { get; init; }
        }

        /// <summary>
        /// Nimmt die gebuchte Abrechnung einer Gruppe für ein Jahr vollständig zurück:
        /// löscht die Abrechnungsresultate samt Buchungssätzen, entfernt die vom Lauf
        /// gebuchten Verteil-Zeilen von den Rechnungssätzen und löscht die
        /// Strompauschale-Umbuchungen. Manuelle NK-Anteil-Sätze bleiben erhalten.
        /// Gesperrt, sobald ein Resultat als abgesendet markiert ist.
        /// </summary>
        public async Task<RueckabwicklungResult> DeleteAsync(List<int> wohnungIds, int jahr)
        {
            var resultate = await LadeResultate(wohnungIds, jahr);
            if (resultate.Any(r => r.Abgesendet))
                throw new InvalidOperationException(
                    "Mindestens eine Abrechnung wurde bereits versendet — die Gruppe kann nur noch storniert werden.");

            var (verteilKontoIds, umlageNkVkIds) = await LadeGruppenKonten(wohnungIds);
            var verteilSaetze = await LadeVerteilSaetze(umlageNkVkIds, jahr);

            var zuLoeschen = new List<Buchungssatz>();
            foreach (var resultat in resultate)
            {
                zuLoeschen.Add(resultat.Buchungssatz);
                if (resultat.Buchungssatz.StornoNach is Buchungssatz storno)
                    zuLoeschen.Add(storno);
            }

            var umbuchungen = 0;
            var bereinigt = 0;
            var anteilZeilen = new List<Buchungszeile>();
            foreach (var satz in verteilSaetze)
            {
                if (satz.Beschreibung.StartsWith(StrompauschaleMarker))
                {
                    // Vom Lauf erzeugte Umbuchung — komplett entfernen (inkl. Storno-Paar)
                    zuLoeschen.Add(satz);
                    if (satz.StornoNach is Buchungssatz storno)
                        zuLoeschen.Add(storno);
                    umbuchungen++;
                    continue;
                }

                // Rechnungssatz: nur die vom Lauf angefügten Verteil-Zeilen
                // (Vertrags-NK- und Eigenanteil-/AufwandsKonten) entfernen, die Rechnung bleibt.
                var zeilen = satz.Buchungszeilen
                    .Where(z => verteilKontoIds.Contains(z.Buchungskonto.BuchungskontoId))
                    .ToList();
                if (zeilen.Count == 0) continue;
                anteilZeilen.AddRange(zeilen);
                bereinigt++;
            }

            await EntferneOpos(zuLoeschen.SelectMany(s => s.Buchungszeilen).Concat(anteilZeilen));
            _ctx.Buchungszeilen.RemoveRange(anteilZeilen);
            _ctx.Abrechnungsresultate.RemoveRange(resultate);
            _ctx.Buchungssaetze.RemoveRange(zuLoeschen);
            await _ctx.SaveChangesAsync();

            return new RueckabwicklungResult
            {
                Resultate = resultate.Count,
                Umbuchungen = umbuchungen,
                BereinigteVerteilungen = bereinigt
            };
        }

        /// <summary>
        /// Storniert die gebuchte Abrechnung einer Gruppe GoB-konform — der Weg für
        /// bereits abgesendete Abrechnungen: Storno-Sätze für Resultate und
        /// Umbuchungen, Korrektur-Sätze für die Verteil-Zeilen der Rechnungssätze.
        /// Die Abrechnungsresultate bleiben als Beleg bestehen.
        /// </summary>
        public async Task<RueckabwicklungResult> StornoAsync(List<int> wohnungIds, int jahr, string grund)
        {
            var resultate = await LadeResultate(wohnungIds, jahr);
            var (verteilKontoIds, umlageNkVkIds) = await LadeGruppenKonten(wohnungIds);
            var verteilSaetze = await LadeVerteilSaetze(umlageNkVkIds, jahr);

            var stornierteResultate = 0;
            foreach (var resultat in resultate
                .Where(r => r.Buchungssatz.StornoNach == null))
            {
                await _stornoService.StornierenAsync(resultat.Buchungssatz.BuchungssatzId, grund);
                stornierteResultate++;
            }

            var umbuchungen = 0;
            var bereinigt = 0;
            foreach (var satz in verteilSaetze.Where(s => s.StornoNach == null && s.StornoVon == null))
            {
                if (satz.Beschreibung.StartsWith(StrompauschaleMarker))
                {
                    await _stornoService.StornierenAsync(satz.BuchungssatzId, grund);
                    umbuchungen++;
                    continue;
                }

                var zeilen = satz.Buchungszeilen
                    .Where(z => verteilKontoIds.Contains(z.Buchungskonto.BuchungskontoId))
                    .ToList();
                if (zeilen.Count == 0) continue;

                // Schon korrigiert? (Re-Runs dürfen keine doppelten Gegenbuchungen erzeugen)
                var beschreibung = $"Storno NK-Verteilung: {satz.Beschreibung}";
                if (await _ctx.Buchungssaetze.AnyAsync(k =>
                        k.Buchungsjahr == jahr && k.Beschreibung == beschreibung))
                    continue;

                var korrektur = new Buchungssatz(DateOnly.FromDateTime(DateTime.Today), beschreibung)
                {
                    Buchungsjahr = jahr,
                    Notiz = grund
                };
                foreach (var zeile in zeilen)
                {
                    korrektur.Buchungszeilen.Add(new Buchungszeile(
                        zeile.SollHaben == SollHaben.Soll ? SollHaben.Haben : SollHaben.Soll,
                        zeile.Betrag)
                    {
                        Buchungssatz = korrektur,
                        Buchungskonto = zeile.Buchungskonto
                    });
                }
                await EntferneOpos(zeilen);
                _ctx.Buchungssaetze.Add(korrektur);
                bereinigt++;
            }

            await _ctx.SaveChangesAsync();

            return new RueckabwicklungResult
            {
                Resultate = stornierteResultate,
                Umbuchungen = umbuchungen,
                BereinigteVerteilungen = bereinigt
            };
        }

        private Task<List<Abrechnungsresultat>> LadeResultate(List<int> wohnungIds, int jahr) =>
            _ctx.Abrechnungsresultate
                .Include(r => r.Buchungssatz)
                    .ThenInclude(s => s.Buchungszeilen)
                .Include(r => r.Buchungssatz)
                    .ThenInclude(s => s.StornoNach)
                        .ThenInclude(st => st!.Buchungszeilen)
                .Where(r => wohnungIds.Contains(r.Vertrag.Wohnung.WohnungId)
                         && r.Buchungssatz.Buchungsjahr == jahr)
                .ToListAsync();

        private async Task<(List<int> VerteilKontoIds, List<int> UmlageNkVkIds)> LadeGruppenKonten(
            List<int> wohnungIds)
        {
            // Der Lauf bucht Verteil-Zeilen auf die NK-Konten der Verträge UND — für
            // Leerstand/Eigenanteil — auf die AufwandsKonten der Wohnungen. Beide müssen
            // beim Zurücknehmen/Stornieren bereinigt werden, sonst bleibt der Eigenanteil stehen.
            var vertragNkKontoIds = await _ctx.Vertraege
                .Where(v => wohnungIds.Contains(v.Wohnung.WohnungId))
                .Select(v => v.NkBuchungskonto.BuchungskontoId)
                .ToListAsync();
            var aufwandsKontoIds = await _ctx.Wohnungen
                .Where(w => wohnungIds.Contains(w.WohnungId))
                .Select(w => w.AufwandsKonto.BuchungskontoId)
                .ToListAsync();
            var umlageNkVkIds = await _ctx.Umlagen
                .Where(u => u.Wohnungen.Any(w => wohnungIds.Contains(w.WohnungId)))
                .Select(u => u.NkVerrechnungsKonto.BuchungskontoId)
                .ToListAsync();
            var verteilKontoIds = vertragNkKontoIds.Concat(aufwandsKontoIds).Distinct().ToList();
            return (verteilKontoIds, umlageNkVkIds);
        }

        /// <summary>
        /// Sätze des Jahres mit Haben auf einem NK-Verrechnungskonto der Gruppe:
        /// BK-Rechnungen (tragen die Verteil-Zeilen des Laufs) und
        /// Strompauschale-Umbuchungen. Manuelle NK-Anteil-Sätze sind ausgenommen.
        /// </summary>
        private Task<List<Buchungssatz>> LadeVerteilSaetze(List<int> umlageNkVkIds, int jahr) =>
            _ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.Buchungskonto)
                .Include(s => s.StornoNach)
                    .ThenInclude(st => st!.Buchungszeilen)
                .Where(s => s.Buchungsjahr == jahr
                         && !s.Beschreibung.StartsWith(NkAnteilBuchungsService.BeschreibungPrefix)
                         && s.Buchungszeilen.Any(z => z.SollHaben == SollHaben.Haben
                                && umlageNkVkIds.Contains(z.Buchungskonto.BuchungskontoId)))
                .ToListAsync();

        private async Task EntferneOpos(IEnumerable<Buchungszeile> zeilen)
        {
            var zeileIds = zeilen.Select(z => z.BuchungszeileId).Distinct().ToList();
            var ausgleiche = await _ctx.OffenePostenAusgleiche
                .Where(a => zeileIds.Contains(a.SollZeile.BuchungszeileId)
                         || zeileIds.Contains(a.HabenZeile.BuchungszeileId))
                .ToListAsync();
            _ctx.OffenePostenAusgleiche.RemoveRange(ausgleiche);
        }

        // ── Datenladung ───────────────────────────────────────────────────────

        /// <summary>
        /// Lädt alle Umlagen für eine Gruppe und ein Jahr mit allen für
        /// NkGruppenAbrechnungsService und die Preview-/Buchungslogik nötigen Navigations.
        /// Vertraege werden tief über die Wohnungen inkludiert – kein separater Vertrags-Query.
        /// </summary>
        private async Task<List<Umlage>> LadeJahresdaten(List<int> groupWohnungIds, int jahr)
        {
            return await _ctx.Umlagen
                .Include(u => u.Typ)
                // NkVK-Buchungszeilen des Abrechnungsjahrs (Buchungsjahr, NICHT
                // Buchungsdatum — Vorjahresrechnungen werden oft erst im Folgejahr
                // bezahlt): Forderungen (Haben) UND Gutschriften/Zahlungen (Soll) —
                // die Rechnungs-/Zahlungs-Unterscheidung trifft
                // NkGruppenAbrechnungsService über den OPOS-Ausgleich (AlsSollZeile).
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen
                        .Where(z => z.Buchungssatz.Buchungsjahr == jahr))
                    .ThenInclude(z => z.Buchungssatz)
                        .ThenInclude(s => s.Buchungszeilen)
                            .ThenInclude(z => z.Buchungskonto)
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen
                        .Where(z => z.Buchungssatz.Buchungsjahr == jahr))
                    .ThenInclude(z => z.AlsSollZeile)
                // Wohnungen-Stammdaten
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.AufwandsKonto)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Adresse)
                // Vertraege – Basis
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.Versionen)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.Mieter)
                // Vertraege – NK-Buchungskonto mit jahresgefilterter Haben-Vorauszahlung.
                // Inkl. Geschwisterzeilen des Satzes (mit Konto), um echte Vorauszahlungen
                // von NK-Anteil-Gutschriften (Satz berührt ein NkVerrechnungskonto) zu
                // unterscheiden.
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.NkBuchungskonto)
                            .ThenInclude(k => k.Buchungszeilen
                                .Where(z => z.Buchungssatz.Buchungsjahr == jahr
                                            && z.SollHaben == SollHaben.Haben))
                            .ThenInclude(z => z.Buchungssatz)
                                .ThenInclude(s => s.Buchungszeilen)
                                    .ThenInclude(zz => zz.Buchungskonto)
                // Vertraege – Miet-Buchungskonto für MietSaldo
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.MietBuchungskonto)
                            .ThenInclude(k => k.Buchungszeilen
                                .Where(z => z.Buchungssatz.Buchungsjahr == jahr))
                            .ThenInclude(z => z.Buchungssatz)
                // Vertraege – BkAbrechnungsKonto für Vergleich mit gebuchtem Betrag
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.BkAbrechnungsKonto)
                // Vertraege – Abrechnungsresultate mit Buchungszeilen für GebuchtesAbrechnungsResultat
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.Abrechnungsresultate
                            .Where(r => r.Buchungssatz.Buchungsjahr == jahr))
                        .ThenInclude(r => r.Buchungssatz)
                            .ThenInclude(s => s.Buchungszeilen)
                                .ThenInclude(z => z.Buchungskonto)
                // Versionen für Schluessel-Lookup
                .Include(u => u.Versionen)
                // HKVO-Verlauf für warme Betriebskosten
                .Include(u => u.HeizkostenHKVOs)
                    .ThenInclude(h => h.AllgemeinWaermeZaehler)
                        .ThenInclude(z => z.Staende)
                // Betriebsstrom-Umlage (+ Verrechnungskonto) für die Strompauschale-Umbuchung
                .Include(u => u.HeizkostenHKVOs)
                    .ThenInclude(h => h.Betriebsstrom)
                        .ThenInclude(b => b.NkVerrechnungsKonto)
                // Wohnungsversionen für Flächenberechnung
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Versionen)
                // Zähler für Verbrauch-Schlüssel und HKVO-Messung
                .Include(u => u.Zaehler)
                    .ThenInclude(z => z.Staende)
                .Include(u => u.Zaehler)
                    .ThenInclude(z => z.Wohnung)
                .Where(u => u.Wohnungen.Any(w => groupWohnungIds.Contains(w.WohnungId)))
                .Where(u => u.NkVerrechnungsKonto.Buchungszeilen.Any(z =>
                    z.SollHaben == SollHaben.Haben
                    && z.Buchungssatz.Buchungsjahr == jahr))
                .AsSplitQuery()
                .ToListAsync();
        }

        // ── Buchungslogik ─────────────────────────────────────────────────────

        /// <summary>
        /// Bucht das Abrechnungsresultat eines Vertrags. Idempotenz-Prüfung ist
        /// bereits vor dem Aufruf erfolgt (Partei ohne <c>ExistingResultat</c>).
        /// <c>SaveChangesAsync</c> wird vom Caller einmalig für alle Verträge der Gruppe ausgeführt.
        /// </summary>
        private void BucheAbrechnungsresultat(Vertrag vertrag, int jahr, decimal saldo)
        {
            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            _buchungsService.BucheAbrechnung(resultat, jahr, saldo);
            _ctx.Abrechnungsresultate.Add(resultat);
        }

        // ── Resultate / NK-Zeilen-Aufbau ─────────────────────────────────────

        /// <summary>
        /// Aggregiert die NK-Anteile aller Einheiten zu je einer Zeile pro Partei-Identität.
        /// Vertrags-Parteien werden über die Vertrag-Referenz dedupliziert (EF identity-mapped),
        /// Eigenanteil-Parteien über die NkPartei-Instanz (eine je Leerstand-Intervall).
        /// </summary>
        private static List<AbrechnungsresultatInfo> BuildResultate(
            List<NkEinheit> einheiten,
            int jahr)
        {
            return [.. einheiten
                .SelectMany(e => e.Rechnungsplaene)
                .SelectMany(p => p.Anteile)
                .GroupBy(a => a.Partei.Vertrag is not null ? (object)a.Partei.Vertrag : a.Partei)
                .Select(gruppe =>
                {
                    // Ein Vertrag hat je Einheit eine eigene NkPartei. Für die Anzeige
                    // der Personen-Intervalle die Partei aus der größten Einheit nehmen
                    // (max. GesamtPersonenzahl) — sonst zeigt ein Vertrag mit einer
                    // Einzel-Wohnungs-Umlage (z.B. Grundsteuer) fälschlich "1/1".
                    var partei = gruppe
                        .Select(a => a.Partei)
                        .OrderByDescending(p => p.PersonenZeitanteile.Sum(z => z.GesamtPersonenzahl))
                        .First();
                    var rechnungsbetrag = gruppe.Sum(a => a.Betrag);
                    var info = partei.VertragInfo;

                    var mietZeilen = partei.Vertrag?.MietBuchungskonto.Buchungszeilen
                        .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                        .Select(z => new MietZeileInfo
                        {
                            Buchungsdatum = z.Buchungssatz.Buchungsdatum,
                            Buchungsjahr = z.Buchungssatz.Buchungsjahr,
                            Beschreibung = z.Buchungssatz.Beschreibung,
                            IstSoll = z.SollHaben == SollHaben.Soll,
                            Betrag = z.Betrag,
                        })
                        .ToList() ?? [];

                    var kaltmieteSoll = mietZeilen
                        .Where(z => z.IstSoll)
                        .Sum(z => z.Betrag);

                    var personenZeitanteile = partei.PersonenZeitanteile
                        .Select(p => new PersonenZeitanteilInfo
                        {
                            Beginn = p.Beginn,
                            Ende = p.Ende,
                            Tage = (int)p.Tage,
                            Personenzahl = p.Personenzahl,
                            GesamtPersonenzahl = p.GesamtPersonenzahl,
                            Anteil = p.Anteil,
                        })
                        .ToList();

                    return new AbrechnungsresultatInfo
                    {
                        AbrechnungsresultatId = info?.ExistingResultat?.AbrechnungsresultatId ?? Guid.Empty,
                        VertragId = partei.Vertrag?.VertragId,
                        WohnungId = partei.Wohnung.WohnungId,
                        WohnungBezeichnung =
                            $"{partei.Wohnung.Adresse?.Anschrift ?? "?"} – {partei.Wohnung.Bezeichnung}",
                        MieterBezeichnung = partei.Vertrag is { } v
                            ? string.Join(", ", v.Mieter.Select(m => m.Bezeichnung))
                            : "Leerstand",
                        NutzungVon = partei.Vertrag is { } v2 && v2.Versionen.Count > 0
                            ? v2.Versionen.Min(ver => ver.Beginn)
                            : partei.Nutzungsbeginn,
                        NutzungBis = partei.Vertrag?.Ende ?? partei.Nutzungsende,
                        Jahr = jahr,
                        Rechnungsbetrag = rechnungsbetrag,
                        Vorauszahlung = info?.Vorauszahlung ?? 0,
                        Saldo = rechnungsbetrag - (info?.Vorauszahlung ?? 0),
                        MietSaldo = info?.MietSaldo ?? 0,
                        KaltmieteSoll = kaltmieteSoll,
                        Wohnflaeche = partei.Wohnung.VersionAt(new DateOnly(jahr, 12, 31)).Wohnflaeche,
                        Nutzflaeche = partei.Wohnung.VersionAt(new DateOnly(jahr, 12, 31)).Nutzflaeche,
                        Nutzeinheiten = partei.Wohnung.VersionAt(new DateOnly(jahr, 12, 31)).Nutzeinheit,
                        Mieten = mietZeilen,
                        PersonenZeitanteile = personenZeitanteile,
                        GebuchterSaldo = info?.GebuchterSaldo,
                        Abgesendet = info?.ExistingResultat?.Abgesendet,
                    };
                })];
        }

        /// <summary>
        /// Baut die AbrechnungseinheitInfos aus den NkEinheiten auf.
        /// Merged geplante Anteile (aus NkGruppenAbrechnungsService) mit bereits gebuchten
        /// Zeilen (aus rechnung.Buchungssatz).
        /// </summary>
        private static List<AbrechnungseinheitInfo> BuildAbrechnungseinheitenInfos(
            List<NkEinheit> einheiten,
            List<string> warnungen,
            Dictionary<int, decimal> prevYearBetragByUmlage,
            int jahr)
        {
            var einheitenByKey =
                new Dictionary<string, (string Namen, List<NkZeileInfo> Zeilen, decimal GesamtWF, decimal GesamtNF, decimal GesamtNE, decimal GesamtMEA, List<StrompauschaleInfo> Strompauschalen)>();

            foreach (var einheit in einheiten)
            {
                // Wohnungen aus den Umlagen ableiten (alle Wohnungen in dieser Abrechnungseinheit)
                var wohnungenInEinheit = einheit.Umlagen
                    .SelectMany(u => u.Wohnungen)
                    .DistinctBy(w => w.WohnungId)
                    .ToList();
                var wohnungIds = wohnungenInEinheit
                    .Select(w => w.WohnungId)
                    .OrderBy(id => id)
                    .ToList();
                var einheitKey = string.Join(",", wohnungIds);

                if (!einheitenByKey.ContainsKey(einheitKey))
                {
                    var abrechnungsEnde = new DateOnly(einheit.Rechnungsplaene.FirstOrDefault()?.Buchungssatz.Buchungsjahr ?? DateTime.Now.Year, 12, 31);
                    var gWF = wohnungenInEinheit.Sum(w => w.VersionAt(abrechnungsEnde).Wohnflaeche);
                    var gNF = wohnungenInEinheit.Sum(w => w.VersionAt(abrechnungsEnde).Nutzflaeche);
                    var gNE = wohnungenInEinheit.Sum(w => (decimal)w.VersionAt(abrechnungsEnde).Nutzeinheit);
                    var gMEA = wohnungenInEinheit.Sum(w => w.VersionAt(abrechnungsEnde).Miteigentumsanteile);
                    einheitenByKey[einheitKey] = (einheit.Bezeichnung, [], gWF, gNF, gNE, gMEA, []);
                }

                einheitenByKey[einheitKey].Strompauschalen.AddRange(einheit.Strompauschalen.Select(sp =>
                    new StrompauschaleInfo
                    {
                        HeizUmlageId = sp.HeizUmlageId,
                        HeizBezeichnung = sp.HeizBezeichnung,
                        BetriebsstromUmlageId = sp.BetriebsstromUmlageId,
                        BetriebsstromBezeichnung = sp.BetriebsstromBezeichnung,
                        Delta = sp.Delta,
                        Warnungen = [.. sp.Warnungen]
                    }));

                // KontoId → Partei-Lookup für Merge (planned vs booked); nur Vertrags-Parteien.
                var kontoIdToPartei = new Dictionary<int, NkPartei>();
                foreach (var partei in einheit.Parteien.Where(p => p.Vertrag is not null))
                    kontoIdToPartei[partei.Buchungskonto.BuchungskontoId] = partei;

                // Verrechnungskonten der Einheit: Soll-Zeilen darauf sind Umbuchungen
                // (z.B. Strompauschale), keine Mieter-/Eigenanteile.
                var verrechnungsKontoIds = einheit.Umlagen
                    .Select(u => u.NkVerrechnungsKonto.BuchungskontoId)
                    .ToHashSet();

                foreach (var plan in einheit.Rechnungsplaene)
                {
                    var buchungssatz = plan.Buchungssatz;

                    if (plan.Warnungen.Count > 0)
                        warnungen.AddRange(plan.Warnungen.Select(
                            w => $"BS {buchungssatz.BuchungssatzId}: {w}"));

                    // Strompauschale-Umbuchung: reines Buchungs-Artefakt, keine Anzeigezeile.
                    if (plan.IstStrompauschale)
                        continue;

                    // Geplante Anteile, indexiert nach Buchungskonto-Id
                    var plannedByKonto = plan.Anteile
                        .Select(a => (
                            KontoId: a.Partei.Buchungskonto.BuchungskontoId,
                            Betrag: a.Betrag,
                            Partei: a.Partei))
                        .GroupBy(x => x.KontoId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    // Bereits gebuchte Anteil-Zeilen (Transfer-/Umbuchungszeilen auf
                    // Verrechnungskonten ausgeschlossen – das sind keine Anteile).
                    // Soll positiv, Haben negativ — Gutschrift-Anteile werden als
                    // Haben-Zeilen gebucht und müssen negativ gegen den Plan stehen.
                    var bookedByKonto = buchungssatz.Buchungszeilen
                        .Where(z => !verrechnungsKontoIds.Contains(z.Buchungskonto.BuchungskontoId))
                        .GroupBy(z => z.Buchungskonto.BuchungskontoId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Sum(z => z.SollHaben == SollHaben.Soll ? z.Betrag : -z.Betrag));

                    var mergedAnteile = new List<NkAnteilInfo>();
                    foreach (var kontoId in bookedByKonto.Keys.Union(plannedByKonto.Keys).OrderBy(k => k))
                    {
                        var hasBooked = bookedByKonto.TryGetValue(kontoId, out var booked);
                        var hasPlanned = plannedByKonto.TryGetValue(kontoId, out var plannedList);

                        var bookedSum = hasBooked ? booked : (decimal?)null;
                        var plannedSum = hasPlanned ? plannedList!.Sum(x => x.Betrag) : (decimal?)null;

                        // Partei bestimmen: aus Plan oder aus booked-Konto-Lookup
                        var partei = hasPlanned ? plannedList![0].Partei : null;
                        if (partei is null && !hasPlanned)
                            kontoIdToPartei.TryGetValue(kontoId, out partei);

                        string bezeichnung = partei is { Vertrag: not null }
                            ? string.Join(", ", partei.Vertrag.Mieter.Select(m => m.Bezeichnung))
                            : "Eigenanteil";

                        var effectiveBetrag = plannedSum ?? bookedSum ?? 0;
                        var anteilFaktor = plan.Betrag != 0
                            ? effectiveBetrag / plan.Betrag
                            : 0;

                        decimal? heizVerbrauchAnteil = null;
                        decimal? wwVerbrauchAnteil = null;
                        List<ZaehlerVerbrauchInfo> heizZaehler = [];
                        List<ZaehlerVerbrauchInfo> wwZaehler = [];
                        List<ZaehlerVerbrauchInfo> verbrauchZaehler = [];
                        if (plan.HkvoVerbrauchAnteile != null && partei != null
                            && plan.HkvoVerbrauchAnteile.TryGetValue(partei, out var hkvo))
                        {
                            heizVerbrauchAnteil = hkvo.HeizAnteil;
                            wwVerbrauchAnteil = hkvo.WWAnteil;
                            heizZaehler = hkvo.HeizZaehler.Select(x => new ZaehlerVerbrauchInfo
                            {
                                ZaehlerId = x.Zaehler.ZaehlerId,
                                Kennnummer = x.Zaehler.Kennnummer,
                                Verbrauch = x.Verbrauch,
                                Einheit = "kWh",
                            }).ToList();
                            wwZaehler = hkvo.WwZaehler.Select(x => new ZaehlerVerbrauchInfo
                            {
                                ZaehlerId = x.Zaehler.ZaehlerId,
                                Kennnummer = x.Zaehler.Kennnummer,
                                Verbrauch = x.Verbrauch,
                                Einheit = "m³",
                            }).ToList();
                        }
                        else if (plan.HkvoVerbrauchAnteile == null && partei?.VerbrauchAnteileDetail != null
                            && partei.VerbrauchAnteileDetail.TryGetValue(plan.Umlage.UmlageId, out var vDetail))
                        {
                            verbrauchZaehler = vDetail.DieseZaehler
                                .SelectMany(kv => kv.Value.Select(v => new ZaehlerVerbrauchInfo
                                {
                                    ZaehlerId = v.Zaehler.ZaehlerId,
                                    Kennnummer = v.Zaehler.Kennnummer,
                                    Verbrauch = v.Delta,
                                    Einheit = kv.Key.ToUnitString(),
                                }))
                                .ToList();
                        }

                        mergedAnteile.Add(new NkAnteilInfo
                        {
                            VertragId = partei?.Vertrag?.VertragId,
                            Bezeichnung = bezeichnung,
                            GeplanterBetrag = plannedSum,
                            GebuchterBetrag = bookedSum,
                            AnteilFaktor = anteilFaktor,
                            NfZeitanteil = partei?.NFZeitanteil ?? 0,
                            Nutzungsbeginn = partei?.Nutzungsbeginn ?? default,
                            Nutzungsende = partei?.Nutzungsende ?? default,
                            HeizVerbrauchAnteil = heizVerbrauchAnteil,
                            WWVerbrauchAnteil = wwVerbrauchAnteil,
                            HeizZaehler = heizZaehler,
                            WwZaehler = wwZaehler,
                            VerbrauchZaehler = verbrauchZaehler,
                        });
                    }

                    // Vollständig gebucht = jeder geplante Anteil ist als Zeile gebucht.
                    // Bewusst NICHT über den Satz-Ausgleich (Soll >= Haben) definiert:
                    // beim Betriebsstrom wandert die Strompauschale in eine separate
                    // Umbuchung, der Forderungssatz selbst gleicht sich nie aus.
                    bool istVollstaendigGebucht = mergedAnteile.Count > 0
                        && mergedAnteile.All(a =>
                            a.GebuchterBetrag.HasValue
                            && (!a.GeplanterBetrag.HasValue
                                || Math.Abs(a.GebuchterBetrag.Value - a.GeplanterBetrag.Value) <= 0.005m));

                    decimal? gesamtVerbrauch = null;
                    string? verbrauchEinheit = null;
                    if (plan.HkvoVerbrauchAnteile == null)
                    {
                        var verbrauchPartei = plan.Anteile
                            .Select(a => a.Partei)
                            .FirstOrDefault(p => p.Vertrag != null && p.VerbrauchAnteileDetail.ContainsKey(plan.Umlage.UmlageId));
                        if (verbrauchPartei != null)
                        {
                            var detail = verbrauchPartei.VerbrauchAnteileDetail[plan.Umlage.UmlageId];
                            foreach (var kv in detail.AlleVerbrauch.Where(kv => kv.Value > 0).Take(1))
                            {
                                gesamtVerbrauch = kv.Value;
                                verbrauchEinheit = kv.Key.ToUnitString();
                            }
                        }
                    }

                    einheitenByKey[einheitKey].Zeilen.Add(new NkZeileInfo
                    {
                        UmlageId = plan.Umlage.UmlageId,
                        Bezeichnung = plan.Umlage.Typ.Bezeichnung,
                        Beschreibung = plan.Umlage.Beschreibung ?? "",
                        Schluessel = plan.Umlage.VersionAt(new DateOnly(plan.Buchungssatz.Buchungsjahr, 12, 31)).Schluessel.ToDescriptionString(),
                        UmlagetypId = plan.Umlage.Typ.UmlagetypId,
                        BuchungssatzId = buchungssatz.BuchungssatzId,
                        Betrag = plan.Betrag,
                        BetragLetztesJahr = prevYearBetragByUmlage.GetValueOrDefault(plan.Umlage.UmlageId),
                        IstVollstaendigGebucht = istVollstaendigGebucht,
                        Anteile = mergedAnteile,
                        Para9_2 = plan.Para9_2,
                        P9Details = plan.P9Details is { } p9 ? new P9DetailsInfo
                        {
                            V = p9.V,
                            Q = p9.Q,
                            Tw = p9.Tw,
                            AllgemeinZaehler = p9.AllgemeinZaehlerKennnummer,
                            QAnfangsdatum = p9.QAnfangsdatum,
                            QEnddatum = p9.QEnddatum,
                            WwZaehler = p9.WwZaehler.Select(x => new ZaehlerVerbrauchInfo
                            {
                                ZaehlerId = x.Zaehler.ZaehlerId,
                                Kennnummer = x.Zaehler.Kennnummer,
                                Verbrauch = x.Verbrauch,
                                Einheit = "m³",
                            }).ToList(),
                        } : null,
                        P7 = plan.Umlage.HkvoAt(new DateOnly(plan.Buchungssatz.Buchungsjahr, 12, 31))?.HKVO_P7,
                        P8 = plan.Umlage.HkvoAt(new DateOnly(plan.Buchungssatz.Buchungsjahr, 12, 31))?.HKVO_P8,
                        GesamtWaerme = plan.GesamtWaerme,
                        GesamtWW = plan.GesamtWW,
                        GesamtVerbrauch = gesamtVerbrauch,
                        VerbrauchEinheit = verbrauchEinheit,
                    });
                }

                // Umlagen ohne Buchungen im Abrechnungsjahr als fehlend markieren
                var endeJahr = new DateOnly(jahr, 12, 31);
                var coveredUmlageIds = einheit.Rechnungsplaene
                    .Where(p => !p.IstStrompauschale)
                    .Select(p => p.Umlage.UmlageId)
                    .ToHashSet();
                foreach (var umlage in einheit.Umlagen.Where(u => !coveredUmlageIds.Contains(u.UmlageId)))
                {
                    warnungen.Add($"Keine Buchungen für '{umlage.Typ.Bezeichnung}' im Jahr {jahr}.");
                    einheitenByKey[einheitKey].Zeilen.Add(new NkZeileInfo
                    {
                        UmlageId = umlage.UmlageId,
                        Bezeichnung = umlage.Typ.Bezeichnung,
                        Beschreibung = umlage.Beschreibung ?? "",
                        Schluessel = umlage.VersionAt(endeJahr).Schluessel.ToDescriptionString(),
                        UmlagetypId = umlage.Typ.UmlagetypId,
                        BuchungssatzId = null,
                        Betrag = 0,
                        BetragLetztesJahr = prevYearBetragByUmlage.GetValueOrDefault(umlage.UmlageId),
                        IstVollstaendigGebucht = false,
                        IstFehlend = true,
                        Anteile = [],
                    });
                }
            }

            return [.. einheitenByKey
                .OrderBy(kvp => kvp.Value.Namen)
                .Select(kvp => new AbrechnungseinheitInfo
                {
                    WohnungNamen = kvp.Value.Namen,
                    NkZeilen = [.. kvp.Value.Zeilen.OrderBy(z => z.Bezeichnung, StringComparer.OrdinalIgnoreCase)],
                    GesamtWohnflaeche = kvp.Value.GesamtWF,
                    GesamtNutzflaeche = kvp.Value.GesamtNF,
                    GesamtNutzeinheit = kvp.Value.GesamtNE,
                    GesamtMiteigentumsanteile = kvp.Value.GesamtMEA,
                    Strompauschalen = kvp.Value.Strompauschalen,
                })];
        }

        // ── Gruppenauflösung ──────────────────────────────────────────────────

        private static AbrechnungsGruppe ResolveSelectedGruppe(
            List<int> wohnungIds,
            List<AbrechnungsGruppe> alleGruppen)
        {
            var normalizedKey = BuildWohnungIdsKey(wohnungIds);
            if (string.IsNullOrWhiteSpace(normalizedKey))
                throw new InvalidOperationException("Leere Abrechnungsgruppen sind nicht zulässig.");

            var gruppenByKey = alleGruppen.ToDictionary(g => BuildWohnungIdsKey(g.WohnungIds));

            if (!gruppenByKey.TryGetValue(normalizedKey, out var gruppe))
                throw new InvalidOperationException(
                    "Die Auswahl muss exakt bestehenden Abrechnungsgruppen entsprechen. " +
                    $"Ungültige Gruppe: {normalizedKey}");

            return gruppe;
        }

        private static string BuildWohnungIdsKey(IEnumerable<int> wohnungIds)
            => string.Join(",", wohnungIds.Distinct().OrderBy(id => id));
    }
}
