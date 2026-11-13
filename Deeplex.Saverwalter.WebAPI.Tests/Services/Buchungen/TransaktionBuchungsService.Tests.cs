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
    public class TransaktionBuchungsServiceTests
    {
        private static Bankkonto MakeBankkonto(string nr) => new()
        {
            Iban = $"DE{nr}",
            BuchungsKonto = new Buchungskonto(nr, "Bank", BuchungskontoTyp.Aktiv),
        };

        // ── Mietzahlung ────────────────────────────────────────────────────────

        [Fact]
        public async Task MietzahlungLegtSollstellungAnUndGleichtAus()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 500m,
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Verwendungszweck = "",
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                        NkVorauszahlung = 0m,
                    }
                }
            };

            var transaktion = await service.BucheAsync(input);

            transaktion.Betrag.Should().Be(500m);
            // Sollstellung (Mietsoll) + Zahlung gebucht
            var saetze = await ctx.Buchungssaetze.Include(s => s.Buchungszeilen).ToListAsync();
            saetze.Should().Contain(s => s.Beschreibung.Contains("Mietsoll"));
            saetze.Should().Contain(s => s.Beschreibung.Contains("Kaltmiete"));
            // OPOS-Ausgleich zwischen Sollstellung und Zahlung
            (await ctx.OffenePostenAusgleiche.CountAsync()).Should().Be(1);
            // Verwendungszweck automatisch erzeugt
            transaktion.Verwendungszweck.Should().Contain("Miete 01/2024");
        }

        [Fact]
        public async Task MietzahlungMitNkVorauszahlungBuchtZweiSaetze()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 650m,
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Verwendungszweck = "Miete",
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                        NkVorauszahlung = 150m,
                    }
                }
            };

            await service.BucheAsync(input);

            var saetze = await ctx.Buchungssaetze.ToListAsync();
            saetze.Should().Contain(s => s.Beschreibung.Contains("NK-VZ"));
        }

        [Fact]
        public async Task NkVorauszahlungZaehltZumJahrDesBetreffendenMonats()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            // Dezember-Miete, erst im Januar des Folgejahres gezahlt.
            var service = new TransaktionBuchungsService(ctx);
            await service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 650m,
                Zahlungsdatum = new DateOnly(2025, 1, 3),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 12, 1),
                        Kaltmiete = 500m,
                        NkVorauszahlung = 150m,
                    }
                }
            });

            var nkZeile = await ctx.Buchungszeilen
                .Include(z => z.Buchungssatz)
                .SingleAsync(z => z.Buchungskonto.BuchungskontoId == vertrag.NkBuchungskonto.BuchungskontoId);
            nkZeile.Buchungssatz.Buchungsjahr.Should().Be(2024);
            nkZeile.Buchungssatz.Buchungsdatum.Should().Be(new DateOnly(2025, 1, 3));
        }

        [Fact]
        public async Task NkVorauszahlungFuerAbgerechnetesJahrWirdAbgelehnt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, 2024, 0m);
            ctx.Abrechnungsresultate.Add(resultat);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var act = () => service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 150m,
                Zahlungsdatum = new DateOnly(2025, 1, 3),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 12, 1),
                        Kaltmiete = 0m,
                        NkVorauszahlung = 150m,
                    }
                }
            });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*2024*bereits abgerechnet*");
        }

        [Fact]
        public async Task WirftWennSummeNichtZumBetragPasst()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 999m, // passt nicht zu Kaltmiete 500
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                    }
                }
            };

            var act = () => service.BucheAsync(input);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task WirftWennKeinePositionMitBetrag()
        {
            var ctx = TestUtils.GetContext();
            var service = new TransaktionBuchungsService(ctx);
            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 0m,
                Zahlungsdatum = new DateOnly(2024, 1, 1),
            };

            var act = () => service.BucheAsync(input);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task WirftWennZahlungVollstaendigeForderungUebersteigt()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);

            // Erste Zahlung gleicht die Sollstellung vollständig aus.
            await service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 500m,
                Zahlungsdatum = new DateOnly(2024, 1, 5),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                    }
                }
            });

            // Zweite Zahlung für denselben Monat → übersteigt verbleibende Forderung.
            var act = () => service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 500m,
                Zahlungsdatum = new DateOnly(2024, 1, 6),
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = new DateOnly(2024, 1, 1),
                        Kaltmiete = 500m,
                    }
                }
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // ── Sonstiger Buchungssatz ─────────────────────────────────────────────

        [Fact]
        public async Task SonstigerBuchungssatzBrauchtZahlerUndEmpfaenger()
        {
            var ctx = TestUtils.GetContext();
            var service = new TransaktionBuchungsService(ctx);

            var input = new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 100m,
                Zahlungsdatum = new DateOnly(2024, 1, 1),
                Sonstige = { new TransaktionBuchungsService.SonstigerBuchungssatzInput { Betrag = 100m, Beschreibung = "X" } }
            };

            var act = () => service.BucheAsync(input);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SonstigerBuchungssatzBuchtZwischenBankkonten()
        {
            var ctx = TestUtils.GetContext();
            var zahler = MakeBankkonto("100");
            var empfaenger = MakeBankkonto("200");
            ctx.Bankkontos.AddRange(zahler, empfaenger);
            await ctx.SaveChangesAsync();

            var service = new TransaktionBuchungsService(ctx);
            var transaktion = await service.BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 100m,
                Zahlungsdatum = new DateOnly(2024, 1, 1),
                ZahlerId = zahler.BankkontoId,
                ZahlungsempfaengerId = empfaenger.BankkontoId,
                Verwendungszweck = "Übertrag",
                Sonstige = { new TransaktionBuchungsService.SonstigerBuchungssatzInput { Betrag = 100m, Beschreibung = "Übertrag" } }
            });

            var satz = await ctx.Buchungssaetze.Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .SingleAsync();
            satz.Buchungszeilen.Should().HaveCount(2);
            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll).Buchungskonto
                .Should().Be(empfaenger.BuchungsKonto);
            satz.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben).Buchungskonto
                .Should().Be(zahler.BuchungsKonto);
        }

        // ── AbrechnungsAusgleich (NK-Abrechnung Nachzahlung/Erstattung) ────────

        private static async Task<(SaverwalterContext ctx, Abrechnungsresultat resultat)>
            SetupAbrechnung(decimal saldo, bool abgesendet = true)
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var resultat = new Abrechnungsresultat { Vertrag = vertrag, Abgesendet = abgesendet };
            ctx.Abrechnungsresultate.Add(resultat);
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, 2024, saldo);
            await ctx.SaveChangesAsync();
            return (ctx, resultat);
        }

        private static TransaktionBuchungsService.TransaktionsInput AusgleichsInput(
            Abrechnungsresultat resultat, decimal betrag, int? zahlerId = null) => new()
            {
                Betrag = betrag,
                Zahlungsdatum = new DateOnly(2025, 3, 10),
                Verwendungszweck = "NK-Abrechnung",
                ZahlerId = zahlerId,
                AbrechnungsAusgleiche =
            {
                new TransaktionBuchungsService.AbrechnungsAusgleichInput
                {
                    AbrechnungsresultatId = resultat.AbrechnungsresultatId,
                    Betrag = betrag
                }
            }
            };

        [Fact]
        public async Task AbrechnungsAusgleichNachzahlungGleichtForderungAus()
        {
            var (ctx, resultat) = await SetupAbrechnung(saldo: 200m);
            var service = new TransaktionBuchungsService(ctx);

            await service.BucheAsync(AusgleichsInput(resultat, 200m));

            var zahlung = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .SingleAsync(s => s.Beschreibung.Contains("Zahlung BK-Abrechnung"));
            zahlung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll).Buchungskonto
                .Should().Be(resultat.Vertrag.ZahlungsKonto);
            zahlung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben).Buchungskonto
                .Should().Be(resultat.Vertrag.BkAbrechnungsKonto);

            var ausgleich = await ctx.OffenePostenAusgleiche
                .Include(o => o.SollZeile).Include(o => o.HabenZeile)
                .SingleAsync();
            ausgleich.SollZeile.Buchungssatz.Should().Be(resultat.Buchungssatz);
            ausgleich.HabenZeile.Betrag.Should().Be(200m);
        }

        [Fact]
        public async Task AbrechnungsAusgleichTeilzahlungenBisVollstaendig()
        {
            var (ctx, resultat) = await SetupAbrechnung(saldo: 200m);
            var service = new TransaktionBuchungsService(ctx);

            await service.BucheAsync(AusgleichsInput(resultat, 150m));
            await service.BucheAsync(AusgleichsInput(resultat, 50m));

            (await ctx.OffenePostenAusgleiche.CountAsync()).Should().Be(2);

            // Überzahlung nach vollständigem Ausgleich → Fehler
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BucheAsync(AusgleichsInput(resultat, 1m)));
        }

        [Fact]
        public async Task AbrechnungsAusgleichUeberzahlungWirdAbgelehnt()
        {
            var (ctx, resultat) = await SetupAbrechnung(saldo: 200m);
            var service = new TransaktionBuchungsService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BucheAsync(AusgleichsInput(resultat, 250m)));
        }

        [Fact]
        public async Task AbrechnungsAusgleichNichtAbgesendetWirdAbgelehnt()
        {
            var (ctx, resultat) = await SetupAbrechnung(saldo: 200m, abgesendet: false);
            var service = new TransaktionBuchungsService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BucheAsync(AusgleichsInput(resultat, 200m)));
        }

        [Fact]
        public async Task AbrechnungsAusgleichErstattungOhneZahlerBuchtAufZahlungskonto()
        {
            // Ohne erfasstes Bankkonto: Soll BkAbrechnungsKonto (tilgt die Guthaben-
            // Verbindlichkeit per OPOS) / Haben ZahlungsKonto (Geldabfluss).
            var (ctx, resultat) = await SetupAbrechnung(saldo: -200m);
            var service = new TransaktionBuchungsService(ctx);

            await service.BucheAsync(AusgleichsInput(resultat, 200m));

            var erstattung = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .SingleAsync(s => s.Beschreibung.Contains("Erstattung BK-Abrechnung"));
            erstattung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll).Buchungskonto
                .Should().Be(resultat.Vertrag.BkAbrechnungsKonto);
            erstattung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben).Buchungskonto
                .Should().Be(resultat.Vertrag.ZahlungsKonto);

            // OPOS tilgt die Guthaben-Haben-Zeile des ABRECHNUNGSsatzes.
            var ausgleich = await ctx.OffenePostenAusgleiche
                .Include(o => o.SollZeile).Include(o => o.HabenZeile)
                .SingleAsync();
            ausgleich.HabenZeile.Buchungssatz.Should().Be(resultat.Buchungssatz);
            ausgleich.SollZeile.Betrag.Should().Be(200m);
        }

        [Fact]
        public async Task AbrechnungsAusgleichErstattungMitZahlerBuchtAufBankkonto()
        {
            var (ctx, resultat) = await SetupAbrechnung(saldo: -200m);
            var zahler = MakeBankkonto("100");
            ctx.Bankkontos.Add(zahler);
            await ctx.SaveChangesAsync();
            var service = new TransaktionBuchungsService(ctx);

            await service.BucheAsync(AusgleichsInput(resultat, 200m, zahler.BankkontoId));

            var erstattung = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .SingleAsync(s => s.Beschreibung.Contains("Erstattung BK-Abrechnung"));
            erstattung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll).Buchungskonto
                .Should().Be(resultat.Vertrag.BkAbrechnungsKonto);
            erstattung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben).Buchungskonto
                .Should().Be(zahler.BuchungsKonto);

            var ausgleich = await ctx.OffenePostenAusgleiche
                .Include(o => o.SollZeile).Include(o => o.HabenZeile)
                .SingleAsync();
            ausgleich.HabenZeile.Buchungssatz.Should().Be(resultat.Buchungssatz);
            ausgleich.SollZeile.Betrag.Should().Be(200m);
        }

        [Fact]
        public async Task StornoDerAbrechnungEntferntAusgleichsOpos()
        {
            var (ctx, resultat) = await SetupAbrechnung(saldo: 200m);
            var service = new TransaktionBuchungsService(ctx);
            await service.BucheAsync(AusgleichsInput(resultat, 200m));
            (await ctx.OffenePostenAusgleiche.CountAsync()).Should().Be(1);

            await new StornoBuchungsService(ctx)
                .StornierenAsync(resultat.Buchungssatz.BuchungssatzId);

            // OPOS wird bereinigt; der Zahlungssatz (echtes Geld) bleibt bestehen.
            (await ctx.OffenePostenAusgleiche.CountAsync()).Should().Be(0);
            (await ctx.Buchungssaetze.CountAsync(s =>
                s.Beschreibung.Contains("Zahlung BK-Abrechnung"))).Should().Be(1);
        }

        private static TransaktionBuchungsService.TransaktionsInput Miete(Vertrag vertrag, DateOnly monat, decimal kaltmiete, decimal nk) => new()
        {
            Betrag = kaltmiete + nk,
            Zahlungsdatum = monat.AddDays(4),
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
        };

        [Fact]
        public async Task MietzahlungImMonatEinerMietKorrekturTrifftDieSollstellung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var service = new TransaktionBuchungsService(ctx);

            // Vorjahr bezahlt, dann Erhöhung nachgetragen und heute korrigiert.
            var vorjahr = new DateOnly(DateTime.Today.Year - 1, 1, 1);
            await service.BucheAsync(Miete(vertrag, vorjahr, 500m, 150m));
            vertrag.Versionen.Add(new VertragVersion(vorjahr, 600m, 1) { Vertrag = vertrag });
            await ctx.SaveChangesAsync();
            var korrektur = await new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx))
                .BucheDifferenzAsync(vertrag.VertragId, vorjahr.Year, "Erhöhung nachgetragen");

            // Miete des laufenden Monats — im selben Monat wie die Korrektur gebucht.
            var monat = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
            var transaktion = await service.BucheAsync(Miete(vertrag, monat, 600m, 0m));

            var zahlung = transaktion.Buchungssaetze.Single();
            var sollZeile = zahlung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben).AlsHabenZeile.Single().SollZeile;
            sollZeile.Buchungssatz.BuchungssatzId.Should().NotBe(korrektur.BuchungssatzId);
            sollZeile.Betrag.Should().Be(600m);
            sollZeile.Buchungssatz.Buchungsdatum.Month.Should().Be(monat.Month);
        }

        [Fact]
        public async Task StornierteSollstellungWirdNichtWiederverwendet()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var service = new TransaktionBuchungsService(ctx);
            var monat = new DateOnly(2024, 3, 1);

            var erste = await service.BucheAsync(Miete(vertrag, monat, 500m, 0m));
            var zahlung = erste.Buchungssaetze.Single();
            var sollstellung = zahlung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben).AlsHabenZeile.Single().SollZeile.Buchungssatz;
            var storno = new StornoBuchungsService(ctx);
            await storno.StornierenAsync(zahlung.BuchungssatzId, "falsch");
            await storno.StornierenAsync(sollstellung.BuchungssatzId, "falsch");

            var zweite = await service.BucheAsync(Miete(vertrag, monat, 500m, 0m));

            var neueSoll = zweite.Buchungssaetze.Single().Buchungszeilen
                .Single(z => z.SollHaben == SollHaben.Haben).AlsHabenZeile.Single().SollZeile.Buchungssatz;
            neueSoll.BuchungssatzId.Should().NotBe(sollstellung.BuchungssatzId);
            neueSoll.StornoNach.Should().BeNull();
        }

        private static List<string> Zeilen(IEnumerable<(string Beschreibung, IEnumerable<(string Konto, SollHaben Seite, decimal Betrag)> Zeilen)> saetze) =>
            saetze.SelectMany(s => s.Zeilen.Select(z => $"{s.Beschreibung}|{z.Konto}|{z.Seite}|{z.Betrag:0.00}"))
                .OrderBy(x => x).ToList();

        [Fact]
        public async Task VorschauZeigtGenauDieSpaetereBuchungUndSpeichertNichts()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var input = () => Miete(vertrag, new DateOnly(2024, 3, 1), 500m, 150m);
            var saetzeVorher = ctx.Buchungssaetze.Count();

            var vorschau = await new TransaktionBuchungsService(ctx).VorschauAsync(input());

            ctx.Buchungssaetze.Count().Should().Be(saetzeVorher, "die Vorschau speichert nichts");
            ctx.Transaktionen.Should().BeEmpty();
            vorschau.Betrag.Should().Be(650m);
            vorschau.Ausgleiche.Should().Be(1);
            vorschau.Saetze.Should().ContainSingle(s => !s.IstZahlung, "die Sollstellung des Monats entsteht mit");

            var transaktion = await new TransaktionBuchungsService(ctx).BucheAsync(input());
            var gebucht = transaktion.Buchungssaetze
                .Concat(transaktion.Buchungssaetze
                    .SelectMany(z => z.Buchungszeilen)
                    .SelectMany(z => z.AlsHabenZeile)
                    .Select(a => a.SollZeile.Buchungssatz))
                .Distinct()
                .ToList();
            gebucht.Should().HaveCount(3, "Sollstellung, Kaltmiete und NK-Vorauszahlung");
            vorschau.Saetze.Should().HaveCount(3);

            Zeilen(vorschau.Saetze.Select(v => (v.Beschreibung, v.Zeilen.Select(z => (z.Kontonummer, z.SollHaben, z.Betrag)))))
                .Should().Equal(Zeilen(gebucht.Select(g => (g.Beschreibung,
                    g.Buchungszeilen.Select(z => (z.Buchungskonto.Kontonummer, z.SollHaben, z.Betrag))))));
        }

        [Fact]
        public async Task VorschauPrueftWieDieBuchung()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();

            var zuViel = () => new TransaktionBuchungsService(ctx).VorschauAsync(Miete(vertrag, new DateOnly(2024, 3, 1), 900m, 0m));

            await zuViel.Should().ThrowAsync<InvalidOperationException>().WithMessage("*übersteigt*");
            ctx.ChangeTracker.HasChanges().Should().BeFalse();
        }
    }
}
