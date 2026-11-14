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
    /// Zahlung einer Rechnung (Erhaltungsaufwand, Betriebskosten) in der Fachsicht.
    /// Datum ist das Zahlungsdatum der Transaktion, ohne Transaktion das Buchungsdatum.
    /// </summary>
    public class RechnungsZahlung
    {
        public Guid BuchungssatzId { get; set; }
        public Guid? TransaktionId { get; set; }
        public DateOnly Datum { get; set; }
        public decimal Betrag { get; set; }
        public string? Zahler { get; set; }
        public string? Empfaenger { get; set; }

        /// <summary>Aus der Geld-Zeile eines Zahlungssatzes; Zahler und Empfänger aus seiner Transaktion.</summary>
        public static RechnungsZahlung Aus(Buchungszeile zeile)
        {
            var satz = zeile.Buchungssatz;
            var t = satz.Transaktion;
            return new RechnungsZahlung
            {
                BuchungssatzId = satz.BuchungssatzId,
                TransaktionId = t?.TransaktionId,
                Datum = t?.Zahlungsdatum ?? satz.Buchungsdatum,
                Betrag = zeile.Betrag,
                Zahler = Name(t?.Zahler),
                Empfaenger = Name(t?.Zahlungsempfaenger),
            };
        }

        public static string? Name(Bankkonto? b) => b?.Besitzer.FirstOrDefault()?.Bezeichnung ?? b?.Iban;
    }
}
