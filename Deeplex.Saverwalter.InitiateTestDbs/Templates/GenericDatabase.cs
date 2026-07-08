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

        public static SaverwalterContext ConnectExistingDatabase(
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
            var mieter = FillMieterSet(ctx, vertraege, random);
            FillErhaltungsaufwendungen(ctx, wohnungen, random);
            FillMietminderungen(ctx, vertraege, random);
            var (mieterBankkontos, vertragBankkontos, eigentuemerBankkontos) = FillBankkontos(ctx, eigentuemer, mieter, vertraege, random);
            var (_, garageVertraegeByVertrag) = FillGaragen(ctx, wohnungen, vertraege, mieter, random);
            FillTransaktionen(ctx, vertraege, mieterBankkontos, vertragBankkontos, garageVertraegeByVertrag, random);

            var umlagen = FillUmlagen(ctx, adressen, random);
            var hauszaehlerHeizung = FillZaehlerSet(ctx, umlagen);
            FillZaehlerstaende(ctx, vertraege, random);
            FillHKVO(ctx, umlagen, hauszaehlerHeizung);
            FillBetriebskostenrechnungen(ctx, umlagen, eigentuemerBankkontos, random);
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

                    var wIdx = wohnungen.Count;
                    var w = new Wohnung(bezeichnung)
                    {
                        Adresse = adresse,
                        MietErtragskonto = new Buchungskonto($"W{wIdx:D5}-M", $"Mieterlöse {bezeichnung}", BuchungskontoTyp.Ertrag),
                        AufwandsKonto = new Buchungskonto($"W{wIdx:D5}-E", $"Erhaltungsaufwand {bezeichnung}", BuchungskontoTyp.Aufwand),
                    };
                    w.Eigentuemer.Add(new WohnungEigentuemer(new DateOnly(2000, 1, 1)) { Wohnung = w, Kontakt = owner });
                    w.Versionen.Add(new WohnungVersion(new DateOnly(2000, 1, 1), flaeche, flaeche, flaeche, 1) { Wohnung = w });
                    wohnungen.Add(w);
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

                    var vIdx = vertraege.Count;
                    var vertrag = new Vertrag
                    {
                        Ansprechpartner = wohnung.Eigentuemer.FirstOrDefault()?.Kontakt,
                        Ende = isActive ? null : contractEnd,
                        Wohnung = wohnung,
                        Notiz = $"Laufzeit: {contractStart:yyyy-MM} bis {(isActive ? "offen" : contractEnd.ToString("yyyy-MM", CultureInfo.InvariantCulture))}",
                        MietBuchungskonto = new Buchungskonto($"V{vIdx:D5}-MB", "Mietforderungen", BuchungskontoTyp.Aktiv),
                        NkBuchungskonto = new Buchungskonto($"V{vIdx:D5}-NK", "NK-Vorauszahlungen", BuchungskontoTyp.Passiv),
                        BkAbrechnungsKonto = new Buchungskonto($"V{vIdx:D5}-BK", "BK-Abrechnung", BuchungskontoTyp.Aktiv),
                        ZahlungsKonto = new Buchungskonto($"V{vIdx:D5}-ZK", "Zahlung", BuchungskontoTyp.Aktiv),
                        MietminderungsKonto = new Buchungskonto($"V{vIdx:D5}-MM", "Mietminderung", BuchungskontoTyp.Aufwand),
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
                var baseRentPerSqm = (decimal)(8.5 + random.NextDouble() * 7.0);

                for (var i = 0; i < starts.Count; i++)
                {
                    var raiseFactor = (decimal)(1.0 + (i * (0.02 + random.NextDouble() * 0.03)));
                    var grundmiete = Math.Round(vertrag.Wohnung.VersionAt(starts[i]).Wohnflaeche * baseRentPerSqm * raiseFactor, 2);
                    var personenzahl = 1 + random.Next(4);
                    var nebenkostenvorauszahlung = CalculateMonthlyNebenkostenVorauszahlung(
                        vertrag.Wohnung.VersionAt(starts[i]).Wohnflaeche,
                        personenzahl,
                        random);

                    vertragVersionen.Add(new VertragVersion(starts[i], grundmiete, personenzahl)
                    {
                        Vertrag = vertrag,
                        Nebenkostenvorauszahlung = nebenkostenvorauszahlung
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

        private static List<Transaktion> FillTransaktionen(
            SaverwalterContext ctx,
            List<Vertrag> vertraege,
            Dictionary<Kontakt, Bankkonto> mieterBankkontos,
            Dictionary<Vertrag, Bankkonto> vertragBankkontos,
            Dictionary<Vertrag, List<GarageVertrag>> garageVertraegeByVertrag,
            Random random)
        {
            Console.Write("Füge Transaktionen hinzu: ");

            var transaktionen = new List<Transaktion>();
            // Track Sollstellungen already created to avoid duplicates across versions
            // Use object reference as key — VertragId is 0 before SaveChanges
            var sollstellungen = new Dictionary<(Vertrag, int Jahr, int Monat), Buchungszeile>();
            var garageSollstellungen = new Dictionary<(GarageVertrag, int Jahr, int Monat), Buchungszeile>();

            foreach (var vertrag in vertraege)
            {
                mieterBankkontos.TryGetValue(vertrag.Mieter.FirstOrDefault()!, out var zahlerKonto);
                vertragBankkontos.TryGetValue(vertrag, out var empfaengerKonto);

                var mieterNamen = vertrag.Mieter.Count > 0
                    ? string.Join(", ", vertrag.Mieter.Select(m => m.Bezeichnung))
                    : "Leerstand";
                var wohnungInfo = vertrag.Wohnung.Adresse?.Anschrift is { } a
                    ? $"{a} – {vertrag.Wohnung.Bezeichnung}"
                    : vertrag.Wohnung.Bezeichnung;

                foreach (var version in vertrag.Versionen.OrderBy(v => v.Beginn))
                {
                    var payUntil = GlobalToday.AddMonths(-3);
                    var ende = version.Ende() is { } versionEnde && versionEnde < payUntil
                        ? versionEnde
                        : payUntil;

                    for (var monat = new DateOnly(version.Beginn.Year, version.Beginn.Month, 1);
                         monat <= new DateOnly(ende.Year, ende.Month, 1);
                         monat = monat.AddMonths(1))
                    {
                        // ~7% chance of partial/late payment, ~93% normal
                        var zahlungsfaktor = 0.97m + ((decimal)random.NextDouble() * 0.08m);
                        if (random.NextDouble() < 0.07)
                        {
                            zahlungsfaktor = random.NextDouble() < 0.5
                                ? 0.90m + ((decimal)random.NextDouble() * 0.06m)
                                : 1.00m; // skip NK variation for simplicity
                        }

                        var kaltmiete = version.Grundmiete;
                        var nkVz = Math.Round(version.Nebenkostenvorauszahlung * zahlungsfaktor, 2);

                        // Determine active garage components for this month
                        var garageComponents = new List<(GarageVertrag gv, decimal garagenMiete)>();
                        if (garageVertraegeByVertrag.TryGetValue(vertrag, out var linkedGv))
                        {
                            foreach (var gv in linkedGv)
                            {
                                var gvBeginn = gv.Beginn();
                                if (gvBeginn == DateOnly.MinValue || gvBeginn > monat) continue;
                                if (gv.Ende.HasValue && gv.Ende.Value < monat) continue;
                                var gvVersion = gv.Versionen
                                    .Where(v => v.Beginn <= monat)
                                    .OrderByDescending(v => v.Beginn)
                                    .FirstOrDefault();
                                if (gvVersion != null)
                                    garageComponents.Add((gv, gvVersion.GaragenMiete));
                            }
                        }

                        var garageTotal = garageComponents.Sum(g => g.garagenMiete);
                        var gesamt = kaltmiete + nkVz + garageTotal;
                        var zahlungsdatum = DritterWerktag(monat).AddDays(random.Next(0, 5));
                        var verwendungszweck = $"Miete {monat:MM/yyyy} | {mieterNamen} | {wohnungInfo}";

                        var transaktion = new Transaktion
                        {
                            Zahlungsdatum = zahlungsdatum,
                            Betrag = gesamt,
                            Verwendungszweck = verwendungszweck,
                            Zahler = zahlerKonto,
                            Zahlungsempfaenger = empfaengerKonto
                        };

                        // Sollstellung (if not already created for this month)
                        var sollKey = (vertrag, monat.Year, monat.Month);
                        if (!sollstellungen.TryGetValue(sollKey, out var sollZeile))
                        {
                            var sollDatum = DritterWerktag(monat);
                            var sollSatz = new Buchungssatz(sollDatum, $"Mietsoll {monat:MM/yyyy}");
                            sollZeile = new Buchungszeile(SollHaben.Soll, kaltmiete)
                            {
                                Buchungssatz = sollSatz,
                                Buchungskonto = vertrag.MietBuchungskonto
                            };
                            sollSatz.Buchungszeilen.Add(sollZeile);
                            sollSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, kaltmiete)
                            {
                                Buchungssatz = sollSatz,
                                Buchungskonto = vertrag.Wohnung.MietErtragskonto
                            });
                            ctx.Buchungssaetze.Add(sollSatz);
                            sollstellungen[sollKey] = sollZeile;
                        }

                        // Kaltmiete payment: Soll ZahlungsKonto / Haben MietBuchungskonto
                        var kaltmieteSatz = new Buchungssatz(zahlungsdatum, $"Mietzahlung Kaltmiete {monat:MM/yyyy}")
                        {
                            Transaktion = transaktion
                        };
                        var kaltmieteHaben = new Buchungszeile(SollHaben.Haben, kaltmiete)
                        {
                            Buchungssatz = kaltmieteSatz,
                            Buchungskonto = vertrag.MietBuchungskonto
                        };
                        kaltmieteSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, kaltmiete)
                        {
                            Buchungssatz = kaltmieteSatz,
                            Buchungskonto = vertrag.ZahlungsKonto
                        });
                        kaltmieteSatz.Buchungszeilen.Add(kaltmieteHaben);
                        ctx.Buchungssaetze.Add(kaltmieteSatz);

                        ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                        {
                            SollZeile = sollZeile,
                            HabenZeile = kaltmieteHaben
                        });

                        // NK-Vorauszahlung payment: Soll ZahlungsKonto / Haben NkBuchungskonto
                        if (nkVz > 0)
                        {
                            var nkSatz = new Buchungssatz(zahlungsdatum, $"Mietzahlung NK-VZ {monat:MM/yyyy}")
                            {
                                Transaktion = transaktion
                            };
                            nkSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, nkVz)
                            {
                                Buchungssatz = nkSatz,
                                Buchungskonto = vertrag.ZahlungsKonto
                            });
                            nkSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, nkVz)
                            {
                                Buchungssatz = nkSatz,
                                Buchungskonto = vertrag.NkBuchungskonto
                            });
                            ctx.Buchungssaetze.Add(nkSatz);
                        }

                        // Garage rent components — embedded in the same Transaktion
                        foreach (var (gv, garagenMiete) in garageComponents)
                        {
                            // Garage Sollstellung
                            var gvSollKey = (gv, monat.Year, monat.Month);
                            if (!garageSollstellungen.TryGetValue(gvSollKey, out var gvSollZeile))
                            {
                                var gvSollSatz = new Buchungssatz(DritterWerktag(monat), $"Garagenmietsoll {monat:MM/yyyy} | {gv.Garage.Kennung}");
                                gvSollZeile = new Buchungszeile(SollHaben.Soll, garagenMiete)
                                {
                                    Buchungssatz = gvSollSatz,
                                    Buchungskonto = gv.MietBuchungskonto
                                };
                                gvSollSatz.Buchungszeilen.Add(gvSollZeile);
                                gvSollSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, garagenMiete)
                                {
                                    Buchungssatz = gvSollSatz,
                                    Buchungskonto = gv.Garage.Ertragskonto
                                });
                                ctx.Buchungssaetze.Add(gvSollSatz);
                                garageSollstellungen[gvSollKey] = gvSollZeile;
                            }

                            // Garage payment Buchungssatz (part of same Transaktion)
                            var gvZahlSatz = new Buchungssatz(zahlungsdatum, $"Garagenmietzahlung {monat:MM/yyyy} | {gv.Garage.Kennung}")
                            {
                                Transaktion = transaktion
                            };
                            var gvHabenZeile = new Buchungszeile(SollHaben.Haben, garagenMiete)
                            {
                                Buchungssatz = gvZahlSatz,
                                Buchungskonto = gv.MietBuchungskonto
                            };
                            gvZahlSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, garagenMiete)
                            {
                                Buchungssatz = gvZahlSatz,
                                Buchungskonto = gv.ZahlungsKonto
                            });
                            gvZahlSatz.Buchungszeilen.Add(gvHabenZeile);
                            ctx.Buchungssaetze.Add(gvZahlSatz);

                            ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                            {
                                SollZeile = gvSollZeile,
                                HabenZeile = gvHabenZeile
                            });
                        }

                        ctx.Transaktionen.Add(transaktion);
                        transaktionen.Add(transaktion);
                    }
                }
            }

            Console.WriteLine($"{transaktionen.Count} Transaktionen hinzugefügt");
            return transaktionen;
        }

        private static DateOnly DritterWerktag(DateOnly monat) => DateUtils.DritterWerktag(monat);

        private static void FillErhaltungsaufwendungen(
            SaverwalterContext ctx,
            List<Wohnung> wohnungen,
            Random random)
        {
            Console.Write("Füge Erhaltungsaufwendungen hinzu: ");

            var aussteller = new List<Kontakt>();
            for (var i = 0; i < 12; i++)
            {
                var kontakt = GenerateJuristischePerson(50_000 + i, random);
                kontakt.Notiz = "Dienstleister";
                kontakt.VerbindlichkeitsKonto = new Buchungskonto($"DL{i:D3}-V", $"Verbindlichkeiten {kontakt.Name}", BuchungskontoTyp.Passiv);
                aussteller.Add(kontakt);
            }
            ctx.Kontakte.AddRange(aussteller);

            var invoiceCount = Math.Max(wohnungen.Count * 3, 80);
            var count = 0;
            for (var i = 0; i < invoiceCount; i++)
            {
                var wohnung = wohnungen[random.Next(wohnungen.Count)];
                var contractor = aussteller[random.Next(aussteller.Count)];
                var topic = MaintenanceTopics[random.Next(MaintenanceTopics.Length)];
                var betrag = CreateMaintenanceAmount(random);
                var date = RandomDate(random, GlobalToday.AddYears(-6), GlobalToday);

                var satz = new Buchungssatz(date, $"{topic} #{i + 1:0000}");
                satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, betrag)
                {
                    Buchungssatz = satz,
                    Buchungskonto = wohnung.AufwandsKonto
                });
                satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
                {
                    Buchungssatz = satz,
                    Buchungskonto = contractor.VerbindlichkeitsKonto!
                });
                ctx.Buchungssaetze.Add(satz);
                count++;
            }

            Console.WriteLine($"{count} Erhaltungsaufwendungen hinzugefügt");
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

        private static (Dictionary<Kontakt, Bankkonto> mieterBankkontos, Dictionary<Vertrag, Bankkonto> vertragBankkontos, Dictionary<Kontakt, Bankkonto> eigentuemerBankkontos) FillBankkontos(
            SaverwalterContext ctx,
            List<Kontakt> eigentuemer,
            List<Kontakt> mieter,
            List<Vertrag> vertraege,
            Random random)
        {
            Console.Write("Füge Bankkontos hinzu: ");

            var bankkontos = new List<Bankkonto>();
            var banken = new[] { "Sparkasse", "Deutsche Bank", "Commerzbank", "Volksbank", "DKB", "ING" };

            // One Bankkonto per Kontakt (Eigentuemer + Mieter)
            var mieterBankkontos = new Dictionary<Kontakt, Bankkonto>();
            var eigentuemerBankkontos = new Dictionary<Kontakt, Bankkonto>();
            foreach (var kontakt in eigentuemer.Concat(mieter))
            {
                var idx = bankkontos.Count;
                var bk = new Bankkonto
                {
                    Bank = banken[random.Next(banken.Length)],
                    Iban = CreateGermanIban(random),
                    Besitzer = [kontakt],
                    BuchungsKonto = new Buchungskonto($"BANK{idx:D5}", $"Bankkonto {kontakt.Bezeichnung}", BuchungskontoTyp.Aktiv),
                };
                bankkontos.Add(bk);
                if (mieter.Contains(kontakt))
                    mieterBankkontos[kontakt] = bk;
                else
                    eigentuemerBankkontos[kontakt] = bk;
            }

            // One Bankkonto per Vertrag's ZahlungsKonto (so OffeneForderungen can resolve it)
            var vertragBankkontos = new Dictionary<Vertrag, Bankkonto>();
            foreach (var vertrag in vertraege)
            {
                var idx = bankkontos.Count;
                var besitzer = vertrag.Wohnung.Eigentuemer.FirstOrDefault()?.Kontakt;
                var bk = new Bankkonto
                {
                    Bank = banken[random.Next(banken.Length)],
                    Iban = CreateGermanIban(random),
                    Besitzer = besitzer != null ? [besitzer] : [],
                    BuchungsKonto = vertrag.ZahlungsKonto,
                };
                bankkontos.Add(bk);
                vertragBankkontos[vertrag] = bk;
            }

            ctx.Bankkontos.AddRange(bankkontos);
            Console.WriteLine($"{bankkontos.Count} Bankkontos hinzugefügt");
            return (mieterBankkontos, vertragBankkontos, eigentuemerBankkontos);
        }

        private static (List<Garage>, Dictionary<Vertrag, List<GarageVertrag>>) FillGaragen(
            SaverwalterContext ctx,
            List<Wohnung> wohnungen,
            List<Vertrag> vertraege,
            List<Kontakt> mieter,
            Random random)
        {
            Console.Write("Füge Garagen hinzu: ");

            var garagen = new List<Garage>();
            var allGarageVertraege = new List<GarageVertrag>();
            var linkedByVertrag = new Dictionary<Vertrag, List<GarageVertrag>>();
            var gvCounter = 0;

            var activeVertraege = vertraege
                .Where(v => v.Ende == null || v.Ende >= GlobalToday)
                .ToList();

            foreach (var wohnung in wohnungen)
            {
                if (random.NextDouble() >= 0.22) continue;

                var garageKennung = $"G-{wohnung.WohnungId:0000}";
                var garage = new Garage(garageKennung)
                {
                    Besitzer = wohnung.Eigentuemer.FirstOrDefault()?.Kontakt!,
                    Adresse = wohnung.Adresse,
                    Notiz = "Stellplatz im Hinterhof",
                    Ertragskonto = new Buchungskonto($"{garageKennung}-EK", "Garagenmietertrag", BuchungskontoTyp.Ertrag)
                };
                garagen.Add(garage);

                var activeVertrag = activeVertraege.FirstOrDefault(v => v.Wohnung == wohnung);
                var standaloneMieter = mieter.Count > 0 ? mieter[random.Next(mieter.Count)] : null;
                var scenario = random.NextDouble();

                if (scenario < 0.10)
                {
                    // Vacant — no GarageVertrag
                    continue;
                }

                if (scenario < 0.45 && activeVertrag != null)
                {
                    // Linked to active Wohnungsvertrag; Mieter inherited from Vertrag (list stays empty)
                    var gv = MakeLinkedGarageVertrag(garage, activeVertrag, gvCounter++, random);
                    allGarageVertraege.Add(gv);
                    if (!linkedByVertrag.ContainsKey(activeVertrag))
                        linkedByVertrag[activeVertrag] = [];
                    linkedByVertrag[activeVertrag].Add(gv);

                    // ~20 % chance: garage was rented by someone else before this Wohnungsvertrag started
                    if (random.NextDouble() < 0.20)
                    {
                        var histEnde = gv.Beginn().AddDays(-1);
                        if (histEnde > GlobalToday.AddYears(-6))
                        {
                            var histGv = MakeStandaloneHistoricalGarageVertrag(garage, standaloneMieter, histEnde, gvCounter++, random);
                            allGarageVertraege.Add(histGv);
                            CreateStandaloneGarageTransaktionen(ctx, histGv, random);
                        }
                    }
                }
                else if (scenario < 0.70)
                {
                    // Standalone active rental with own Mieter
                    var gv = MakeStandaloneActiveGarageVertrag(garage, standaloneMieter, gvCounter++, random);
                    allGarageVertraege.Add(gv);
                    CreateStandaloneGarageTransaktionen(ctx, gv, random);
                }
                else if (scenario < 0.88)
                {
                    // Standalone historical (ended), optionally followed by a new renter
                    var endDate = GlobalToday.AddMonths(-random.Next(2, 24));
                    var gv = MakeStandaloneHistoricalGarageVertrag(garage, standaloneMieter, endDate, gvCounter++, random);
                    allGarageVertraege.Add(gv);
                    CreateStandaloneGarageTransaktionen(ctx, gv, random);

                    // ~40 %: a new active tenant moved in after
                    if (random.NextDouble() < 0.40 && mieter.Count > 0)
                    {
                        var newMieter = mieter[random.Next(mieter.Count)];
                        var gv2 = MakeStandaloneActiveGarageVertrag(garage, newMieter, gvCounter++, random, endDate.AddDays(1));
                        allGarageVertraege.Add(gv2);
                        CreateStandaloneGarageTransaktionen(ctx, gv2, random);
                    }
                }
                else
                {
                    // Succession: old standalone ended, then linked to new Wohnungsvertrag
                    var endDate = GlobalToday.AddMonths(-random.Next(6, 18));
                    var gvOld = MakeStandaloneHistoricalGarageVertrag(garage, standaloneMieter, endDate, gvCounter++, random);
                    allGarageVertraege.Add(gvOld);
                    CreateStandaloneGarageTransaktionen(ctx, gvOld, random);

                    if (activeVertrag != null)
                    {
                        var gvNew = MakeLinkedGarageVertrag(garage, activeVertrag, gvCounter++, random, endDate.AddDays(1));
                        allGarageVertraege.Add(gvNew);
                        if (!linkedByVertrag.ContainsKey(activeVertrag))
                            linkedByVertrag[activeVertrag] = [];
                        linkedByVertrag[activeVertrag].Add(gvNew);
                    }
                    else if (mieter.Count > 0)
                    {
                        var newMieter = mieter[random.Next(mieter.Count)];
                        var gvNew = MakeStandaloneActiveGarageVertrag(garage, newMieter, gvCounter++, random, endDate.AddDays(1));
                        allGarageVertraege.Add(gvNew);
                        CreateStandaloneGarageTransaktionen(ctx, gvNew, random);
                    }
                }
            }

            ctx.Garagen.AddRange(garagen);
            ctx.GarageVertraege.AddRange(allGarageVertraege);
            Console.WriteLine($"{garagen.Count} Garagen, {allGarageVertraege.Count} GarageVerträge hinzugefügt");
            return (garagen, linkedByVertrag);
        }

        private static GarageVertrag MakeLinkedGarageVertrag(
            Garage garage,
            Vertrag linkedVertrag,
            int idx,
            Random random,
            DateOnly? forceBeginn = null)
        {
            var vertragBeginn = linkedVertrag.Beginn();
            if (vertragBeginn == default) vertragBeginn = GlobalToday.AddYears(-2);
            var beginn = forceBeginn ?? vertragBeginn.AddMonths(random.Next(0, 7));
            var garagenMiete = Math.Round(50m + (decimal)(random.NextDouble() * 100), 0);
            var gvIdx = $"G{idx:D5}";

            var gv = new GarageVertrag
            {
                Garage = garage,
                Vertrag = linkedVertrag,
                MietBuchungskonto = new Buchungskonto($"{gvIdx}-MB", "Garagenmiete", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto($"{gvIdx}-ZK", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            gv.Versionen.Add(new GarageVertragVersion(beginn, garagenMiete) { GarageVertrag = gv });
            return gv;
        }

        private static GarageVertrag MakeStandaloneActiveGarageVertrag(
            Garage garage,
            Kontakt? mieter,
            int idx,
            Random random,
            DateOnly? forceBeginn = null)
        {
            var beginn = forceBeginn ?? GlobalToday.AddMonths(-random.Next(3, 48));
            var garagenMiete = Math.Round(50m + (decimal)(random.NextDouble() * 100), 0);
            var gvIdx = $"G{idx:D5}";

            var gv = new GarageVertrag
            {
                Garage = garage,
                MietBuchungskonto = new Buchungskonto($"{gvIdx}-MB", "Garagenmiete", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto($"{gvIdx}-ZK", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            if (mieter != null) gv.Mieter.Add(mieter);
            gv.Versionen.Add(new GarageVertragVersion(beginn, garagenMiete) { GarageVertrag = gv });
            return gv;
        }

        private static GarageVertrag MakeStandaloneHistoricalGarageVertrag(
            Garage garage,
            Kontakt? mieter,
            DateOnly endDate,
            int idx,
            Random random)
        {
            var duration = random.Next(6, 36);
            var beginn = endDate.AddMonths(-duration);
            var garagenMiete = Math.Round(45m + (decimal)(random.NextDouble() * 85), 0);
            var gvIdx = $"G{idx:D5}";

            var gv = new GarageVertrag
            {
                Garage = garage,
                Ende = endDate,
                MietBuchungskonto = new Buchungskonto($"{gvIdx}-MB", "Garagenmiete", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto($"{gvIdx}-ZK", "Zahlung", BuchungskontoTyp.Aktiv),
            };
            if (mieter != null) gv.Mieter.Add(mieter);
            gv.Versionen.Add(new GarageVertragVersion(beginn, garagenMiete) { GarageVertrag = gv });
            return gv;
        }

        private static void CreateStandaloneGarageTransaktionen(
            SaverwalterContext ctx,
            GarageVertrag gv,
            Random random)
        {
            // ~15 % chance: no payments at all (unpaid/problematic tenant)
            if (random.NextDouble() < 0.15) return;

            var version = gv.Versionen.FirstOrDefault();
            if (version == null) return;

            var payUntil = GlobalToday.AddMonths(-1);
            var ende = gv.Ende is { } e && e < payUntil ? e : payUntil;

            var sollstellungen = new Dictionary<(int Jahr, int Monat), Buchungszeile>();

            for (var monat = new DateOnly(version.Beginn.Year, version.Beginn.Month, 1);
                 monat <= new DateOnly(ende.Year, ende.Month, 1);
                 monat = monat.AddMonths(1))
            {
                // ~8 % missed payment
                if (random.NextDouble() < 0.08) continue;

                var miete = version.GaragenMiete;
                var zahlungsdatum = DritterWerktag(monat).AddDays(random.Next(0, 7));

                var transaktion = new Transaktion
                {
                    Zahlungsdatum = zahlungsdatum,
                    Betrag = miete,
                    Verwendungszweck = $"Garagenmiete {monat:MM/yyyy} | {gv.Garage.Kennung}",
                };

                // Sollstellung (once per month, deduped)
                var sollKey = (monat.Year, monat.Month);
                if (!sollstellungen.TryGetValue(sollKey, out var sollZeile))
                {
                    var sollSatz = new Buchungssatz(DritterWerktag(monat), $"Garagenmietsoll {monat:MM/yyyy} | {gv.Garage.Kennung}");
                    sollZeile = new Buchungszeile(SollHaben.Soll, miete)
                    {
                        Buchungssatz = sollSatz,
                        Buchungskonto = gv.MietBuchungskonto
                    };
                    sollSatz.Buchungszeilen.Add(sollZeile);
                    sollSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, miete)
                    {
                        Buchungssatz = sollSatz,
                        Buchungskonto = gv.Garage.Ertragskonto
                    });
                    ctx.Buchungssaetze.Add(sollSatz);
                    sollstellungen[sollKey] = sollZeile;
                }

                // Payment: Soll ZahlungsKonto / Haben MietBuchungskonto
                var zahlSatz = new Buchungssatz(zahlungsdatum, $"Garagenmietzahlung {monat:MM/yyyy} | {gv.Garage.Kennung}") { Transaktion = transaktion };
                var zahlHaben = new Buchungszeile(SollHaben.Haben, miete)
                {
                    Buchungssatz = zahlSatz,
                    Buchungskonto = gv.MietBuchungskonto
                };
                zahlSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, miete)
                {
                    Buchungssatz = zahlSatz,
                    Buchungskonto = gv.ZahlungsKonto
                });
                zahlSatz.Buchungszeilen.Add(zahlHaben);

                ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich { SollZeile = sollZeile, HabenZeile = zahlHaben });
                ctx.Buchungssaetze.Add(zahlSatz);
                ctx.Transaktionen.Add(transaktion);
            }
        }

        private static List<Umlage> FillUmlagen(SaverwalterContext ctx, List<Adresse> adressen, Random random)
        {
            Console.Write("Füge Umlagen hinzu: ");

            var umlagen = new List<Umlage>();
            var typen = ctx.Umlagetypen.ToDictionary(t => t.Bezeichnung);

            foreach (var adresse in adressen.Where(a => a.Wohnungen.Count > 0))
            {
                umlagen.Add(AddUmlage(adresse, typen["Allgemeinstrom/Hausbeleuchtung"], Umlageschluessel.NachWohnflaeche, umlagen.Count));
                umlagen.Add(AddUmlage(adresse, typen["Dachrinnenreinigung"], Umlageschluessel.NachWohnflaeche, umlagen.Count));
                umlagen.Add(AddUmlage(adresse, typen["Grundsteuer"], Umlageschluessel.NachWohnflaeche, umlagen.Count));
                umlagen.Add(AddUmlage(adresse, typen["Müllbeseitigung"], Umlageschluessel.NachPersonenzahl, umlagen.Count));
                umlagen.Add(AddUmlage(adresse, typen["Wasserversorgung"], Umlageschluessel.NachVerbrauch, umlagen.Count));
                umlagen.Add(AddUmlage(adresse, typen["Entwässerung/Schmutzwasser"], Umlageschluessel.NachVerbrauch, umlagen.Count));
                umlagen.Add(AddUmlage(adresse, typen["Strassenreinigung"], Umlageschluessel.NachWohnflaeche, umlagen.Count));

                if (random.NextDouble() < 0.35)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Breitbandkabelanschluss"], Umlageschluessel.NachNutzeinheit, umlagen.Count));
                }
                if (random.NextDouble() < 0.7)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Gartenpflege"], Umlageschluessel.NachWohnflaeche, umlagen.Count));
                }
                if (random.NextDouble() < 0.85)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Sachversicherung"], Umlageschluessel.NachWohnflaeche, umlagen.Count));
                }
                if (random.NextDouble() < 0.45)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Haftpflichtversicherung"], Umlageschluessel.NachWohnflaeche, umlagen.Count));
                }
                if (adresse.Wohnungen.Count >= 3 || random.NextDouble() < 0.35)
                {
                    umlagen.Add(AddUmlage(adresse, typen["Heizkosten"], Umlageschluessel.NachVerbrauch, umlagen.Count));
                    umlagen.Add(AddUmlage(adresse, typen["Betriebsstrom (Heizung)"], Umlageschluessel.NachWohnflaeche, umlagen.Count));
                    umlagen.Add(AddUmlage(adresse, typen["Wartung Therme/Speicher"], Umlageschluessel.NachWohnflaeche, umlagen.Count));
                }
            }

            ctx.Umlagen.AddRange(umlagen);
            Console.WriteLine($"{umlagen.Count} Umlagen hinzugefügt");
            return umlagen;
        }

        private static Dictionary<Adresse, Zaehler> FillZaehlerSet(SaverwalterContext ctx, List<Umlage> umlagen)
        {
            Console.Write("Füge Zähler hinzu: ");

            var zaehlerListe = new List<Zaehler>();
            var wohnungszaehler = new Dictionary<(Wohnung, Zaehlertyp), Zaehler>();
            var hauszaehlerHeizung = new Dictionary<Adresse, Zaehler>();

            Zaehler GetOrAddWohnungszaehler(Wohnung w, Zaehlertyp t)
            {
                var key = (w, t);
                if (!wohnungszaehler.TryGetValue(key, out var existing))
                {
                    var kennung = $"{t switch
                    {
                        Zaehlertyp.Kaltwasser => "KW",
                        Zaehlertyp.Warmwasser => "WW",
                        Zaehlertyp.Gas => "HZ",
                        _ => t.ToString()
                    }}-{w.Adresse?.Anschrift ?? "?"} – {w.Bezeichnung}";
                    existing = new Zaehler(kennung, t) { Wohnung = w };
                    zaehlerListe.Add(existing);
                    wohnungszaehler[key] = existing;
                }
                return existing;
            }

            foreach (var umlage in umlagen)
            {
                if (umlage.Typ.Bezeichnung == "Wasserversorgung"
                    || umlage.Typ.Bezeichnung == "Entwässerung/Schmutzwasser")
                {
                    foreach (var wohnung in umlage.Wohnungen)
                    {
                        var kw = GetOrAddWohnungszaehler(wohnung, Zaehlertyp.Kaltwasser);
                        var ww = GetOrAddWohnungszaehler(wohnung, Zaehlertyp.Warmwasser);
                        if (!umlage.Zaehler.Contains(kw)) umlage.Zaehler.Add(kw);
                        if (!umlage.Zaehler.Contains(ww)) umlage.Zaehler.Add(ww);
                    }
                }

                if (umlage.Typ.Bezeichnung == "Heizkosten")
                {
                    var adresse = umlage.Wohnungen.FirstOrDefault()?.Adresse;
                    if (adresse != null && !hauszaehlerHeizung.ContainsKey(adresse))
                    {
                        var hauszaehler = new Zaehler($"HZ-HAUS-{adresse.Anschrift}", Zaehlertyp.Gas) { Adresse = adresse };
                        zaehlerListe.Add(hauszaehler);
                        hauszaehlerHeizung[adresse] = hauszaehler;
                    }

                    foreach (var wohnung in umlage.Wohnungen)
                    {
                        var hz = GetOrAddWohnungszaehler(wohnung, Zaehlertyp.Gas);
                        var ww = GetOrAddWohnungszaehler(wohnung, Zaehlertyp.Warmwasser);
                        if (!umlage.Zaehler.Contains(hz)) umlage.Zaehler.Add(hz);
                        if (!umlage.Zaehler.Contains(ww)) umlage.Zaehler.Add(ww);
                    }
                }
            }

            ctx.ZaehlerSet.AddRange(zaehlerListe);
            Console.WriteLine($"{zaehlerListe.Count} Zähler hinzugefügt");
            return hauszaehlerHeizung;
        }

        private static void FillHKVO(
            SaverwalterContext ctx,
            List<Umlage> umlagen,
            Dictionary<Adresse, Zaehler> hauszaehlerHeizung)
        {
            var hkvoList = new List<HKVO>();
            var betriebsstromByAdresse = umlagen
                .Where(u => u.Typ.Bezeichnung == "Betriebsstrom (Heizung)")
                .ToDictionary(u => u.Wohnungen.First().Adresse!);

            foreach (var heizUmlage in umlagen.Where(u => u.Typ.Bezeichnung == "Heizkosten"))
            {
                var adresse = heizUmlage.Wohnungen.FirstOrDefault()?.Adresse;
                if (adresse == null
                    || !hauszaehlerHeizung.TryGetValue(adresse, out var hauszaehler)
                    || !betriebsstromByAdresse.TryGetValue(adresse, out var betriebsstrom))
                    continue;

                var hkvo = new HKVO(new DateOnly(2000, 1, 1), 0.7m, 0.7m, HKVO_P9A2.Satz_2, 0m)
                {
                    Heizkosten = heizUmlage,
                    Betriebsstrom = betriebsstrom,
                };
                if (hauszaehler != null) hkvo.AllgemeinWaermeZaehler.Add(hauszaehler);
                heizUmlage.HeizkostenHKVOs.Add(hkvo);
                hkvoList.Add(hkvo);
            }

            ctx.HKVO.AddRange(hkvoList);
            Console.WriteLine($"{hkvoList.Count} HKVO-Einträge hinzugefügt");
        }

        private static List<Zaehlerstand> FillZaehlerstaende(SaverwalterContext ctx, List<Vertrag> vertraege, Random random)
        {
            Console.Write("Füge Zählerstände hinzu: ");

            var zaehlerstaende = new List<Zaehlerstand>();
            var zaehlerSet = ctx.ZaehlerSet.Local.Count > 0
                ? new List<Zaehler>(ctx.ZaehlerSet.Local)
                : ctx.ZaehlerSet.ToList();
            var zeitraeumeByWohnung = vertraege
                .Where(v => v.Wohnung != null)
                .GroupBy(v => v.Wohnung)  // object reference — no dependency on DB-assigned IDs
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(v => (
                            Beginn: v.Beginn(),
                            Ende: (v.Ende ?? GlobalToday)))
                        .Where(x => x.Beginn != default && x.Ende >= x.Beginn)
                        .OrderBy(x => x.Beginn)
                        .ToList());

            foreach (var zaehler in zaehlerSet)
            {
                var start = GlobalToday.AddYears(-6);
                List<(DateOnly Beginn, DateOnly Ende)>? zeitraeume = null;

                if (zaehler.Wohnung != null
                    && zeitraeumeByWohnung.TryGetValue(zaehler.Wohnung, out var wohnungZeitraeume)
                    && wohnungZeitraeume.Count > 0)
                {
                    start = wohnungZeitraeume.Min(z => z.Beginn);
                    zeitraeume = wohnungZeitraeume;
                }

                var stichtage = BuildMeterReadingDates(start, GlobalToday, zeitraeume);
                if (stichtage.Count == 0)
                {
                    continue;
                }

                var wohnflaeche = zaehler.Wohnung?.VersionAt(GlobalToday).Wohnflaeche
                    ?? zaehler.Adresse?.Wohnungen.Sum(w => w.VersionAt(GlobalToday).Wohnflaeche)
                    ?? 65m;
                var stand = StartingMeterStand(zaehler.Typ, random);
                var letzterStichtag = stichtage[0].AddDays(-1);

                foreach (var stichtag in stichtage)
                {
                    var tage = Math.Max(1, stichtag.DayNumber - letzterStichtag.DayNumber);
                    var increment = ConsumptionIncrement(zaehler.Typ, random, wohnflaeche, tage);
                    stand = Math.Round(stand + increment, 2);
                    zaehlerstaende.Add(new Zaehlerstand(stichtag, stand) { Zaehler = zaehler });
                    letzterStichtag = stichtag;
                }
            }

            ctx.Zaehlerstaende.AddRange(zaehlerstaende);
            Console.WriteLine($"{zaehlerstaende.Count} Zählerstände hinzugefügt");
            return zaehlerstaende;
        }

        private static void FillBetriebskostenrechnungen(
            SaverwalterContext ctx,
            List<Umlage> umlagen,
            Dictionary<Kontakt, Bankkonto> eigentuemerBankkontos,
            Random random)
        {
            Console.Write("Füge Betriebskostenrechnungen hinzu: ");

            var count = 0;

            foreach (var umlage in umlagen)
            {
                var zahlerBankkonto = umlage.Wohnungen
                    .Select(w =>
                    {
                        var b = w.Eigentuemer.FirstOrDefault()?.Kontakt;
                        return b != null ? eigentuemerBankkontos.GetValueOrDefault(b) : null;
                    })
                    .FirstOrDefault(b => b != null);

                var beginn = GetEarliestDate(umlage.Wohnungen.ToList());
                if (beginn == default)
                    beginn = GlobalToday.AddYears(-3);

                for (var year = beginn.Year; year < GlobalToday.Year; year++)
                {
                    var abrechnungsEnde = new DateOnly(year, 12, 31);
                    var wohnflaeche = Math.Max(umlage.Wohnungen.Sum(w => w.VersionAt(abrechnungsEnde).Wohnflaeche), 40m);
                    var basisbetrag = umlage.Typ.Bezeichnung switch
                    {
                        "Heizkosten" => wohnflaeche * (decimal)(9 + random.NextDouble() * 4),
                        "Grundsteuer" => wohnflaeche * (decimal)(1.2 + random.NextDouble() * 0.4),
                        "Wasserversorgung" => wohnflaeche * (decimal)(1.8 + random.NextDouble() * 0.8),
                        _ => wohnflaeche * (decimal)(0.8 + random.NextDouble() * 1.5)
                    };

                    var annualFactor = (decimal)(1 + ((year - beginn.Year) * (0.01 + random.NextDouble() * 0.015)));
                    var betrag = Math.Round(basisbetrag * annualFactor, 2);
                    var datum = new DateOnly(year, 12, 31);

                    // Forderungs-Buchungssatz: Haben NkVerrechnungsKonto (Kosten-Pool wächst)
                    var forderungsSatz = new Buchungssatz(datum, $"BK-Eingang {umlage.Typ.Bezeichnung} {year}");
                    var forderungsHaben = new Buchungszeile(SollHaben.Haben, betrag)
                    {
                        Buchungssatz = forderungsSatz,
                        Buchungskonto = umlage.NkVerrechnungsKonto
                    };
                    forderungsSatz.Buchungszeilen.Add(forderungsHaben);

                    // Zahlungs-Transaktion + Buchungssatz: Soll NkVerrechnungsKonto / Haben ZahlungsKonto
                    var zahlungsSatz = new Buchungssatz(datum, $"BK-Zahlung {umlage.Typ.Bezeichnung} {year}");
                    var zahlungsTransaktion = new Transaktion
                    {
                        Zahlungsdatum = datum,
                        Betrag = betrag,
                        Verwendungszweck = zahlungsSatz.Beschreibung,
                        Zahler = zahlerBankkonto,
                    };
                    zahlungsSatz.Transaktion = zahlungsTransaktion;
                    var zahlungsSoll = new Buchungszeile(SollHaben.Soll, betrag)
                    {
                        Buchungssatz = zahlungsSatz,
                        Buchungskonto = umlage.NkVerrechnungsKonto
                    };
                    zahlungsSatz.Buchungszeilen.Add(zahlungsSoll);
                    zahlungsSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
                    {
                        Buchungssatz = zahlungsSatz,
                        Buchungskonto = umlage.ZahlungsKonto
                    });

                    ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                    {
                        SollZeile = zahlungsSoll,
                        HabenZeile = forderungsHaben
                    });

                    ctx.Buchungssaetze.Add(forderungsSatz);
                    ctx.Buchungssaetze.Add(zahlungsSatz);
                    ctx.Transaktionen.Add(zahlungsTransaktion);
                    count++;
                }
            }

            Console.WriteLine($"{count} Betriebskostenrechnungen hinzugefügt");
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

        private static Umlage AddUmlage(Adresse adresse, Umlagetyp typ, Umlageschluessel schluessel, int idx)
        {
            var umlage = new Umlage
            {
                Typ = typ,
                Beschreibung = $"{typ.Bezeichnung} für {adresse.Anschrift}",
                Wohnungen = adresse.Wohnungen,
                NkVerrechnungsKonto = new Buchungskonto($"U{idx:D5}-NR", $"NK-Verrechnung {typ.Bezeichnung}", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto($"U{idx:D5}-ZK", $"NK-Zahlung {typ.Bezeichnung}", BuchungskontoTyp.Aktiv),
            };
            umlage.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), schluessel) { Umlage = umlage });
            return umlage;
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

        private static decimal CreateMaintenanceAmount(Random random)
        {
            var r = random.NextDouble();
            if (r < 0.7)
            {
                return Math.Round((decimal)(120 + (random.NextDouble() * 900)), 2);
            }

            if (r < 0.95)
            {
                return Math.Round((decimal)(1000 + (random.NextDouble() * 4000)), 2);
            }

            return Math.Round((decimal)(5000 + (random.NextDouble() * 20000)), 2);
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

        private static decimal AnnualConsumption(Zaehlertyp zaehlertyp, Random random, decimal wohnflaeche)
        {
            return zaehlertyp switch
            {
                Zaehlertyp.Warmwasser => (decimal)(10 + random.NextDouble() * 15),
                Zaehlertyp.Kaltwasser => (decimal)(45 + random.NextDouble() * 120),
                Zaehlertyp.Strom => (decimal)(1000 + random.NextDouble() * 2800),
                Zaehlertyp.Gas => Math.Max(40m, wohnflaeche) * (decimal)(45 + random.NextDouble() * 35),
                _ => (decimal)(100 + random.NextDouble() * 400)
            };
        }

        private static decimal CalculateMonthlyNebenkostenVorauszahlung(decimal wohnflaeche, int personenzahl, Random random)
        {
            var flaechenanteil = wohnflaeche * (decimal)(1.15 + (random.NextDouble() * 0.55));
            var personenanteil = personenzahl * (decimal)(18 + (random.NextDouble() * 14));
            var rohwert = flaechenanteil + personenanteil;
            var begrenzt = Math.Min(320m, Math.Max(70m, rohwert));
            return Math.Round(begrenzt, 2);
        }

        private static decimal StartingMeterStand(Zaehlertyp zaehlertyp, Random random)
        {
            return zaehlertyp switch
            {
                Zaehlertyp.Warmwasser => Math.Round((decimal)(100 + random.NextDouble() * 500), 2),
                Zaehlertyp.Kaltwasser => Math.Round((decimal)(200 + random.NextDouble() * 1000), 2),
                Zaehlertyp.Strom => Math.Round((decimal)(1000 + random.NextDouble() * 7000), 2),
                Zaehlertyp.Gas => Math.Round((decimal)(5000 + random.NextDouble() * 30000), 2),
                _ => Math.Round((decimal)(100 + random.NextDouble() * 900), 2)
            };
        }

        private static decimal ConsumptionIncrement(Zaehlertyp zaehlertyp, Random random, decimal wohnflaeche, int tage)
        {
            var jahresverbrauch = AnnualConsumption(zaehlertyp, random, wohnflaeche);
            var tagesverbrauch = jahresverbrauch / 365m;
            var streuung = (decimal)(0.92 + (random.NextDouble() * 0.16));
            var inkrement = tagesverbrauch * tage * streuung;
            var minimum = zaehlertyp switch
            {
                Zaehlertyp.Warmwasser or Zaehlertyp.Kaltwasser => 0.05m,
                _ => 1m
            };

            return Math.Max(minimum, Math.Round(inkrement, 2));
        }

        private static List<DateOnly> BuildMeterReadingDates(
            DateOnly start,
            DateOnly ende,
            List<(DateOnly Beginn, DateOnly Ende)>? zeitraeume)
        {
            var stichtage = new SortedSet<DateOnly>();

            for (var jahr = start.Year; jahr <= ende.Year; jahr++)
            {
                var jahresende = new DateOnly(jahr, 12, 31);
                if (jahresende >= start && jahresende <= ende)
                {
                    stichtage.Add(jahresende);
                }
            }

            if (zeitraeume != null && zeitraeume.Count > 0)
            {
                for (var i = 0; i < zeitraeume.Count; i++)
                {
                    var zeitraum = zeitraeume[i];
                    var naechster = i + 1 < zeitraeume.Count ? zeitraeume[i + 1] : ((DateOnly Beginn, DateOnly Ende)?)null;
                    var direkterUebergang = naechster != null && naechster.Value.Beginn == zeitraum.Ende.AddDays(1);

                    if (zeitraum.Beginn >= start && zeitraum.Beginn <= ende)
                    {
                        stichtage.Add(zeitraum.Beginn);
                    }

                    if (!direkterUebergang && zeitraum.Ende >= start && zeitraum.Ende <= ende)
                    {
                        stichtage.Add(zeitraum.Ende);
                    }
                }
            }

            return [.. stichtage];
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
