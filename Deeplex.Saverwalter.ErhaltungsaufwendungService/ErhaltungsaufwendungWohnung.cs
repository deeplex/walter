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

using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.Model
{
    public interface IErhaltungsaufwendungWohnung
    {
        ImmutableList<ErhaltungsaufwendungListeEntry> Liste { get; set; }
        Wohnung Wohnung { get; }
        double Summe { get; }
    }

    public sealed class ErhaltungsaufwendungWohnung : IErhaltungsaufwendungWohnung
    {
        public ImmutableList<ErhaltungsaufwendungListeEntry> Liste { get; set; }
        public Wohnung Wohnung { get; }
        public double Summe => Liste.Sum(e => e.Betrag);

        public ErhaltungsaufwendungWohnung(SaverwalterContext ctx, int WohnungId, int Jahr)
        {
            Wohnung = ctx.Wohnungen.Find(WohnungId)!;

            // TODO sort by... Aussteller, then Datum, then Bezeichnung
            Liste = ctx.Erhaltungsaufwendungen
                .Include(e => e.Wohnung)
                .Where(e => e.Wohnung.WohnungId == WohnungId)
                .Where(e => e.Datum.Year == Jahr)
                .Select(e => new ErhaltungsaufwendungListeEntry(e, ctx))
                .ToImmutableList();
        }
    }
}
