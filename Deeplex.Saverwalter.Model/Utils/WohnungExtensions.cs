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

namespace Deeplex.Saverwalter.Model
{
    public static class WohnungExtensions
    {
        /// <summary>Returns the WohnungVersion active at the given date (latest Beginn ≤ date), falling back to the earliest version.</summary>
        public static WohnungVersion VersionAt(this Wohnung w, DateOnly date)
            => w.Versionen.OrderByDescending(v => v.Beginn).FirstOrDefault(v => v.Beginn <= date)
               ?? w.Versionen.OrderBy(v => v.Beginn).First();

        /// <summary>Returns the UmlageVersion active at the given date, falling back to the earliest version.</summary>
        public static UmlageVersion VersionAt(this Umlage u, DateOnly date)
            => u.Versionen.OrderByDescending(v => v.Beginn).FirstOrDefault(v => v.Beginn <= date)
               ?? u.Versionen.OrderBy(v => v.Beginn).First();

        /// <summary>Returns the HKVO active at the given date (latest Beginn ≤ date), or null if none.</summary>
        public static HKVO? HkvoAt(this Umlage u, DateOnly date)
            => u.HeizkostenHKVOs.OrderByDescending(h => h.Beginn).FirstOrDefault(h => h.Beginn <= date);


        public static string GetWohnungenBezeichnung(this Umlage u)
            => u.Wohnungen.ToList().GetWohnungenBezeichnung();

        public static string GetWohnungenBezeichnung(this List<Wohnung> Wohnungen)
            => string.Join(" — ", Wohnungen
                .Where(w => w.Adresse != null)
                .GroupBy(w => w.Adresse!)
                .ToDictionary(g => g.Key, g => g.ToList())
                .Select(adr =>
                {
                    var a = adr.Key;
                    var ret = a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;
                    if (adr.Value.Count() != a.Wohnungen.Count)
                    {
                        ret += ": " + string.Join(", ", adr.Value.Select(w => w.Bezeichnung));
                    }
                    else
                    {
                        ret += " (gesamt)";
                    }
                    return ret;
                }));
    }
}
