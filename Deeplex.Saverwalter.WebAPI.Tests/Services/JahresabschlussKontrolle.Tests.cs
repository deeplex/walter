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
    public class JahresabschlussKontrolleTests
    {
        /// <summary>Baut ein Preview-Ergebnis mit einem Vertrag (Resultat + ein Anteil).</summary>
        private static AbrechnungslaufGruppeResult Preview(
            int vertragId,
            decimal saldo,
            decimal? gebuchterSaldo,
            decimal? geplanterAnteil,
            decimal? gebuchterAnteil)
        {
            var anteile = new List<NkAnteilInfo>
            {
                new() { VertragId = vertragId, GeplanterBetrag = geplanterAnteil, GebuchterBetrag = gebuchterAnteil }
            };
            return new AbrechnungslaufGruppeResult
            {
                Resultate =
                [
                    new AbrechnungsresultatInfo
                    {
                        VertragId = vertragId,
                        WohnungBezeichnung = "WE 1",
                        MieterBezeichnung = "Mieter",
                        Saldo = saldo,
                        GebuchterSaldo = gebuchterSaldo
                    }
                ],
                Abrechnungseinheiten =
                [
                    new AbrechnungseinheitInfo { NkZeilen = [new NkZeileInfo { Anteile = anteile }] }
                ]
            };
        }

        private static PruefStatus Status(AbrechnungslaufGruppeResult preview, params int[] verzichtet) =>
            JahresabschlussKontrolle.Klassifiziere(preview, "Gruppe", [.. verzichtet]).Single().Status;

        [Fact]
        public void Bestanden_wenn_saldo_und_anteil_uebereinstimmen()
        {
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: 100m, geplanterAnteil: 50m, gebuchterAnteil: 50m);
            Status(preview).Should().Be(PruefStatus.Bestanden);
        }

        [Fact]
        public void NichtBestanden_wenn_saldo_abweicht()
        {
            var preview = Preview(1, saldo: 120m, gebuchterSaldo: 100m, geplanterAnteil: 50m, gebuchterAnteil: 50m);
            Status(preview).Should().Be(PruefStatus.NichtBestanden);
        }

        [Fact]
        public void NichtBestanden_wenn_anteil_abweicht()
        {
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: 100m, geplanterAnteil: 60m, gebuchterAnteil: 50m);
            Status(preview).Should().Be(PruefStatus.NichtBestanden);
        }

        [Fact]
        public void Bestanden_wenn_anteile_gebucht_und_passend_aber_kein_resultat()
        {
            // Ob die Glattstellung (Resultat) gebucht wurde, ist eine Frage offener Posten,
            // keine Frage der Konsistenz — die gebuchten Anteile stimmen, das reicht.
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: null, geplanterAnteil: 50m, gebuchterAnteil: 50m);
            Status(preview).Should().Be(PruefStatus.Bestanden);
        }

        [Fact]
        public void NichtBestanden_wenn_anteile_gebucht_aber_abweichend_und_kein_resultat()
        {
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: null, geplanterAnteil: 60m, gebuchterAnteil: 50m);
            Status(preview).Should().Be(PruefStatus.NichtBestanden);
        }

        [Fact]
        public void Bestanden_wenn_resultat_passt_aber_anteile_nicht_gebucht()
        {
            // Resultat/Saldo stimmt, Anteile sind noch gar nicht gebucht — auch das ist
            // konsistent, nicht "abweichend".
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: 100m, geplanterAnteil: 50m, gebuchterAnteil: null);
            Status(preview).Should().Be(PruefStatus.Bestanden);
        }

        [Fact]
        public void Fehlt_wenn_nichts_gebucht_und_kein_verzicht()
        {
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: null, geplanterAnteil: 50m, gebuchterAnteil: null);
            Status(preview).Should().Be(PruefStatus.Fehlt);
        }

        [Fact]
        public void Verzichtet_wenn_verzicht_und_nichts_gebucht()
        {
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: null, geplanterAnteil: 50m, gebuchterAnteil: null);
            Status(preview, verzichtet: 1).Should().Be(PruefStatus.Verzichtet);
        }

        [Fact]
        public void Bestanden_wenn_verzicht_und_saldo_und_anteil_trotzdem_uebereinstimmen()
        {
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: 100m, geplanterAnteil: 50m, gebuchterAnteil: 50m);
            Status(preview, verzichtet: 1).Should().Be(PruefStatus.Bestanden);
        }

        [Fact]
        public void NichtBestanden_wenn_verzicht_aber_gebuchter_saldo_abweicht()
        {
            var preview = Preview(1, saldo: 120m, gebuchterSaldo: 100m, geplanterAnteil: 50m, gebuchterAnteil: 50m);
            Status(preview, verzichtet: 1).Should().Be(PruefStatus.NichtBestanden);
        }

        [Fact]
        public void VerwaisteEigenanteile_ergeben_zusaetzliche_NichtBestanden_Position()
        {
            var preview = Preview(1, saldo: 100m, gebuchterSaldo: 100m, geplanterAnteil: 50m, gebuchterAnteil: 50m);
            // Ein gebuchter Eigenanteil (VertragId null), den die Neuberechnung nicht kennt.
            preview.Abrechnungseinheiten[0].NkZeilen[0].Anteile.Add(
                new NkAnteilInfo { VertragId = null, GeplanterBetrag = null, GebuchterBetrag = 42m });

            var positionen = JahresabschlussKontrolle.Klassifiziere(preview, "Gruppe", []).ToList();

            positionen.Should().HaveCount(2);
            positionen.Should().Contain(p =>
                p.VertragId == null && p.Status == PruefStatus.NichtBestanden);
        }

        [Fact]
        public void Aggregiere_zaehlt_je_Status()
        {
            var result = new JahresabschlussKontrolleResult
            {
                Positionen =
                [
                    new PruefPosition { Status = PruefStatus.Bestanden },
                    new PruefPosition { Status = PruefStatus.Bestanden },
                    new PruefPosition { Status = PruefStatus.NichtBestanden },
                    new PruefPosition { Status = PruefStatus.Fehlt },
                    new PruefPosition { Status = PruefStatus.Verzichtet }
                ]
            };

            JahresabschlussKontrolle.Aggregiere(result);

            result.Bestanden.Should().Be(2);
            result.NichtBestanden.Should().Be(1);
            result.Fehlt.Should().Be(1);
            result.Verzichtet.Should().Be(1);
            result.Gesamt.Should().Be(5);
        }
    }
}
