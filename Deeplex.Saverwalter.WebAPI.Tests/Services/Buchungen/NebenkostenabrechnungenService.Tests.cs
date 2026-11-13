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
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Services.Buchungen.NebenkostenabrechnungenService;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class NebenkostenabrechnungenServiceTests
    {
        private static async Task<Vertrag> VertragAsync(SaverwalterContext ctx)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            return vertrag;
        }

        private static async Task<Abrechnungsresultat> AbrechnungAsync(
            SaverwalterContext ctx, Vertrag vertrag, int jahr, decimal saldo, bool abgesendet = true)
        {
            var resultat = new Abrechnungsresultat { Vertrag = vertrag, Abgesendet = abgesendet };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, jahr, saldo);
            ctx.Abrechnungsresultate.Add(resultat);
            await ctx.SaveChangesAsync();
            return resultat;
        }

        private static Task<Transaktion> AusgleichAsync(
            SaverwalterContext ctx, Abrechnungsresultat resultat, decimal betrag, DateOnly datum) =>
            new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = betrag,
                Zahlungsdatum = datum,
                AbrechnungsAusgleiche =
                {
                    new TransaktionBuchungsService.AbrechnungsAusgleichInput
                    {
                        AbrechnungsresultatId = resultat.AbrechnungsresultatId,
                        Betrag = betrag,
                    }
                },
            });

        private static Task<Nebenkostenabrechnungen> ErmittleAsync(SaverwalterContext ctx, Vertrag vertrag) =>
            new NebenkostenabrechnungenService(ctx).ErmittleAsync(vertrag.VertragId);

        [Fact]
        public async Task TeilbezahlteNachzahlungIstTeilweiseOffen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var resultat = await AbrechnungAsync(ctx, vertrag, 2023, 120m);
            var transaktion = await AusgleichAsync(ctx, resultat, 50m, new DateOnly(2024, 3, 5));

            var abrechnung = (await ErmittleAsync(ctx, vertrag)).Abrechnungen.Should().ContainSingle().Subject;

            abrechnung.Jahr.Should().Be(2023);
            abrechnung.Status.Should().Be(AbrechnungsStatus.Abgesendet);
            abrechnung.Saldo.Should().Be(120m);
            abrechnung.Betrag.Should().Be(120m);
            abrechnung.Bezahlt.Should().Be(50m);
            abrechnung.Offen.Should().Be(70m);
            abrechnung.KannAusgleichen.Should().BeTrue();
            abrechnung.Zahlungen.Should().ContainSingle().Which.Should().Match<AbrechnungsZahlung>(z =>
                z.TransaktionId == transaktion.TransaktionId
                && z.Betrag == 50m
                && z.Datum == new DateOnly(2024, 3, 5)
                && !z.Erstattung);
        }

        [Fact]
        public async Task AusgezahlteErstattungIstNichtMehrOffen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var resultat = await AbrechnungAsync(ctx, vertrag, 2023, -80m);
            await AusgleichAsync(ctx, resultat, 80m, new DateOnly(2024, 3, 5));

            var abrechnung = (await ErmittleAsync(ctx, vertrag)).Abrechnungen.Single();

            abrechnung.Saldo.Should().Be(-80m);
            abrechnung.Betrag.Should().Be(80m);
            abrechnung.Bezahlt.Should().Be(80m);
            abrechnung.Offen.Should().Be(0m);
            abrechnung.KannAusgleichen.Should().BeFalse();
            abrechnung.Zahlungen.Should().ContainSingle().Which.Erstattung.Should().BeTrue();
        }

        [Fact]
        public async Task EntwurfLaesstSichNichtAusgleichen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            await AbrechnungAsync(ctx, vertrag, 2023, 120m, abgesendet: false);

            var abrechnung = (await ErmittleAsync(ctx, vertrag)).Abrechnungen.Single();

            abrechnung.Status.Should().Be(AbrechnungsStatus.Entwurf);
            abrechnung.Offen.Should().Be(120m);
            abrechnung.KannAusgleichen.Should().BeFalse();
        }

        [Fact]
        public async Task StornierteAbrechnungBleibtAlsBelegOhneOffenenBetrag()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var alt = await AbrechnungAsync(ctx, vertrag, 2023, 120m);
            await new StornoBuchungsService(ctx).StornierenAsync(alt.Buchungssatz.BuchungssatzId, "Widerspruch");
            await AbrechnungAsync(ctx, vertrag, 2023, 90m);
            await AbrechnungAsync(ctx, vertrag, 2022, 30m);

            var abrechnungen = (await ErmittleAsync(ctx, vertrag)).Abrechnungen;

            abrechnungen.Select(a => (a.Jahr, a.Status, a.Offen)).Should().Equal(
                (2023, AbrechnungsStatus.Abgesendet, 90m),
                (2023, AbrechnungsStatus.Storniert, 0m),
                (2022, AbrechnungsStatus.Abgesendet, 30m));
        }

        [Fact]
        public async Task StornierteZahlungIstRueckgaengigGemacht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var resultat = await AbrechnungAsync(ctx, vertrag, 2023, 120m);
            var transaktion = await AusgleichAsync(ctx, resultat, 120m, new DateOnly(2024, 3, 5));
            foreach (var satz in await ctx.Buchungssaetze
                .Where(s => s.Transaktion!.TransaktionId == transaktion.TransaktionId).ToListAsync())
            {
                await new StornoBuchungsService(ctx).StornierenAsync(satz.BuchungssatzId, "falsch zugeordnet");
            }

            var ergebnis = await ErmittleAsync(ctx, vertrag);

            ergebnis.Abrechnungen.Single().Offen.Should().Be(120m);
            ergebnis.RueckgaengigGemacht.Should().ContainSingle().Which.Should().Match<AbrechnungsZahlung>(z =>
                z.TransaktionId == transaktion.TransaktionId && z.Betrag == 120m);
        }
    }
}
