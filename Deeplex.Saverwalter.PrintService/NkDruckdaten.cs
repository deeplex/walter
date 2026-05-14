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
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.NkGruppenAbrechnungsService;
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.Utils;

namespace Deeplex.Saverwalter.PrintService
{
    /// <summary>Druckdaten für eine NK-Abrechnung je Mieter-Partei.</summary>
    public sealed class NkDruckdaten
    {
        // ── Briefkopf ─────────────────────────────────────────────────────────
        public required Kontakt Vermieter { get; init; }
        public required Kontakt Ansprechpartner { get; init; }
        public required List<Kontakt> Mieter { get; init; }
        public required Wohnung Wohnung { get; init; }

        // ── Zeitraum ──────────────────────────────────────────────────────────
        public required int Jahr { get; init; }
        public required DateOnly Nutzungsbeginn { get; init; }
        public required DateOnly Nutzungsende { get; init; }
        /// <summary>Gesamttage des Abrechnungsjahres (365 oder 366).</summary>
        public required int Abrechnungstage { get; init; }

        // ── Finanzen ──────────────────────────────────────────────────────────
        public required decimal Vorauszahlung { get; init; }
        public required decimal Rechnungsbetrag { get; init; }
        /// <summary>Positiv = Guthaben des Mieters; negativ = Nachforderung.</summary>
        public decimal Saldo => Vorauszahlung - Rechnungsbetrag;
        public required decimal Mietminderung { get; init; }
        public required decimal NebenkostenMietminderung { get; init; }

        // ── NK-Einheiten ──────────────────────────────────────────────────────
        public required IReadOnlyList<NkDruckEinheit> Einheiten { get; init; }
        public required IReadOnlyList<PersonenZeitanteil> PersonenZeitanteile { get; init; }
        public required List<Note> Notes { get; init; }

        // ── Builder ───────────────────────────────────────────────────────────

        /// <summary>
        /// Baut NkDruckdaten aus einer Mieter-NkPartei und allen NkEinheiten des Laufs.
        /// Nur Einheiten, in denen diese Partei einen Anteil hat, werden einbezogen.
        /// </summary>
        public static NkDruckdaten Build(
            NkPartei targetPartei,
            IReadOnlyList<NkEinheit> alleEinheiten,
            int jahr)
        {
            if (targetPartei.Vertrag is null)
                throw new ArgumentException("targetPartei must be a Mieter-Partei (Vertrag != null).");

            var vertrag = targetPartei.Vertrag;
            var vermieter = vertrag.Wohnung.Besitzer
                ?? throw new InvalidOperationException(
                    $"Wohnung {vertrag.Wohnung.WohnungId} hat keinen Besitzer.");
            var ansprechpartner = vertrag.Ansprechpartner ?? vermieter;

            var abrechnungsbeginn = new DateOnly(jahr, 1, 1);
            var abrechnungsende = new DateOnly(jahr, 12, 31);
            var abrechnungstage = abrechnungsende.DayNumber - abrechnungsbeginn.DayNumber + 1;

            var notes = new List<Note>();

            // Mietminderung aus Vertragshistorie
            var mietminderung = GetMietminderung(vertrag, abrechnungsbeginn, abrechnungsende);

            // Nur Einheiten, in denen diese Partei (über VertragId + Zeitraum) auftaucht
            var druckEinheiten = new List<NkDruckEinheit>();
            decimal rechnungsbetragGesamt = 0;

            foreach (var einheit in alleEinheiten)
            {
                var einheitPartei = FindEinheitPartei(einheit, targetPartei);
                if (einheitPartei is null) continue;

                var (druckEinheit, betrag) = BuildDruckEinheit(einheit, einheitPartei, vertrag, notes, jahr);
                druckEinheiten.Add(druckEinheit);
                rechnungsbetragGesamt += betrag;
            }

            var nebenkostenMietminderung = rechnungsbetragGesamt * mietminderung;

            return new NkDruckdaten
            {
                Vermieter = vermieter,
                Ansprechpartner = ansprechpartner,
                Mieter = vertrag.Mieter,
                Wohnung = vertrag.Wohnung,
                Jahr = jahr,
                Nutzungsbeginn = targetPartei.Nutzungsbeginn,
                Nutzungsende = targetPartei.Nutzungsende,
                Abrechnungstage = abrechnungstage,
                Vorauszahlung = targetPartei.VertragInfo?.Vorauszahlung ?? 0,
                Rechnungsbetrag = rechnungsbetragGesamt,
                Mietminderung = mietminderung,
                NebenkostenMietminderung = nebenkostenMietminderung,
                Einheiten = druckEinheiten,
                PersonenZeitanteile = targetPartei.PersonenZeitanteile,
                Notes = notes,
            };
        }

