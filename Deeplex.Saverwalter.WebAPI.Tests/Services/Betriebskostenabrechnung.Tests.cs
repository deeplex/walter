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

using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BetriebskostenabrechnungHandlerTests
    {
        [Fact]
        public void GetTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var handler = new BetriebskostenabrechnungHandler(ctx);

            var result = handler.Get(vertrag.VertragId, 2021);

            result.Should().NotBeNull();
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public void GetWordDocumentTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var handler = new BetriebskostenabrechnungHandler(ctx);

            var result = handler.GetWordDocument(vertrag.VertragId, 2021);

            result.Should().NotBeNull();
            result.Value.Should().NotBeNull();
        }

        [Fact(Skip = "PDF is TODO")]
        public void GetPdfDocumentTest()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var handler = new BetriebskostenabrechnungHandler(ctx);

            var result = handler.GetPdfDocument(vertrag.VertragId, 2021);

            result.Should().NotBeNull();
            result.Value.Should().NotBeNull();
        }
    }
}
