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

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Die Sollstellung eines Monats auf einem Miet- oder Garagenkonto — eine Regel für
    /// Buchen, Mietkonto und Startseite. Sie ist eine Soll-Zeile, deren Satz
    /// - keine Transaktion trägt (keine Zahlung),
    /// - weder Storno noch storniert ist und
    /// - nicht auch das NK-Konto des Vertrags bebucht (das wäre eine Miet-Korrektur, die ihr
    ///   Buchungsdatum trägt, nicht den Monat einer Miete).
    /// Der Monat ist der des Buchungsdatums (per Konstruktion der dritte Werktag).
    ///
    /// Die Zeilen brauchen geladen: Buchungssatz mit Transaktion, StornoVon, StornoNach und
    /// den Buchungszeilen samt Buchungskonto.
    /// </summary>
    public static class Sollstellung
    {
        private static readonly SemaphoreSlim Sperre = new(1, 1);

        /// <summary>
        /// Führt aus, was fehlende Sollstellungen anlegt (prüfen, anlegen, speichern), ohne
        /// dass ein anderer Vorgang gleichzeitig dasselbe tut — sonst legen zwei Anfragen
        /// denselben Monat beide an. Die WebAPI läuft als ein Prozess; die Sperre gilt
        /// prozessweit. Innerhalb der Aktion nicht erneut aufrufen.
        /// </summary>
        public static async Task<T> GesperrtAsync<T>(Func<Task<T>> aktion)
        {
            await Sperre.WaitAsync();
            try { return await aktion(); }
            finally { Sperre.Release(); }
        }

        /// <param name="nkKontoId">NK-Konto des Vertrags; bei Garagenkonten <c>null</c>.</param>
        public static bool Ist(Buchungszeile z, int? nkKontoId = null) =>
            z.SollHaben == SollHaben.Soll
            && z.Buchungssatz.Transaktion == null
            && z.Buchungssatz.StornoVon == null
            && z.Buchungssatz.StornoNach == null
            && (nkKontoId is not int nk
                || !z.Buchungssatz.Buchungszeilen.Any(g => g.Buchungskonto.BuchungskontoId == nk));

        public static bool IstImMonat(Buchungszeile z, DateOnly monat, int? nkKontoId = null) =>
            Ist(z, nkKontoId)
            && z.Buchungssatz.Buchungsdatum.Year == monat.Year
            && z.Buchungssatz.Buchungsdatum.Month == monat.Month;

        /// <summary>Per Ausgleich an der Sollstellung bezahlter Betrag.</summary>
        public static decimal Bezahlt(Buchungszeile sollstellung) =>
            sollstellung.AlsSollZeile.Sum(a => a.HabenZeile.Betrag);
    }
}
