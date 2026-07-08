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
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AbrechnungslaufRueckabwicklungTests
    {
        private const int Jahr = 2026;

        private static AbrechnungslaufService Service(SaverwalterContext ctx) =>
            new(ctx,
                new NkAnteilBuchungsService(ctx),
                new AbrechnungsresultatBuchungsService(ctx),
                null!,
                new StornoBuchungsService(ctx));

        /// <summary>
        /// Gebuchte Abrechnung im Kleinen: eine Umlage mit Rechnungssatz samt
        /// Verteil-Zeile (wie vom Lauf angefügt), ein Abrechnungsresultat mit
        /// Satz und ein manueller NK-Anteil-Satz, der erhalten bleiben muss.
        /// </summary>
        private static (Vertrag vertrag, Umlage umlage, Buchungssatz rechnung, Abrechnungsresultat resultat, Buchungssatz manuellerAnteil)
            Seed(SaverwalterContext ctx)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 0);
            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Grundsteuer"),
                Wohnungen = [vertrag.Wohnung],
                NkVerrechnungsKonto = new Buchungskonto("7001", "NK-VK Grundsteuer", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1201", "Zahlung Grundsteuer", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(umlage);

            // BK-Rechnung mit vom Lauf angefügter Verteil-Zeile
            var rechnung = new Buchungssatz(new DateOnly(Jahr, 6, 1), $"BK-Eingang Grundsteuer {Jahr}")
            {
                Buchungsjahr = Jahr
            };
            rechnung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 1000m)
            {
                Buchungssatz = rechnung,
                Buchungskonto = umlage.NkVerrechnungsKonto
            });
            rechnung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 1000m)
            {
                Buchungssatz = rechnung,
                Buchungskonto = vertrag.NkBuchungskonto
            });
            ctx.Buchungssaetze.Add(rechnung);

            // Abrechnungsresultat mit Buchungssatz
            var abrechnungssatz = new Buchungssatz(new DateOnly(Jahr, 12, 31), $"BK-Abrechnung {Jahr}");
            abrechnungssatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 800m)
            {
                Buchungssatz = abrechnungssatz,
                Buchungskonto = vertrag.NkBuchungskonto
            });
            abrechnungssatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 1000m)
            {
                Buchungssatz = abrechnungssatz,
                Buchungskonto = vertrag.BkAbrechnungsKonto
            });
            ctx.Buchungssaetze.Add(abrechnungssatz);
            var resultat = new Abrechnungsresultat
            {
                Vertrag = vertrag,
                Buchungssatz = abrechnungssatz
            };
            ctx.Abrechnungsresultate.Add(resultat);

            // Manueller NK-Anteil — eigener Satz mit Marker-Präfix, bleibt erhalten
            var manuell = new Buchungssatz(new DateOnly(Jahr, 7, 1),
                $"{NkAnteilBuchungsService.BeschreibungPrefix}Grundsteuer {Jahr}")
            {
                Buchungsjahr = Jahr
            };
            manuell.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 50m)
            {
                Buchungssatz = manuell,
                Buchungskonto = vertrag.NkBuchungskonto
            });
            manuell.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 50m)
            {
                Buchungssatz = manuell,
                Buchungskonto = umlage.NkVerrechnungsKonto
            });
            ctx.Buchungssaetze.Add(manuell);

            ctx.SaveChanges();
            return (vertrag, umlage, rechnung, resultat, manuell);
        }

        [Fact]
        public async Task Delete_entfernt_resultat_und_verteilzeilen_aber_nicht_rechnung_und_manuelle_anteile()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, rechnung, resultat, manuell) = Seed(ctx);

            var result = await Service(ctx).DeleteAsync([vertrag.Wohnung.WohnungId], Jahr);

            result.Resultate.Should().Be(1);
            result.BereinigteVerteilungen.Should().Be(1);

            // Resultat + Abrechnungssatz sind weg
            (await ctx.Abrechnungsresultate.CountAsync()).Should().Be(0);
            (await ctx.Buchungssaetze.AnyAsync(s => s.BuchungssatzId == resultat.Buchungssatz.BuchungssatzId))
                .Should().BeFalse();

            // Rechnung bleibt, aber ohne die Verteil-Zeile
            var verbleibend = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen)
                .SingleAsync(s => s.BuchungssatzId == rechnung.BuchungssatzId);
            verbleibend.Buchungszeilen.Should().ContainSingle(z => z.SollHaben == SollHaben.Haben);

            // Manueller NK-Anteil unangetastet
            var manuellDb = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen)
                .SingleAsync(s => s.BuchungssatzId == manuell.BuchungssatzId);
            manuellDb.Buchungszeilen.Should().HaveCount(2);
        }

        [Fact]
        public async Task Delete_entfernt_auch_Eigenanteil_auf_AufwandsKonto()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, rechnung, _, _) = Seed(ctx);

            // Zusätzliche Eigenanteil-Verteilzeile (Leerstand) auf dem AufwandsKonto der Wohnung.
            rechnung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 200m)
            {
                Buchungssatz = rechnung,
                Buchungskonto = vertrag.Wohnung.AufwandsKonto
            });
            ctx.SaveChanges();

            await Service(ctx).DeleteAsync([vertrag.Wohnung.WohnungId], Jahr);

            // Rechnung bleibt, aber BEIDE Verteil-Zeilen (Vertrag-NK + Eigenanteil) sind weg.
            var verbleibend = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .SingleAsync(s => s.BuchungssatzId == rechnung.BuchungssatzId);
            verbleibend.Buchungszeilen.Should().ContainSingle()
                .Which.SollHaben.Should().Be(SollHaben.Haben);
            verbleibend.Buchungszeilen.Should().NotContain(z =>
                z.Buchungskonto.BuchungskontoId == vertrag.Wohnung.AufwandsKonto.BuchungskontoId);
        }

        [Fact]
        public async Task Delete_ist_bei_abgesendetem_resultat_gesperrt()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, _, resultat, _) = Seed(ctx);
            resultat.Abgesendet = true;
            ctx.SaveChanges();

            var act = () => Service(ctx).DeleteAsync([vertrag.Wohnung.WohnungId], Jahr);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Storno_neutralisiert_abrechnung_und_verteilung()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, _, rechnung, resultat, _) = Seed(ctx);
            resultat.Abgesendet = true;
            ctx.SaveChanges();

            var result = await Service(ctx).StornoAsync(
                [vertrag.Wohnung.WohnungId], Jahr, "Abrechnung fehlerhaft");

            result.Resultate.Should().Be(1);
            result.BereinigteVerteilungen.Should().Be(1);

            // Abrechnungssatz storniert, Resultat bleibt als Beleg
            var abrechnungssatz = await ctx.Buchungssaetze
                .Include(s => s.StornoNach)
                .SingleAsync(s => s.BuchungssatzId == resultat.Buchungssatz.BuchungssatzId);
            abrechnungssatz.StornoNach.Should().NotBeNull();
            abrechnungssatz.StornoNach!.Notiz.Should().Be("Abrechnung fehlerhaft");
            (await ctx.Abrechnungsresultate.CountAsync()).Should().Be(1);

            // Verteil-Zeile der Rechnung durch Gegenbuchung neutralisiert
            var korrektur = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen)
                .SingleAsync(s => s.Beschreibung == $"Storno NK-Verteilung: {rechnung.Beschreibung}");
            korrektur.Buchungszeilen.Should().ContainSingle(z =>
                z.SollHaben == SollHaben.Haben && z.Betrag == 1000m);

            // Idempotent: zweiter Lauf erzeugt keine weitere Gegenbuchung
            var zweiter = await Service(ctx).StornoAsync(
                [vertrag.Wohnung.WohnungId], Jahr, "Abrechnung fehlerhaft");
            zweiter.BereinigteVerteilungen.Should().Be(0);
            zweiter.Resultate.Should().Be(0);
        }
    }
}
