// Copyright (c) 2023-2026 Henrik S. Gaßmann, Kai Lawrence
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

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.InitiateTestDbs.Templates
{
    internal sealed class GenericDatabase
    {
        private static DateOnly GlobalToday => DateOnly.FromDateTime(DateTime.Today);
        private const string AdminDevUser = "admin.dev";
        private const string OwnerDevUser = "owner.dev";
        private const string ManagerDevUser = "manager.dev";
        private const string ViewerDevUser = "viewer.dev";
        private const string LimitedDevUser = "limited.dev";
        private static readonly string[] MaintenanceTopics =
        [
            "Wartung Heiztherme",
            "Fensterreparatur",
            "Tueranlage Instandsetzung",
            "Dachreparatur",
            "Treppenhausanstrich",
            "Abflussreinigung",
            "Elektroinstallation",
            "Kellerabdichtung",
            "Schliessanlage",
            "Gegensprechanlage"
        ];

        public static async Task ConnectAndPopulate(
            string databaseHost,
            string databasePort,
            string databaseName,
            string databaseUser,
            string databasePass,
            int targetWohnungen,
            int? seed = null)
        {
            var ctx = await ConnectDatabase(
                databaseHost,
                databasePort,
                databaseName,
                databaseUser,
                databasePass);

            await PopulateDatabase(ctx, databaseUser, databasePass, targetWohnungen, seed ?? 1337);
            await ctx.DisposeAsync();
        }

        public static async Task<SaverwalterContext> ConnectDatabase(
            string databaseHost,
            string databasePort,
            string databaseName,
            string databaseUser,
            string databasePass)
        {
            var options = CreateContextOptions(databaseHost, databasePort, databaseName, databaseUser, databasePass);

            Console.WriteLine($"Erstelle Datenbank {databaseName}");
            var ctx = new SaverwalterContext(options);
            await ctx.Database.EnsureDeletedAsync();
            await ctx.Database.EnsureCreatedAsync();

            return ctx;
        }

        public static async Task EnsureDevelopmentUsers(
            string databaseHost,
            string databasePort,
            string databaseName,
            string databaseUser,
            string databasePass,
            string userPassword)
        {
            await using var ctx = ConnectExistingDatabase(databaseHost, databasePort, databaseName, databaseUser, databasePass);
            var users = EnsureDevelopmentUsersAndAssignments(ctx, databaseUser, userPassword);
            var wohnungen = await ctx.Wohnungen
                .Include(w => w.Verwalter)
                    .ThenInclude(v => v.UserAccount)
                .OrderBy(w => w.WohnungId)
                .ToListAsync();
            if (wohnungen.Count > 0)
            {
                AssignVerwalterPermissions(wohnungen, users, databaseUser);
            }
            await ctx.SaveChangesAsync();
        }

        public static async Task PrintAccessOverview(
            string databaseHost,
            string databasePort,
            string databaseName,
            string databaseUser,
            string databasePass,
            string userPassword)
        {
            await using var ctx = ConnectExistingDatabase(databaseHost, databasePort, databaseName, databaseUser, databasePass);

            var users = await ctx.UserAccounts
                .Include(u => u.Verwalter)
                    .ThenInclude(v => v.Wohnung)
                        .ThenInclude(w => w.Adresse)
                .OrderByDescending(u => u.Role)
                .ThenBy(u => u.Username)
                .ToListAsync();

            var totalWohnungen = await ctx.Wohnungen.CountAsync();

            Console.WriteLine();
            Console.WriteLine($"=== Access Overview for {databaseName} ===");
            Console.WriteLine($"Known dev password for seeded users: {userPassword}");
            Console.WriteLine($"Total Wohnungen: {totalWohnungen}");

            foreach (var user in users)
            {
                if (user.Role == UserRole.Admin)
                {
                    Console.WriteLine($"- {user.Username} [{user.Role}] -> sees all Wohnungen ({totalWohnungen}), full access");
                    continue;
                }

                var roleAssignments = user.Verwalter
                    .GroupBy(v => v.Rolle)
                    .ToDictionary(g => g.Key, g => g.Count());

                var readCount = user.Verwalter.Select(v => v.Wohnung.WohnungId).Distinct().Count();
                var writeCount = user.Verwalter
                    .Where(v => v.Rolle == VerwalterRolle.Vollmacht || v.Rolle == VerwalterRolle.Eigentuemer)
                    .Select(v => v.Wohnung.WohnungId)
                    .Distinct()
                    .Count();
                var ownerCount = user.Verwalter
                    .Where(v => v.Rolle == VerwalterRolle.Eigentuemer)
                    .Select(v => v.Wohnung.WohnungId)
                    .Distinct()
                    .Count();

                var noAccessCount = Math.Max(0, totalWohnungen - readCount);
                var sample = user.Verwalter
                    .Select(v => v.Wohnung)
                    .Distinct()
                    .OrderBy(w => w.WohnungId)
                    .Take(3)
                    .Select(w => $"{w.WohnungId}:{w.Bezeichnung} ({w.Adresse?.Anschrift ?? "ohne Adresse"})")
                    .ToList();

                var keine = roleAssignments.GetValueOrDefault(VerwalterRolle.Keine, 0);
                var vollmacht = roleAssignments.GetValueOrDefault(VerwalterRolle.Vollmacht, 0);
                var eigentuemer = roleAssignments.GetValueOrDefault(VerwalterRolle.Eigentuemer, 0);

                Console.WriteLine($"- {user.Username} [{user.Role}] -> read:{readCount}, write:{writeCount}, owner:{ownerCount}, no-access:{noAccessCount} | assignments K:{keine} V:{vollmacht} E:{eigentuemer}");
                if (sample.Count > 0)
                {
                    Console.WriteLine($"  sample Wohnungen: {string.Join(" | ", sample)}");
                }
            }

            Console.WriteLine("========================================");
            Console.WriteLine();
        }

        private static DbContextOptions<SaverwalterContext> CreateContextOptions(
            string databaseHost,
            string databasePort,
            string databaseName,
            string databaseUser,
            string databasePass)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseNpgsql(
                $@"Server={databaseHost}
            ;Port={databasePort}
            ;Database={databaseName}
            ;Username={databaseUser}
            ;Password={databasePass}");
            return optionsBuilder.Options;
        }

        private static SaverwalterContext ConnectExistingDatabase(
            string databaseHost,
            string databasePort,
            string databaseName,
            string databaseUser,
            string databasePass)
        {
            var options = CreateContextOptions(databaseHost, databasePort, databaseName, databaseUser, databasePass);
            return new SaverwalterContext(options);
        }

        public static async Task PopulateDatabase(
            SaverwalterContext ctx,
            string databaseUser,
            string databasePass,
            int targetWohnungen,
            int seed)
        {
            if (targetWohnungen < 10)
            {
                throw new ArgumentOutOfRangeException(nameof(targetWohnungen), "targetWohnungen must be >= 10.");
            }

            var random = new Random(seed);

            GenericData.FillUmlagetypen(ctx);

            var devUsers = EnsureDevelopmentUsersAndAssignments(ctx, databaseUser, databasePass);
            var adressen = FillAdressen(ctx, targetWohnungen);
            var eigentuemer = FillEigentuemer(ctx, adressen, targetWohnungen, random);
            var wohnungen = FillWohnungen(ctx, adressen, eigentuemer, targetWohnungen, random);
            AssignVerwalterPermissions(wohnungen, devUsers, databaseUser);
            var vertraege = FillVertraege(ctx, wohnungen, random);

            FillVertragversionen(ctx, vertraege, random);
            FillMieterSet(ctx, vertraege, random);
            FillMieten(ctx, vertraege, random);
            FillErhaltungsaufwendungen(ctx, wohnungen, random);
            FillMietminderungen(ctx, vertraege, random);
            FillKontos(ctx, random);
            FillGaragen(ctx, wohnungen, vertraege, random);

            var umlagen = FillUmlagen(ctx, adressen, random);
            FillZaehlerSet(ctx, umlagen);
            FillZaehlerstaende(ctx, vertraege, random);
            FillBetriebskostenrechnungen(ctx, umlagen, random);

            Console.WriteLine("Lade erzeugte Daten in Datenbank...");
            await ctx.SaveChangesAsync();
            Console.WriteLine("Fertig!");
        }

        private static Dictionary<string, UserAccount> EnsureDevelopmentUsersAndAssignments(
            SaverwalterContext ctx,
            string primaryUsername,
            string password)
        {
            Console.WriteLine("Prüfe/erstelle Development-Nutzer...");

            var users = new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase)
            {
                [primaryUsername] = EnsureUserAccount(ctx, primaryUsername, "Primary Dev User", UserRole.User, password),
                [AdminDevUser] = EnsureUserAccount(ctx, AdminDevUser, "Admin Test User", UserRole.Admin, password),
                [OwnerDevUser] = EnsureUserAccount(ctx, OwnerDevUser, "Owner Test User", UserRole.Owner, password),
                [ManagerDevUser] = EnsureUserAccount(ctx, ManagerDevUser, "Manager Test User", UserRole.User, password),
                [ViewerDevUser] = EnsureUserAccount(ctx, ViewerDevUser, "Viewer Test User", UserRole.User, password),
                [LimitedDevUser] = EnsureUserAccount(ctx, LimitedDevUser, "Limited Test User", UserRole.Guest, password),
            };

            Console.WriteLine($"{users.Count} Dev-Nutzer bereit. Passwort für seeded Nutzer: {password}");
            return users;
        }

        private static UserAccount EnsureUserAccount(
            SaverwalterContext ctx,
            string username,
            string displayName,
            UserRole role,
            string password)
        {
            var account = ctx.UserAccounts
                .Include(u => u.Pbkdf2PasswordCredential)
                .FirstOrDefault(u => u.Username == username);

            if (account == null)
            {
                account = new UserAccount
                {
                    Username = username,
                    Name = displayName,
                    Role = role,
                    Email = $"{username}@dev.local"
                };
                ctx.UserAccounts.Add(account);
            }
            else
            {
                account.Name = displayName;
                account.Role = role;
                account.Email ??= $"{username}@dev.local";
            }

            if (account.Pbkdf2PasswordCredential == null)
            {
                var credential = new Pbkdf2PasswordCredential
                {
                    User = account,
                    Iterations = 210000,
                    Salt = RandomNumberGenerator.GetBytes(32)
                };

                var utf8Password = Encoding.UTF8.GetBytes(password);
                credential.PasswordHash = Rfc2898DeriveBytes.Pbkdf2(
                    utf8Password,
                    credential.Salt,
                    credential.Iterations,
                    HashAlgorithmName.SHA512,
                    64);

                account.Pbkdf2PasswordCredential = credential;
                ctx.Pbkdf2PasswordCredentials.Add(credential);
            }

            return account;
        }

        private static List<Adresse> FillAdressen(SaverwalterContext ctx, int targetWohnungen)
        {
            Console.Write("Füge Adressen hinzu: ");

            var adressen = new List<Adresse>();
            var gebaeudeCount = Math.Max(12, targetWohnungen / 3);

            for (var i = 0; i < gebaeudeCount; i++)
            {
                var strasse = GenericData.strasseList[i * 7 % GenericData.strasseList.Count];
                var hausnummer = (2 + (i * 3)).ToString(CultureInfo.InvariantCulture);
                var postleitzahl = GenericData.postleitzahlList[i * 5 % GenericData.postleitzahlList.Count];
                var stadt = GenericData.stadtList[i * 11 % GenericData.stadtList.Count];
                adressen.Add(new Adresse(strasse, hausnummer, postleitzahl, stadt));
            }

            ctx.Adressen.AddRange(adressen);
            Console.WriteLine($"{adressen.Count} Adressen hinzugefügt");
            return adressen;
        }

        private static List<Kontakt> FillEigentuemer(
            SaverwalterContext ctx,
            List<Adresse> adressen,
            int targetWohnungen,
            Random random)
        {
            Console.Write("Füge Eigentümer hinzu: ");

            var eigentuemer = new List<Kontakt>();
            var eigentuemerCount = Math.Max(8, targetWohnungen / 10);

            for (var i = 0; i < eigentuemerCount; i++)
            {
                Kontakt kontakt;
                if (random.NextDouble() < 0.7)
                {
                    kontakt = GenerateJuristischePerson(i, random);
                }
                else
                {
                    kontakt = GenerateNatuerlichePerson(i, random);
                }

                kontakt.Adresse = adressen[i % adressen.Count];
                eigentuemer.Add(kontakt);
            }

            ctx.Kontakte.AddRange(eigentuemer);
            Console.WriteLine($"{eigentuemer.Count} Eigentümer hinzugefügt");
            return eigentuemer;
        }

        private static List<Wohnung> FillWohnungen(
            SaverwalterContext ctx,
            List<Adresse> adressen,
            List<Kontakt> eigentuemer,
            int targetWohnungen,
            Random random)
        {
            Console.Write("Füge Wohnungen hinzu: ");

            var wohnungen = new List<Wohnung>();

            var adresseIndex = 0;
            while (wohnungen.Count < targetWohnungen)
            {
                var adresse = adressen[adresseIndex % adressen.Count];
                var owner = eigentuemer[(adresseIndex + random.Next(eigentuemer.Count)) % eigentuemer.Count];
                var unitsInBuilding = 2 + random.Next(5);

                for (var unit = 1; unit <= unitsInBuilding && wohnungen.Count < targetWohnungen; unit++)
                {
                    var flaeche = 28 + random.Next(115);
                    var bezeichnung = $"Wohnung {unit}";

                    wohnungen.Add(new Wohnung(bezeichnung, flaeche, flaeche, flaeche, 1)
                    {
                        Adresse = adresse,
                        Besitzer = owner
                    });
                }

                adresseIndex++;
            }

            ctx.Wohnungen.AddRange(wohnungen);
            Console.WriteLine($"{wohnungen.Count} Wohnungen hinzugefügt");
            return wohnungen;
        }

        private static void AssignVerwalterPermissions(
            List<Wohnung> wohnungen,
            Dictionary<string, UserAccount> users,
            string primaryUsername)
        {
            Console.Write("Verteile Verwalter-Rollen: ");

            var primary = users[primaryUsername];
            var admin = users[AdminDevUser];
            var owner = users[OwnerDevUser];
            var manager = users[ManagerDevUser];
            var viewer = users[ViewerDevUser];
            var limited = users[LimitedDevUser];

            for (var i = 0; i < wohnungen.Count; i++)
            {
                var wohnung = wohnungen[i];

                EnsureVerwalter(wohnung, primary, VerwalterRolle.Keine);

                if (i % 2 == 0)
                {
                    EnsureVerwalter(wohnung, manager, VerwalterRolle.Vollmacht);
                }

                if (i < Math.Max(2, wohnungen.Count / 3))
                {
                    EnsureVerwalter(wohnung, owner, VerwalterRolle.Eigentuemer);
                }

                if (i % 4 == 0)
                {
                    EnsureVerwalter(wohnung, viewer, VerwalterRolle.Keine);
                }

                if (i < Math.Max(1, wohnungen.Count / 10))
                {
                    EnsureVerwalter(wohnung, limited, VerwalterRolle.Keine);
                }
            }

            // Admin role itself gives global access, but one sample assignment helps when debugging mappings.
            EnsureVerwalter(wohnungen.First(), admin, VerwalterRolle.Eigentuemer);

            Console.WriteLine("Rollen verteilt");
        }

        private static void EnsureVerwalter(Wohnung wohnung, UserAccount account, VerwalterRolle rolle)
        {
            var existing = wohnung.Verwalter.FirstOrDefault(v => v.UserAccount.Username == account.Username);
            if (existing == null)
            {
                wohnung.Verwalter.Add(new Verwalter(rolle) { UserAccount = account });
                return;
            }

            if (existing.Rolle < rolle)
            {
                existing.Rolle = rolle;
            }
        }

        private static List<Vertrag> FillVertraege(SaverwalterContext ctx, List<Wohnung> wohnungen, Random random)
        {
            Console.Write("Füge Verträge hinzu: ");

            var vertraege = new List<Vertrag>();

            foreach (var wohnung in wohnungen)
            {
                var vertragCount = random.NextDouble() switch
                {
                    < 0.15 => 3,
                    < 0.45 => 2,
                    _ => 1
                };

                var contractEnd = GlobalToday.AddMonths(-random.Next(6, 100));
                for (var i = 0; i < vertragCount; i++)
                {
                    var durationMonths = random.Next(12, 72);
                    var contractStart = contractEnd.AddMonths(-durationMonths + 1);
                    var isLatest = i == vertragCount - 1;
                    var isActive = isLatest && random.NextDouble() < 0.65;

                    var vertrag = new Vertrag
                    {
                        Ansprechpartner = wohnung.Besitzer,
                        Ende = isActive ? null : contractEnd,
                        Wohnung = wohnung,
                        Notiz = $"Laufzeit: {contractStart:yyyy-MM} bis {(isActive ? "offen" : contractEnd.ToString("yyyy-MM", CultureInfo.InvariantCulture))}"
                    };

                    vertraege.Add(vertrag);
                    contractEnd = contractStart.AddDays(-1);
                }
            }

            ctx.Vertraege.AddRange(vertraege);
            Console.WriteLine($"{vertraege.Count} Verträge hinzugefügt");
            return vertraege;
        }

        private static List<VertragVersion> FillVertragversionen(SaverwalterContext ctx, List<Vertrag> vertraege, Random random)
        {
            Console.Write("Füge Vertragversionen hinzu: ");

            var vertragVersionen = new List<VertragVersion>();

            foreach (var vertrag in vertraege)
            {
                var ende = vertrag.Ende ?? GlobalToday;
                var totalMonths = random.Next(12, 72);
                var beginn = ende.AddMonths(-totalMonths + 1);
                var versions = random.NextDouble() switch
                {
                    < 0.6 => 1,
                    < 0.9 => 2,
                    _ => 3
                };

                var starts = BuildVersionStarts(beginn, ende, versions, random);
                var baseRentPerSqm = 8.5 + random.NextDouble() * 7.0;

                for (var i = 0; i < starts.Count; i++)
                {
                    var raiseFactor = 1.0 + (i * (0.02 + random.NextDouble() * 0.03));
                    var grundmiete = Math.Round(vertrag.Wohnung.Wohnflaeche * baseRentPerSqm * raiseFactor, 2);
                    var personenzahl = 1 + random.Next(4);

                    vertragVersionen.Add(new VertragVersion(starts[i], grundmiete, personenzahl)
                    {
                        Vertrag = vertrag
                    });
                }
            }

            ctx.VertragVersionen.AddRange(vertragVersionen);
            Console.WriteLine($"{vertragVersionen.Count} Vertragversionen hinzugefügt");
            return vertragVersionen;
        }

        private static List<Kontakt> FillMieterSet(SaverwalterContext ctx, List<Vertrag> vertraege, Random random)
        {
            Console.Write("Füge Mieter hinzu: ");

            var mieter = new List<Kontakt>();
            var seed = 10_000;

            foreach (var vertrag in vertraege)
            {
                var anzahl = random.NextDouble() switch
                {
                    < 0.45 => 1,
                    < 0.8 => 2,
                    < 0.95 => 3,
                    _ => 4
                };

                for (var i = 0; i < anzahl; i++)
                {
                    var person = GenerateNatuerlichePerson(seed++, random);
                    person.Adresse = vertrag.Wohnung.Adresse;
                    person.Mietvertraege.Add(vertrag);
                    mieter.Add(person);
                }
            }

            ctx.Kontakte.AddRange(mieter);
            Console.WriteLine($"{mieter.Count} Mieter hinzugefügt");
            return mieter;
        }

        private static List<Miete> FillMieten(SaverwalterContext ctx, List<Vertrag> vertraege, Random random)
        {
            Console.Write("Füge Mieten hinzu: ");

            var mieten = new List<Miete>();

            foreach (var vertrag in vertraege)
            {
                foreach (var version in vertrag.Versionen.OrderBy(v => v.Beginn))
                {
                    var ende = version.Ende() ?? GlobalToday;
                    for (var date = version.Beginn; date <= ende; date = date.AddMonths(1))
                    {
                        var nebenkosten = 130 + (version.Personenzahl * 25) + random.Next(90);
                        var gesamt = Math.Round(version.Grundmiete + nebenkosten, 2);
                        mieten.Add(new Miete(date, date, gesamt) { Vertrag = vertrag });
                    }
                }
            }

            ctx.Mieten.AddRange(mieten);
            Console.WriteLine($"{mieten.Count} Mieten hinzugefügt");
            return mieten;
        }

        private static List<Erhaltungsaufwendung> FillErhaltungsaufwendungen(
            SaverwalterContext ctx,
            List<Wohnung> wohnungen,
            Random random)
        {
            Console.Write("Füge Erhaltungsaufwendungen hinzu: ");

            var invoices = new List<Erhaltungsaufwendung>();
            var aussteller = new List<Kontakt>();

            for (var i = 0; i < 12; i++)
            {
                var kontakt = GenerateJuristischePerson(50_000 + i, random);
                kontakt.Notiz = "Dienstleister";
                aussteller.Add(kontakt);
            }

            ctx.Kontakte.AddRange(aussteller);

            var invoiceCount = Math.Max(wohnungen.Count * 3, 80);
            for (var i = 0; i < invoiceCount; i++)
            {
                var wohnung = wohnungen[random.Next(wohnungen.Count)];
                var contractor = aussteller[random.Next(aussteller.Count)];
                var topic = MaintenanceTopics[random.Next(MaintenanceTopics.Length)];
                var betrag = CreateMaintenanceAmount(random);
                var date = RandomDate(random, GlobalToday.AddYears(-6), GlobalToday);

                invoices.Add(new Erhaltungsaufwendung(betrag, $"{topic} #{i + 1:0000}", date)
                {
                    Aussteller = contractor,
                    Wohnung = wohnung
                });
            }

            ctx.Erhaltungsaufwendungen.AddRange(invoices);
            Console.WriteLine($"{invoices.Count} Erhaltungsaufwendungen hinzugefügt");
            return invoices;
        }

        private static List<Mietminderung> FillMietminderungen(SaverwalterContext ctx, List<Vertrag> vertraege, Random random)
        {
            Console.Write("Füge Mietminderungen hinzu: ");

            var mietminderungen = new List<Mietminderung>();

            foreach (var vertrag in vertraege)
            {
                if (random.NextDouble() >= 0.12)
                {
                    continue;
                }

                var vertragBeginn = vertrag.Beginn();
                if (vertragBeginn == default)
                {
                    continue;
                }

                var vertragEnde = vertrag.Ende ?? GlobalToday;
                if (vertragBeginn >= vertragEnde)
                {
                    continue;
                }

                var beginn = RandomDate(random, vertragBeginn, vertragEnde.AddMonths(-1));
                var dauerMonate = 1 + random.Next(3);
                var ende = beginn.AddMonths(dauerMonate).AddDays(-1);
                var minderung = 10 + random.Next(30);

                mietminderungen.Add(new Mietminderung(beginn, minderung)
                {
                    Vertrag = vertrag,
                    Ende = ende <= vertragEnde ? ende : vertragEnde,
                    Notiz = "Temporäre Mietminderung wegen Mangel"
                });
            }

            ctx.Mietminderungen.AddRange(mietminderungen);
            Console.WriteLine($"{mietminderungen.Count} Mietminderungen hinzugefügt");
            return mietminderungen;
        }

        private static List<Konto> FillKontos(SaverwalterContext ctx, Random random)
        {
            Console.Write("Füge Kontos hinzu: ");

            var kontos = new List<Konto>();
            var banken = new[] { "Sparkasse", "Deutsche Bank", "Commerzbank", "Volksbank", "DKB", "ING" };

            foreach (var kontakt in ctx.Kontakte)
            {
                var createAccount = kontakt.Rechtsform != Rechtsform.natuerlich || random.NextDouble() < 0.6;
                if (!createAccount)
                {
                    continue;
                }

                kontos.Add(new Konto(
                    banken[random.Next(banken.Length)],
                    CreateGermanIban(random))
                {
                    Besitzer = kontakt,
                    Notiz = "Dev-Testkonto"
                });
            }

            ctx.Kontos.AddRange(kontos);
            Console.WriteLine($"{kontos.Count} Kontos hinzugefügt");
            return kontos;
        }

        private static List<Garage> FillGaragen(
            SaverwalterContext ctx,
            List<Wohnung> wohnungen,
            List<Vertrag> vertraege,
            Random random)
        {
            Console.Write("Füge Garagen hinzu: ");

            var garagen = new List<Garage>();

            foreach (var wohnung in wohnungen)
            {
                if (random.NextDouble() >= 0.22)
                {
                    continue;
                }

                var garage = new Garage($"G-{wohnung.WohnungId:0000}")
                {
                    Besitzer = wohnung.Besitzer!,
                    Adresse = wohnung.Adresse,
                    Notiz = "Stellplatz im Hinterhof"
                };

                var activeContract = vertraege.FirstOrDefault(v => v.Wohnung == wohnung && (v.Ende == null || v.Ende >= GlobalToday));
                if (activeContract != null && random.NextDouble() < 0.55)
                {
                    garage.Vertraege.Add(activeContract);
                }

                garagen.Add(garage);
            }

            ctx.Garagen.AddRange(garagen);
            Console.WriteLine($"{garagen.Count} Garagen hinzugefügt");
            return garagen;
        }

        private static List<Umlage> FillUmlagen(SaverwalterContext ctx, List<Adresse> adressen, Random random)
        {
            Console.Write("Füge Umlagen hinzu: ");

            var umlagen = new List<Umlage>();
            var typen = ctx.Umlagetypen.ToDictionary(t => t.Bezeichnung);

            foreach (var adresse in adressen.Where(a => a.Wohnungen.Count > 0))
            {
                umlagen.Add(AddUmlage(adresse, typen["Allgemeinstrom/Hausbeleuchtung"], Umlageschluessel.NachWohnflaeche));
                umlagen.Add(AddUmlage(adresse, typen["Dachrinnenreinigung"], Umlageschluessel.NachWohnflaeche));
                umlagen.Add(AddUmlage(adresse, typen["Grundsteuer"], Umlageschluessel.NachWohnflaeche));
                umlagen.Add(AddUmlage(adresse, typen["Müllbeseitigung"], Umlageschluessel.NachPersonenzahl));
                umlagen.Add(AddUmlage(adresse, typen["Wasserversorgung"], Umlageschluessel.NachVerbrauch));
                umlagen.Add(AddUmlage(adresse, typen["Entwässerung/Schmutzwasser"], Umlageschluessel.NachVerbrauch));
                umlagen.Add(AddUmlage(adresse, typen["Strassenreinigung"], Umlageschluessel.NachWohnflaeche));

                if (random.NextDouble() < 0.35)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Breitbandkabelanschluss"], Umlageschluessel.NachNutzeinheit));
                }
                if (random.NextDouble() < 0.7)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Gartenpflege"], Umlageschluessel.NachWohnflaeche));
                }
                if (random.NextDouble() < 0.85)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Sachversicherung"], Umlageschluessel.NachWohnflaeche));
                }
                if (random.NextDouble() < 0.45)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Haftpflichtversicherung"], Umlageschluessel.NachWohnflaeche));
                }
                if (adresse.Wohnungen.Count >= 3 || random.NextDouble() < 0.35)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Heizkosten"], Umlageschluessel.NachVerbrauch));
                    umlagen.Add(AddUmlage(adresse, typen["Wartung Therme/Speicher"], Umlageschluessel.NachWohnflaeche));
                }
            }

            ctx.Umlagen.AddRange(umlagen);
            Console.WriteLine($"{umlagen.Count} Umlagen hinzugefügt");
            return umlagen;
        }

        private static List<Zaehler> FillZaehlerSet(SaverwalterContext ctx, List<Umlage> umlagen)
        {
            Console.Write("Füge Zähler hinzu: ");

            var zaehler = new List<Zaehler>();
            var seen = new HashSet<string>();

            foreach (var umlage in umlagen)
            {
                if (umlage.Typ.Bezeichnung == "Wasserversorgung")
                {
                    foreach (var wohnung in umlage.Wohnungen)
                    {
                        AddMeter(zaehler, seen, $"KW-{wohnung.WohnungId}", Zaehlertyp.Kaltwasser, wohnung);
                        AddMeter(zaehler, seen, $"WW-{wohnung.WohnungId}", Zaehlertyp.Warmwasser, wohnung);
                    }
                }

                if (umlage.Typ.Bezeichnung == "Heizkosten")
                {
                    var hausId = umlage.Wohnungen.FirstOrDefault()?.Adresse?.AdresseId ?? 0;
                    AddMeter(zaehler, seen, $"HZ-HAUS-{hausId}", Zaehlertyp.Gas, null);
                    foreach (var wohnung in umlage.Wohnungen)
                    {
                        AddMeter(zaehler, seen, $"HZ-{wohnung.WohnungId}", Zaehlertyp.Gas, wohnung);
                    }
                }
            }

            ctx.ZaehlerSet.AddRange(zaehler);
            Console.WriteLine($"{zaehler.Count} Zähler hinzugefügt");
            return zaehler;
        }

        private static List<Zaehlerstand> FillZaehlerstaende(SaverwalterContext ctx, List<Vertrag> vertraege, Random random)
        {
            Console.Write("Füge Zählerstände hinzu: ");

            var zaehlerstaende = new List<Zaehlerstand>();
            var earliestByWohnung = vertraege
                .Where(v => v.Wohnung != null)
                .GroupBy(v => v.Wohnung.WohnungId)
                .ToDictionary(g => g.Key, g => g.Min(v => v.Beginn()));

            foreach (var zaehler in ctx.ZaehlerSet)
            {
                var start = GlobalToday.AddYears(-6);
                if (zaehler.Wohnung != null && earliestByWohnung.TryGetValue(zaehler.Wohnung.WohnungId, out var wohnungStart) && wohnungStart != default)
                {
                    start = wohnungStart;
                }

                var stand = 0d;
                for (var date = new DateOnly(start.Year, 1, 1); date <= GlobalToday; date = date.AddYears(1))
                {
                    stand += AnnualConsumption(zaehler.Typ, random, zaehler.Wohnung?.Wohnflaeche ?? 65);
                    zaehlerstaende.Add(new Zaehlerstand(date, Math.Round(stand, 2)) { Zaehler = zaehler });
                }
            }

            ctx.Zaehlerstaende.AddRange(zaehlerstaende);
            Console.WriteLine($"{zaehlerstaende.Count} Zählerstände hinzugefügt");
            return zaehlerstaende;
        }

        private static List<Betriebskostenrechnung> FillBetriebskostenrechnungen(
            SaverwalterContext ctx,
            List<Umlage> umlagen,
            Random random)
        {
            Console.Write("Füge Betriebskostenrechnung hinzu: ");

            var betriebskostenrechnungen = new List<Betriebskostenrechnung>();

            foreach (var umlage in umlagen)
            {
                var beginn = GetEarliestDate(umlage.Wohnungen.ToList());
                if (beginn == default)
                {
                    beginn = GlobalToday.AddYears(-3);
                }

                for (var year = beginn.Year; year < GlobalToday.Year; year++)
                {
                    var wohnflaeche = Math.Max(umlage.Wohnungen.Sum(w => w.Wohnflaeche), 40);
                    var basisbetrag = umlage.Typ.Bezeichnung switch
                    {
                        "Heizkosten" => wohnflaeche * (9 + random.NextDouble() * 4),
                        "Grundsteuer" => wohnflaeche * (1.2 + random.NextDouble() * 0.4),
                        "Wasserversorgung" => wohnflaeche * (1.8 + random.NextDouble() * 0.8),
                        _ => wohnflaeche * (0.8 + random.NextDouble() * 1.5)
                    };

                    var annualFactor = 1 + ((year - beginn.Year) * (0.01 + random.NextDouble() * 0.015));
                    var betrag = Math.Round(basisbetrag * annualFactor, 2);
                    betriebskostenrechnungen.Add(new Betriebskostenrechnung(betrag, new DateOnly(year, 12, 31), year)
                    {
                        Umlage = umlage
                    });
                }
            }

            ctx.Betriebskostenrechnungen.AddRange(betriebskostenrechnungen);
            Console.WriteLine($"{betriebskostenrechnungen.Count} Betriebskostenrechnungen hinzugefügt");
            return betriebskostenrechnungen;
        }

        private static DateOnly GetEarliestDate(List<Wohnung> wohnungen)
        {
            DateOnly? earliest = null;
            foreach (var wohnung in wohnungen)
            {
                foreach (var vertrag in wohnung.Vertraege)
                {
                    var beginn = vertrag.Beginn();
                    if (beginn == default)
                    {
                        continue;
                    }

                    earliest = earliest == null || beginn < earliest ? beginn : earliest;
                }
            }

            return earliest ?? default;
        }

        private static Umlage AddUmlage(Adresse adresse, Umlagetyp typ, Umlageschluessel schluessel)
        {
            return new Umlage(schluessel)
            {
                Typ = typ,
                Beschreibung = $"{typ.Bezeichnung} für {adresse.Anschrift}",
                Wohnungen = adresse.Wohnungen
            };
        }

        private static void AddMeter(List<Zaehler> set, HashSet<string> seen, string key, Zaehlertyp typ, Wohnung? wohnung)
        {
            if (!seen.Add(key))
            {
                return;
            }

            set.Add(new Zaehler(key, typ)
            {
                Wohnung = wohnung
            });
        }

        private static Kontakt GenerateNatuerlichePerson(int seed, Random random)
        {
            var useMale = random.NextDouble() < 0.5;
            var vorname = useMale
                ? GenericData.FirstNamesMale[seed * 17 % GenericData.FirstNamesMale.Count]
                : GenericData.FirstNamesFemale[seed * 19 % GenericData.FirstNamesFemale.Count];
            var nachname = GenericData.lastNames[seed * 13 % GenericData.lastNames.Count];
            var provider = GenericData.emailProvider[seed * 5 % GenericData.emailProvider.Count];

            return new Kontakt(nachname, Rechtsform.natuerlich)
            {
                Vorname = vorname,
                Anrede = useMale ? Anrede.Herr : Anrede.Frau,
                Telefon = GenericData.telefonnummerList[seed * 7 % GenericData.telefonnummerList.Count],
                Mobil = GenericData.mobilePhoneNumbers[seed * 11 % GenericData.mobilePhoneNumbers.Count],
                Fax = GenericData.telefonnummerList[seed * 23 % GenericData.telefonnummerList.Count],
                Email = $"{Slug(vorname)}.{Slug(nachname)}@{provider}",
            };
        }

        private static Kontakt GenerateJuristischePerson(int seed, Random random)
        {
            var name = GenericData.companyNames[seed * 13 % GenericData.companyNames.Count];
            var provider = GenericData.companyEmails[seed * 3 % GenericData.companyEmails.Count].Split('@').Last();
            var rechtsform = random.NextDouble() switch
            {
                < 0.6 => Rechtsform.gmbh,
                < 0.75 => Rechtsform.gbr,
                < 0.9 => Rechtsform.ag,
                _ => Rechtsform.kg
            };

            return new Kontakt(name, rechtsform)
            {
                Email = $"kontakt@{Slug(name)}.{provider}",
                Telefon = GenericData.telefonnummerList[seed * 7 % GenericData.telefonnummerList.Count],
                Notiz = "Verwaltung/Unternehmen"
            };
        }

        private static List<DateOnly> BuildVersionStarts(DateOnly beginn, DateOnly ende, int count, Random random)
        {
            var starts = new List<DateOnly> { beginn };
            if (count == 1)
            {
                return starts;
            }

            var totalMonths = Math.Max(2, ((ende.Year - beginn.Year) * 12) + ende.Month - beginn.Month + 1);
            var last = beginn;

            for (var i = 1; i < count; i++)
            {
                var baseOffset = (i * totalMonths) / count;
                var jitter = random.Next(-2, 3);
                var candidate = beginn.AddMonths(Math.Min(totalMonths - 1, Math.Max(baseOffset + jitter, 1)));
                if (candidate <= last)
                {
                    candidate = last.AddMonths(1);
                }

                if (candidate >= ende)
                {
                    candidate = ende.AddMonths(-1);
                }

                starts.Add(candidate);
                last = candidate;
            }

            return starts.Distinct().OrderBy(d => d).ToList();
        }

        private static DateOnly RandomDate(Random random, DateOnly min, DateOnly max)
        {
            if (max <= min)
            {
                return min;
            }

            var delta = max.DayNumber - min.DayNumber;
            return min.AddDays(random.Next(delta + 1));
        }

        private static double CreateMaintenanceAmount(Random random)
        {
            var r = random.NextDouble();
            if (r < 0.7)
            {
                return Math.Round(120 + (random.NextDouble() * 900), 2);
            }

            if (r < 0.95)
            {
                return Math.Round(1000 + (random.NextDouble() * 4000), 2);
            }

            return Math.Round(5000 + (random.NextDouble() * 20000), 2);
        }

        private static string CreateGermanIban(Random random)
        {
            var sb = new StringBuilder("DE");
            for (var i = 0; i < 20; i++)
            {
                sb.Append(random.Next(10));
            }

            return sb.ToString();
        }

        private static double AnnualConsumption(Zaehlertyp zaehlertyp, Random random, double wohnflaeche)
        {
            return zaehlertyp switch
            {
                Zaehlertyp.Warmwasser => 20 + random.NextDouble() * 80,
                Zaehlertyp.Kaltwasser => 45 + random.NextDouble() * 120,
                Zaehlertyp.Strom => 1000 + random.NextDouble() * 2800,
                Zaehlertyp.Gas => Math.Max(40, wohnflaeche) * (45 + random.NextDouble() * 35),
                _ => 100 + random.NextDouble() * 400
            };
        }

        private static string Slug(string value)
        {
            return value
                .Trim()
                .ToLowerInvariant()
                .Replace("ä", "ae")
                .Replace("ö", "oe")
                .Replace("ü", "ue")
                .Replace("ß", "ss")
                .Replace(" ", "-")
                .Replace("&", "und")
                .Replace(".", string.Empty)
                .Replace(",", string.Empty)
                .Replace("/", "-")
                .Replace("_", "-");
        }
    }
}