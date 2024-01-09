// Copyright (c) 2023-2024 Kai Lawrence
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
        public double Delta;

        public Verbrauch(Zaehler zaehler, DateOnly beginn, DateOnly ende, List<Note> notes)
        {
            Zaehler = zaehler;
            var endstand = GetZaehlerEndStand(zaehler, ende);
            var anfangsstand = GetZaehlerAnfangsStand(zaehler, beginn);
            if (anfangsstand == null)
            {
                notes.Add($"Keinen gültigen Anfangsstand für Zähler {zaehler.Kennnummer} ({zaehler.Typ}) " +
                    $"innerhalb des Zeitraums ({beginn.ToString("dd.MM.yyyy")} - {ende.ToString("dd.MM.yyyy")}) gefunden.",
                    Severity.Error);
            }
            else
            {
                Anfangsdatum = anfangsstand.Datum;
            }

            if (endstand == null)
            {
                notes.Add($"Keinen gültigen Endstand für Zähler {zaehler.Kennnummer} ({zaehler.Typ})" +
                    $"innerhalb des Zeitraums ({beginn.ToString("dd.MM.yyyy")} - {ende.ToString("dd.MM.yyyy")}) gefunden.",
                    Severity.Error);
            }
            else
            {
                Enddatum = endstand.Datum;
            }

            if (anfangsstand != null && endstand != null)
            {
                Delta = endstand.Stand - anfangsstand.Stand;
            }


            else if (anfangsstand == null && endstand?.Stand == 0)
            {
                Delta = 0;
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

        private static Zaehlerstand? GetZaehlerAnfangsStand(Zaehler zaehler, DateOnly beginn)
        {
            var earliest = beginn.AddDays(-14).DayNumber;
            return zaehler.Staende
                .OrderBy(zaehlerstand => zaehlerstand.Datum)
                .FirstOrDefault(zaehlerstand => earliest <= zaehlerstand.Datum.DayNumber);
        }


    }
}
