// Copyright (c) 2023-2024 Kai Lawrence
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

using Deeplex.Saverwalter.InitiateTestDbs.Templates;

namespace Deeplex.Saverwalter.InitiateTestDbs
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var databaseUser = RequireEnv("DATABASE_USER");
            var databasePass = RequireEnv("DATABASE_PASS");
            var databaseHost = RequireEnv("DATABASE_HOST");
            var databasePort = RequireEnv("DATABASE_PORT");

            var printAccess = args.Contains("--print-access", StringComparer.OrdinalIgnoreCase);
            var ensureDevUsers = args.Contains("--ensure-dev-users", StringComparer.OrdinalIgnoreCase);
            var seedFiles = args.Contains("--seed-files", StringComparer.OrdinalIgnoreCase);
            var bucheHistorisch = args.Contains("--buche-historisch", StringComparer.OrdinalIgnoreCase);
            var vergleicheAbrechnung = args.Contains("--vergleiche-abrechnung", StringComparer.OrdinalIgnoreCase);
            var bucheAbrechnungArg = GetArgValue(args, "--buche-abrechnung");
            var seedDatabases = !printAccess && !ensureDevUsers && !seedFiles && !bucheHistorisch
                && bucheAbrechnungArg is null && !vergleicheAbrechnung;

            var targetDb = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "walter_dev_generic_db";
            var s3Provider = Environment.GetEnvironmentVariable("WALTER_DEV_S3_PROVIDER");

            if (seedDatabases)
            {
                var defaultWohnungen = GetIntEnv("WALTER_DEV_TARGET_WOHNUNGEN", 120);
                var fullWohnungen = GetIntEnv("WALTER_DEV_FULL_TARGET_WOHNUNGEN", 320);
                var seed = GetIntEnv("WALTER_DEV_RANDOM_SEED", 1337);

                await GenericDatabase.ConnectAndPopulate(
                    databaseHost,
                    databasePort,
                    "walter_dev_generic_db",
                    databaseUser,
                    databasePass,
                    defaultWohnungen,
                    seed);

                await GenericDatabase.ConnectAndPopulate(
                    databaseHost,
                    databasePort,
                    "walter_dev_full_generic_db",
                    databaseUser,
                    databasePass,
                    fullWohnungen,
                    seed + 1);

                Console.WriteLine($"Databases seeded: walter_dev_generic_db ({defaultWohnungen}), walter_dev_full_generic_db ({fullWohnungen})");

                await GenericDatabase.PrintAccessOverview(databaseHost, databasePort, "walter_dev_generic_db", databaseUser, databasePass, databasePass);
                await GenericDatabase.PrintAccessOverview(databaseHost, databasePort, "walter_dev_full_generic_db", databaseUser, databasePass, databasePass);

                if (!string.IsNullOrWhiteSpace(s3Provider))
                {
                    await SeedFilesFor(databaseHost, databasePort, "walter_dev_generic_db", databaseUser, databasePass, s3Provider, seed);
                    await SeedFilesFor(databaseHost, databasePort, "walter_dev_full_generic_db", databaseUser, databasePass, s3Provider, seed + 1);
                }
                else
                {
                    Console.WriteLine("WALTER_DEV_S3_PROVIDER not set - skipping file storage seeding.");
                }

                return;
            }

            if (ensureDevUsers)
            {
                await GenericDatabase.EnsureDevelopmentUsers(databaseHost, databasePort, targetDb, databaseUser, databasePass, databasePass);
            }

            if (printAccess)
            {
                await GenericDatabase.PrintAccessOverview(databaseHost, databasePort, targetDb, databaseUser, databasePass, databasePass);
            }

            if (bucheHistorisch)
            {
                await using var ctx = GenericDatabase.ConnectExistingDatabase(databaseHost, databasePort, targetDb, databaseUser, databasePass);
                await BuchungssaetzeErstellen.BucheHistorischAsync(ctx);
            }

            if (seedFiles)
            {
                if (string.IsNullOrWhiteSpace(s3Provider))
                {
                    Console.WriteLine("WALTER_DEV_S3_PROVIDER must be set when --seed-files is requested.");
                    Environment.ExitCode = 1;
                    return;
                }

                var seed = GetIntEnv("WALTER_DEV_RANDOM_SEED", 1337);
                await SeedFilesFor(databaseHost, databasePort, targetDb, databaseUser, databasePass, s3Provider, seed);
            }

            if (bucheAbrechnungArg is not null)
            {
                if (!int.TryParse(bucheAbrechnungArg, out var jahr) || jahr < 1900 || jahr > 2100)
                {
                    Console.WriteLine($"Ungültiges Jahr für --buche-abrechnung: '{bucheAbrechnungArg}'");
                    Environment.ExitCode = 1;
                    return;
                }

                await using var ctx = GenericDatabase.ConnectExistingDatabase(databaseHost, databasePort, targetDb, databaseUser, databasePass);
                await BkAbrechnungNeu.BucheJahresabrechnungAsync(ctx, jahr);
            }

            if (vergleicheAbrechnung)
            {
                await using var ctx = GenericDatabase.ConnectExistingDatabase(databaseHost, databasePort, targetDb, databaseUser, databasePass);
                await AbrechnungsVergleich.ErstelleVergleichsreportAsync(ctx);
            }
        }

        private static async Task SeedFilesFor(
            string databaseHost,
            string databasePort,
            string databaseName,
            string databaseUser,
            string databasePass,
            string s3Provider,
            int seed)
        {
            Console.WriteLine($"Seeding sample files for {databaseName} into {s3Provider}");
            await using var ctx = GenericDatabase.ConnectExistingDatabase(databaseHost, databasePort, databaseName, databaseUser, databasePass);
            await FileStorage.SeedSampleFiles(ctx, s3Provider, seed);
        }

        private static string RequireEnv(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing required environment variable {name}");
            }

            return value;
        }

        private static string? GetArgValue(string[] args, string flag)
        {
            var idx = Array.FindIndex(args, a => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0 && idx + 1 < args.Length && !args[idx + 1].StartsWith("--", StringComparison.Ordinal))
                return args[idx + 1];
            return null;
        }

        private static int GetIntEnv(string name, int defaultValue)
        {
            var raw = Environment.GetEnvironmentVariable(name);
            if (int.TryParse(raw, out var parsed) && parsed > 0)
            {
                return parsed;
            }

            return defaultValue;
        }
    }
}
