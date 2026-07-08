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
    public sealed class VerbrauchAnteil
    {
        public Umlage Umlage { get; }
        public Dictionary<Zaehlereinheit, List<Verbrauch>> AlleZaehler { get; } = new();
        public Dictionary<Zaehlereinheit, decimal> AlleVerbrauch { get; } = new();
        public Dictionary<Zaehlereinheit, List<Verbrauch>> DieseZaehler { get; } = new();
        public Dictionary<Zaehlereinheit, decimal> DieseVerbrauch { get; } = new();
        public Dictionary<Zaehlereinheit, decimal> Anteil { get; } = new();

        public VerbrauchAnteil(Umlage umlage, Wohnung wohnung, Zeitraum zeitraum, List<Note> notes)
        {
            Umlage = umlage;
            var zaehlerGroups = umlage.Zaehler
                // if no Wohnung is attached => zaehler is allgemeinZaehler
                .Where(zaehler => zaehler.Wohnung != null)
                .GroupBy(zaehler => zaehler.Typ.ToUnit());

            foreach (var zaehlergroup in zaehlerGroups)
            {
                var unit = zaehlergroup.Key;

                AlleZaehler[unit] = new();
                AlleVerbrauch[unit] = new();
                DieseZaehler[unit] = new();
                DieseVerbrauch[unit] = new();

                foreach (var zaehler in zaehlergroup)
                {
                    if (zaehler.Staende.Count == 0)
                    {
                        continue;
                    }
                    if (zaehler.Ende < zeitraum.Abrechnungsbeginn)
                    {
                        continue;
                    }
                    var daysToFirstZaehler = zaehler.Staende
                        .OrderBy(stand => stand.Datum)
                        .FirstOrDefault()?
                        .Datum
                        .DayNumber - zeitraum.Abrechnungsende.DayNumber;
                    if (daysToFirstZaehler > -31)
                    {
                        continue;
                    }
                    var verbrauch = new Verbrauch(
                        zaehler,
                        zeitraum.Abrechnungsbeginn,
                        zeitraum.Abrechnungsende,
                        notes);
                    AlleZaehler[unit].Add(verbrauch);
                    AlleVerbrauch[unit] += verbrauch.Delta;

                    if (zaehler.Wohnung == wohnung)
                    {
                        var verbrauchMieter = new Verbrauch(zaehler, zeitraum.Nutzungsbeginn, zeitraum.Nutzungsende, notes);
                        DieseZaehler[unit].Add(verbrauchMieter);
                        DieseVerbrauch[unit] += verbrauchMieter.Delta;
                    }
                }

                foreach (var entry in AlleZaehler)
                {
                    CheckIfZaehlerstaendeValid(entry.Value, zeitraum.Abrechnungsbeginn, zeitraum.Abrechnungsende, notes);
                }
                foreach (var entry in DieseZaehler)
                {
                    CheckIfZaehlerstaendeValid(entry.Value, zeitraum.Nutzungsbeginn, zeitraum.Nutzungsende, notes);
                }
            }

            foreach (var zaehlerTyp in DieseVerbrauch)
            {
                var alleVerbrauch = AlleVerbrauch[zaehlerTyp.Key];
                Anteil[zaehlerTyp.Key] = alleVerbrauch == 0 ? 0 : zaehlerTyp.Value / alleVerbrauch;
            }
        }

        private static void CheckIfZaehlerstaendeValid(
            List<Verbrauch> verbraeuche,
            DateOnly beginn,
            DateOnly ende,
            List<Note> notes)
        {
            const int thresholdOfDaysBeforeNotOkay = 14;

            foreach (var verbrauch in verbraeuche)
            {
                if (verbrauch.Enddatum <= verbrauch.Anfangsdatum)
                {
                    notes.Add($"Enddatum von {verbrauch.Zaehler.Kennnummer} ist kleiner oder gleich dem Anfang der Zählung " +
                        $"({verbrauch.Anfangsdatum:dd.MM.yyyy} - {verbrauch.Enddatum:dd.MM.yyyy})", Severity.Error);
                }
                else if (verbrauch.Enddatum.DayNumber < (ende.DayNumber - thresholdOfDaysBeforeNotOkay))
                {
                    var candidates = verbraeuche.Where(other =>
                        other.Zaehler != verbrauch.Zaehler &&
                        other.Anfangsdatum == verbrauch.Enddatum &&
                        other.Zaehler.Typ == verbrauch.Zaehler.Typ &&
                        other.Zaehler.Staende?.OrderBy(stand => stand.Datum).FirstOrDefault()?.Datum == other.Anfangsdatum &&
                        verbrauch.Zaehler.Wohnung == other.Zaehler.Wohnung);

                    if (candidates.Count() > 1)
                    {
                        var ersatzString = string.Join(", ", candidates.Select(v => v.Zaehler.Kennnummer));
                        notes.Add($"Mehr als einen Ersatz für {verbrauch.Zaehler.Kennnummer} gefunden: {ersatzString}", Severity.Error);
                    }

                    var ersatz = candidates.FirstOrDefault();

                    if (ersatz != null)
                    {
                        notes.Add($"Zählerwechsel erkannt am {verbrauch.Enddatum:dd.MM.yyyy} von Zähler " +
                            $"{verbrauch.Zaehler.Kennnummer} auf {ersatz.Zaehler.Kennnummer} ({ersatz.Zaehler.Typ})",
                            Severity.Info);
                    }
                    else
                    {
                        notes.Add(
                            $"Kein Endstand für Zähler {verbrauch.Zaehler.Kennnummer} ({verbrauch.Zaehler.Typ}) gefunden. " +
                            $"Letzter gültiger Zählerstand ist vom {verbrauch.Enddatum:dd.MM.yyyy}, was {ende.DayNumber - verbrauch.Enddatum.DayNumber}" +
                            $" Tage vor Nutzungsende ist.",
                            Severity.Error);
                    }
                }

                if (verbrauch.Anfangsdatum > beginn && verbrauch.Anfangsdatum != verbrauch.Enddatum)
                {
                    var candidates = verbraeuche.Where(other =>
                        other.Anfangsdatum != other.Enddatum &&
                        other.Zaehler != verbrauch.Zaehler &&
                        other.Enddatum == verbrauch.Anfangsdatum &&
                        other.Zaehler.Typ == verbrauch.Zaehler.Typ &&
                        verbrauch.Zaehler.Wohnung == other.Zaehler.Wohnung);

                    if (candidates.Count() > 1)
                    {
                        var ersatzString = string.Join(", ", candidates.Select(v => v.Zaehler.Kennnummer));
                        notes.Add($"Mehr als einen Ersatz für {verbrauch.Zaehler.Kennnummer} gefunden: {ersatzString}", Severity.Error);
                    }

                    var vorgaenger = candidates.FirstOrDefault();

                    if (vorgaenger == null)
                    {
                        notes.Add(
                            $"Kein Anfangsstand für Zähler {verbrauch.Zaehler.Kennnummer} ({verbrauch.Zaehler.Typ}) gefunden. " +
                            $"Erster gültiger Zählerstand ist vom {verbrauch.Anfangsdatum}, was nach Nutzungsbeginn ist.",
                            Severity.Error);
                    }
                }
            }
        }

        public static List<VerbrauchAnteil> GetVerbrauchAnteile(
            List<Umlage> umlagen,
            Vertrag vertrag,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return [.. umlagen
                .Where(umlage => umlage.Zaehler.Count > 0)
                .Select(umlage => new VerbrauchAnteil(umlage, vertrag.Wohnung, zeitraum, notes))];
        }
    }
}