        private static NkPartei? FindEinheitPartei(NkEinheit einheit, NkPartei target)
        {
            if (target.Vertrag is null) return null;
            return einheit.Parteien.FirstOrDefault(p =>
                p.Vertrag?.VertragId == target.Vertrag.VertragId &&
                p.Nutzungsbeginn == target.Nutzungsbeginn &&
                p.Nutzungsende == target.Nutzungsende);
        }

        private static (NkDruckEinheit Einheit, decimal Betrag) BuildDruckEinheit(
            NkEinheit einheit, NkPartei partei, Vertrag vertrag, List<Note> notes, int jahr)
        {
            var wohnungen = einheit.Parteien
                .Select(p => p.Wohnung)
                .DistinctBy(w => w.WohnungId)
                .ToList();

            var gesamtWF = wohnungen.Sum(w => w.Wohnflaeche);
            var gesamtNF = wohnungen.Sum(w => w.Nutzflaeche);
            var gesamtNE = wohnungen.Sum(w => w.Nutzeinheit);
            var gesamtMEA = wohnungen.Sum(w => w.Miteigentumsanteile);

            var rechnungen = new List<NkDruckRechnung>();
            decimal betragKalt = 0;

            foreach (var umlage in einheit.Umlagen.Where(u => u.HKVO is null))
            {
                var plaene = einheit.Rechnungsplaene
                    .Where(p => p.Umlage.UmlageId == umlage.UmlageId)
                    .ToList();
                if (plaene.Count == 0) continue;

                var gesamtbetrag = plaene.Sum(p => p.Rechnung.Betrag);
                var anteilFaktor = partei.GetAnteil(umlage);
                var meinBetrag = plaene
                    .SelectMany(p => p.Anteile)
                    .Where(a => a.Partei == partei)
                    .Sum(a => a.Betrag);

                if (meinBetrag == 0 && anteilFaktor == 0) continue;

                rechnungen.Add(new NkDruckRechnung
                {
                    Umlage = umlage,
                    Gesamtbetrag = gesamtbetrag,
                    AnteilFaktor = anteilFaktor,
                    MeinBetrag = meinBetrag,
                    VerbrauchAnteil = partei.VerbrauchAnteileDetail.GetValueOrDefault(umlage.UmlageId),
                });
                betragKalt += meinBetrag;
            }

            var hkvoRechnungen = BuildHkvoRechnungen(einheit, partei, notes, jahr);

            var druckEinheit = new NkDruckEinheit
            {
                Bezeichnung = einheit.Bezeichnung,
                WohnungsAnzahl = wohnungen.Count,
                GesamtNutzeinheiten = gesamtNE,
                GesamtWohnflaeche = gesamtWF,
                GesamtNutzflaeche = gesamtNF,
                GesamtMiteigentumsanteile = gesamtMEA,
                WFZeitanteil = partei.WFZeitanteil,
                NFZeitanteil = partei.NFZeitanteil,
                NEZeitanteil = partei.NEZeitanteil,
                MEAZeitanteil = partei.MEAZeitanteil,
                PersonenZeitanteile = partei.PersonenZeitanteile,
                Rechnungen = rechnungen,
                BetragKalt = betragKalt,
                HkvoRechnungen = hkvoRechnungen,
            };

            return (druckEinheit, betragKalt + druckEinheit.BetragWarm);
        }

