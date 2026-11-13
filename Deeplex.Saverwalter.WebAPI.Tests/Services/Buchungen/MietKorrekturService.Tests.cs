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

        private static MietKorrekturService Service(SaverwalterContext ctx) =>
            new(ctx, new BuchungssatzSchutzService(ctx));

        /// <summary>Bezahlte Monate 2024 mit 500 Kaltmiete und der angegebenen NK-VZ, danach neue Versionen.</summary>
        private static async Task<Vertrag> VertragAsync(
            SaverwalterContext ctx, decimal nk, int monate, params (int Monat, decimal Grundmiete)[] versionen)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            for (var m = 1; m <= monate; m++)
                await ZahleMonatAsync(ctx, vertrag, new DateOnly(2024, m, 1), kaltmiete: 500m, nk: nk, zahlungsTag: 4 + m);
            foreach (var (monat, grundmiete) in versionen)
                vertrag.Versionen.Add(new VertragVersion(new DateOnly(2024, monat, 1), grundmiete, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();
            return vertrag;
        }

        private static async Task<MietKorrekturService.MietKorrekturJahr> JahrAsync(SaverwalterContext ctx, Vertrag vertrag, int jahr = 2024) =>
            (await Service(ctx).ErmittleAsync(vertrag.VertragId)).Should().ContainSingle(j => j.Jahr == jahr).Subject;

        private static async Task<decimal> SaldoAsync(SaverwalterContext ctx, Buchungskonto konto) =>
            await ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == konto.BuchungskontoId)
                .SumAsync(z => z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag);

        private static void Abrechnen(SaverwalterContext ctx, Vertrag vertrag, int jahr)
        {
            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, jahr, 0m);
            ctx.Abrechnungsresultate.Add(resultat);
            ctx.SaveChanges();
        }

        [Fact]
        public async Task ErhoehungWirdAlsEinGewoehnlicherSatzGebucht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 600m));

            var vorher = await JahrAsync(ctx, vertrag);
            vorher.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Anwendbar);
            vorher.Monate.Should().ContainSingle(m => m.Monat == 1 && m.IstKaltmiete == 500m && m.SollKaltmiete == 600m);
            vorher.Differenz.Should().Be(100m);

            var satz = await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Mieterhöhung nachgetragen");

            satz.Transaktion.Should().BeNull();
            satz.Buchungsjahr.Should().Be(2024);
            satz.Buchungsdatum.Should().Be(DateOnly.FromDateTime(DateTime.Today));
            satz.Notiz.Should().Be("Mieterhöhung nachgetragen");
            satz.Buchungszeilen.Should().HaveCount(4).And.OnlyContain(z => z.Betrag == 100m);

            // Mietforderung ausgeglichen, Kaltmiete 600, NK-VZ um 100 kleiner.
            (await SaldoAsync(ctx, vertrag.MietBuchungskonto)).Should().Be(0m);
            (await SaldoAsync(ctx, vertrag.Wohnung.MietErtragskonto)).Should().Be(600m);
            (await SaldoAsync(ctx, vertrag.NkBuchungskonto)).Should().Be(50m);

            var nachher = await JahrAsync(ctx, vertrag);
            nachher.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Stimmt);
            nachher.Korrigiert.Should().Be(100m);
            nachher.NkVorauszahlung.Should().Be(50m);
            nachher.Korrekturen.Should().ContainSingle(k => k.BuchungssatzId == satz.BuchungssatzId && k.KannLoeschen);
        }

        [Fact]
        public async Task SenkungWirdUmgekehrtGebucht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 420m));

            (await JahrAsync(ctx, vertrag)).Differenz.Should().Be(-80m);
            await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Senkung");

            (await SaldoAsync(ctx, vertrag.MietBuchungskonto)).Should().Be(0m);
            (await SaldoAsync(ctx, vertrag.Wohnung.MietErtragskonto)).Should().Be(420m);
            (await SaldoAsync(ctx, vertrag.NkBuchungskonto)).Should().Be(230m);
            var jahr = await JahrAsync(ctx, vertrag);
            jahr.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Stimmt);
            jahr.Korrekturen.Should().ContainSingle(k => k.Betrag == -80m);
        }

        [Fact]
        public async Task MonateEinesJahresWerdenZusammenGebucht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 2, (1, 600m));

            var jahr = await JahrAsync(ctx, vertrag);
            jahr.Monate.Should().HaveCount(2);
            jahr.Differenz.Should().Be(200m);

            await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Grund");

            var nachher = await JahrAsync(ctx, vertrag);
            nachher.Korrekturen.Should().ContainSingle(k => k.Betrag == 200m);
            nachher.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Stimmt);
        }

        [Fact]
        public async Task SenkungUndErhoehungEinesJahresGleichenSichAus()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 0m, monate: 2, (1, 400m), (2, 600m));

            var jahr = await JahrAsync(ctx, vertrag);
            jahr.Monate.Should().HaveCount(2);
            jahr.Differenz.Should().Be(0m);
            jahr.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Stimmt);
        }

        [Fact]
        public async Task ErhoehungWirdAusNkVorauszahlungenDesJahresGedeckt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            // Januar ohne NK-VZ, Februar mit doppelter NK-VZ; nur der Januar wird erhöht.
            await ZahleMonatAsync(ctx, vertrag, new DateOnly(2024, 1, 1), kaltmiete: 500m, nk: 0m);
            await ZahleMonatAsync(ctx, vertrag, new DateOnly(2024, 2, 1), kaltmiete: 500m, nk: 300m, zahlungsTag: 6);
            vertrag.Versionen.Add(new VertragVersion(new DateOnly(2024, 1, 1), 600m, 1) { Vertrag = vertrag });
            vertrag.Versionen.Add(new VertragVersion(new DateOnly(2024, 2, 1), 500m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();

            (await JahrAsync(ctx, vertrag)).Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Anwendbar);
            await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Grund");
            (await JahrAsync(ctx, vertrag)).Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Stimmt);
        }

        [Fact]
        public async Task NkReichtNichtAusWirdNichtGebucht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 50m, monate: 1, (1, 600m));

            (await JahrAsync(ctx, vertrag)).Status.Should().Be(MietKorrekturService.MietKorrekturStatus.NkReichtNichtAus);
            var saetze = await ctx.Buchungssaetze.CountAsync();

            var buchen = () => Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Grund");
            await buchen.Should().ThrowAsync<InvalidOperationException>().WithMessage("*NK-Vorauszahlungen*");
            (await ctx.Buchungssaetze.CountAsync()).Should().Be(saetze);
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

            (await Service(ctx).ErmittleAsync(vertrag.VertragId)).Should().BeEmpty();
        }

        [Fact]
        public async Task AbgerechnetesJahrWirdNichtKorrigiert()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 600m));
            Abrechnen(ctx, vertrag, 2024);

            (await JahrAsync(ctx, vertrag)).Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Abgerechnet);
            var buchen = () => Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Grund");
            await buchen.Should().ThrowAsync<InvalidOperationException>().WithMessage("*2024*abgerechnet*");
        }

        [Fact]
        public async Task OhneDifferenzWirdNichtsGebucht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 600m));
            await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Erste Korrektur");
            var saetze = await ctx.Buchungssaetze.CountAsync();

            var nochmal = () => Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Zweiter Versuch");
            await nochmal.Should().ThrowAsync<InvalidOperationException>();
            (await ctx.Buchungssaetze.CountAsync()).Should().Be(saetze, "es darf nicht doppelt gebucht werden");
        }

        [Fact]
        public async Task ErneuteVersionsaenderungBuchtNurDieNeueDifferenz()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 600m));
            await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Erhöhung");

            vertrag.Versionen.Single(v => v.Grundmiete == 600m).Grundmiete = 650m;
            await ctx.SaveChangesAsync();

            var jahr = await JahrAsync(ctx, vertrag);
            jahr.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Anwendbar);
            jahr.Differenz.Should().Be(50m);

            await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Erhöhung angepasst");

            var nachher = await JahrAsync(ctx, vertrag);
            nachher.Korrekturen.Select(k => k.Betrag).Should().BeEquivalentTo([100m, 50m]);
            nachher.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Stimmt);
            (await SaldoAsync(ctx, vertrag.Wohnung.MietErtragskonto)).Should().Be(650m);
        }

        [Fact]
        public async Task ZurueckgesetzteVersionZeigtDieGegenrichtung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 600m));
            await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Erhöhung");

            ctx.Remove(vertrag.Versionen.Single(v => v.Grundmiete == 600m));
            await ctx.SaveChangesAsync();

            var jahr = await JahrAsync(ctx, vertrag);
            jahr.Monate.Should().BeEmpty();
            jahr.Differenz.Should().Be(-100m);
            jahr.Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Anwendbar);
        }

        [Theory]
        [InlineData(600)] // Erhöhung
        [InlineData(400)] // Senkung
        public async Task KorrekturLaesstSichWieJederFreieSatzLoeschen(int neueGrundmiete)
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, neueGrundmiete));
            var saetzeVorher = await ctx.Buchungssaetze.CountAsync();
            var ausgleicheVorher = await ctx.OffenePostenAusgleiche.CountAsync();
            var nkVorher = await SaldoAsync(ctx, vertrag.NkBuchungskonto);

            var satz = await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Grund");

            var schutzService = new BuchungssatzSchutzService(ctx);
            var schutz = await schutzService.PruefeAsync(satz);
            schutz.KannLoeschen.Should().BeTrue("der Ausgleich innerhalb des Satzes ist keine Abhängigkeit");
            schutzService.Entfernen(satz);
            await ctx.SaveChangesAsync();

            (await ctx.Buchungssaetze.CountAsync()).Should().Be(saetzeVorher);
            (await ctx.OffenePostenAusgleiche.CountAsync()).Should().Be(ausgleicheVorher);
            (await SaldoAsync(ctx, vertrag.NkBuchungskonto)).Should().Be(nkVorher);
            (await JahrAsync(ctx, vertrag)).Status.Should().Be(MietKorrekturService.MietKorrekturStatus.Anwendbar);
        }

        [Fact]
        public async Task StornierteKorrekturZaehltNichtMehr()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 600m));
            var satz = await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Grund");

            await new StornoBuchungsService(ctx).StornierenAsync(satz.BuchungssatzId, "Falsch");

            var jahr = await JahrAsync(ctx, vertrag);
            jahr.Korrekturen.Should().BeEmpty();
            jahr.Differenz.Should().Be(100m);
            jahr.NkVorauszahlung.Should().Be(150m);
        }

        [Fact]
        public async Task KorrekturImAbgerechnetenJahrIstGesperrt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 600m));
            await Service(ctx).BucheDifferenzAsync(vertrag.VertragId, 2024, "Grund");
            Abrechnen(ctx, vertrag, 2024);

            var korrektur = (await JahrAsync(ctx, vertrag)).Korrekturen.Should().ContainSingle().Subject;
            korrektur.KannLoeschen.Should().BeFalse();
            korrektur.KannStornieren.Should().BeFalse();
            korrektur.Sperrgrund.Should().Contain("Abrechnung");
        }

        [Fact]
        public async Task KorrekturOhneErtragszeileAendertDieKaltmieteNicht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx, nk: 150m, monate: 1, (1, 600m));

            // Umbuchung zwischen Miet- und NK-Konto ohne Ertragszeile
            var satz = new Buchungssatz(new DateOnly(2024, 12, 31), "Umbuchung") { Buchungsjahr = 2024 };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 50m) { Buchungssatz = satz, Buchungskonto = vertrag.MietBuchungskonto });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 50m) { Buchungssatz = satz, Buchungskonto = vertrag.NkBuchungskonto });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var jahr = await JahrAsync(ctx, vertrag);
            jahr.Korrekturen.Should().ContainSingle(k => k.Betrag == 0m);
            jahr.Differenz.Should().Be(100m);
        }
    }
}
