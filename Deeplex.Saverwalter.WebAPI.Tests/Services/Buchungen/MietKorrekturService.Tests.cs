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

namespace Deeplex.Saverwalter.WebAPI.Tests.Buchungen
{
    public class MietKorrekturServiceTests
    {
        private static async Task ZahleMonatAsync(
            SaverwalterContext ctx, Vertrag vertrag, DateOnly monat, decimal kaltmiete, decimal nk, int zahlungsTag = 5)
        {
            var service = new TransaktionBuchungsService(ctx);
            await service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = kaltmiete + nk,
                Zahlungsdatum = new DateOnly(monat.Year, monat.Month, zahlungsTag),
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

        /// <summary>
        /// Legt eine Sollstellung ohne Zahlung an (analog zu
        /// TransaktionBuchungsService.FindeOderErstelleSollstellung, aber ohne Zahlung).
        /// </summary>
        private static void BucheUnbezahlteSollstellung(SaverwalterContext ctx, Vertrag vertrag, DateOnly monat, decimal betrag)
        {
            var satz = new Buchungssatz(monat, $"Mietsoll {monat:MM/yyyy}");
            var soll = new Buchungszeile(SollHaben.Soll, betrag) { Buchungssatz = satz, Buchungskonto = vertrag.MietBuchungskonto };
            var haben = new Buchungszeile(SollHaben.Haben, betrag) { Buchungssatz = satz, Buchungskonto = vertrag.Wohnung.MietErtragskonto };
            satz.Buchungszeilen.Add(soll);
            satz.Buchungszeilen.Add(haben);
            ctx.Buchungssaetze.Add(satz);
            ctx.SaveChanges();
        }

        private static async Task<Vertrag> ReloadAsync(SaverwalterContext ctx, int vertragId) =>
            await ctx.Vertraege.Include(v => v.Versionen).FirstAsync(v => v.VertragId == vertragId);

        [Fact]
        public async Task BezahlterMonatMitErhoehungWirdKorrigiert()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var monat = new DateOnly(2024, 1, 1);
            await ZahleMonatAsync(ctx, vertrag, monat, kaltmiete: 500m, nk: 150m);

            // Rückwirkende Mieterhöhung: neue VertragVersion ab demselben Monat.
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var service = new MietKorrekturService(ctx);
            var monate = await service.ErmittleAsync(vertrag.VertragId);

            monate.Should().ContainSingle();
            var m = monate.Single();
            m.Jahr.Should().Be(2024);
            m.Monat.Should().Be(1);
            m.IstKaltmiete.Should().Be(500m);
            m.SollKaltmiete.Should().Be(600m);
            m.Delta.Should().Be(100m);
            m.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Anwendbar);

            var ergebnis = await service.AnwendenAsync(
                vertrag.VertragId, new HashSet<(int, int)> { (2024, 1) }, "Testkorrektur");

            ergebnis.Korrigiert.Should().ContainSingle(k => k.Jahr == 2024 && k.Monat == 1);
            ergebnis.Uebersprungen.Should().BeEmpty();

