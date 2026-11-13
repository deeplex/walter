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

using System.Linq.Expressions;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    /// <summary>
    /// Geleistete NK-Vorauszahlungen eines Vertrags — die einzige Berechnung, die
    /// Abrechnung, Anzeige und Miet-Korrektur gemeinsam verwenden.
    ///
    /// Es zählen die Zeilen auf dem NkBuchungskonto aus
    /// - Zahlungen des Mieters (Satz mit Transaktion, der das ZahlungsKonto des Vertrags
    ///   bebucht) und
    /// - Miet-Korrektur-Umbuchungen (Satz berührt auch das MietBuchungskonto),
    /// jeweils Haben positiv, Soll negativ. Stornierte Sätze und Stornos zählen nicht.
    /// NK-Anteile und Sonderforderungen (auch die Verteil-Zeilen an bezahlten
    /// BK-Rechnungen, die eine Transaktion tragen) und das Abrechnungsergebnis sind
    /// keine Vorauszahlungen und fallen damit heraus.
    /// </summary>
    public static class NkVorauszahlung
    {
        public static Expression<Func<Buchungszeile, bool>> Zaehlt(Vertrag vertrag) =>
            Zaehlt(
                vertrag.NkBuchungskonto.BuchungskontoId,
                vertrag.MietBuchungskonto.BuchungskontoId,
                vertrag.ZahlungsKonto.BuchungskontoId);

        private static Expression<Func<Buchungszeile, bool>> Zaehlt(int nkKontoId, int mietKontoId, int zahlungsKontoId) =>
            z => z.Buchungskonto.BuchungskontoId == nkKontoId
                && z.Buchungssatz.StornoVon == null
                && z.Buchungssatz.StornoNach == null
                && ((z.Buchungssatz.Transaktion != null
                        && z.Buchungssatz.Buchungszeilen.Any(g => g.Buchungskonto.BuchungskontoId == zahlungsKontoId))
                    || z.Buchungssatz.Buchungszeilen.Any(g => g.Buchungskonto.BuchungskontoId == mietKontoId));

        public static decimal Betrag(Buchungszeile z) =>
            z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag;

        /// <summary>Summe der Vorauszahlungen des Jahres aus den Zeilen des NkBuchungskontos.</summary>
        public static decimal Summe(Vertrag vertrag, IEnumerable<Buchungszeile> nkZeilen, int jahr)
        {
            var zaehlt = Zaehlt(vertrag).Compile();
            return nkZeilen
                .Where(z => z.Buchungssatz.Buchungsjahr == jahr && zaehlt(z))
                .Sum(Betrag);
        }
    }
}
