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
    /// Ergebnis einer NK-Abrechnung auf dem BkAbrechnungsKonto des Vertrags: Die Abrechnung
    /// bucht den Saldo dorthin — Soll = Nachzahlung des Mieters, Haben = Erstattung an den
    /// Mieter. Zahlungen hängen per Ausgleich an dieser Zeile.
    ///
    /// Braucht geladen: Vertrag.BkAbrechnungsKonto, Buchungssatz.Buchungszeilen samt
    /// Buchungskonto und Ausgleichen (AlsSollZeile.HabenZeile bzw. AlsHabenZeile.SollZeile).
    /// </summary>
    public static class AbrechnungsSaldo
    {
        /// <summary>Die Saldo-Zeile; <c>null</c>, wenn die Abrechnung bei 0 aufging.</summary>
        public static Buchungszeile? Zeile(Abrechnungsresultat r) =>
            r.Buchungssatz.Buchungszeilen.FirstOrDefault(z =>
                z.Buchungskonto.BuchungskontoId == r.Vertrag.BkAbrechnungsKonto.BuchungskontoId);

        /// <summary>Positiv: Nachzahlung des Mieters; negativ: Erstattung an den Mieter.</summary>
        public static decimal Saldo(Abrechnungsresultat r) =>
            Zeile(r) is Buchungszeile z ? (z.SollHaben == SollHaben.Soll ? z.Betrag : -z.Betrag) : 0m;

        /// <summary>Die Zeilen der Zahlungen, die die Saldo-Zeile ausgleichen.</summary>
        public static List<Buchungszeile> Zahlungen(Abrechnungsresultat r) =>
            Zeile(r) switch
            {
                { SollHaben: SollHaben.Soll } z => z.AlsSollZeile.Select(a => a.HabenZeile).ToList(),
                { SollHaben: SollHaben.Haben } z => z.AlsHabenZeile.Select(a => a.SollZeile).ToList(),
                _ => [],
            };
    }
}
