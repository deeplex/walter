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
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    /// <summary>
    /// BookAsync muss verweigern, wenn eine bereits gebuchte Miet-Forderung nicht mehr zur
    /// aktuellen VertragVersion passt (z. B. rückwirkende Mieterhöhung, die noch nicht per
    /// MietKorrekturService nachgebucht wurde) — sonst würde die Abrechnung mit einer falschen
    /// NK-Vorauszahlung gebucht (siehe NkGruppenAbrechnungsService.Vorauszahlung).
    /// </summary>
    public class AbrechnungslaufMietForderungGuardTests
    {
        private const int Jahr = 2024;

        private static AbrechnungslaufService Service(SaverwalterContext ctx) =>
            new(ctx,
                new NkAnteilBuchungsService(ctx),
                new AbrechnungsresultatBuchungsService(ctx),
                new AbrechnungsgruppenService(ctx),
                new StornoBuchungsService(ctx),
                new MietKorrekturService(ctx));

        private static async Task ZahleMonatAsync(
            SaverwalterContext ctx, Vertrag vertrag, DateOnly monat, decimal kaltmiete, decimal nk)
        {
            var service = new TransaktionBuchungsService(ctx);
            await service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = kaltmiete + nk,
                Zahlungsdatum = new DateOnly(monat.Year, monat.Month, 5),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = monat,
                        Kaltmiete = kaltmiete,
                        NkVorauszahlung = nk,
                    }
                }
            });
        }

        private static Vertrag Seed(SaverwalterContext ctx)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);

            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Grundsteuer"),
                Wohnungen = [vertrag.Wohnung],
                NkVerrechnungsKonto = new Buchungskonto("7001", "NK-VK Grundsteuer", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1201", "Zahlung Grundsteuer", BuchungskontoTyp.Aktiv),
            };
            umlage.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), Umlageschluessel.NachWohnflaeche) { Umlage = umlage });
            ctx.Umlagen.Add(umlage);

            var rechnung = new Buchungssatz(new DateOnly(Jahr, 6, 1), $"Betriebskosten Grundsteuer {Jahr}") { Buchungsjahr = Jahr };
            rechnung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 1000m)
            {
                Buchungssatz = rechnung,
                Buchungskonto = umlage.NkVerrechnungsKonto
            });
            ctx.Buchungssaetze.Add(rechnung);

            ctx.SaveChanges();
            return vertrag;
        }

        [Fact]
        public async Task PreviewAsync_WarntBeiAbweichenderMietForderung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = Seed(ctx);

            var monat = new DateOnly(Jahr, 1, 1);
            await ZahleMonatAsync(ctx, vertrag, monat, kaltmiete: 500m, nk: 150m);

            // Rückwirkende Mieterhöhung ab Januar — noch nicht per Miet-Korrektur nachgebucht.
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var service = Service(ctx);
            var wohnungIds = new List<int> { vertrag.Wohnung.WohnungId };

            var preview = await service.PreviewAsync(wohnungIds, Jahr);

            preview.Warnungen.Should().Contain(w =>
                w.Contains(AbrechnungslaufService.MietForderungAbweichungMarker)
                && w.Contains($"Vertrag {vertrag.VertragId}")
                && w.Contains("01/2024"));
        }

        [Fact]
        public async Task BookAsync_WirftBeiAbweichenderMietForderung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = Seed(ctx);

            var monat = new DateOnly(Jahr, 1, 1);
            await ZahleMonatAsync(ctx, vertrag, monat, kaltmiete: 500m, nk: 150m);
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var service = Service(ctx);
            var wohnungIds = new List<int> { vertrag.Wohnung.WohnungId };

            Func<Task> buchen = () => service.BookAsync(wohnungIds, Jahr);
            await buchen.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Miet-Forderung*");

            // Nichts wurde gebucht — insbesondere kein Abrechnungsresultat.
            ctx.Abrechnungsresultate.Should().BeEmpty();
        }

        [Fact]
        public async Task BookAsync_FunktioniertNachDurchgefuehrterMietKorrektur()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = Seed(ctx);

            var monat = new DateOnly(Jahr, 1, 1);
            await ZahleMonatAsync(ctx, vertrag, monat, kaltmiete: 500m, nk: 150m);
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            // Miet-Korrektur wie über die UI durchgeführt (WalterMietKorrektur.svelte).
            var mietKorrektur = new MietKorrekturService(ctx);
            await mietKorrektur.AnwendenAsync(
                vertrag.VertragId, new HashSet<(int, int)> { (Jahr, 1) }, "Testkorrektur");

            var service = Service(ctx);
            var wohnungIds = new List<int> { vertrag.Wohnung.WohnungId };

            var preview = await service.PreviewAsync(wohnungIds, Jahr);
            preview.Warnungen.Should().NotContain(w => w.Contains(AbrechnungslaufService.MietForderungAbweichungMarker));

            Func<Task> buchen = () => service.BookAsync(wohnungIds, Jahr);
            await buchen.Should().NotThrowAsync();

            ctx.Abrechnungsresultate.Should().ContainSingle(r => r.Vertrag.VertragId == vertrag.VertragId);
        }
    }
}
