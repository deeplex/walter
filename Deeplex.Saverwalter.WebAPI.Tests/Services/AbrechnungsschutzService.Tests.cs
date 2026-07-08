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

using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AbrechnungsschutzServiceTests
    {
        [Theory]
        [InlineData(6, 15, new[] { 2024 })]        // Mitte des Jahres → nur dieses Jahr
        [InlineData(12, 15, new[] { 2024 })]       // früher Dezember → noch nicht im Anfangsstand-Fenster
        [InlineData(12, 31, new[] { 2024, 2025 })] // später Dezember → auch Anfangsstand des Folgejahres
        [InlineData(1, 1, new[] { 2024 })]         // 01.01. → nur eigenes Jahr (nie Endstand des Vorjahres)
        public void StandBetroffeneJahre_BeachtetJahresgrenzen(int monat, int tag, int[] erwartet)
        {
            var jahre = AbrechnungsschutzService
                .StandBetroffeneJahre(new DateOnly(2024, monat, tag)).ToHashSet();
            jahre.Should().BeEquivalentTo(erwartet);
        }

        [Fact]
        public void BetroffeneAbgerechneteJahre_NimmtNurJahreAbBetroffen()
        {
            var abgerechnet = new HashSet<int> { 2021, 2023, 2024 };
            // Änderung wirkt ab 2023 → 2021 bleibt unberührt.
            AbrechnungsschutzService.BetroffeneAbgerechneteJahre(abgerechnet, 2023)
                .Should().Equal(2023, 2024);
            // Änderung nur ab 2025 → nichts betroffen.
            AbrechnungsschutzService.BetroffeneAbgerechneteJahre(abgerechnet, 2025)
                .Should().BeEmpty();
        }

        [Fact]
        public void Grenzverschiebung_BetrifftNurBereichZwischenAltUndNeu()
        {
            var abgerechnet = new HashSet<int> { 2023, 2024 };
            // Beginn 15.09.2023 → 01.10.2023: nur 2023 betroffen, 2024 bleibt unberührt.
            AbrechnungsschutzService.BetroffeneJahreGrenzverschiebung(
                abgerechnet, new DateOnly(2023, 9, 15), new DateOnly(2023, 10, 1))
                .Should().Equal(2023);
            // Beginn nach vorne 2024 → 2022: Bereich 2022–2024 → nur abgerechnete 2023, 2024.
            AbrechnungsschutzService.BetroffeneJahreGrenzverschiebung(
                abgerechnet, new DateOnly(2024, 3, 1), new DateOnly(2022, 3, 1))
                .Should().Equal(2023, 2024);
        }

        [Fact]
        public void Grenzverschiebung_OffeneSeiteWirktBisInDieZukunft()
        {
            var abgerechnet = new HashSet<int> { 2023, 2024, 2025 };
            // Ende von unbefristet (null) auf 2023: ab 2023 fällt der Mieter weg → 2023–2025.
            AbrechnungsschutzService.BetroffeneJahreGrenzverschiebung(
                abgerechnet, null, new DateOnly(2023, 12, 31))
                .Should().Equal(2023, 2024, 2025);
            // Ende 2024 → 2025: nur der Bereich 2024–2025.
            AbrechnungsschutzService.BetroffeneJahreGrenzverschiebung(
                abgerechnet, new DateOnly(2024, 6, 1), new DateOnly(2025, 6, 1))
                .Should().Equal(2024, 2025);
        }

        [Fact]
        public void Versionsaenderung_WertWirktVorwaertsGrenzeNurImBereich()
        {
            var abgerechnet = new HashSet<int> { 2023, 2024, 2025 };
            // Nur Beginn 15.09.2023 → 01.10.2023 verschoben (kein Wert): nur 2023.
            AbrechnungsschutzService.BetroffeneJahreVersionsaenderung(
                abgerechnet, new DateOnly(2023, 9, 15), new DateOnly(2023, 10, 1), wertGeaendert: false)
                .Should().Equal(2023);
            // Wertänderung (z.B. Personenzahl) ab Versionsbeginn 2023 → 2023, 2024, 2025.
            AbrechnungsschutzService.BetroffeneJahreVersionsaenderung(
                abgerechnet, new DateOnly(2023, 9, 15), new DateOnly(2023, 9, 15), wertGeaendert: true)
                .Should().Equal(2023, 2024, 2025);
        }

        [Fact]
        public void Schnittmenge_TrifftNurAbgerechneteStandjahre()
        {
            var abgerechnet = new HashSet<int> { 2023 };
            // Dezember-2023-Stand betrifft 2023 (abgerechnet) und 2024 (nicht) → 2023.
            AbrechnungsschutzService.Schnittmenge(
                abgerechnet, AbrechnungsschutzService.StandBetroffeneJahre(new DateOnly(2023, 12, 31)))
                .Should().Equal(2023);
            // Mitten-2025-Stand betrifft 2025 (nicht abgerechnet) → leer.
            AbrechnungsschutzService.Schnittmenge(
                abgerechnet, AbrechnungsschutzService.StandBetroffeneJahre(new DateOnly(2025, 6, 1)))
                .Should().BeEmpty();
        }
    }
}
