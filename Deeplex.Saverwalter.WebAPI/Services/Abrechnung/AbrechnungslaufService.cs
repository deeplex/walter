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

        public AbrechnungslaufService(
            SaverwalterContext ctx,
            NkAnteilBuchungsService nkAnteilBuchungsService,
            AbrechnungsresultatBuchungsService buchungsService,
            AbrechnungsgruppenService gruppenService)
        {
            _ctx = ctx;
            _nkAnteilBuchungsService = nkAnteilBuchungsService;
            _buchungsService = buchungsService;
            _gruppenService = gruppenService;
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
            var einheitenInfos = BuildAbrechnungseinheitenInfos(einheiten, warnungen, prevYearBetragByUmlage);

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

            // Abbrechen wenn irgend ein bereits gebuchter Betrag vom berechneten abweicht —
            // das gilt für Abrechnungsresultate und NK-Anteile gleichermaßen.
            // In dem Fall: gar nichts buchen, erst stornieren.
            var resultatKonflikt = preview.Resultate.Any(r =>
                r.VertragId.HasValue
                && r.GebuchtesAbrechnungsResultat.HasValue
                && Math.Abs(r.GebuchtesAbrechnungsResultat.Value - r.Rechnungsbetrag) > 0.005m);

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

            // Verträge ohne bestehendes Resultat: Abrechnungsresultat buchen.
            // Quelle: NkPartei (Vertrag + VertragInfo) aus den berechneten Einheiten —
            // kein erneuter DB-Trip notwendig.
            var parteiByVertragId = einheiten
                .SelectMany(e => e.Parteien)
                .Where(p => p.Vertrag is not null && p.VertragInfo?.ExistingResultat is null)
                .GroupBy(p => p.Vertrag!.VertragId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var resultat in preview.Resultate
                .Where(r => r.VertragId.HasValue && !r.GebuchtesAbrechnungsResultat.HasValue))
            {
                if (!parteiByVertragId.TryGetValue(resultat.VertragId!.Value, out var partei))
                    continue;

                try
                {
                    BucheAbrechnungsresultat(partei.Vertrag!, jahr, resultat.Rechnungsbetrag, partei.VertragInfo!.Vorauszahlung);
                    resultat.GebuchtesAbrechnungsResultat = resultat.Rechnungsbetrag;
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
                    var nkResult = await _nkAnteilBuchungsService.BucheAnteileAsync(plan.Buchungssatz, plan.Anteile);
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
                // BK-Forderungs-Buchungszeilen (Haben auf NkVK) für das Abrechnungsjahr
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Haben
                                 && z.Buchungssatz.Buchungsjahr == jahr))
                    .ThenInclude(z => z.Buchungssatz)
                        .ThenInclude(s => s.Buchungszeilen)
                            .ThenInclude(z => z.Buchungskonto)
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
                // Vertraege – NK-Buchungskonto mit jahresgefilterter Haben-Vorauszahlung
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Vertraege)
                        .ThenInclude(v => v.NkBuchungskonto)
                            .ThenInclude(k => k.Buchungszeilen
                                .Where(z => z.Buchungssatz.Buchungsjahr == jahr
                                            && z.SollHaben == SollHaben.Haben))
                            .ThenInclude(z => z.Buchungssatz)
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
                // HKVO-Verweis für warme Betriebskosten
                .Include(u => u.HKVO)
                    .ThenInclude(h => h!.AllgemeinWaerme)
                        .ThenInclude(z => z!.Staende)
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
        private void BucheAbrechnungsresultat(Vertrag vertrag, int jahr, decimal rechnungsbetrag, decimal vorauszahlung)
        {
            var saldo = rechnungsbetrag - vorauszahlung;

            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            _buchungsService.BucheAbrechnung(resultat, jahr, vorauszahlung, rechnungsbetrag, saldo);
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
                    var partei = gruppe.First().Partei;
                    var rechnungsbetrag = gruppe.Sum(a => a.Betrag);
                    var info = partei.VertragInfo;

                    var mietZeilen = partei.Vertrag?.MietBuchungskonto.Buchungszeilen
                        .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                        .Select(z => new MietZeileInfo
                        {
                            Buchungsdatum = z.Buchungssatz.Buchungsdatum,
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
                        Wohnflaeche = partei.Wohnung.Wohnflaeche,
                        Nutzflaeche = partei.Wohnung.Nutzflaeche,
                        Nutzeinheiten = partei.Wohnung.Nutzeinheit,
                        Mieten = mietZeilen,
                        PersonenZeitanteile = personenZeitanteile,
                        GebuchtesAbrechnungsResultat = info?.GebuchtesAbrechnungsResultat,
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
            Dictionary<int, decimal> prevYearBetragByUmlage)
        {
            var einheitenByKey =
                new Dictionary<string, (string Namen, List<NkZeileInfo> Zeilen, decimal GesamtWF, decimal GesamtNF, decimal GesamtNE, decimal GesamtMEA)>();

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
                    var gWF = wohnungenInEinheit.Sum(w => w.Wohnflaeche);
                    var gNF = wohnungenInEinheit.Sum(w => w.Nutzflaeche);
                    var gNE = wohnungenInEinheit.Sum(w => (decimal)w.Nutzeinheit);
                    var gMEA = wohnungenInEinheit.Sum(w => w.Miteigentumsanteile);
                    einheitenByKey[einheitKey] = (einheit.Bezeichnung, [], gWF, gNF, gNE, gMEA);
                }

                // KontoId → Partei-Lookup für Merge (planned vs booked); nur Vertrags-Parteien.
                var kontoIdToPartei = new Dictionary<int, NkPartei>();
                foreach (var partei in einheit.Parteien.Where(p => p.Vertrag is not null))
                    kontoIdToPartei[partei.Buchungskonto.BuchungskontoId] = partei;

                foreach (var plan in einheit.Rechnungsplaene)
                {
                    var buchungssatz = plan.Buchungssatz;

                    if (plan.Warnungen.Count > 0)
                        warnungen.AddRange(plan.Warnungen.Select(
                            w => $"BS {buchungssatz.BuchungssatzId}: {w}"));

                    // Geplante Anteile, indexiert nach Buchungskonto-Id
                    var plannedByKonto = plan.Anteile
                        .Select(a => (
                            KontoId: a.Partei.Buchungskonto.BuchungskontoId,
                            Betrag: a.Betrag,
                            Partei: a.Partei))
                        .GroupBy(x => x.KontoId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    // Bereits gebuchte Soll-Zeilen
                    var bookedByKonto = buchungssatz.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Soll)
                        .GroupBy(z => z.Buchungskonto.BuchungskontoId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var mergedAnteile = new List<NkAnteilInfo>();
                    foreach (var kontoId in bookedByKonto.Keys.Union(plannedByKonto.Keys).OrderBy(k => k))
                    {
                        var hasBooked = bookedByKonto.TryGetValue(kontoId, out var bookedList);
                        var hasPlanned = plannedByKonto.TryGetValue(kontoId, out var plannedList);

                        var bookedSum = hasBooked ? bookedList!.Sum(x => x.Betrag) : (decimal?)null;
                        var plannedSum = hasPlanned ? plannedList!.Sum(x => x.Betrag) : (decimal?)null;

                        // Partei bestimmen: aus Plan oder aus booked-Konto-Lookup
                        var partei = hasPlanned ? plannedList![0].Partei : null;
                        if (partei is null && !hasPlanned)
                            kontoIdToPartei.TryGetValue(kontoId, out partei);

                        string bezeichnung = partei is { Vertrag: not null }
                            ? string.Join(", ", partei.Vertrag.Mieter.Select(m => m.Bezeichnung))
                            : "Eigenanteil";

                        var effectiveBetrag = plannedSum ?? bookedSum ?? 0;
                        var anteilFaktor = plan.Betrag > 0
                            ? effectiveBetrag / plan.Betrag
                            : 0;

                        decimal? heizVerbrauchAnteil = null;
                        decimal? wwVerbrauchAnteil = null;
                        List<ZaehlerVerbrauchInfo> heizZaehler = [];
                        List<ZaehlerVerbrauchInfo> wwZaehler = [];
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

                        mergedAnteile.Add(new NkAnteilInfo
                        {
                            VertragId = partei?.Vertrag?.VertragId,
                            Bezeichnung = bezeichnung,
                            GeplanterBetrag = plannedSum,
                            GebuchterBetrag = bookedSum,
                            AnteilFaktor = anteilFaktor,
                            Nutzungsbeginn = partei?.Nutzungsbeginn ?? default,
                            Nutzungsende = partei?.Nutzungsende ?? default,
                            HeizVerbrauchAnteil = heizVerbrauchAnteil,
                            WWVerbrauchAnteil = wwVerbrauchAnteil,
                            HeizZaehler = heizZaehler,
                            WwZaehler = wwZaehler,
                        });
                    }

                    var habenSum = buchungssatz.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag);
                    var sollSum = buchungssatz.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag);
                    bool istVollstaendigGebucht = habenSum > 0 && habenSum == sollSum;

                    einheitenByKey[einheitKey].Zeilen.Add(new NkZeileInfo
                    {
                        UmlageId = plan.Umlage.UmlageId,
                        Bezeichnung = plan.Umlage.Typ.Bezeichnung,
                        Beschreibung = plan.Umlage.Beschreibung ?? "",
                        Schluessel = plan.Umlage.Schluessel.ToDescriptionString(),
                        UmlagetypId = plan.Umlage.Typ.UmlagetypId,
                        BuchungssatzId = buchungssatz.BuchungssatzId,
                        Betrag = plan.Betrag,
                        BetragLetztesJahr = prevYearBetragByUmlage.GetValueOrDefault(plan.Umlage.UmlageId),
                        IstVollstaendigGebucht = istVollstaendigGebucht,
                        Anteile = mergedAnteile,
                        Para9_2 = plan.Para9_2,
                        P7 = plan.Umlage.HKVO?.HKVO_P7,
                        P8 = plan.Umlage.HKVO?.HKVO_P8,
                        GesamtWaerme = plan.GesamtWaerme,
                        GesamtWW = plan.GesamtWW,
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