        private static IReadOnlyList<NkDruckHkvoRechnung> BuildHkvoRechnungen(
            NkEinheit einheit, NkPartei partei, List<Note> notes, int jahr)
        {
            var hkvoUmlagen = einheit.Umlagen.Where(u => u.HKVO != null).ToList();
            if (hkvoUmlagen.Count == 0) return [];

            var beginn = new DateOnly(jahr, 1, 1);
            var ende = new DateOnly(jahr, 12, 31);
            var result = new List<NkDruckHkvoRechnung>();

            foreach (var umlage in hkvoUmlagen)
            {
                var wohnungWW = umlage.Zaehler
                    .Where(z => z.Wohnung != null && z.Typ == Zaehlertyp.Warmwasser)
                    .ToList();
                var p9_2 = HkvoP9_2Berechnung.Compute(umlage.HKVO!, wohnungWW, jahr, notes);
                if (p9_2 == null) continue;

                var plaene = einheit.Rechnungsplaene
                    .Where(p => p.Umlage.UmlageId == umlage.UmlageId)
                    .ToList();
                if (plaene.Count == 0) continue;

                var gesamtbetrag = plaene.Sum(p => p.Rechnung.Betrag);
                var meinBetrag = plaene
                    .SelectMany(p => p.Anteile)
                    .Where(a => a.Partei == partei)
                    .Sum(a => a.Betrag);

                // Individuelle Zähler für Detailanzeige (§7 Wärme, §8 WW)
                var wohnungWaerme = umlage.Zaehler
                    .Where(z => z.Wohnung != null && IsWärmequelle(z.Typ))
                    .ToList();

                // Nenner: Summe aller Parteien-Verbräuche (wie in NkGruppenAbrechnungsService)
                var totalWaerme = einheit.Parteien.Sum(p =>
                    wohnungWaerme
                        .Where(z => z.Wohnung!.WohnungId == p.Wohnung.WohnungId)
                        .Sum(z => new Verbrauch(z, p.Nutzungsbeginn, p.Nutzungsende, notes).Delta));
                var totalWW = einheit.Parteien.Sum(p =>
                    wohnungWW
                        .Where(z => z.Wohnung!.WohnungId == p.Wohnung.WohnungId)
                        .Sum(z => new Verbrauch(z, p.Nutzungsbeginn, p.Nutzungsende, notes).Delta));

                // Zähler: dieser Partei's Verbrauch während des Nutzungszeitraums
                var dieseWaerme = wohnungWaerme
                    .Where(z => z.Wohnung!.WohnungId == partei.Wohnung.WohnungId)
                    .Sum(z => new Verbrauch(z, partei.Nutzungsbeginn, partei.Nutzungsende, notes).Delta);
                var dieseWW = wohnungWW
                    .Where(z => z.Wohnung!.WohnungId == partei.Wohnung.WohnungId)
                    .Sum(z => new Verbrauch(z, partei.Nutzungsbeginn, partei.Nutzungsende, notes).Delta);

                result.Add(new NkDruckHkvoRechnung
                {
                    Umlage = umlage,
                    P9_2 = p9_2,
                    Gesamtbetrag = gesamtbetrag,
                    P7 = umlage.HKVO!.HKVO_P7,
                    P8 = umlage.HKVO!.HKVO_P8,
                    HeizVerbrauchDiese = dieseWaerme,
                    HeizVerbrauchAlle = totalWaerme,
                    WWVerbrauchDiese = dieseWW,
                    WWVerbrauchAlle = totalWW,
                    WFZeitanteil = partei.WFZeitanteil,
                    MeinBetragGesamt = meinBetrag,
                });
            }

            return result;
        }

        private static bool IsWärmequelle(Zaehlertyp typ) =>
            typ is Zaehlertyp.Gas or Zaehlertyp.Wärme;
    }

    public sealed class NkDruckEinheit
    {
        public required string Bezeichnung { get; init; }
        /// <summary>Anzahl der Wohnungen in dieser Einheit. 1 = Direkt-Zuordnung.</summary>
        public required int WohnungsAnzahl { get; init; }
        public required int GesamtNutzeinheiten { get; init; }
        public required decimal GesamtWohnflaeche { get; init; }
        public required decimal GesamtNutzflaeche { get; init; }
        public required decimal GesamtMiteigentumsanteile { get; init; }

        // Diese Partei's Anteile (Zeitanteil bereits eingerechnet)
        public required decimal WFZeitanteil { get; init; }
        public required decimal NFZeitanteil { get; init; }
        public required decimal NEZeitanteil { get; init; }
        public required decimal MEAZeitanteil { get; init; }
        public required IReadOnlyList<PersonenZeitanteil> PersonenZeitanteile { get; init; }

        public required IReadOnlyList<NkDruckRechnung> Rechnungen { get; init; }
        public required decimal BetragKalt { get; init; }
        public required IReadOnlyList<NkDruckHkvoRechnung> HkvoRechnungen { get; init; }

        public decimal BetragWarm => HkvoRechnungen.Sum(r => r.MeinBetragGesamt);
        public decimal BetragGesamt => BetragKalt + BetragWarm;

        /// <summary>Nicht null wenn diese Einheit eine HKVO-Umlage mit §9(2)-Zählerständen hat.</summary>
        public HkvoP9_2Berechnung? HkvoP9_2 => HkvoRechnungen.FirstOrDefault()?.P9_2;

        public bool IstDirekt => WohnungsAnzahl == 1;
    }

