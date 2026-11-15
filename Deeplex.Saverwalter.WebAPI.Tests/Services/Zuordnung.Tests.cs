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
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Services.Buchungen.TransaktionBuchungsService;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class ZuordnungTests
    {
        private static Bankkonto Bank(SaverwalterContext ctx, string nr)
        {
            var bank = new Bankkonto { Iban = $"DE{nr}", BuchungsKonto = new Buchungskonto(nr, "Bank", BuchungskontoTyp.Aktiv) };
            ctx.Bankkontos.Add(bank);
            return bank;
        }

        private static Umlage Wasser(SaverwalterContext ctx)
        {
            var umlage = new Umlage
            {
                Typ = new Umlagetyp("Wasser"),
                NkVerrechnungsKonto = new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1200", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(umlage);
            return umlage;
        }

        private static (Wohnung, Kontakt) EgLinks(SaverwalterContext ctx)
        {
            var wohnung = new Wohnung("EG links")
            {
                MietErtragskonto = new Buchungskonto("4000", "Mieterträge", BuchungskontoTyp.Ertrag),
                AufwandsKonto = new Buchungskonto("4900", "Aufwand", BuchungskontoTyp.Aufwand),
            };
            var aussteller = new Kontakt("Handwerker GmbH", Rechtsform.gmbh);
            ctx.Wohnungen.Add(wohnung);
            ctx.Kontakte.Add(aussteller);
            return (wohnung, aussteller);
        }

        private static Task<Transaktion> BucheAsync(SaverwalterContext ctx, TransaktionsInput input) =>
            new TransaktionBuchungsService(ctx).BucheAsync(input);

        private static Task<Transaktion> MieteAsync(SaverwalterContext ctx, Vertrag vertrag, DateOnly monat, DateOnly datum) =>
            BucheAsync(ctx, new TransaktionsInput
            {
                Betrag = 550m,
                Zahlungsdatum = datum,
                Mieten = { new MietzahlungsInput { VertragId = vertrag.VertragId, BetreffenderMonat = monat, Kaltmiete = 500m, NkVorauszahlung = 50m } },
            });

        private static async Task StornoAsync(SaverwalterContext ctx, Transaktion t)
        {
            foreach (var satz in ctx.Buchungssaetze.Where(s => s.Transaktion == t).ToList())
                await new StornoBuchungsService(ctx).StornierenAsync(satz.BuchungssatzId, "falsch");
        }

        private static async Task<List<ZuordnungEintrag>> ZuordnungAsync(SaverwalterContext ctx, Transaktion t)
        {
            ctx.ChangeTracker.Clear();
            return (await Zuordnung.ErmittleAsync(ctx, [t.TransaktionId]))[t.TransaktionId];
        }

        // ── Zahlungen ─────────────────────────────────────────────────────────

        [Fact]
        public async Task MietzahlungNenntMonatDerSollstellungUndJahrDerVorauszahlung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var t = await MieteAsync(ctx, vertrag, new DateOnly(2024, 3, 1), new DateOnly(2024, 4, 3));

            var zuordnung = await ZuordnungAsync(ctx, t);

            zuordnung.Select(z => (z.Text.Split(" – ")[0], z.Betrag)).Should()
                .BeEquivalentTo([("Kaltmiete Mär 2024", 500m), ("NK-Vorauszahlung 2024", 50m)]);
            zuordnung.Should().OnlyContain(z => z.Ref == new EntityRef("Vertrag", vertrag.VertragId.ToString(), EntityRefSection.Mietkonto) && !z.Storniert);
        }

        [Fact]
        public async Task StornierteMietzahlungBehaeltIhrenMonat()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var t = await MieteAsync(ctx, vertrag, new DateOnly(2024, 3, 1), new DateOnly(2024, 4, 3));
            await StornoAsync(ctx, t);

            var zuordnung = await ZuordnungAsync(ctx, t);

            zuordnung.Should().Contain(z => z.Text.StartsWith("Kaltmiete Mär 2024 – ") && z.Storniert);
        }

        [Fact]
        public async Task StornierteBetriebskostenzahlungBehaeltDasJahrDerRechnung()
        {
            var ctx = TestUtils.GetContext();
            var umlage = Wasser(ctx);
            var bank = Bank(ctx, "100");
            await ctx.SaveChangesAsync();
            var rechnung = await new BetriebskostenrechnungBuchungsService(ctx)
                .BucheRechnungAsync(umlage, 300m, new DateOnly(2024, 2, 1), 2023, null);
            var t = await BucheAsync(ctx, new TransaktionsInput
            {
                Betrag = 300m,
                Zahlungsdatum = new DateOnly(2024, 2, 10),
                ZahlerId = bank.BankkontoId,
                BetriebskostenEingaenge = { new BetriebskostenEingangInput { ExistingBuchungssatzId = rechnung.BuchungssatzId, Betrag = 300m } },
            });
            (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Which.Text.Should().Be("Betriebskosten Wasser 2023");

            await StornoAsync(ctx, t);
            var z = (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Subject;

            z.Text.Should().Be("Betriebskosten Wasser 2023");
            z.Ref.Should().Be(new EntityRef("Umlage", umlage.UmlageId.ToString(), EntityRefSection.Rechnungen));
            z.Storniert.Should().BeTrue();
        }

        [Fact]
        public async Task StornierteZahlungEinerHandwerkerrechnungBehaeltDieWohnung()
        {
            var ctx = TestUtils.GetContext();
            var (wohnung, aussteller) = EgLinks(ctx);
            var bank = Bank(ctx, "100");
            await ctx.SaveChangesAsync();
            var rechnung = await new ErhaltungsaufwendungBuchungsService(ctx).BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, 200m, new DateOnly(2024, 3, 1), "Heizung", null);
            var t = await BucheAsync(ctx, new TransaktionsInput
            {
                Betrag = 200m,
                Zahlungsdatum = new DateOnly(2024, 3, 20),
                ZahlerId = bank.BankkontoId,
                Erhaltungsaufwendungen = { new ErhaltungsaufwendungsInput { ExistingBuchungssatzId = rechnung.BuchungssatzId, Betrag = 200m } },
            });
            await StornoAsync(ctx, t);

            var z = (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Subject;

            z.Text.Should().Be("Erhaltungsaufwand – EG links");
            z.Ref.Should().Be(new EntityRef("Wohnung", wohnung.WohnungId.ToString(), EntityRefSection.Erhaltungsaufwand));
            z.Storniert.Should().BeTrue();
        }

        [Fact]
        public async Task StornierteNachzahlungBehaeltDasJahrDerAbrechnung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var resultat = new Abrechnungsresultat { Vertrag = vertrag, Abgesendet = true };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, 2023, 120m);
            ctx.Abrechnungsresultate.Add(resultat);
            await ctx.SaveChangesAsync();
            var t = await BucheAsync(ctx, new TransaktionsInput
            {
                Betrag = 120m,
                Zahlungsdatum = new DateOnly(2024, 6, 1),
                AbrechnungsAusgleiche = { new AbrechnungsAusgleichInput { AbrechnungsresultatId = resultat.AbrechnungsresultatId, Betrag = 120m } },
            });
            await StornoAsync(ctx, t);

            var z = (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Subject;

            z.Text.Should().StartWith("Nachzahlung NK-Abrechnung 2023 – ");
            z.Storniert.Should().BeTrue();
        }

        [Fact]
        public async Task ZahlungOhneZuordnungNenntKeinenMonat()
        {
            // Bestand ohne Ausgleich: was nicht festgehalten ist, wird nicht geschätzt.
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var t = new Transaktion { Zahlungsdatum = new DateOnly(2024, 3, 5), Betrag = 500m };
            var satz = new Buchungssatz(new DateOnly(2024, 3, 5), "Miete") { Transaktion = t };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 500m) { Buchungssatz = satz, Buchungskonto = vertrag.ZahlungsKonto });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 500m) { Buchungssatz = satz, Buchungskonto = vertrag.MietBuchungskonto });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var z = (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Subject;

            z.Text.Should().MatchRegex("^Kaltmiete – ");
        }

        [Fact]
        public async Task ZeileAufKontoOhneBesitzerErscheintMitDemKonto()
        {
            var ctx = TestUtils.GetContext();
            var bank = Bank(ctx, "100");
            var fremd = new Buchungskonto("9999", "Sonstiges", BuchungskontoTyp.Aufwand);
            var t = new Transaktion { Zahlungsdatum = new DateOnly(2024, 3, 5), Betrag = 10m };
            var satz = new Buchungssatz(new DateOnly(2024, 3, 5), "Gebühr") { Transaktion = t };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 10m) { Buchungssatz = satz, Buchungskonto = fremd });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 10m) { Buchungssatz = satz, Buchungskonto = bank.BuchungsKonto });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var z = (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Subject;

            z.Text.Should().Be("Buchungskonto 9999 Sonstiges");
            z.Ref.Should().Be(new EntityRef("Buchungskonto", fremd.BuchungskontoId.ToString()));
        }

        [Fact]
        public async Task UmbuchungZwischenBankkontenIstNichtZugeordnet()
        {
            var ctx = TestUtils.GetContext();
            var zahler = Bank(ctx, "100");
            var empfaenger = Bank(ctx, "200");
            await ctx.SaveChangesAsync();
            var t = await BucheAsync(ctx, new TransaktionsInput
            {
                Betrag = 10m,
                Zahlungsdatum = new DateOnly(2024, 1, 2),
                ZahlerId = zahler.BankkontoId,
                ZahlungsempfaengerId = empfaenger.BankkontoId,
                Sonstige = { new SonstigerBuchungssatzInput { Betrag = 10m } },
            });

            (await ZuordnungAsync(ctx, t)).Should().BeEmpty();
        }

        // ── Rechnungen mit Transaktion als Beleg (ohne Geld-Zeile) ────────────

        [Fact]
        public async Task RechnungMitVerteilungIstEinEintrag()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var umlage = Wasser(ctx);
            var (wohnung, _) = EgLinks(ctx);
            var t = new Transaktion { Zahlungsdatum = new DateOnly(2024, 2, 1), Betrag = 300m };
            var satz = new Buchungssatz(new DateOnly(2024, 2, 1), "Wasser") { Buchungsjahr = 2023, Transaktion = t };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 300m) { Buchungssatz = satz, Buchungskonto = umlage.NkVerrechnungsKonto });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 200m) { Buchungssatz = satz, Buchungskonto = vertrag.NkBuchungskonto });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 100m) { Buchungssatz = satz, Buchungskonto = wohnung.AufwandsKonto });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var z = (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Subject;

            z.Text.Should().Be("Betriebskosten Wasser 2023");
            z.Betrag.Should().Be(300m);
        }

        [Fact]
        public async Task HandwerkerrechnungMitTransaktionIstEinEintrag()
        {
            var ctx = TestUtils.GetContext();
            var (wohnung, aussteller) = EgLinks(ctx);
            await ctx.SaveChangesAsync();
            var satz = await new ErhaltungsaufwendungBuchungsService(ctx).BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, 200m, new DateOnly(2024, 3, 1), "Heizung", null);
            var t = new Transaktion { Zahlungsdatum = new DateOnly(2024, 3, 1), Betrag = 200m };
            satz.Transaktion = t;
            await ctx.SaveChangesAsync();

            var z = (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Subject;

            z.Text.Should().Be("Erhaltungsaufwand – EG links");
            z.Betrag.Should().Be(200m);
        }

        // ── Buchen und Schutz ─────────────────────────────────────────────────

        [Fact]
        public async Task BuchenOrdnetDieZahlungDerSollstellungZu()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var t = await MieteAsync(ctx, vertrag, new DateOnly(2024, 3, 1), new DateOnly(2024, 4, 3));
            ctx.ChangeTracker.Clear();

            var zeile = await ctx.Buchungszeilen
                .Include(z => z.ZugeordnetZu).ThenInclude(p => p!.Buchungssatz)
                .SingleAsync(z => z.Buchungssatz.Transaktion!.TransaktionId == t.TransaktionId
                    && z.Buchungskonto.BuchungskontoId == vertrag.MietBuchungskonto.BuchungskontoId);

            zeile.ZugeordnetZu.Should().NotBeNull();
            zeile.ZugeordnetZu!.Buchungssatz.Buchungsdatum.Month.Should().Be(3);
        }

        [Fact]
        public async Task PostenMitStornierterZahlungIstNichtMehrLoeschbar()
        {
            var ctx = TestUtils.GetContext();
            var umlage = Wasser(ctx);
            var bank = Bank(ctx, "100");
            await ctx.SaveChangesAsync();
            var rechnung = await new BetriebskostenrechnungBuchungsService(ctx)
                .BucheRechnungAsync(umlage, 300m, new DateOnly(2024, 2, 1), 2023, null);
            var t = await BucheAsync(ctx, new TransaktionsInput
            {
                Betrag = 300m,
                Zahlungsdatum = new DateOnly(2024, 2, 10),
                ZahlerId = bank.BankkontoId,
                BetriebskostenEingaenge = { new BetriebskostenEingangInput { ExistingBuchungssatzId = rechnung.BuchungssatzId, Betrag = 300m } },
            });
            await StornoAsync(ctx, t);

            var schutz = await new BuchungssatzSchutzService(ctx).PruefeAsync(rechnung);

            schutz.HatOposVerknuepfung.Should().BeFalse();
            schutz.HatZugeordneteZahlungen.Should().BeTrue();
            schutz.KannLoeschen.Should().BeFalse();
            schutz.KannStornieren.Should().BeTrue();
        }

        /// <summary>
        /// Invariante für jede Zahlungsart des Buchungsservices: jeder Ausgleich einer
        /// Zahlungszeile hat die passende Zuordnung (Gegenstück zum Architekturtest).
        /// </summary>
        [Fact]
        public async Task JederAusgleichEinerZahlungHatSeineZuordnung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var umlage = Wasser(ctx);
            var (wohnung, aussteller) = EgLinks(ctx);
            var bank = Bank(ctx, "100");
            var gv = new GarageVertrag
            {
                Garage = new Garage("G1")
                {
                    Besitzer = vertrag.Ansprechpartner!,
                    Ertragskonto = new Buchungskonto("4100", "Garagenmiete", BuchungskontoTyp.Ertrag),
                },
                Vertrag = vertrag,
                MietBuchungskonto = new Buchungskonto("1100", "Garage", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto("1101", "Garage Zahlung", BuchungskontoTyp.Aktiv),
            };
            gv.Versionen.Add(new GarageVertragVersion(new DateOnly(2024, 1, 1), 50m) { GarageVertrag = gv });
            ctx.GarageVertraege.Add(gv);
            await ctx.SaveChangesAsync();

            var bkRechnung = await new BetriebskostenrechnungBuchungsService(ctx)
                .BucheRechnungAsync(umlage, 300m, new DateOnly(2024, 2, 1), 2023, null);
            var eaRechnung = await new ErhaltungsaufwendungBuchungsService(ctx).BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, 200m, new DateOnly(2024, 3, 1), "Heizung", null);
            var nachzahlung = new Abrechnungsresultat { Vertrag = vertrag, Abgesendet = true };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(nachzahlung, 2023, 120m);
            var erstattung = new Abrechnungsresultat { Vertrag = vertrag, Abgesendet = true };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(erstattung, 2022, -80m);
            ctx.Abrechnungsresultate.AddRange(nachzahlung, erstattung);
            await ctx.SaveChangesAsync();

            var datum = new DateOnly(2024, 4, 3);
            TransaktionsInput Mit(Action<TransaktionsInput> setze) { var i = new TransaktionsInput { Zahlungsdatum = datum, ZahlerId = bank.BankkontoId }; setze(i); return i; }
            foreach (var input in new[]
            {
                Mit(i => { i.Betrag = 550m; i.Mieten.Add(new MietzahlungsInput { VertragId = vertrag.VertragId, BetreffenderMonat = new DateOnly(2024, 3, 1), Kaltmiete = 500m, Garagen = { new GaragenmietInput { GarageVertragId = gv.GarageVertragId, Betrag = 50m } } }); }),
                Mit(i => { i.Betrag = 50m; i.GaragenEingaenge.Add(new StandaloneGaragenmietInput { GarageVertragId = gv.GarageVertragId, BetreffenderMonat = new DateOnly(2024, 4, 1), Betrag = 50m }); }),
                Mit(i => { i.Betrag = 100m; i.BetriebskostenEingaenge.Add(new BetriebskostenEingangInput { ExistingBuchungssatzId = bkRechnung.BuchungssatzId, Betrag = 100m }); }),
                Mit(i => { i.Betrag = 70m; i.BetriebskostenEingaenge.Add(new BetriebskostenEingangInput { UmlageId = umlage.UmlageId, BetreffendesJahr = 2024, RechnungsDatum = datum, Betrag = 70m }); }),
                Mit(i => { i.Betrag = 200m; i.Erhaltungsaufwendungen.Add(new ErhaltungsaufwendungsInput { ExistingBuchungssatzId = eaRechnung.BuchungssatzId, Betrag = 200m }); }),
                Mit(i => { i.Betrag = 120m; i.AbrechnungsAusgleiche.Add(new AbrechnungsAusgleichInput { AbrechnungsresultatId = nachzahlung.AbrechnungsresultatId, Betrag = 120m }); }),
                Mit(i => { i.Betrag = 80m; i.AbrechnungsAusgleiche.Add(new AbrechnungsAusgleichInput { AbrechnungsresultatId = erstattung.AbrechnungsresultatId, Betrag = 80m }); }),
            })
                await BucheAsync(ctx, input);
            ctx.ChangeTracker.Clear();

            var ausgleiche = await ctx.OffenePostenAusgleiche
                .Include(a => a.SollZeile).ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.Transaktion)
                .Include(a => a.SollZeile).ThenInclude(z => z.ZugeordnetZu)
                .Include(a => a.HabenZeile).ThenInclude(z => z.Buchungssatz).ThenInclude(s => s.Transaktion)
                .Include(a => a.HabenZeile).ThenInclude(z => z.ZugeordnetZu)
                .ToListAsync();
            var zahlungsAusgleiche = ausgleiche
                .Select(a => a.SollZeile.Buchungssatz.Transaktion != null
                    ? (Zahlung: a.SollZeile, Posten: a.HabenZeile)
                    : (Zahlung: a.HabenZeile, Posten: a.SollZeile))
                .ToList();

            // Miete, Garage (2×), BK-Rechnung (2×), Erhaltungsaufwand, Nachzahlung, Erstattung.
            zahlungsAusgleiche.Should().HaveCount(8);
            zahlungsAusgleiche.Should().OnlyContain(x =>
                x.Zahlung.Buchungssatz.Transaktion != null
                && x.Zahlung.ZugeordnetZu != null
                && x.Zahlung.ZugeordnetZu.BuchungszeileId == x.Posten.BuchungszeileId);
        }

        // ── Mehrdeutige Konten und Strukturprüfung ────────────────────────────

        [Fact]
        public async Task KontoMitZweiBesitzernIstMehrdeutigUndWirdNichtGeraten()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var (wohnung, _) = EgLinks(ctx);
            wohnung.AufwandsKonto = vertrag.MietBuchungskonto; // Fehler in den Daten: ein Konto, zwei Besitzer
            var bank = Bank(ctx, "100");
            var t = new Transaktion { Zahlungsdatum = new DateOnly(2024, 3, 5), Betrag = 10m };
            var satz = new Buchungssatz(new DateOnly(2024, 3, 5), "Zahlung") { Transaktion = t };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 10m) { Buchungssatz = satz, Buchungskonto = bank.BuchungsKonto });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 10m) { Buchungssatz = satz, Buchungskonto = vertrag.MietBuchungskonto });
            ctx.Buchungssaetze.Add(satz);
            await ctx.SaveChangesAsync();

            var rollen = await Kontorollen.ErmittleAsync(ctx);
            rollen.Mehrdeutig.Should().ContainKey(vertrag.MietBuchungskonto.BuchungskontoId);
            rollen.Eindeutig.Should().NotContainKey(vertrag.MietBuchungskonto.BuchungskontoId);

            var z = (await ZuordnungAsync(ctx, t)).Should().ContainSingle().Subject;
            z.Text.Should().StartWith("Buchungskonto ");

            var pruefung = await Strukturpruefung.ErmittleAsync(ctx, null);
            pruefung.MehrdeutigeKonten.Should().ContainSingle()
                .Which.Besitzer.Should().HaveCount(2);
            pruefung.Auffaellig.Should().BeTrue();
        }

        [Fact]
        public async Task BankkontoHinterDemZahlungskontoIstKeinKonflikt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            ctx.Bankkontos.Add(new Bankkonto { Iban = "DE1", BuchungsKonto = vertrag.ZahlungsKonto });
            await ctx.SaveChangesAsync();

            var rollen = await Kontorollen.ErmittleAsync(ctx);

            rollen.Mehrdeutig.Should().BeEmpty();
            rollen.Eindeutig[vertrag.ZahlungsKonto.BuchungskontoId].Rolle.Should().Be(KontoRolle.Geld);
        }

        [Fact]
        public async Task StrukturpruefungListetZahlungenOhneZuordnung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            // Gebuchte Zahlung: zugeordnet, nicht gelistet.
            await MieteAsync(ctx, vertrag, new DateOnly(2024, 3, 1), new DateOnly(2024, 3, 5));
            // Bestand ohne Zuordnung, einmal offen, einmal storniert.
            Transaktion OhneZuordnung(decimal betrag)
            {
                var t = new Transaktion { Zahlungsdatum = new DateOnly(2024, 5, 5), Betrag = betrag };
                var satz = new Buchungssatz(new DateOnly(2024, 5, 5), "Miete") { Transaktion = t };
                satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, betrag) { Buchungssatz = satz, Buchungskonto = vertrag.ZahlungsKonto });
                satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag) { Buchungssatz = satz, Buchungskonto = vertrag.MietBuchungskonto });
                ctx.Buchungssaetze.Add(satz);
                return t;
            }
            var offen = OhneZuordnung(500m);
            var spaeterStorniert = OhneZuordnung(300m);
            await ctx.SaveChangesAsync();
            await StornoAsync(ctx, spaeterStorniert);
            ctx.ChangeTracker.Clear();

            var pruefung = await Strukturpruefung.ErmittleAsync(ctx, null);

            pruefung.ZahlungenOhneZuordnung.Select(z => (z.TransaktionId, z.Betrag, z.Storniert)).Should()
                .Equal((offen.TransaktionId, 500m, false), (spaeterStorniert.TransaktionId, 300m, true));
            pruefung.ZahlungenOhneZuordnung.Should().OnlyContain(z => z.Text.StartsWith("Kaltmiete – "));
            pruefung.Auffaellig.Should().BeTrue();

            var nurSichtbar = await Strukturpruefung.ErmittleAsync(ctx, new HashSet<int>());
            nurSichtbar.ZahlungenOhneZuordnung.Should().BeEmpty();
        }
    }
}
