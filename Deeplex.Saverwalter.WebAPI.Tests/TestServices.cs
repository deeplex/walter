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
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    /// <summary>Echte Service-Instanzen für Tests, die mehrere Dienste zusammen brauchen.</summary>
    public static class TestServices
    {
        public static AbrechnungslaufService Abrechnungslauf(SaverwalterContext ctx) =>
            new(ctx,
                new NkAnteilBuchungsService(ctx),
                new AbrechnungsresultatBuchungsService(ctx),
                new AbrechnungsgruppenService(ctx),
                new StornoBuchungsService(ctx),
                new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx)));

        public static RueckgaengigService Rueckgaengig(SaverwalterContext ctx) =>
            new(ctx,
                new BuchungssatzSchutzService(ctx),
                new StornoBuchungsService(ctx),
                Abrechnungslauf(ctx),
                new AbrechnungsgruppenService(ctx));
    }
}
