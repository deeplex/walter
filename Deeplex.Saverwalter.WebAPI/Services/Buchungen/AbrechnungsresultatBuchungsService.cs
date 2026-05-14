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
    /// Wird beim Generieren der Abrechnungsvorschau (PreviewAsync) aufgerufen.
    /// </summary>
    public class AbrechnungsresultatBuchungsService
    {
        private readonly SaverwalterContext _ctx;

        public AbrechnungsresultatBuchungsService(SaverwalterContext ctx)
        {
            _ctx = ctx;
        }

        public void BucheAbrechnung(Abrechnungsresultat resultat, int jahr, decimal vorauszahlung, decimal rechnungsbetrag, decimal saldo)
        {
            if (resultat.Buchungssatz != null)
                return; // Bereits gebucht — nicht doppelt buchen.

            var vertrag = resultat.Vertrag;
            var buchungsdatum = new DateOnly(jahr, 12, 31);
            var satz = new Buchungssatz(buchungsdatum, $"BK-Abrechnung {jahr}");

            // Kernlogik:
            // - Soll NK-Konto: bereits geleistete Vorauszahlungen
            // - Haben BK-Abrechnungskonto: tatsächlicher Rechnungsbetrag
            AddZeile(satz, SollHaben.Soll, vorauszahlung, vertrag.NkBuchungskonto);
            AddZeile(satz, SollHaben.Haben, rechnungsbetrag, vertrag.BkAbrechnungsKonto);

            // Ausgleich auf Zahlungskonto (Nachzahlung/Erstattung).
            if (saldo > 0)
                AddZeile(satz, SollHaben.Soll, saldo, vertrag.ZahlungsKonto);
            else if (saldo < 0)
                AddZeile(satz, SollHaben.Haben, -saldo, vertrag.ZahlungsKonto);

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
