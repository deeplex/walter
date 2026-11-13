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
using static Deeplex.Saverwalter.WebAPI.Services.Buchungen.MietkontoService;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MietkontoServiceTests
    {
        private static readonly DateOnly Januar = new(2024, 1, 1);
        private static readonly DateOnly Februar = new(2024, 2, 1);
        private static readonly DateOnly Maerz = new(2024, 3, 1);
        /// <summary>Stichtag: Januar bis März 2024 sind fällig.</summary>
        private static readonly DateOnly Heute = new(2024, 3, 20);

        /// <summary>Vertrag ab 01/2024 mit 500 Kaltmiete und 150 NK-Vorauszahlung.</summary>
        private static async Task<Vertrag> VertragAsync(SaverwalterContext ctx)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var version = vertrag.Versionen.Single();
            version.Beginn = Januar;
            version.Nebenkostenvorauszahlung = 150m;
            await ctx.SaveChangesAsync();
            return vertrag;
        }

        private static async Task<GarageVertrag> GarageAsync(SaverwalterContext ctx, Vertrag vertrag, decimal miete, bool gebunden = true)
        {
            var garage = new Garage("G1")
            {
                Besitzer = vertrag.Ansprechpartner!,
                Ertragskonto = new Buchungskonto("4100", "Garagenmiete", BuchungskontoTyp.Ertrag),
            };
            var gv = new GarageVertrag
            {
                Garage = garage,
                Vertrag = gebunden ? vertrag : null,
                MietBuchungskonto = new Buchungskonto("1100", "Garage", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto("1101", "Garage Zahlung", BuchungskontoTyp.Aktiv),
            };
            gv.Versionen.Add(new GarageVertragVersion(Januar, miete) { GarageVertrag = gv });
            ctx.GarageVertraege.Add(gv);
            await ctx.SaveChangesAsync();
            return gv;
        }

        private static Task<Transaktion> ZahleAsync(
            SaverwalterContext ctx, Vertrag vertrag, DateOnly monat, DateOnly datum,
            decimal kaltmiete, decimal nk = 0m, GarageVertrag? garage = null, decimal garagenmiete = 0m)
        {
            var miete = new TransaktionBuchungsService.MietzahlungsInput
            {
                VertragId = vertrag.VertragId,
                BetreffenderMonat = monat,
                Kaltmiete = kaltmiete,
                NkVorauszahlung = nk,
            };
            if (garage != null)
                miete.Garagen.Add(new TransaktionBuchungsService.GaragenmietInput
                {
                    GarageVertragId = garage.GarageVertragId,
                    Betrag = garagenmiete,
                });
            return new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = kaltmiete + nk + garagenmiete,
                Zahlungsdatum = datum,
                Mieten = { miete },
            });
        }

        private static Task<Mietkonto> MietkontoAsync(SaverwalterContext ctx, Vertrag vertrag) =>
            new MietkontoService(ctx).ErmittleAsync(vertrag.VertragId, Heute);

        private static MietkontoMonat Monat(Mietkonto konto, DateOnly monat) =>
            konto.Monate.Single(m => m.Jahr == monat.Year && m.Monat == monat.Month);

        [Fact]
        public async Task OffeneMonateOhneSollstellungSindMitDerVertragsmieteFaellig()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);

            var konto = await MietkontoAsync(ctx, vertrag);

            konto.Monate.Select(m => m.Monat).Should().Equal(3, 2, 1);
            konto.Monate.Should().AllSatisfy(m =>
            {
                m.Faellig.Should().Be(500m);
                m.Bezahlt.Should().Be(0m);
            });
            ctx.Buchungssaetze.Count().Should().Be(0, "das Mietkonto legt keine Sollstellungen an");
        }

        [Fact]
        public async Task ZukuenftigeMonateSindNichtFaellig()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);

            var konto = await MietkontoAsync(ctx, vertrag);

            konto.Monate.Should().NotContain(m => m.Jahr == 2024 && m.Monat > 3);
        }

        [Fact]
        public async Task VerspaeteteZahlungZaehltFuerDenZugeordnetenMonat()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);

            await ZahleAsync(ctx, vertrag, Januar, new DateOnly(2024, 2, 2), 500m);

            var konto = await MietkontoAsync(ctx, vertrag);
            var januar = Monat(konto, Januar);
            januar.Bezahlt.Should().Be(500m);
            januar.Zahlungen.Should().ContainSingle()
                .Which.Datum.Should().Be(new DateOnly(2024, 2, 2));
            Monat(konto, Februar).Bezahlt.Should().Be(0m);
        }

        [Fact]
        public async Task TeilzahlungBleibtTeilweiseOffen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);

            await ZahleAsync(ctx, vertrag, Februar, new DateOnly(2024, 2, 4), 350m);

            var februar = Monat(await MietkontoAsync(ctx, vertrag), Februar);
            februar.Faellig.Should().Be(500m);
            februar.Bezahlt.Should().Be(350m);
            februar.Offen.Should().Be(150m);
        }

        [Fact]
        public async Task SollstellungGehtDerVertragsmieteVor()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            await ZahleAsync(ctx, vertrag, Januar, new DateOnly(2024, 1, 5), 500m);
            vertrag.Versionen.Single().Grundmiete = 600m;
            await ctx.SaveChangesAsync();

            var konto = await MietkontoAsync(ctx, vertrag);

            Monat(konto, Januar).Faellig.Should().Be(500m, "die Sollstellung steht; abweichende Monate korrigiert die Miet-Korrektur");
            Monat(konto, Februar).Faellig.Should().Be(600m);
        }

        [Fact]
        public async Task NkVorauszahlungenJeJahr()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);

            await ZahleAsync(ctx, vertrag, Januar, new DateOnly(2024, 1, 5), 500m, 150m);
            await ZahleAsync(ctx, vertrag, Februar, new DateOnly(2024, 2, 5), 500m, 150m);

            var nk = (await MietkontoAsync(ctx, vertrag)).NkVorauszahlungen.Should().ContainSingle().Subject;
            nk.Jahr.Should().Be(2024);
            nk.Faellig.Should().Be(450m, "Januar bis März");
            nk.Bezahlt.Should().Be(300m);
            nk.Zahlungen.Should().HaveCount(2)
                .And.AllSatisfy(z => z.Art.Should().Be(MietkontoZahlungsArt.NkVorauszahlung));
        }

        [Fact]
        public async Task NkVorauszahlungZaehltFuerDasJahrDesBetreffendenMonats()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var dezember = new DateOnly(2023, 12, 1);
            vertrag.Versionen.Single().Beginn = dezember;
            await ctx.SaveChangesAsync();

            await ZahleAsync(ctx, vertrag, dezember, new DateOnly(2024, 1, 3), 500m, 150m);

            var konto = await MietkontoAsync(ctx, vertrag);
            konto.NkVorauszahlungen.Single(n => n.Jahr == 2023).Bezahlt.Should().Be(150m);
            konto.NkVorauszahlungen.Single(n => n.Jahr == 2024).Bezahlt.Should().Be(0m);
            Monat(konto, dezember).Bezahlt.Should().Be(500m);
        }

        [Fact]
        public async Task MietKorrekturErscheintBeiDenNkVorauszahlungen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            await ZahleAsync(ctx, vertrag, Januar, new DateOnly(2024, 1, 5), 500m, 150m);
            vertrag.Versionen.Single().Grundmiete = 600m;
            await ctx.SaveChangesAsync();
            await new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx))
                .BucheDifferenzAsync(vertrag.VertragId, 2024, "Mieterhöhung nachgetragen");

            var konto = await MietkontoAsync(ctx, vertrag);

            var nk = konto.NkVorauszahlungen.Single();
            nk.Bezahlt.Should().Be(50m);
            nk.Zahlungen.Should().ContainSingle(z => z.Art == MietkontoZahlungsArt.MietKorrektur)
                .Which.Betrag.Should().Be(-100m);
            Monat(konto, Januar).Bezahlt.Should().Be(500m, "die Korrektur ist keine Zahlung auf einen Monat");
        }

        [Fact]
        public async Task GarageIstImMonatEnthalten()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var garage = await GarageAsync(ctx, vertrag, 50m);

            await ZahleAsync(ctx, vertrag, Januar, new DateOnly(2024, 1, 5), 500m, garage: garage, garagenmiete: 50m);

            var konto = await MietkontoAsync(ctx, vertrag);
            var januar = Monat(konto, Januar);
            januar.Faellig.Should().Be(550m);
            januar.Bezahlt.Should().Be(550m);
            januar.Garagen.Should().ContainSingle().Which.Kennung.Should().Be("G1");
            januar.Zahlungen.Should().Contain(z => z.Art == MietkontoZahlungsArt.Garage && z.Garage == "G1");
            Monat(konto, Februar).Faellig.Should().Be(550m, "auch ohne Sollstellung zählt die Garagenmiete");
        }

        [Fact]
        public async Task StornierteZahlungIstRueckgaengigGemachtUndDerMonatWiederOffen()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var transaktion = await ZahleAsync(ctx, vertrag, Januar, new DateOnly(2024, 1, 5), 500m, 150m);
            foreach (var satz in await ctx.Buchungssaetze
                .Where(s => s.Transaktion!.TransaktionId == transaktion.TransaktionId).ToListAsync())
            {
                await new StornoBuchungsService(ctx).StornierenAsync(satz.BuchungssatzId, "falscher Mieter");
            }

            var konto = await MietkontoAsync(ctx, vertrag);

            Monat(konto, Januar).Bezahlt.Should().Be(0m);
            konto.NkVorauszahlungen.Single().Bezahlt.Should().Be(0m);
            konto.RueckgaengigGemacht.Select(z => (z.Art, z.Betrag)).Should().BeEquivalentTo(
                [(MietkontoZahlungsArt.Kaltmiete, 500m), (MietkontoZahlungsArt.NkVorauszahlung, 150m)]);
            konto.RueckgaengigGemacht.Should().AllSatisfy(z => z.TransaktionId.Should().Be(transaktion.TransaktionId));
        }

        [Fact]
        public async Task ZahlungOhneAusgleichIstGuthaben()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var satz = new Buchungssatz(new DateOnly(2024, 1, 5), "Überzahlung")
            {
                Transaktion = new Transaktion { Zahlungsdatum = new DateOnly(2024, 1, 5), Betrag = 80m },
            };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 80m) { Buchungssatz = satz, Buchungskonto = vertrag.ZahlungsKonto });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 80m) { Buchungssatz = satz, Buchungskonto = vertrag.MietBuchungskonto });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var konto = await MietkontoAsync(ctx, vertrag);

            konto.NichtZugeordnet.Should().ContainSingle().Which.Betrag.Should().Be(80m);
            konto.Offen.Should().Be(3 * 500m + 3 * 150m - 80m);
        }

        [Fact]
        public async Task VertragsendeBegrenztDieFaelligenMonate()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            vertrag.Ende = new DateOnly(2024, 2, 15);
            await ctx.SaveChangesAsync();

            var konto = await MietkontoAsync(ctx, vertrag);

            konto.Monate.Select(m => m.Monat).Should().Equal(2, 1);
            konto.NkVorauszahlungen.Single().Faellig.Should().Be(300m);
        }

        private static Task<Transaktion> ZahleGarageAsync(
            SaverwalterContext ctx, GarageVertrag gv, DateOnly monat, DateOnly datum, decimal betrag) =>
            new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = betrag,
                Zahlungsdatum = datum,
                GaragenEingaenge =
                {
                    new TransaktionBuchungsService.StandaloneGaragenmietInput
                    {
                        GarageVertragId = gv.GarageVertragId,
                        BetreffenderMonat = monat,
                        Betrag = betrag,
                    }
                },
            });

        private static Task<Mietkonto> GaragenkontoAsync(SaverwalterContext ctx, GarageVertrag gv) =>
            new MietkontoService(ctx).ErmittleGarageAsync(gv.GarageVertragId, Heute);

        [Fact]
        public async Task GaragenmietkontoZeigtDieGaragenmieteJeMonat()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var gv = await GarageAsync(ctx, vertrag, 50m, gebunden: false);
            // Januarmiete, gezahlt im Februar.
            await ZahleGarageAsync(ctx, gv, Januar, new DateOnly(2024, 2, 4), 50m);
            await ZahleGarageAsync(ctx, gv, Februar, new DateOnly(2024, 2, 5), 20m);

            var konto = await GaragenkontoAsync(ctx, gv);

            konto.Monate.Select(m => m.Monat).Should().Equal(3, 2, 1);
            konto.Monate.Should().AllSatisfy(m =>
            {
                m.Kaltmiete.Should().Be(0m);
                m.Garagen.Should().ContainSingle().Which.Kennung.Should().Be("G1");
            });
            Monat(konto, Januar).Offen.Should().Be(0m);
            Monat(konto, Januar).Zahlungen.Should().ContainSingle()
                .Which.Should().Match<MietkontoZahlung>(z => z.Art == MietkontoZahlungsArt.Garage && z.Garage == "G1");
            Monat(konto, Februar).Offen.Should().Be(30m);
            Monat(konto, Maerz).Offen.Should().Be(50m, "auch ohne Sollstellung ist die Garagenmiete fällig");
            konto.NkVorauszahlungen.Should().BeEmpty();
            konto.Offen.Should().Be(80m);
        }

        [Fact]
        public async Task GaragenmietkontoEinerGebundenenGarageZeigtNurDieGarage()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var gv = await GarageAsync(ctx, vertrag, 50m);
            await ZahleAsync(ctx, vertrag, Januar, new DateOnly(2024, 1, 5), 500m, 150m, gv, 50m);

            var konto = await GaragenkontoAsync(ctx, gv);

            Monat(konto, Januar).Bezahlt.Should().Be(50m);
            Monat(konto, Januar).Zahlungen.Should().ContainSingle().Which.Betrag.Should().Be(50m);
            konto.Offen.Should().Be(100m);
        }

        [Fact]
        public async Task GaragenmietkontoMitGuthabenUndStorno()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAsync(ctx);
            var gv = await GarageAsync(ctx, vertrag, 50m, gebunden: false);
            var transaktion = await ZahleGarageAsync(ctx, gv, Januar, new DateOnly(2024, 1, 5), 50m);
            foreach (var satz in await ctx.Buchungssaetze
                .Where(s => s.Transaktion!.TransaktionId == transaktion.TransaktionId).ToListAsync())
            {
                await new StornoBuchungsService(ctx).StornierenAsync(satz.BuchungssatzId, "falsche Garage");
            }
            var ueber = new Buchungssatz(new DateOnly(2024, 2, 5), "Überzahlung")
            {
                Transaktion = new Transaktion { Zahlungsdatum = new DateOnly(2024, 2, 5), Betrag = 10m },
            };
            ueber.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 10m) { Buchungssatz = ueber, Buchungskonto = gv.ZahlungsKonto });
            ueber.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 10m) { Buchungssatz = ueber, Buchungskonto = gv.MietBuchungskonto });
            ctx.Buchungssaetze.Add(ueber);
            await ctx.SaveChangesAsync();

            var konto = await GaragenkontoAsync(ctx, gv);

            Monat(konto, Januar).Bezahlt.Should().Be(0m);
            konto.RueckgaengigGemacht.Should().ContainSingle()
                .Which.Should().Match<MietkontoZahlung>(z => z.Betrag == 50m && z.Garage == "G1");
            konto.NichtZugeordnet.Should().ContainSingle()
                .Which.Should().Match<MietkontoZahlung>(z => z.Betrag == 10m && z.Garage == "G1");
            konto.Offen.Should().Be(3 * 50m - 10m);
        }
    }
}
