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
using Deeplex.Saverwalter.WebAPI.Services;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AbrechnungsgruppenServiceTests
    {
        private static Wohnung MakeWohnung(int id)
        {
            var w = new Wohnung("W" + id) { WohnungId = id };
            w.Versionen.Add(new WohnungVersion(new DateOnly(2000, 1, 1), 50, 50, 100, 1) { Wohnung = w });
            return w;
        }

        private static Umlage MakeUmlage(params Wohnung[] wohnungen)
        {
            var u = new Umlage();
            u.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), Umlageschluessel.NachWohnflaeche) { Umlage = u });
            u.Wohnungen.AddRange(wohnungen);
            return u;
        }

        // ── Coverage and uniqueness ──────────────────────────────────────────

        [Fact]
        public void EveryUmlageTargetWohnung_AppearsInExactlyOneGroup()
        {
            var w1 = MakeWohnung(1);
            var w2 = MakeWohnung(2);
            var w3 = MakeWohnung(3);
            var w4 = MakeWohnung(4);

            // w1+w2 share one Umlage, w2+w3 share another → w1,w2,w3 are transitively connected
            // w4 is isolated in its own Umlage
            var umlagen = new[]
            {
                MakeUmlage(w1, w2),
                MakeUmlage(w2, w3),
                MakeUmlage(w4),
            };

            var gruppen = AbrechnungsGruppen.Compute(umlagen);

            var allWohnungIds = umlagen
                .SelectMany(u => u.Wohnungen)
                .Select(w => w.WohnungId)
                .Distinct()
                .ToHashSet();

            // Every Wohnung that is a target of any Umlage must be in at least one group
            var coveredIds = gruppen.SelectMany(g => g.WohnungIds).ToList();
            coveredIds.Should().Contain(allWohnungIds);

            // Every Wohnung must be in exactly one group
            coveredIds.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void EveryUmlageTargetWohnung_AppearsInExactlyOneGroup_WhenAllShareOneUmlage()
        {
            var wohnungen = Enumerable.Range(1, 5).Select(MakeWohnung).ToArray();
            var umlagen = new[] { MakeUmlage(wohnungen) };

            var gruppen = AbrechnungsGruppen.Compute(umlagen);

            var coveredIds = gruppen.SelectMany(g => g.WohnungIds).ToList();
            coveredIds.Should().Contain(wohnungen.Select(w => w.WohnungId));
            coveredIds.Should().OnlyHaveUniqueItems();
        }

        // ── Structural grouping ──────────────────────────────────────────────

        [Fact]
        public void NoUmlagen_ReturnsNoGruppen()
        {
            var gruppen = AbrechnungsGruppen.Compute([]);
            gruppen.Should().BeEmpty();
        }

        [Fact]
        public void SingleUmlageWithOneWohnung_ReturnsOneGruppe()
        {
            var w = MakeWohnung(1);
            var gruppen = AbrechnungsGruppen.Compute([MakeUmlage(w)]);

            gruppen.Should().HaveCount(1);
            gruppen[0].WohnungIds.Should().Equal([1]);
        }

        [Fact]
        public void SingleUmlageWithMultipleWohnungen_AllInOneGruppe()
        {
            var w1 = MakeWohnung(1);
            var w2 = MakeWohnung(2);
            var w3 = MakeWohnung(3);

            var gruppen = AbrechnungsGruppen.Compute([MakeUmlage(w1, w2, w3)]);

            gruppen.Should().HaveCount(1);
            gruppen[0].WohnungIds.Should().BeEquivalentTo([1, 2, 3]);
        }

        [Fact]
        public void TwoUmlagenWithDisjointWohnungen_ReturnsTwoGruppen()
        {
            var w1 = MakeWohnung(1);
            var w2 = MakeWohnung(2);

            var gruppen = AbrechnungsGruppen.Compute(
            [
                MakeUmlage(w1),
                MakeUmlage(w2),
            ]);

            gruppen.Should().HaveCount(2);
        }

        [Fact]
        public void TwoUmlagenSharingOneWohnung_MergedIntoOneGruppe()
        {
            var w1 = MakeWohnung(1);
            var w2 = MakeWohnung(2);
            var w3 = MakeWohnung(3);

            // w1+w2 in one Umlage, w2+w3 in another → all three transitively connected
            var gruppen = AbrechnungsGruppen.Compute(
            [
                MakeUmlage(w1, w2),
                MakeUmlage(w2, w3),
            ]);

            gruppen.Should().HaveCount(1);
            gruppen[0].WohnungIds.Should().BeEquivalentTo([1, 2, 3]);
        }

        [Fact]
        public void TransitiveChain_AllWohnungenInOneGruppe()
        {
            // w1-w2, w2-w3, w3-w4: no single Umlage covers all four, but they're transitively linked
            var w1 = MakeWohnung(1);
            var w2 = MakeWohnung(2);
            var w3 = MakeWohnung(3);
            var w4 = MakeWohnung(4);

            var gruppen = AbrechnungsGruppen.Compute(
            [
                MakeUmlage(w1, w2),
                MakeUmlage(w2, w3),
                MakeUmlage(w3, w4),
            ]);

            gruppen.Should().HaveCount(1);
            gruppen[0].WohnungIds.Should().BeEquivalentTo([1, 2, 3, 4]);
        }

        [Fact]
        public void MixedConnectedAndIsolated_CorrectGroupCount()
        {
            var w1 = MakeWohnung(1);
            var w2 = MakeWohnung(2);
            var w3 = MakeWohnung(3); // isolated
            var w4 = MakeWohnung(4); // isolated

            var gruppen = AbrechnungsGruppen.Compute(
            [
                MakeUmlage(w1, w2),
                MakeUmlage(w3),
                MakeUmlage(w4),
            ]);

            gruppen.Should().HaveCount(3);
            gruppen.Should().ContainSingle(g => g.WohnungIds.SequenceEqual(new[] { 1, 2 }));
            gruppen.Should().ContainSingle(g => g.WohnungIds.Contains(3));
            gruppen.Should().ContainSingle(g => g.WohnungIds.Contains(4));
        }
    }
}
