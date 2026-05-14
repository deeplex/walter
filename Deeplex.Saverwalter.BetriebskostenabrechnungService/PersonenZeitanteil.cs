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
    public class PersonenZeitanteil
    {
        public DateOnly Beginn { get; }
        public DateOnly Ende { get; }
        public decimal Tage { get; }
        public int Personenzahl { get; }
        public int GesamtPersonenzahl { get; }
        public decimal Anteil { get; }

        public PersonenZeitanteil(
            DateOnly beginn, DateOnly ende, int personenzahl, int gesamtPersonenzahl, Zeitraum zeitraum)
        {
            Beginn = beginn;
            Ende = ende;
            Tage = ende.DayNumber - beginn.DayNumber + 1;
            Personenzahl = personenzahl;
            GesamtPersonenzahl = gesamtPersonenzahl;

            var personenAnteil = gesamtPersonenzahl == 0
                ? 0
                : (decimal)personenzahl / gesamtPersonenzahl;

            var zeitanteil = Tage / zeitraum.Abrechnungszeitraum;

            Anteil = personenAnteil * zeitanteil;
        }

        public static List<PersonenZeitanteil> GetPersonenZeitanteile(
            Vertrag vertrag,
            List<Wohnung> wohnungen,
            Zeitraum zeitraum)
        {
            var vertraege = wohnungen.SelectMany(e => e.Vertraege).ToList();

            if (!vertraege.Contains(vertrag))
            {
                throw new ArgumentException("Vertrag not in Einheit!");
            }

            var breakPoints = GetTimestampsOfPersonenAnzahlChanges(vertraege, zeitraum);

            List<(DateOnly beginn, int personenzahl)> einheitAnteile = new();

            foreach (var change in breakPoints)
            {
                var sum = SumPersonenzahlen(vertraege, change);
                einheitAnteile.Add((change, sum));
            }

            List<PersonenZeitanteil> personenzeitanteile = new();

            for (var i = 0; i < einheitAnteile.Count; ++i)
            {
                var current = einheitAnteile[i];
                var ende = i == einheitAnteile.Count - 1
                    ? zeitraum.Abrechnungsende
                    : einheitAnteile[i + 1].beginn.AddDays(-1);

                var personenzahl = GetVersion(vertrag, current.beginn)?.Personenzahl ?? 0;

                var personenZeitanteil = new PersonenZeitanteil(
                    current.beginn,
                    ende,
                    personenzahl,
                    current.personenzahl,
                    zeitraum);

                personenzeitanteile.Add(personenZeitanteil);
            }

            return personenzeitanteile;
        }


        private static List<DateOnly> GetTimestampsOfPersonenAnzahlChanges(List<Vertrag> vertraege, Zeitraum zeitraum)
        {
            List<DateOnly> begins = [];
            foreach (var vertrag in vertraege)
            {
                var personenzahl = 0;
                begins.AddRange(vertrag.Versionen
                    .OrderBy(e => e.Beginn)
                    .Where(e =>
                    {
                        var filter = e.Personenzahl != personenzahl;
                        personenzahl = e.Personenzahl;
                        return filter;
                    })
                    .Select(e => e.Beginn));
            }
            begins.Sort();

            var ends = vertraege
                .Where(e => e.Ende != null)
                .Select(e => e.Ende is DateOnly d ? d.AddDays(1) : new DateOnly())
                .ToList();

            var breakpoints = begins
                .Concat(ends)
                .Where(e => e <= zeitraum.Abrechnungsende && e >= zeitraum.Abrechnungsbeginn)
                .Distinct()
                .OrderBy(e => e)
                .ToList();

            if (breakpoints.FirstOrDefault() != zeitraum.Abrechnungsbeginn)
            {
                breakpoints.Insert(0, zeitraum.Abrechnungsbeginn);
            }

            return breakpoints;
        }

        private static VertragVersion? GetVersion(Vertrag vertrag, DateOnly timestamp)
        {
            return vertrag.Versionen.SingleOrDefault(version =>
            {
                var startedBefore = version.Beginn <= timestamp;
                var end = version.Ende();
                var endsAfter = end == null || end > timestamp;
                return startedBefore && endsAfter;
            });
        }

        private static int SumPersonenzahlen(List<Vertrag> vertraege, DateOnly timestamp)
        {
            var personenzahl = 0;

            foreach (var vertrag in vertraege)
            {
                var version = GetVersion(vertrag, timestamp);
                if (version is VertragVersion v)
                {
                    personenzahl += v.Personenzahl;
                }
            }

            return personenzahl;
        }
    }
}
