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

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class Verbrauch
    {
        public Zaehler Zaehler;
        public DateOnly Anfangsdatum;
        public DateOnly Enddatum;
        public decimal Delta;

        public Verbrauch(Zaehler zaehler, DateOnly beginn, DateOnly ende, List<Note> notes)
        {
            Zaehler = zaehler;
            var endstand = GetZaehlerEndStand(zaehler, ende);
            var anfangsstand = GetZaehlerAnfangsStand(zaehler, beginn);
            if (anfangsstand == null)
            {
                notes.Add($"Keinen gültigen Anfangsstand für Zähler {zaehler.Kennnummer} ({zaehler.Typ}) " +
                    $"innerhalb des Zeitraums ({beginn:dd.MM.yyyy} - {ende:dd.MM.yyyy}) gefunden.",
                    Severity.Error);
            }
            else
            {
                Anfangsdatum = anfangsstand.Datum;
            }

            if (endstand == null)
            {
                notes.Add($"Keinen gültigen Endstand für Zähler {zaehler.Kennnummer} ({zaehler.Typ})" +
                    $"innerhalb des Zeitraums ({beginn:dd.MM.yyyy} - {ende:dd.MM.yyyy}) gefunden.",
                    Severity.Error);
            }
            else
            {
                Enddatum = endstand.Datum;
            }

            if (anfangsstand != null && endstand != null)
            {
                Delta = endstand.Stand - anfangsstand.Stand;

                // Ein stark abweichendes Messfenster verfälscht Verbrauchsanteile
                // (z.B. §9(2)-V/Q) still — deshalb warnen statt schweigen.
                if (anfangsstand.Datum.DayNumber - beginn.DayNumber > 14
                    || ende.DayNumber - endstand.Datum.DayNumber > 14)
                {
                    notes.Add($"Messfenster von Zähler {zaehler.Kennnummer} ({zaehler.Typ}) weicht vom " +
                        $"Zeitraum ab: gemessen {anfangsstand.Datum:dd.MM.yyyy} - {endstand.Datum:dd.MM.yyyy}, " +
                        $"Zeitraum {beginn:dd.MM.yyyy} - {ende:dd.MM.yyyy}.",
                        Severity.Warning);
                }
            }
            else
            {
                Delta = 0;
            }
        }

        private static Zaehlerstand? GetZaehlerEndStand(Zaehler zaehler, DateOnly ende)
        {
            return zaehler.Staende
                    .OrderBy(zaehlerstand => zaehlerstand.Datum)
                    .LastOrDefault(zaehlerstand => zaehlerstand.Datum <= ende);
        }

        /// <summary>
        /// Stand, der dem Periodenbeginn am nächsten liegt (frühestens 14 Tage davor).
        /// Nicht den ersten Stand ab beginn−14 nehmen: Gibt es z.B. Stände am 20.12.
        /// und 31.12., ist der 31.12. der richtige Anfangsstand für den 01.01. —
        /// sonst zählen Tage des Vorjahres in den Verbrauch hinein.
        /// </summary>
        private static Zaehlerstand? GetZaehlerAnfangsStand(Zaehler zaehler, DateOnly beginn)
        {
            var earliest = beginn.AddDays(-14).DayNumber;
            return zaehler.Staende
                .Where(zaehlerstand => earliest <= zaehlerstand.Datum.DayNumber)
                .OrderBy(zaehlerstand => Math.Abs(zaehlerstand.Datum.DayNumber - beginn.DayNumber))
                .ThenBy(zaehlerstand => zaehlerstand.Datum)
                .FirstOrDefault();
        }
    }
}
