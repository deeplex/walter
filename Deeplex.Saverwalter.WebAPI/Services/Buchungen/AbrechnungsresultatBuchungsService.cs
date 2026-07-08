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
    /// Erstellt den Buchungssatz für ein Abrechnungsresultat (NK-Jahresabrechnung).
    ///
    /// Die Abrechnung ist eine reine Glattstellung: Die tatsächlichen Kosten wurden
    /// dem Mieter bereits über die NK-Anteile (Soll auf dem NkBuchungskonto) belastet,
    /// die Vorauszahlungen stehen dort im Haben. Der Restsaldo des NkBuchungskontos
    /// wird auf das ausgleichbare BkAbrechnungsKonto umgebucht:
    ///   Nachzahlung (saldo &gt; 0): Soll BkAbrechnungsKonto / Haben NkBuchungskonto
    ///   Guthaben    (saldo &lt; 0): Soll NkBuchungskonto / Haben BkAbrechnungsKonto
    /// Damit endet das NkBuchungskonto für das Jahr auf 0, und auf dem
    /// BkAbrechnungsKonto steht genau das Resultat als offener Posten — bis die
    /// Ausgleichszahlung (AbrechnungsAusgleich-Transaktion) es per OPOS tilgt.
    /// </summary>
    public class AbrechnungsresultatBuchungsService
    {
        private readonly SaverwalterContext _ctx;

        public AbrechnungsresultatBuchungsService(SaverwalterContext ctx)
        {
            _ctx = ctx;
        }

        public void BucheAbrechnung(Abrechnungsresultat resultat, int jahr, decimal saldo)
        {
            if (resultat.Buchungssatz != null)
                return; // Bereits gebucht — nicht doppelt buchen.

            var vertrag = resultat.Vertrag;
            var buchungsdatum = new DateOnly(jahr, 12, 31);
            var satz = new Buchungssatz(buchungsdatum, $"BK-Abrechnung {jahr}");

            if (saldo > 0)
            {
                AddZeile(satz, SollHaben.Soll, saldo, vertrag.BkAbrechnungsKonto);
                AddZeile(satz, SollHaben.Haben, saldo, vertrag.NkBuchungskonto);
            }
            else if (saldo < 0)
            {
                AddZeile(satz, SollHaben.Soll, -saldo, vertrag.NkBuchungskonto);
                AddZeile(satz, SollHaben.Haben, -saldo, vertrag.BkAbrechnungsKonto);
            }
            // saldo == 0: leerer Satz als Beleg — nichts glattzustellen.

            _ctx.Buchungssaetze.Add(satz);
            resultat.Buchungssatz = satz;
        }

        private static void AddZeile(Buchungssatz satz, SollHaben sollHaben, decimal betrag, Buchungskonto konto)
        {
            var zeile = new Buchungszeile(sollHaben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = konto
            };
            satz.Buchungszeilen.Add(zeile);
        }
    }
}
