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
    public class ErhaltungsaufwendungWohnungTests
    {
        [Fact]
        public void ErhaltungsaufwendungWohnung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity1 = new Erhaltungsaufwendung(1000, "Test", new DateOnly(2021, 1, 1))
            {
                Aussteller = vertrag.Wohnung.Besitzer!,
                Wohnung = vertrag.Wohnung
            };
            var entity2 = new Erhaltungsaufwendung(2000, "Test2", new DateOnly(2021, 1, 1))
            {
                Aussteller = vertrag.Wohnung.Besitzer!,
                Wohnung = vertrag.Wohnung
            };
            var entity3 = new Erhaltungsaufwendung(3000, "Test3", new DateOnly(2021, 1, 1))
            {
                Aussteller = vertrag.Wohnung.Besitzer!,
                Wohnung = vertrag.Wohnung
            };

            ctx.Erhaltungsaufwendungen.Add(entity1);
            ctx.Erhaltungsaufwendungen.Add(entity2);
            ctx.Erhaltungsaufwendungen.Add(entity3);
            ctx.SaveChanges();

            var entry = new ErhaltungsaufwendungWohnung(
                ctx,
                vertrag.Wohnung.WohnungId,
                2021);

            entry.Wohnung.Should().Be(vertrag.Wohnung);
            entry.Summe.Should().Be(6000);
        }

        [Fact]
        public void ErhaltungsaufwendungWohnungWithOtherWohnung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity1 = new Erhaltungsaufwendung(1000, "Test", new DateOnly(2021, 1, 1))
            {
                Aussteller = vertrag.Wohnung.Besitzer!,
                Wohnung = vertrag.Wohnung
            };
            var entity2 = new Erhaltungsaufwendung(2000, "Test2", new DateOnly(2021, 1, 1))
            {
                Aussteller = vertrag.Wohnung.Besitzer!,
                Wohnung = vertrag.Wohnung
            };
            var entity3 = new Erhaltungsaufwendung(3000, "Test3", new DateOnly(2021, 1, 1))
            {
                Aussteller = vertrag.Wohnung.Besitzer!,
                Wohnung = vertrag.Wohnung
            };

            var wohnung = new Wohnung("Test", 100, 100, 100, 1);
            var entity4 = new Erhaltungsaufwendung(2000, "Test4", new DateOnly(2021, 1, 1))
            {
                Wohnung = wohnung
            };
            var entity5 = new Erhaltungsaufwendung(3000, "Test5", new DateOnly(2021, 1, 1))
            {
                Aussteller = vertrag.Wohnung.Besitzer!,
                Wohnung = wohnung
            };

            ctx.Erhaltungsaufwendungen.Add(entity1);
            ctx.Erhaltungsaufwendungen.Add(entity2);
            ctx.Erhaltungsaufwendungen.Add(entity3);
            ctx.Erhaltungsaufwendungen.Add(entity4);
            ctx.Erhaltungsaufwendungen.Add(entity5);
            ctx.SaveChanges();

            var entry = new ErhaltungsaufwendungWohnung(
                ctx,
                wohnung.WohnungId,
                2021);

            entry.Wohnung.Should().Be(wohnung);
            entry.Summe.Should().Be(5000);
        }
    }
}
