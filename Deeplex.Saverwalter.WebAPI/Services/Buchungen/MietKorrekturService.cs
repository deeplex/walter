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
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Gleicht die gebuchte Kaltmiete eines Jahres an die <see cref="VertragVersion"/>en an,
    /// wenn eine Version rückwirkend mit einer anderen Grundmiete angelegt wurde (z. B. eine
    /// Mieterhöhung, die erst nachträglich im System erfasst wurde). Was über die Kaltmiete
    /// hinaus gezahlt wurde, ist NK-Vorauszahlung; die Korrektur stellt diese Aufteilung her.
    ///
    /// Eine Korrektur ist ein gewöhnlicher Buchungssatz ohne Transaktion, der Miet- und
    /// NK-Konto des Vertrags bebucht — daran wird er erkannt. Mit d = Differenz der Kaltmiete:
    ///
    ///   d &gt; 0 (Miete erhöht sich):
    ///     Soll MietBuchungskonto d / Haben MietErtragskonto d   (Nachtrag)
    ///     Soll NkBuchungskonto d   / Haben MietBuchungskonto d  (Umbuchung)
    ///   d &lt; 0 (Miete sinkt), Seiten jeweils getauscht.
    ///
    /// Die beiden Zeilen auf dem MietBuchungskonto sind miteinander ausgeglichen. Die
    /// Kaltmiete-Wirkung einer Korrektur ist ihr Saldo auf dem MietErtragskonto (Haben
    /// positiv); ältere Korrekturen ohne Ertragszeile ändern die Kaltmiete nicht.
    ///
    /// Gerechnet wird je Buchungsjahr über die bezahlten Monate: Soll ist die Grundmiete laut
    /// Version, Ist die Sollstellung des Monats; dazu kommen die Korrekturen des Jahres.
    /// Unbezahlte Monate fehlen bewusst — ihre Sollstellung ist noch frei und lässt sich
    /// direkt ändern. Bleibt eine Differenz, wird genau diese gebucht; mehrere Korrekturen
    /// eines Jahres addieren sich. Eine Erhöhung muss durch die NK-Vorauszahlungen des
    /// Jahres gedeckt sein (siehe <see cref="NkVorauszahlung"/>). Abgerechnete Jahre werden
    /// nicht korrigiert.
    ///
    /// Rückgängig und Storno laufen wie bei jedem Buchungssatz (<see cref="BuchungssatzSchutzService"/>).
    /// </summary>
    public class MietKorrekturService(SaverwalterContext ctx, BuchungssatzSchutzService schutzService)
    {
        public enum MietKorrekturStatus
        {
            /// <summary>Gebuchte Kaltmiete entspricht den Vertragsversionen.</summary>
            Stimmt,
            Anwendbar,
            Abgerechnet,
            NkReichtNichtAus,
        }

        public class MietKorrekturMonat
        {
            public int Monat { get; set; }
            /// <summary>Kaltmiete der Sollstellung des Monats.</summary>
            public decimal IstKaltmiete { get; set; }
            public decimal SollKaltmiete { get; set; }
            public decimal Delta => SollKaltmiete - IstKaltmiete;
        }

        public class MietKorrekturBuchung
        {
            public Guid BuchungssatzId { get; set; }
            public DateOnly Buchungsdatum { get; set; }
            /// <summary>Änderung der Kaltmiete (Erhöhung positiv).</summary>
            public decimal Betrag { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
            public string? Notiz { get; set; }
            public bool KannLoeschen { get; set; }
            public bool KannStornieren { get; set; }
            public string? Sperrgrund { get; set; }
        }

        public class MietKorrekturJahr
        {
            public int Jahr { get; set; }
            /// <summary>Monate, deren Sollstellung von der Version abweicht.</summary>
            public List<MietKorrekturMonat> Monate { get; set; } = [];
            public List<MietKorrekturBuchung> Korrekturen { get; set; } = [];
            public decimal Abweichung => Monate.Sum(m => m.Delta);
            public decimal Korrigiert => Korrekturen.Sum(k => k.Betrag);
            /// <summary>Noch zu buchende Änderung der Kaltmiete.</summary>
            public decimal Differenz => Abweichung - Korrigiert;
            public decimal NkVorauszahlung { get; set; }
            public MietKorrekturStatus Status { get; set; }
        }

        private record Sollstellung(int Jahr, int Monat, decimal Betrag);

        /// <summary>Jahre mit abweichenden Monaten oder bestehenden Korrekturen.</summary>
        public async Task<IReadOnlyList<MietKorrekturJahr>> ErmittleAsync(int vertragId)
        {
            var vertrag = await LadeVertragAsync(vertragId);
            var nkVorauszahlungen = await LadeNkVorauszahlungenAsync(vertrag);
            var abgerechnet = await AbrechnungsschutzService.AbgerechneteJahreVertrag(ctx, vertrag.VertragId);
            var korrekturen = await LadeKorrekturenAsync(vertrag);

            var monate = new Dictionary<int, List<MietKorrekturMonat>>();
            foreach (var s in await LadeBezahlteSollstellungenAsync(vertrag))
            {
                var version = vertrag.Versionen
                    .Where(v => v.Beginn <= new DateOnly(s.Jahr, s.Monat, 1))
                    .MaxBy(v => v.Beginn);
                if (version is null || Math.Abs(version.Grundmiete - s.Betrag) < 0.005m) continue;

                if (!monate.TryGetValue(s.Jahr, out var liste)) monate[s.Jahr] = liste = [];
                liste.Add(new MietKorrekturMonat { Monat = s.Monat, IstKaltmiete = s.Betrag, SollKaltmiete = version.Grundmiete });
            }

            var jahre = monate.Keys.Union(korrekturen.Keys).Order();
            var result = new List<MietKorrekturJahr>();
            foreach (var jahr in jahre)
            {
                var eintrag = new MietKorrekturJahr
                {
                    Jahr = jahr,
                    Monate = monate.GetValueOrDefault(jahr)?.OrderBy(m => m.Monat).ToList() ?? [],
                    Korrekturen = korrekturen.GetValueOrDefault(jahr) ?? [],
                    NkVorauszahlung = nkVorauszahlungen.GetValueOrDefault(jahr),
                };
                eintrag.Status =
                    Math.Abs(eintrag.Differenz) < 0.005m ? MietKorrekturStatus.Stimmt
                    : abgerechnet.Contains(jahr) ? MietKorrekturStatus.Abgerechnet
                    : eintrag.Differenz > 0 && eintrag.NkVorauszahlung < eintrag.Differenz - 0.005m
                        ? MietKorrekturStatus.NkReichtNichtAus
                    : MietKorrekturStatus.Anwendbar;
                result.Add(eintrag);
            }
            return result;
        }

        /// <summary>
        /// Bucht die offene Differenz des Jahres als einen Korrektur-Satz. Wirft
        /// <see cref="InvalidOperationException"/>, wenn das Jahr nicht korrigierbar ist.
        /// </summary>
        public async Task<Buchungssatz> BucheDifferenzAsync(int vertragId, int jahr, string grund)
        {
            if (string.IsNullOrWhiteSpace(grund))
                throw new ArgumentException("Für eine Miet-Korrektur muss ein Grund angegeben werden.");

            var eintrag = (await ErmittleAsync(vertragId)).FirstOrDefault(j => j.Jahr == jahr);
            switch (eintrag?.Status)
            {
                case null or MietKorrekturStatus.Stimmt:
                    throw new InvalidOperationException($"Die Kaltmiete {jahr} entspricht bereits dem Vertrag.");
                case MietKorrekturStatus.Abgerechnet:
                    throw new InvalidOperationException($"Das Jahr {jahr} ist für diesen Vertrag abgerechnet.");
                case MietKorrekturStatus.NkReichtNichtAus:
                    throw new InvalidOperationException(
                        $"Die NK-Vorauszahlungen {jahr} ({eintrag.NkVorauszahlung:C}) decken die Erhöhung ({eintrag.Differenz:C}) nicht.");
            }

            var vertrag = await LadeVertragAsync(vertragId);
            var d = eintrag.Differenz;
            var betrag = Math.Abs(d);
            var erhoehung = d > 0;
            var hin = erhoehung ? SollHaben.Soll : SollHaben.Haben;
            var zurueck = erhoehung ? SollHaben.Haben : SollHaben.Soll;

            var satz = new Buchungssatz(
                DateOnly.FromDateTime(DateTime.Today),
                $"Miet-Korrektur {jahr}: Kaltmiete {(erhoehung ? "+" : "−")}{betrag:N2} €")
            {
                Buchungsjahr = jahr,
                Notiz = grund.Trim(),
            };
            var forderung = AddZeile(satz, hin, betrag, vertrag.MietBuchungskonto);
            AddZeile(satz, zurueck, betrag, vertrag.Wohnung.MietErtragskonto);
            AddZeile(satz, hin, betrag, vertrag.NkBuchungskonto);
            var umbuchung = AddZeile(satz, zurueck, betrag, vertrag.MietBuchungskonto);

            ctx.Buchungssaetze.Add(satz);
            ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
            {
                SollZeile = erhoehung ? forderung : umbuchung,
                HabenZeile = erhoehung ? umbuchung : forderung,
            });
            await ctx.SaveChangesAsync();
            return satz;
        }

        /// <summary>
        /// Korrektur-Sätze je Buchungsjahr: ohne Transaktion, nicht storniert und kein Storno,
        /// mit Zeilen auf Miet- und NK-Konto des Vertrags.
        /// </summary>
        private async Task<Dictionary<int, List<MietKorrekturBuchung>>> LadeKorrekturenAsync(Vertrag vertrag)
        {
            var mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            var nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;
            var ertragKontoId = vertrag.Wohnung.MietErtragskonto.BuchungskontoId;

            var saetze = await ctx.Buchungssaetze
                .Where(IstKorrektur(mietKontoId, nkKontoId))
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .OrderBy(s => s.Buchungsdatum)
                .ToListAsync();

            var result = new Dictionary<int, List<MietKorrekturBuchung>>();
            foreach (var satz in saetze)
            {
                var schutz = await schutzService.PruefeAsync(satz);
                if (!result.TryGetValue(satz.Buchungsjahr, out var liste)) result[satz.Buchungsjahr] = liste = [];
                liste.Add(new MietKorrekturBuchung
                {
                    BuchungssatzId = satz.BuchungssatzId,
                    Buchungsdatum = satz.Buchungsdatum,
                    Betrag = satz.Buchungszeilen
                        .Where(z => z.Buchungskonto.BuchungskontoId == ertragKontoId)
                        .Sum(z => z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag),
                    Beschreibung = satz.Beschreibung,
                    Notiz = satz.Notiz,
                    KannLoeschen = schutz.KannLoeschen,
                    KannStornieren = schutz.KannStornieren,
                    Sperrgrund = schutz.Sperrgrund,
                });
            }
            return result;
        }

        private static System.Linq.Expressions.Expression<Func<Buchungssatz, bool>> IstKorrektur(int mietKontoId, int nkKontoId) =>
            s => s.Transaktion == null
                && s.StornoVon == null
                && s.StornoNach == null
                && s.Buchungszeilen.Any(z => z.Buchungskonto.BuchungskontoId == mietKontoId)
                && s.Buchungszeilen.Any(z => z.Buchungskonto.BuchungskontoId == nkKontoId);

        /// <summary>NK-Vorauszahlungen je Buchungsjahr, berechnet wie in der Abrechnung.</summary>
        private async Task<Dictionary<int, decimal>> LadeNkVorauszahlungenAsync(Vertrag vertrag)
        {
            var zeilen = await ctx.Buchungszeilen
                .Where(NkVorauszahlung.Zaehlt(vertrag))
                .Select(z => new { z.Buchungssatz.Buchungsjahr, z.SollHaben, z.Betrag })
                .ToListAsync();

            return zeilen
                .GroupBy(z => z.Buchungsjahr)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(z => z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag));
        }

        private async Task<Vertrag> LadeVertragAsync(int vertragId) =>
            await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.ZahlungsKonto)
                .Include(v => v.Wohnung).ThenInclude(w => w.MietErtragskonto)
                .FirstOrDefaultAsync(v => v.VertragId == vertragId)
                ?? throw new ArgumentException($"Vertrag {vertragId} nicht gefunden.");

        /// <summary>
        /// Je Monat die Sollstellung (Soll auf dem MietBuchungskonto, ohne Transaktion, kein
        /// Satz mit NK-Zeile) mit einer bereits erfolgten Kaltmiete-Zahlung
        /// (transaktionsgebundene OPOS-Gegenzeile).
        /// </summary>
        private async Task<List<Sollstellung>> LadeBezahlteSollstellungenAsync(Vertrag vertrag)
        {
            var mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            var nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;

            var sollZeilen = await ctx.Buchungszeilen
                .Where(z =>
                    z.Buchungskonto.BuchungskontoId == mietKontoId &&
                    z.SollHaben == SollHaben.Soll &&
                    z.Buchungssatz.Transaktion == null &&
                    !z.Buchungssatz.Buchungszeilen.Any(g => g.Buchungskonto.BuchungskontoId == nkKontoId))
                .Include(z => z.Buchungssatz)
                .Include(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile).ThenInclude(h => h.Buchungssatz)
                    .ThenInclude(s => s.Transaktion)
                .ToListAsync();

            var result = new List<Sollstellung>();
            foreach (var gruppe in sollZeilen.GroupBy(z => new
            { z.Buchungssatz.Buchungsjahr, z.Buchungssatz.Buchungsdatum.Month }))
            {
                var sollZeile = gruppe.OrderBy(z => z.Buchungssatz.Buchungsdatum).First();
                var bezahlt = sollZeile.AlsSollZeile.Any(a => a.HabenZeile.Buchungssatz.Transaktion != null);
                if (!bezahlt) continue;

                result.Add(new Sollstellung(gruppe.Key.Buchungsjahr, gruppe.Key.Month, sollZeile.Betrag));
            }
            return result;
        }

        private static Buchungszeile AddZeile(Buchungssatz satz, SollHaben sollHaben, decimal betrag, Buchungskonto konto)
        {
            var zeile = new Buchungszeile(sollHaben, betrag) { Buchungssatz = satz, Buchungskonto = konto };
            satz.Buchungszeilen.Add(zeile);
            return zeile;
        }
    }
}
