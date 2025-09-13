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
    public static class Utils
    {
        public static void Add(this List<Note> notes, string message, Severity severity)
        {
            notes.Add(new Note(message, severity));
        }

        public static T Max<T>(T l, T r) where T : IComparable<T>
            => Max(l, r, Comparer<T>.Default);
        public static T Max<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) < 0 ? r : l;

        public static T Min<T>(T l, T r) where T : IComparable<T>
            => Min(l, r, Comparer<T>.Default);
        public static T Min<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) > 0 ? r : l;

        public static double checkVerbrauch(Dictionary<Umlagetyp, double> verbrauchAnteil, Umlagetyp typ, List<Note> notes)
        {
            if (verbrauchAnteil.ContainsKey(typ))
            {
                return verbrauchAnteil[typ];
            }
            else
            {
                notes.Add("Konnte keinen Anteil für " + typ.Bezeichnung + " feststellen.", Severity.Error);
                return 0;
            }
        }

        public static double Mietzahlungen(Vertrag vertrag, Zeitraum zeitraum)
        {
            return vertrag.Mieten
                .Where(m => m.BetreffenderMonat >= zeitraum.Abrechnungsbeginn &&
                            m.BetreffenderMonat < zeitraum.Abrechnungsende)
                .ToList()
                .Sum(z => z.Betrag);
        }

        public static double GetMietminderung(Vertrag vertrag, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            var Minderungen = vertrag.Mietminderungen
                .Where(m =>
                {
                    var beginBeforeEnd = m.Beginn <= abrechnungsende;
                    var endAfterBegin = m.Ende == null || m.Ende > abrechnungsbeginn;

                    return beginBeforeEnd && endAfterBegin;
                }).ToList();

            return Minderungen
                .Sum(m =>
                {
                    var endDate = Min(m.Ende ?? abrechnungsende, abrechnungsende);
                    var beginDate = Max(m.Beginn, abrechnungsbeginn);

                    return m.Minderung * (endDate.DayNumber - beginDate.DayNumber + 1);
                }) / (abrechnungsende.DayNumber - abrechnungsbeginn.DayNumber + 1);
        }

        public static double GetKaltMiete(Vertrag vertrag, Zeitraum zeitraum)
        {
            if (zeitraum.Jahr < vertrag.Beginn().Year ||
               (vertrag.Ende is DateOnly d && d.Year < zeitraum.Jahr))
            {
                return 0;
            }

            List<double> lasts = [];

            return vertrag.Versionen
                .OrderBy(v => v.Beginn)
                .Sum(version =>
                {
                    var versionEnde = version.Ende();
                    if (versionEnde is DateOnly d && d < zeitraum.Abrechnungsbeginn)
                    {
                        return 0;
                    }
                    else if (version.Beginn > zeitraum.Abrechnungsende)
                    {
                        return 0;
                    }
                    else
                    {
                        var last = Min(versionEnde ?? zeitraum.Abrechnungsende, zeitraum.Abrechnungsende).Month;
                        var first = Max(version.Beginn, zeitraum.Abrechnungsbeginn).Month;
                        // Skip a month if it was already considered in a previous version (e.g. change in mid of month)
                        if (lasts.Contains(first))
                        {
                            first++;
                        }

                        lasts.Add(last);
                        return (last - first + 1) * version.Grundmiete;
                    }
                });
        }
    }
}
