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
using Npgsql;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests.Buchungen
{
    /// <summary>
    /// Läuft nur, wenn <c>WALTER_TEST_POSTGRES</c> gesetzt ist: eine Verbindungszeichenfolge
    /// ohne Datenbanknamen, z. B. <c>Host=db;Port=5432;Username=postgres;Password=postgres</c>.
    /// </summary>
    public sealed class PostgresFactAttribute : FactAttribute
    {
        public const string Variable = "WALTER_TEST_POSTGRES";

        public PostgresFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
                Skip = $"Umgebungsvariable {Variable} nicht gesetzt (PostgreSQL-Verbindung ohne Datenbanknamen).";
        }
    }

    /// <summary>Eine eigene Datenbank <c>walter_test_&lt;guid&gt;</c> auf dem Server aus <c>WALTER_TEST_POSTGRES</c>.</summary>
    public sealed class PostgresDatabase : IAsyncLifetime
    {
        private readonly string _name = $"walter_test_{Guid.NewGuid():N}";
        private NpgsqlConnectionStringBuilder _server = null!;

        private string ConnectionString => new NpgsqlConnectionStringBuilder(_server.ConnectionString)
        {
            Database = _name,
            // Kleine Pools: jeder Kontext hält höchstens kurz eine Verbindung.
            MaxPoolSize = 40,
        }.ConnectionString;

        public async Task InitializeAsync()
        {
            _server = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(PostgresFactAttribute.Variable)
                ?? throw new InvalidOperationException($"{PostgresFactAttribute.Variable} fehlt."));

            await ServerBefehlAsync($"CREATE DATABASE \"{_name}\"");
            await using var ctx = NeuerKontext();
            await ctx.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            NpgsqlConnection.ClearAllPools();
            await ServerBefehlAsync($"DROP DATABASE IF EXISTS \"{_name}\" WITH (FORCE)");
        }

        /// <summary>Ein neuer Kontext mit eigener Verbindung und eigenen Transaktionen.</summary>
        public SaverwalterContext NeuerKontext() =>
            new(new DbContextOptionsBuilder<SaverwalterContext>().UseNpgsql(ConnectionString).Options);

        private async Task ServerBefehlAsync(string sql)
        {
            var builder = new NpgsqlConnectionStringBuilder(_server.ConnectionString) { Database = "postgres", Pooling = false };
            await using var conn = new NpgsqlConnection(builder.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Die Datenbank-Sperre (<c>pg_advisory_xact_lock</c>) in <see cref="Sollstellung.GesperrtAsync{T}"/>:
    /// gleichzeitige Anfragen mit je eigenem Kontext, also eigener Verbindung und Transaktion, legen
    /// keine Sollstellung doppelt an. Jeder Test hat seine eigene Datenbank.
    /// </summary>
    public class SollstellungSperrePostgresTests : IAsyncLifetime
    {
        private static readonly DateOnly Januar = new(2024, 1, 1);
        private static readonly DateOnly Heute = new(2024, 12, 20);
        private const int Monate = 12;
        private static readonly TimeSpan Frist = TimeSpan.FromSeconds(60);

        private readonly PostgresDatabase _db = new();
        private int _vertragId;
        private int _mietKontoId;
        private int _nkKontoId;

        public Task InitializeAsync() => Environment.GetEnvironmentVariable(PostgresFactAttribute.Variable) is { Length: > 0 }
            ? SeedAsync()
            : Task.CompletedTask;

        public Task DisposeAsync() => Environment.GetEnvironmentVariable(PostgresFactAttribute.Variable) is { Length: > 0 }
            ? _db.DisposeAsync()
            : Task.CompletedTask;

        private async Task SeedAsync()
        {
            await _db.InitializeAsync();
            await using var ctx = _db.NeuerKontext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var version = vertrag.Versionen.Single();
            version.Beginn = Januar;
            version.Nebenkostenvorauszahlung = 150m;
            await ctx.SaveChangesAsync();
            _vertragId = vertrag.VertragId;
            _mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            _nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;
        }

        /// <summary>Startet alle Aufgaben gleichzeitig (je mit eigenem Kontext) und wartet auf alle.</summary>
        private async Task<T[]> ParallelAsync<T>(params Func<SaverwalterContext, Task<T>>[] aufgaben)
        {
            var start = new TaskCompletionSource();
            var laufende = aufgaben.Select(aufgabe => Task.Run(async () =>
            {
                await using var ctx = _db.NeuerKontext();
                await start.Task;
                return await aufgabe(ctx);
            })).ToArray();
            start.SetResult();
            return await Task.WhenAll(laufende).WaitAsync(Frist);
        }

        /// <summary>Die Sollstellungen des Mietkontos (Soll-Zeilen nach <see cref="Sollstellung.Ist"/>).</summary>
        private async Task<List<Buchungszeile>> SollstellungenAsync(SaverwalterContext ctx) =>
            (await ctx.Buchungszeilen
                .Include(z => z.Buchungssatz).ThenInclude(s => s.Buchungszeilen).ThenInclude(g => g.Buchungskonto)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.Transaktion)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.StornoVon)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.StornoNach)
                .Include(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile)
                .Where(z => z.Buchungskonto.BuchungskontoId == _mietKontoId)
                .ToListAsync())
            .Where(z => Sollstellung.Ist(z, _nkKontoId))
            .ToList();

        private static TransaktionBuchungsService.TransaktionsInput Miete(decimal betrag, DateOnly zahlungsdatum) => new()
        {
            Betrag = betrag,
            Zahlungsdatum = zahlungsdatum,
            Mieten =
            {
                new TransaktionBuchungsService.MietzahlungsInput
                {
                    VertragId = 0, // wird vom Aufrufer gesetzt
                    BetreffenderMonat = Januar,
                    Kaltmiete = betrag,
                    NkVorauszahlung = 0m,
                },
            },
        };

        private TransaktionBuchungsService.TransaktionsInput MieteJanuar(decimal betrag, int tag)
        {
            var input = Miete(betrag, new DateOnly(2024, 1, tag));
            input.Mieten[0].VertragId = _vertragId;
            return input;
        }

        private static async Task PruefeBuchungsnummernAsync(SaverwalterContext ctx)
        {
            var nummern = await ctx.Buchungssaetze.Select(s => s.Buchungsnummer).ToListAsync();
            nummern.Should().OnlyContain(n => n > 0);
            nummern.Should().OnlyHaveUniqueItems();
        }

        [PostgresFact]
        public async Task T1_AchtParalleleAnlegenLegenJedenMonatGenauEinmalAn()
        {
            var angelegt = await ParallelAsync(Enumerable.Repeat<Func<SaverwalterContext, Task<int>>>(
                ctx => new SollstellungService(ctx).AnlegenAsync(2024, Heute), 8).ToArray());

            angelegt.Sum().Should().Be(Monate);
            await using var pruef = _db.NeuerKontext();
            var soll = await SollstellungenAsync(pruef);
            soll.Should().HaveCount(Monate);
            soll.Select(z => z.Buchungssatz.Buchungsdatum.Month).Should().BeEquivalentTo(Enumerable.Range(1, Monate));
            await PruefeBuchungsnummernAsync(pruef);
        }

        [PostgresFact]
        public async Task T2_ZweiParalleleMietzahlungenLegenDieSollstellungGenauEinmalAn()
        {
            var transaktionen = await ParallelAsync<Transaktion>(
                ctx => new TransaktionBuchungsService(ctx).BucheAsync(MieteJanuar(250m, 5)),
                ctx => new TransaktionBuchungsService(ctx).BucheAsync(MieteJanuar(250m, 6)));

            transaktionen.Should().HaveCount(2);
            await PruefeEineSollstellungMitZweiZahlungenAsync();
        }

        [PostgresFact]
        public async Task T3_AnlegenUndBuchenGleichzeitigLegenDieSollstellungGenauEinmalAn()
        {
            // Der Lauf legt nur den Januar an (Stichtag im Januar), die Zahlung braucht denselben Monat.
            var heuteImJanuar = new DateOnly(2024, 1, 20);
            await ParallelAsync<object>(
                async ctx => await new SollstellungService(ctx).AnlegenAsync(2024, heuteImJanuar),
                async ctx => await new TransaktionBuchungsService(ctx).BucheAsync(MieteJanuar(250m, 5)),
                async ctx => await new TransaktionBuchungsService(ctx).BucheAsync(MieteJanuar(250m, 6)));

            await PruefeEineSollstellungMitZweiZahlungenAsync();
        }

        private async Task PruefeEineSollstellungMitZweiZahlungenAsync()
        {
            await using var pruef = _db.NeuerKontext();
            var soll = await SollstellungenAsync(pruef);
            soll.Should().ContainSingle(z => z.Buchungssatz.Buchungsdatum.Month == 1);
            var januar = soll.Single(z => z.Buchungssatz.Buchungsdatum.Month == 1);
            januar.AlsSollZeile.Should().HaveCount(2);
            Sollstellung.Bezahlt(januar).Should().Be(500m);
            (await pruef.Transaktionen.CountAsync()).Should().BeGreaterThanOrEqualTo(2);
            (await pruef.OffenePostenAusgleiche.CountAsync()).Should().Be(2);
            await PruefeBuchungsnummernAsync(pruef);
        }

        [PostgresFact]
        public async Task T4_DieDatenbankSperreHaeltEinenZweitenKontextAuf()
        {
            var aHatSperre = new TaskCompletionSource();
            var aFreigeben = new TaskCompletionSource();
            await using var ctxA = _db.NeuerKontext();
            await using var ctxB = _db.NeuerKontext();

            var a = Task.Run(() => Sollstellung.GesperrtAsync(ctxA, async () =>
            {
                aHatSperre.SetResult();
                await aFreigeben.Task;
                return 1;
            }));
            await aHatSperre.Task.WaitAsync(Frist);

            var bGestartet = new TaskCompletionSource();
            var b = Task.Run(() =>
            {
                bGestartet.SetResult();
                return Sollstellung.GesperrtAsync(ctxB, () => Task.FromResult(2));
            });
            await bGestartet.Task.WaitAsync(Frist);

            var frueh = await Task.WhenAny(b, Task.Delay(TimeSpan.FromMilliseconds(300)));
            frueh.Should().NotBe(b, "A hält die Datenbank-Sperre, B muss warten");

            aFreigeben.SetResult();
            (await a.WaitAsync(Frist)).Should().Be(1);
            (await b.WaitAsync(Frist)).Should().Be(2);
        }
    }
}