    /// <summary>§9 Abs. 2 HKVO – Warmwasseranteil an den Heizkosten.</summary>
    public sealed record HkvoP9_2Berechnung
    {
        /// <summary>Warmwassermenge in m³.</summary>
        public required decimal V { get; init; }
        /// <summary>Wärmemenge des Allgemeinzählers in kWh.</summary>
        public required decimal Q { get; init; }
        /// <summary>Geschätzte Warmwassertemperatur in °C (HKVO: 60 °C).</summary>
        public required decimal Tw { get; init; }
        /// <summary>Berechneter Warmwasseranteil: 2,5 × (V/Q) × (Tw − 10).</summary>
        public required decimal Para9_2 { get; init; }

        /// <summary>
        /// Berechnet den §9(2)-Warmwasseranteil aus den Zählerständen der HKVO-Umlage.
        /// Gibt null zurück wenn keine Warmwasser- oder Gas-Zähler vorhanden sind, oder Q = 0.
        /// </summary>
        public static HkvoP9_2Berechnung? Compute(HKVO hkvo, IEnumerable<Zaehler> wohnungWWZaehler, int jahr, List<Note> notes)
        {
            if (hkvo.AllgemeinWaerme == null) return null;

            var beginn = new DateOnly(jahr, 1, 1);
            var ende = new DateOnly(jahr, 12, 31);

            var Q = new Verbrauch(hkvo.AllgemeinWaerme, beginn, ende, notes).Delta;
            var V = wohnungWWZaehler.Sum(z => new Verbrauch(z, beginn, ende, notes).Delta);

            if (Q == 0 || V == 0) return null;

            const decimal tw = 60m;
            return new HkvoP9_2Berechnung
            {
                V = V,
                Q = Q,
                Tw = tw,
                Para9_2 = 2.5m * (V / Q) * (tw - 10m),
            };
        }
    }

    public sealed class NkDruckRechnung
    {
        public required Umlage Umlage { get; init; }
        /// <summary>Summe aller Rechnungen für diese Umlage in der Einheit.</summary>
        public required decimal Gesamtbetrag { get; init; }
        /// <summary>Anteilsfaktor dieser Partei (0–1).</summary>
        public required decimal AnteilFaktor { get; init; }
        /// <summary>Dieser Partei's Betrag (nach Rundungskorrektur).</summary>
        public required decimal MeinBetrag { get; init; }
        /// <summary>Null für Leerstand-Parteien oder nicht-Verbrauch-Umlagen.</summary>
        public VerbrauchAnteil? VerbrauchAnteil { get; init; }
    }

    /// <summary>
    /// Druckdetails für eine HKVO-Umlage (warme Betriebskosten).
    /// MeinBetragGesamt kommt aus dem Rechnungsplan (autoritativ, post-Rundungskorrektur).
    /// Die Verbrauchsfelder dienen der Detailanzeige.
    /// </summary>
    public sealed class NkDruckHkvoRechnung
    {
        public required Umlage Umlage { get; init; }
        public required HkvoP9_2Berechnung P9_2 { get; init; }
        public required decimal Gesamtbetrag { get; init; }

        public required decimal P7 { get; init; }
        public required decimal P8 { get; init; }

        // §7 Heizung: individueller Wärmeverbrauch
        public required decimal HeizVerbrauchDiese { get; init; }
        public required decimal HeizVerbrauchAlle { get; init; }
        public decimal HeizVerbrauchAnteil => HeizVerbrauchAlle > 0 ? HeizVerbrauchDiese / HeizVerbrauchAlle : 0;

        // §8 Warmwasser: individueller WW-Verbrauch
        public required decimal WWVerbrauchDiese { get; init; }
        public required decimal WWVerbrauchAlle { get; init; }
        public decimal WWVerbrauchAnteil => WWVerbrauchAlle > 0 ? WWVerbrauchDiese / WWVerbrauchAlle : 0;

        // WF-Zeitanteil dieser Partei (Fallback wenn keine individuellen Zähler)
        public required decimal WFZeitanteil { get; init; }

        // Autoritativer Gesamtbetrag dieser Partei (aus Rechnungsplan)
        public required decimal MeinBetragGesamt { get; init; }

        // Abgeleitete Werte für Detailanzeige
        public decimal HeizBetrag => Gesamtbetrag * (1 - P9_2.Para9_2);
        public decimal WWBetrag => Gesamtbetrag * P9_2.Para9_2;
        public decimal HeizAnteilFaktor => HeizVerbrauchAlle > 0
            ? P7 * HeizVerbrauchAnteil + (1 - P7) * WFZeitanteil
            : WFZeitanteil;
        public decimal WWAnteilFaktor => WWVerbrauchAlle > 0
            ? P8 * WWVerbrauchAnteil + (1 - P8) * WFZeitanteil
            : WFZeitanteil;
    }
}
