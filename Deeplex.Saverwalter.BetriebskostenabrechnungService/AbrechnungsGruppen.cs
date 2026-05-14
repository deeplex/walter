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
    /// <summary>
    /// Eine Abrechnungsgruppe: Wohnungen, die transitiv über geteilte Umlagen verbunden sind.
    /// </summary>
    public sealed class AbrechnungsGruppe
    {
        public string GroupKey { get; init; } = "";
        public List<int> WohnungIds { get; init; } = [];
        public string Bezeichnung { get; init; } = "";
    }

    /// <summary>
    /// Pure Union-Find-Gruppierung von Umlagen zu Abrechnungsgruppen.
    /// Frei von EF/IO — Datenladung passiert im Caller (siehe WebAPI-Repository).
    /// </summary>
    public static class AbrechnungsGruppen
    {
        public static List<AbrechnungsGruppe> Compute(IEnumerable<Umlage> umlagen)
        {
            var parent = new Dictionary<int, int>();

            int Find(int x)
            {
                if (!parent.ContainsKey(x)) parent[x] = x;
                if (parent[x] != x) parent[x] = Find(parent[x]);
                return parent[x];
            }

            void Union(int a, int b)
            {
                a = Find(a);
                b = Find(b);
                if (a != b) parent[a] = b;
            }

            var wohnungById = new Dictionary<int, Wohnung>();
            foreach (var umlage in umlagen)
            {
                var ws = umlage.Wohnungen.ToList();
                foreach (var w in ws) wohnungById[w.WohnungId] = w;
                for (int i = 1; i < ws.Count; i++) Union(ws[0].WohnungId, ws[i].WohnungId);
            }

            return wohnungById.Values
                .GroupBy(w => Find(w.WohnungId))
                .Select(g =>
                {
                    var wohnungIds = g.Select(w => w.WohnungId).OrderBy(id => id).ToList();
                    var groupKey = wohnungIds[0].ToString();
                    var adressen = g
                        .Select(w => w.Adresse?.Anschrift ?? "?")
                        .Distinct()
                        .OrderBy(a => a)
                        .ToList();
                    var bezeichnung = string.Join(" · ", adressen);
                    if (wohnungIds.Count > 1) bezeichnung += $" ({wohnungIds.Count} Wohnungen)";
                    return new AbrechnungsGruppe
                    {
                        GroupKey = groupKey,
                        WohnungIds = wohnungIds,
                        Bezeichnung = bezeichnung
                    };
                })
                .OrderBy(g => g.Bezeichnung)
                .ToList();
        }
    }
}