            // Mietforderungskonto ist für den Monat wieder voll ausgeglichen.
            var mbZeilen = await ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == vertrag.MietBuchungskonto.BuchungskontoId)
                .ToListAsync();
            var sollSumme = mbZeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag);
            var habenSumme = mbZeilen.Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag);
            sollSumme.Should().Be(habenSumme);
            sollSumme.Should().Be(600m);

            // Ursprüngliche Sollstellung/Zahlung bleiben unverändert.
            var sollstellung = await ctx.Buchungssaetze.Include(s => s.Buchungszeilen)
                .SingleAsync(s => s.Beschreibung == $"Mietsoll {monat:MM/yyyy}");
            sollstellung.Buchungszeilen.Should().OnlyContain(z => z.Betrag == 500m);

            var kaltmieteZahlung = await ctx.Buchungssaetze.Include(s => s.Buchungszeilen)
                .SingleAsync(s => s.Beschreibung.Contains("Mietzahlung Kaltmiete"));
            kaltmieteZahlung.Buchungszeilen.Should().OnlyContain(z => z.Betrag == 500m);

            // NK-Konto wurde um genau das Delta reduziert.
            var nkSaldo = await ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == vertrag.NkBuchungskonto.BuchungskontoId)
                .SumAsync(z => z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag);
            nkSaldo.Should().Be(150m - 100m);
        }

        [Fact]
        public async Task BezahlterMonatMitReduzierungWirdUmgekehrtKorrigiert()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var monat = new DateOnly(2024, 1, 1);
            await ZahleMonatAsync(ctx, vertrag, monat, kaltmiete: 500m, nk: 150m);

            // Rückwirkende Reduzierung.
            vertrag.Versionen.Add(new VertragVersion(monat, 420m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var service = new MietKorrekturService(ctx);
            var monate = await service.ErmittleAsync(vertrag.VertragId);
            var m = monate.Should().ContainSingle().Subject;
            m.Delta.Should().Be(-80m);
            m.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Anwendbar);

            await service.AnwendenAsync(vertrag.VertragId, new HashSet<(int, int)> { (2024, 1) }, "Reduzierung");

            // Mietforderungskonto bleibt voll ausgeglichen (kein offener Posten) — die
            // Bruttosumme je Seite ist kein Abbild der "wahren" Kaltmiete mehr, sobald
            // Korrekturbuchungen existieren; maßgeblich ist Kaltmiete = Grundmiete × Monate
            // (siehe docs/buchhalter.md), nicht die Kontosumme.
            var mbZeilen = await ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == vertrag.MietBuchungskonto.BuchungskontoId)
                .ToListAsync();
            mbZeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag)
                .Should().Be(mbZeilen.Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag));

            var nkSaldo = await ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == vertrag.NkBuchungskonto.BuchungskontoId)
                .SumAsync(z => z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag);
            nkSaldo.Should().Be(150m + 80m);
        }

        [Fact]
        public async Task UnbezahlterMonatTauchtNichtAuf()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var monat = new DateOnly(2024, 1, 1);
            BucheUnbezahlteSollstellung(ctx, vertrag, monat, 500m);

            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var service = new MietKorrekturService(ctx);
            var monate = await service.ErmittleAsync(vertrag.VertragId);

            monate.Should().BeEmpty();
        }

        [Fact]
        public async Task AbgerechnetesJahrWirdGesperrt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var monat = new DateOnly(2024, 1, 1);
            await ZahleMonatAsync(ctx, vertrag, monat, kaltmiete: 500m, nk: 150m);
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });

            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, 2024, 0m);
            ctx.Abrechnungsresultate.Add(resultat);
            await ctx.SaveChangesAsync();

            var service = new MietKorrekturService(ctx);
            var monate = await service.ErmittleAsync(vertrag.VertragId);
            var m = monate.Should().ContainSingle().Subject;
            m.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Abgerechnet);

            var ergebnis = await service.AnwendenAsync(
                vertrag.VertragId, new HashSet<(int, int)> { (2024, 1) }, "Grund");
            ergebnis.Korrigiert.Should().BeEmpty();
            ergebnis.Uebersprungen.Should().ContainSingle(u => u.Jahr == 2024 && u.Monat == 1);
        }

        [Fact]
        public async Task NkReichtNichtAusWirdGesperrt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var monat = new DateOnly(2024, 1, 1);
            // Nur 50 NK gezahlt, aber Delta wird 100 sein -> reicht nicht.
            await ZahleMonatAsync(ctx, vertrag, monat, kaltmiete: 500m, nk: 50m);
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var service = new MietKorrekturService(ctx);
            var monate = await service.ErmittleAsync(vertrag.VertragId);
            var m = monate.Should().ContainSingle().Subject;
            m.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.NkReichtNichtAus);

            var ergebnis = await service.AnwendenAsync(
                vertrag.VertragId, new HashSet<(int, int)> { (2024, 1) }, "Grund");
            ergebnis.Korrigiert.Should().BeEmpty();
            ergebnis.Uebersprungen.Should().ContainSingle();
        }

        [Fact]
        public async Task ErneuteAnwendungIstIdempotent()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var monat = new DateOnly(2024, 1, 1);
            await ZahleMonatAsync(ctx, vertrag, monat, kaltmiete: 500m, nk: 150m);
            vertrag.Versionen.Add(new VertragVersion(monat, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var service = new MietKorrekturService(ctx);
            await service.AnwendenAsync(vertrag.VertragId, new HashSet<(int, int)> { (2024, 1) }, "Erste Korrektur");

            var monateNachher = await service.ErmittleAsync(vertrag.VertragId);
            monateNachher.Should().ContainSingle(m => m.Status == MietKorrekturService.MietKorrekturStatus.BereitsKorrigiert,
                "die Korrektur wurde bereits gebucht und darf nicht nochmal als 'Anwendbar' vorgeschlagen werden");

            var buchungssaetzeVorher = await ctx.Buchungssaetze.CountAsync();
            var ergebnis = await service.AnwendenAsync(
                vertrag.VertragId, new HashSet<(int, int)> { (2024, 1) }, "Zweiter Versuch");
            ergebnis.Korrigiert.Should().BeEmpty();
            (await ctx.Buchungssaetze.CountAsync()).Should().Be(buchungssaetzeVorher, "es darf nicht doppelt gebucht werden");
        }

        [Fact]
        public async Task MehrereMonateWerdenJeEigenKorrigiert()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var jan = new DateOnly(2024, 1, 1);
            var feb = new DateOnly(2024, 2, 1);
            await ZahleMonatAsync(ctx, vertrag, jan, kaltmiete: 500m, nk: 150m);
            await ZahleMonatAsync(ctx, vertrag, feb, kaltmiete: 500m, nk: 150m, zahlungsTag: 6);
            vertrag.Versionen.Add(new VertragVersion(jan, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            var service = new MietKorrekturService(ctx);
            var monate = await service.ErmittleAsync(vertrag.VertragId);
            monate.Should().HaveCount(2);

            var ergebnis = await service.AnwendenAsync(
                vertrag.VertragId,
                new HashSet<(int, int)> { (2024, 1), (2024, 2) },
                "Beide Monate");

            ergebnis.Korrigiert.Should().HaveCount(2);
            (await service.ErmittleAsync(vertrag.VertragId)).Should()
                .OnlyContain(m => m.Status == MietKorrekturService.MietKorrekturStatus.BereitsKorrigiert);
        }
    }
}
