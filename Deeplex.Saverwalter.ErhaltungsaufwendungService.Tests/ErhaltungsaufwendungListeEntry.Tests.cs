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
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.ErhaltungsaufwendungService.Tests
{
    public class ErhaltungsaufwendungListeEntryTests
    {
        [Fact]
        public void ErhaltungsaufwendungListeEntry()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Erhaltungsaufwendung(1000, "Test", new DateOnly(2021, 1, 1))
            {
                Aussteller = vertrag.Wohnung.Besitzer!,
                Wohnung = vertrag.Wohnung
            };

            var entry = new ErhaltungsaufwendungListeEntry(entity, ctx);

            entry.Wohnung.Should().Be(vertrag.Wohnung);
            entry.Betrag.Should().Be(1000);
            entry.Datum.Should().Be(new DateOnly(2021, 1, 1));
            entry.Bezeichnung.Should().Be("Test");
        }
    }
}
