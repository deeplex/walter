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
using Deeplex.Saverwalter.WebAPI.Services;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class FileFamiliesTests
    {
        [Fact]
        public async Task VertragUmfasstNachtraegeUndMietminderungen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            vertrag.Versionen.Single().Beginn = new DateOnly(2024, 1, 1);
            vertrag.Versionen.Add(new VertragVersion(new DateOnly(2025, 3, 1), 520m, 1) { Vertrag = vertrag });
            vertrag.Mietminderungen.Add(new Mietminderung(new DateOnly(2025, 6, 1), 50m) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var family = FileFamilies.Of(vertrag);

            family.Members.Select(m => (m.Route, m.Label)).Should().Equal(
                ("vertraege", "Vertrag"),
                ("vertragversionen", "Nachtrag ab 01.01.2024"),
                ("vertragversionen", "Nachtrag ab 01.03.2025"),
                ("mietminderungen", "Mietminderung ab 01.06.2025"));
            family.Members[0].Id.Should().Be($"{vertrag.VertragId}");
        }

        [Fact]
        public async Task EinKindHatDieselbeFamilieWieSeinEltern()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var minderung = new Mietminderung(new DateOnly(2025, 6, 1), 50m) { Vertrag = vertrag };
            vertrag.Mietminderungen.Add(minderung);
            await ctx.SaveChangesAsync();

            FileFamilies.Of(minderung).Members.Should().BeEquivalentTo(FileFamilies.Of(vertrag).Members);
            FileFamilies.Of(vertrag.Versionen.Single()).Members.Should().BeEquivalentTo(FileFamilies.Of(vertrag).Members);
        }

        [Fact]
        public async Task ZaehlerUmfasstSeineZaehlerstaende()
        {
            var ctx = TestUtils.GetContext();
            var zaehler = new Zaehler("Z1", Zaehlertyp.Strom);
            var stand = new Zaehlerstand(new DateOnly(2024, 12, 31), 4000m) { Zaehler = zaehler };
            zaehler.Staende.Add(stand);
            ctx.ZaehlerSet.Add(zaehler);
            await ctx.SaveChangesAsync();

            var family = FileFamilies.Of(stand);

            family.Members.Select(m => (m.Route, m.Label)).Should().Equal(
                ("zaehler", "Zähler"), ("zaehlerstaende", "Zählerstand 31.12.2024"));
        }
    }
}
