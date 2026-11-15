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
    /// Ablauf nach Widerspruch: versendete Abrechnung stornieren, Daten korrigieren,
    /// das Jahr neu abrechnen. Die stornierte Abrechnung bleibt als Beleg stehen und
    /// zählt nicht mehr als gebucht.
    /// </summary>
    public class AbrechnungslaufNeuAbrechnenTests
    {
        private const int Jahr = 2021;

        private static AbrechnungslaufService Service(SaverwalterContext ctx) =>
            new(ctx,
                new NkAnteilBuchungsService(ctx),
                new AbrechnungsresultatBuchungsService(ctx),
                new AbrechnungsgruppenService(ctx),
                new StornoBuchungsService(ctx),
                new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx)));

        private static Buchungssatz AddRechnung(SaverwalterContext ctx, Umlage umlage, decimal betrag)
        {
            var satz = new Buchungssatz(new DateOnly(Jahr, 6, 1), $"Betriebskosten {umlage.Typ.Bezeichnung} {Jahr}")
            {
                Buchungsjahr = Jahr
            };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = umlage.NkVerrechnungsKonto
            });
            ctx.Buchungssaetze.Add(satz);
            ctx.SaveChanges();
            return satz;
        }

        private static (Vertrag vertrag, Umlage umlage, Buchungssatz rechnung, List<int> wohnungIds) Seed(SaverwalterContext ctx)
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
            ctx.SaveChanges();
            var rechnung = AddRechnung(ctx, umlage, 1000m);
            return (vertrag, umlage, rechnung, [vertrag.Wohnung.WohnungId]);
        }

        private static decimal Saldo(SaverwalterContext ctx, Buchungskonto konto) =>
            ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == konto.BuchungskontoId)
                .Sum(z => z.SollHaben == SollHaben.Soll ? z.Betrag : -z.Betrag);

        private static async Task<Abrechnungsresultat> BuchenUndVersendenAsync(SaverwalterContext ctx, Vertrag vertrag, List<int> wohnungIds)
        {
            await Service(ctx).BookAsync(wohnungIds, Jahr);
            var resultat = ctx.Abrechnungsresultate.Single(r => r.Vertrag.VertragId == vertrag.VertragId && r.Buchungssatz.StornoNach == null);
            resultat.Abgesendet = true;
            await ctx.SaveChangesAsync();
            return resultat;
        }

        [Fact]
        public async Task NachStornoLaesstSichDasJahrNeuAbrechnen()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, _, wohnungIds) = Seed(ctx);
            var erstes = await BuchenUndVersendenAsync(ctx, vertrag, wohnungIds);
            var nkNachErstemLauf = Saldo(ctx, vertrag.NkBuchungskonto);
            var bkNachErstemLauf = Saldo(ctx, vertrag.BkAbrechnungsKonto);

            await Service(ctx).StornoAsync(wohnungIds, Jahr, "Widerspruch des Mieters");

            var preview = await Service(ctx).PreviewAsync(wohnungIds, Jahr);
            preview.Resultate.Single().GebuchterSaldo.Should().BeNull("die stornierte Abrechnung gilt nicht als gebucht");
            preview.Resultate.Single().Abgesendet.Should().BeNull();

            await Service(ctx).BookAsync(wohnungIds, Jahr);

            var resultate = ctx.Abrechnungsresultate.Where(r => r.Vertrag.VertragId == vertrag.VertragId).ToList();
            resultate.Should().HaveCount(2);
            erstes.Buchungssatz.StornoNach.Should().NotBeNull();
            var neues = resultate.Single(r => r.AbrechnungsresultatId != erstes.AbrechnungsresultatId);
            neues.Abgesendet.Should().BeFalse();
            neues.Buchungssatz.StornoNach.Should().BeNull();

            Saldo(ctx, vertrag.NkBuchungskonto).Should().Be(nkNachErstemLauf);
            Saldo(ctx, vertrag.BkAbrechnungsKonto).Should().Be(bkNachErstemLauf);
        }

        [Fact]
        public async Task KorrigierteDatenWirkenInDerNeuenAbrechnung()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, umlage, _, wohnungIds) = Seed(ctx);
            await BuchenUndVersendenAsync(ctx, vertrag, wohnungIds);
            var saldoAlt = (await Service(ctx).PreviewAsync(wohnungIds, Jahr)).Resultate.Single().Saldo;

            await Service(ctx).StornoAsync(wohnungIds, Jahr, "Rechnung fehlte");
            AddRechnung(ctx, umlage, 300m);
            await Service(ctx).BookAsync(wohnungIds, Jahr);

            var preview = await Service(ctx).PreviewAsync(wohnungIds, Jahr);
            var resultat = preview.Resultate.Single();
            resultat.Saldo.Should().BeGreaterThan(saldoAlt);
            resultat.GebuchterSaldo.Should().Be(resultat.Saldo);
            preview.Abrechnungseinheiten.SelectMany(e => e.NkZeilen)
                .Should().OnlyContain(z => z.IstVollstaendigGebucht);
        }

        [Fact]
        public async Task RueckabwicklungDerNeuenAbrechnungLaesstDenStornoStehen()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, _, wohnungIds) = Seed(ctx);
            await BuchenUndVersendenAsync(ctx, vertrag, wohnungIds);
            await Service(ctx).StornoAsync(wohnungIds, Jahr, "Widerspruch");
            var saetzeNachStorno = ctx.Buchungssaetze.Count();
            var nkNachStorno = Saldo(ctx, vertrag.NkBuchungskonto);

            await Service(ctx).BookAsync(wohnungIds, Jahr);
            await Service(ctx).DeleteAsync(wohnungIds, Jahr);

            ctx.Abrechnungsresultate.Should().ContainSingle(r => r.Abgesendet && r.Buchungssatz.StornoNach != null);
            ctx.Buchungssaetze.Count().Should().Be(saetzeNachStorno);
            Saldo(ctx, vertrag.NkBuchungskonto).Should().Be(nkNachStorno);
        }

        [Fact]
        public async Task StornoDerRechnungNachAbrechnungsStornoKehrtDieVerteilungNichtDoppeltUm()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, umlage, rechnung, wohnungIds) = Seed(ctx);
            var nkVorher = Saldo(ctx, vertrag.NkBuchungskonto);
            var schutz = new BuchungssatzSchutzService(ctx);

            await BuchenUndVersendenAsync(ctx, vertrag, wohnungIds);
            (await schutz.PruefeAsync(rechnung)).KannStornieren.Should().BeFalse("die Rechnung ist abgerechnet");

            await Service(ctx).StornoAsync(wohnungIds, Jahr, "Rechnung falsch");
            (await schutz.PruefeAsync(rechnung)).KannStornieren.Should().BeTrue("die Abrechnung ist storniert");

            await new StornoBuchungsService(ctx).StornierenAsync(rechnung.BuchungssatzId, "Rechnung falsch");

            Saldo(ctx, vertrag.NkBuchungskonto).Should().Be(nkVorher);
            Saldo(ctx, umlage.NkVerrechnungsKonto).Should().Be(0m);
        }
    }
}
