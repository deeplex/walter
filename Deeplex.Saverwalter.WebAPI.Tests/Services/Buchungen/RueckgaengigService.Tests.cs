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
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Services.Buchungen.RueckgaengigService;

namespace Deeplex.Saverwalter.WebAPI.Tests.Buchungen
{
    public class RueckgaengigServiceTests
    {
        private const int Jahr = 2021;

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

            var rechnung = new Buchungssatz(new DateOnly(Jahr, 6, 1), "Grundsteuer") { Buchungsjahr = Jahr };
            rechnung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 1000m) { Buchungssatz = rechnung, Buchungskonto = umlage.NkVerrechnungsKonto });
            ctx.Buchungssaetze.Add(rechnung);
            ctx.SaveChanges();
            return (vertrag, umlage, rechnung, [vertrag.Wohnung.WohnungId]);
        }

        private static Buchungssatz NkZahlung(SaverwalterContext ctx, Vertrag vertrag, decimal betrag = 100m)
        {
            var satz = new Buchungssatz(new DateOnly(Jahr, 3, 5), "NK-Vorauszahlung") { Buchungsjahr = Jahr };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, betrag) { Buchungssatz = satz, Buchungskonto = vertrag.ZahlungsKonto });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag) { Buchungssatz = satz, Buchungskonto = vertrag.NkBuchungskonto });
            ctx.Buchungssaetze.Add(satz);
            ctx.SaveChanges();
            return satz;
        }

        private static async Task AbrechnenAsync(SaverwalterContext ctx, List<int> wohnungIds, bool versenden)
        {
            await TestServices.Abrechnungslauf(ctx).BookAsync(wohnungIds, Jahr);
            if (versenden)
            {
                foreach (var r in ctx.Abrechnungsresultate) r.Abgesendet = true;
                await ctx.SaveChangesAsync();
            }
        }

        private static decimal Saldo(SaverwalterContext ctx, Buchungskonto konto) =>
            ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == konto.BuchungskontoId)
                .Sum(z => z.SollHaben == SollHaben.Soll ? z.Betrag : -z.Betrag);

        private static bool Existiert(SaverwalterContext ctx, Buchungssatz satz) =>
            ctx.Buchungssaetze.Any(s => s.BuchungssatzId == satz.BuchungssatzId);

        [Fact]
        public async Task FreierSatzWirdGeloescht()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, _, _) = Seed(ctx);
            var zahlung = NkZahlung(ctx, vertrag);
            var service = TestServices.Rueckgaengig(ctx);

            var plan = await service.PruefeAsync([zahlung]);
            plan.Art.Should().Be(RueckgaengigArt.Loeschen);
            plan.Abrechnungen.Should().BeEmpty();

            await service.RueckgaengigAsync([zahlung], grund: null, abrechnungenBestaetigt: false);
            Existiert(ctx, zahlung).Should().BeFalse();
        }

        [Fact]
        public async Task AusgeglichenerSatzWirdMitGrundStorniert()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, _, _) = Seed(ctx);
            var forderung = new Buchungssatz(new DateOnly(Jahr, 3, 1), "Mietsoll") { Buchungsjahr = Jahr };
            var soll = new Buchungszeile(SollHaben.Soll, 500m) { Buchungssatz = forderung, Buchungskonto = vertrag.MietBuchungskonto };
            forderung.Buchungszeilen.Add(soll);
            forderung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 500m) { Buchungssatz = forderung, Buchungskonto = vertrag.Wohnung.MietErtragskonto });
            var zahlung = new Buchungssatz(new DateOnly(Jahr, 3, 5), "Miete") { Buchungsjahr = Jahr };
            var haben = new Buchungszeile(SollHaben.Haben, 500m) { Buchungssatz = zahlung, Buchungskonto = vertrag.MietBuchungskonto };
            zahlung.Buchungszeilen.Add(haben);
            zahlung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 500m) { Buchungssatz = zahlung, Buchungskonto = vertrag.ZahlungsKonto });
            ctx.Buchungssaetze.AddRange(forderung, zahlung);
            ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich { SollZeile = soll, HabenZeile = haben });
            ctx.SaveChanges();
            var service = TestServices.Rueckgaengig(ctx);

            (await service.PruefeAsync([zahlung])).Art.Should().Be(RueckgaengigArt.Storno);
            var ohneGrund = () => service.RueckgaengigAsync([zahlung], null, false);
            await ohneGrund.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Grund*");

            await service.RueckgaengigAsync([zahlung], "Falsch zugeordnet", false);
            zahlung.StornoNach.Should().NotBeNull();
            zahlung.StornoNach!.Notiz.Should().Be("Falsch zugeordnet");
        }

        [Fact]
        public async Task StornoPaarIstGesperrt()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, _, _) = Seed(ctx);
            var zahlung = NkZahlung(ctx, vertrag);
            var storno = await new StornoBuchungsService(ctx).StornierenAsync(zahlung.BuchungssatzId, "Test");
            var service = TestServices.Rueckgaengig(ctx);

            (await service.PruefeAsync([zahlung])).Art.Should().Be(RueckgaengigArt.Gesperrt);
            var plan = await service.PruefeAsync([storno]);
            plan.Art.Should().Be(RueckgaengigArt.Gesperrt);
            plan.Sperrgrund.Should().Contain("Stornobuchung");
        }

        [Fact]
        public async Task NichtVersendeteAbrechnungWirdNachBestaetigungZurueckgenommen()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, _, wohnungIds) = Seed(ctx);
            var zahlung = NkZahlung(ctx, vertrag);
            await AbrechnenAsync(ctx, wohnungIds, versenden: false);
            var service = TestServices.Rueckgaengig(ctx);

            var plan = await service.PruefeAsync([zahlung]);
            plan.Art.Should().Be(RueckgaengigArt.Loeschen);
            plan.Abrechnungen.Should().ContainSingle(a => a.Jahr == Jahr && !a.Versendet);

            var unbestaetigt = () => service.RueckgaengigAsync([zahlung], null, abrechnungenBestaetigt: false);
            await unbestaetigt.Should().ThrowAsync<InvalidOperationException>().WithMessage("*bestätigen*");
            ctx.Abrechnungsresultate.Should().ContainSingle();
            Existiert(ctx, zahlung).Should().BeTrue();

            await service.RueckgaengigAsync([zahlung], null, abrechnungenBestaetigt: true);
            ctx.Abrechnungsresultate.Should().BeEmpty();
            Existiert(ctx, zahlung).Should().BeFalse();
        }

        [Fact]
        public async Task VersendeteAbrechnungWirdMitStorniertUndDasJahrIstWiederOffen()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, _, wohnungIds) = Seed(ctx);
            var zahlung = NkZahlung(ctx, vertrag);
            await AbrechnenAsync(ctx, wohnungIds, versenden: true);
            var service = TestServices.Rueckgaengig(ctx);

            var plan = await service.PruefeAsync([zahlung]);
            plan.Art.Should().Be(RueckgaengigArt.Storno, "die Zahlung steckt in einer versendeten Abrechnung");
            plan.Abrechnungen.Should().ContainSingle(a => a.Versendet);

            await service.RueckgaengigAsync([zahlung], "Zahlung gehörte zu einem anderen Mieter", true);

            zahlung.StornoNach.Should().NotBeNull();
            ctx.Abrechnungsresultate.Single().Buchungssatz.StornoNach.Should().NotBeNull();
            var preview = await TestServices.Abrechnungslauf(ctx).PreviewAsync(wohnungIds, Jahr);
            preview.Resultate.Single().GebuchterSaldo.Should().BeNull();
        }

        [Fact]
        public async Task RechnungInVersendeterAbrechnungLaesstDieKontenStimmig()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, umlage, rechnung, wohnungIds) = Seed(ctx);
            var nkVorher = Saldo(ctx, vertrag.NkBuchungskonto);
            await AbrechnenAsync(ctx, wohnungIds, versenden: true);
            var service = TestServices.Rueckgaengig(ctx);

            await service.RueckgaengigAsync([rechnung], "Rechnung doppelt erfasst", true);

            rechnung.StornoNach.Should().NotBeNull();
            Saldo(ctx, vertrag.NkBuchungskonto).Should().Be(nkVorher);
            Saldo(ctx, umlage.NkVerrechnungsKonto).Should().Be(0m);
        }

        [Fact]
        public async Task AbrechnungssatzNimmtSeineGruppeZurueck()
        {
            var ctx = TestUtils.GetContext();
            var (_, _, rechnung, wohnungIds) = Seed(ctx);
            await AbrechnenAsync(ctx, wohnungIds, versenden: false);
            var abrechnungssatz = ctx.Abrechnungsresultate.Single().Buchungssatz;
            var service = TestServices.Rueckgaengig(ctx);

            var plan = await service.PruefeAsync([abrechnungssatz]);
            plan.Art.Should().Be(RueckgaengigArt.Loeschen);
            plan.Abrechnungen.Should().ContainSingle();

            await service.RueckgaengigAsync([abrechnungssatz], null, true);
            ctx.Abrechnungsresultate.Should().BeEmpty();
            Existiert(ctx, abrechnungssatz).Should().BeFalse();
            Existiert(ctx, rechnung).Should().BeTrue("die Rechnung selbst bleibt");
        }
    }
}
