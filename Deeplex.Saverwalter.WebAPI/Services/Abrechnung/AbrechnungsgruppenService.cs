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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services.Abrechnung
{
    /// <summary>
    /// Dünner EF-Wrapper: lädt Umlagen aus der DB und delegiert die
    /// pure Gruppierungslogik an <see cref="AbrechnungsGruppen.Compute"/>.
    /// </summary>
    public class AbrechnungsgruppenService
    {
        private readonly SaverwalterContext _ctx;

        public AbrechnungsgruppenService(SaverwalterContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<AbrechnungsGruppe>> GetGruppenAsync()
        {
            var umlagen = await _ctx.Umlagen
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Adresse)
                .Where(u => u.Wohnungen.Any())
                .ToListAsync();

            return AbrechnungsGruppen.Compute(umlagen);
        }
    }
}
