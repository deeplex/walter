using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using System.Security.Cryptography;
using System.Text;

namespace Deeplex.Saverwalter.InitiateTestDbs.Templates
{
    internal sealed class FullGenericDatabase
    {
        public static async Task PopulateDatabase(
            SaverwalterContext ctx,
            string databaseUser,
            string databasePass)
        {
            await CreateUserAccount(ctx, databaseUser, databasePass);
            var adressen = await FillAdressen(ctx);
            var natuerlichePersonen = await FillNatuerlichePersonen(ctx, adressen);
            var juristischePersonen = await FillJuristischePersonen(ctx, adressen);
            var wohnungen = await FillWohnungen(ctx, adressen, natuerlichePersonen, juristischePersonen);
            var vertraege = await FillVertraege(ctx, wohnungen, natuerlichePersonen, juristischePersonen);
            var mieten = await FillMieten(ctx, vertraege);
            var erhaltungsaufwendungen = await FillErhaltungsaufwendungen(ctx, wohnungen, juristischePersonen);

            // Still empty
            var mietminderungen = await FillMietminderungen(ctx);
            var kontos = await FillKontos(ctx);
            var garagen = await FillGaragen(ctx);

            // TODO
            var umlagen = await FillUmlagen(ctx);
            var mieterSet = await FillMieterSet(ctx);
            var zaehlerSet = await FillZaehlerSet(ctx);
            var zaehlerstaende = await FillZaehlerstaende(ctx);
            var betriebskostenrechnungen = await FillBetriebskostenrechnungen(ctx);

            Console.WriteLine("Lade erzeugte Daten in Datenbank...");
            await ctx.SaveChangesAsync();
            Console.WriteLine("Fertig!");
        }

        private static async Task CreateUserAccount(SaverwalterContext ctx, string databaseUser, string databasePass)
        {
            Console.Write($"Erstelle Nutzer mit Nutzernamen {databaseUser} und Passwort {databasePass}");
            var account = new UserAccount { Username = databaseUser };
            ctx.UserAccounts.Add(account);
            var credential = new Pbkdf2PasswordCredential
            {
                User = account,
                Iterations = 210000,
                Salt = RandomNumberGenerator.GetBytes(32),
            };

            var utf8Password = Encoding.UTF8.GetBytes(databasePass);

            credential.PasswordHash = Rfc2898DeriveBytes.Pbkdf2(utf8Password, credential.Salt, credential.Iterations, HashAlgorithmName.SHA512, 64);
            account.Pbkdf2PasswordCredential = credential;
            ctx.Pbkdf2PasswordCredentials.Add(credential);
            await ctx.SaveChangesAsync();
        }

        public static async Task<List<Adresse>> FillAdressen(SaverwalterContext ctx)
        {
            Console.WriteLine("Füge Adressen hinzu:");

            var adressen = new List<Adresse> { };

            var strasseList = GenericData.strasseList;
            var hausnummerList = GenericData.hausnummerList;
            var postleitzahlList = GenericData.postleitzahlList;
            var stadtList = GenericData.stadtList;

            for (int i = 0; i < 100; ++i)
            {
                var strasse = strasseList[i % strasseList.Count];
                var hausnummer = hausnummerList[i % hausnummerList.Count];
                var postleitzahl = postleitzahlList[i % postleitzahlList.Count];
                var stadt = stadtList[i % stadtList.Count];

                adressen.Add(new Adresse(strasse, hausnummer, postleitzahl, stadt));
            }
            
            ctx.Adressen.AddRange(adressen);
            //await ctx.SaveChangesAsync();

            return adressen;
        }

        public static async Task<List<JuristischePerson>> FillJuristischePersonen(SaverwalterContext ctx, List<Adresse> adressen)
        {
            Console.WriteLine("Füge juristische Personen hinzu:");

            var juristischePersonen = new List<JuristischePerson> { };

            List<string> bezeichnungen = GenericData.companyNames;
            List<string?> emailList = GenericData.companyEmails
                .Select((e, i) => (i % 2 == 0) ? null : e).ToList();
            List<string?> telefonList = GenericData.telefonnummerList
                .Select((e, i) => (i * 2 % 3 == 0) ? null : e).ToList();
            List<string?> mobilList = GenericData.telefonnummerList
                .Select((e, i) => (i % 2 == 0) ? null : e).ToList();
            List<string?> faxList = GenericData.telefonnummerList
                .Select((e, i) => i % 10 == 0 ? null : e).ToList();

            for (var i = 0; i < bezeichnungen.Count * 1.5; ++i)
            {
                var bezeichnung = bezeichnungen[i % bezeichnungen.Count];
                var email = emailList[i % emailList.Count];
                var telefon = telefonList[i % telefonList.Count];
                var fax = faxList[i % faxList.Count];
                var mobil = mobilList[i % mobilList.Count];
                var adresse = adressen[i * 7 % adressen.Count];

                juristischePersonen.Add(new JuristischePerson(bezeichnung)
                {
                    Telefon = telefon,
                    Adresse = adresse,
                    Email = email,
                    Fax = fax,
                    Mobil = mobil,
                });
            }

            ctx.JuristischePersonen.AddRange(juristischePersonen);
            //await ctx.SaveChangesAsync();

            return juristischePersonen;
        }

        public static async Task<List<NatuerlichePerson>> FillNatuerlichePersonen(SaverwalterContext ctx, List<Adresse> adressen)
        {
            Console.WriteLine("Füge natürliche Personen hinzu:");

            var natuerlichePersonen = new List<NatuerlichePerson> { };

            List<string> vornamenMale = GenericData.FirstNamesMale;
            List<string> vornamenFemale = GenericData.FirstNamesFemale;
            List<string> vornamenAll = vornamenMale.Concat(vornamenFemale).ToList();
            List<string> nachnamen = GenericData.lastNames;

            List<string> reversedTelefonList = GenericData.telefonnummerList;
            reversedTelefonList.Reverse();
            List<string?> telefonList = reversedTelefonList
                .Select((e, i) => (i * 2 % 3 == 0) ? null : e).ToList();

            List<string> reversedMobilList = GenericData.mobilePhoneNumbers;
            reversedMobilList.Reverse();
            List<string?> mobilList = reversedMobilList
                .Select((e, i) => (i % 2 == 0) ? null : e).ToList();

            for (var i = 0; i < nachnamen.Count * 1.5; ++i)
            {
                string nachname = nachnamen[i % nachnamen.Count];
                Adresse adresse = adressen[i % adressen.Count];

                for (var j = i; j < (i + (i % 4)); ++j)
                {
                    string vorname;
                    Anrede anrede;

                    if (i % 100 == 0 || i % 51 == 0)
                    {
                        vorname = vornamenAll[j % vornamenAll.Count];
                        anrede = Anrede.Keine;
                    }
                    if (i % 2 == 0)
                    {
                        vorname = vornamenMale[j % vornamenMale.Count];
                        anrede = Anrede.Herr;
                    }
                    else
                    {
                        vorname = vornamenFemale[j % vornamenMale.Count];
                        anrede = Anrede.Frau;
                    }

                    var email = i % 3 == 0 ?
                        $"{vorname}.{nachname}@{GenericData.emailProvider[i % GenericData.emailProvider.Count]}".ToLower() :
                        null;
                    var telefon = telefonList[i % telefonList.Count];
                    var mobil = mobilList[i % mobilList.Count];

                    natuerlichePersonen.Add(new NatuerlichePerson(nachname)
                    {
                        Telefon = telefon,
                        Adresse = adresse,
                        Anrede = anrede,
                        Email = email,
                        Mobil = mobil,
                        Vorname = vorname,
                    });
                }
            }

            ctx.NatuerlichePersonen.AddRange(natuerlichePersonen);
            //await ctx.SaveChangesAsync();

            return natuerlichePersonen;
        }

        public static async Task<List<Wohnung>> FillWohnungen(SaverwalterContext ctx, List<Adresse> adressen, List<NatuerlichePerson> natuerlichePersonen, List<JuristischePerson> juristischePersonen)
        {
            Console.WriteLine("Füge Wohnungen hinzu:");

            var wohnungen = new List<Wohnung> { };

            var personIds = natuerlichePersonen.Select(e => e.PersonId)
                .Concat(juristischePersonen.Select(e => e.PersonId)).ToList();

            for (var i = 0; i < 100; i++)
            {
                var adresse = adressen[i * 3 % adressen.Count];
                // Should run 200 times
                for (var j = 1; j < (i%5) + 1; j++)
                {
                    var bezeichnung = $"Wohnung Nr. {j}";
                    wohnungen.Add(new Wohnung(bezeichnung, i * 2, i * 2, 1)
                    {
                        Adresse = adresse,
                        BesitzerId = personIds[i%personIds.Count],
                    });
                }
            }

            ctx.Wohnungen.AddRange(wohnungen);
            //await ctx.SaveChangesAsync();

            return wohnungen;
        }

        public static async Task<List<Erhaltungsaufwendung>> FillErhaltungsaufwendungen(SaverwalterContext ctx, List<Wohnung> wohnungen, List<JuristischePerson> juristischePersonen)
        {
            Console.WriteLine("Füge Erhaltungsaufwendungen hinzu:");

            var erhaltungsaufwendungen = new List<Erhaltungsaufwendung> { };

            for (var i = 0; i < 3000; i++)
            {
                var betrag = 100;
                var bezeichnung = $"Rechnungsnr. {i}";
                var ausstellerId = juristischePersonen[i%juristischePersonen.Count].PersonId;
                var jahr = 2020 + i % 5;
                var monat = 1 + i % 12;
                var tag = 1 + i % DateTime.DaysInMonth(jahr, monat);
                var datum = new DateOnly(jahr, monat, tag);
                var wohnung = wohnungen[i % wohnungen.Count];

                erhaltungsaufwendungen.Add(new Erhaltungsaufwendung(betrag, bezeichnung, ausstellerId, datum)
                {
                    Wohnung = wohnung
                });
            }

            ctx.Erhaltungsaufwendungen.AddRange(erhaltungsaufwendungen);
            //await ctx.SaveChangesAsync();

            return erhaltungsaufwendungen;
        }

        public static async Task<List<Vertrag>> FillVertraege(
            SaverwalterContext ctx,
            List<Wohnung> wohnungen,
            List<NatuerlichePerson> natuerlichePersonen,
            List<JuristischePerson> juristischePersonen)
        {
            Console.WriteLine("Füge Verträge hinzu:");

            var vertraege = new List<Vertrag>();

            var personIds = natuerlichePersonen.Select(i => i.PersonId)
                .Concat(juristischePersonen.Select(i => i.PersonId)).ToList();

            for (var i = 0; i < wohnungen.Count; ++i)
            {
                var wohnung = wohnungen[i];
                DateOnly ende;
                if (i % 3 == 0)
                {
                    var jahr = 2020 - (i % 5);
                    var monat = 1 + i % 12;
                    var tag = 1 + i % DateTime.DaysInMonth(jahr, monat);
                    ende = new DateOnly(jahr, monat, tag);
                }

                var vertrag = new Vertrag()
                {
                    AnsprechpartnerId = personIds[i * 33 % personIds.Count],
                    Ende = ende,
                    Wohnung = wohnung
                };

                var versionen = new List<VertragVersion> { };
                for (var j = 0; j <= (j + 1) % 3; ++j)
                {
                    var grundmiete = wohnung.Wohnflaeche * (6 + i % 3 + j % 3);
                    var personenzahl = (i + 1) % 3 * (j + 1) % 3;
                    var beginn = ende.AddDays(-217 * ((j + 1) % 2) * j);
                    versionen.Add(new VertragVersion(beginn, grundmiete, personenzahl)
                    {
                        Vertrag = vertrag
                    });
                }

                vertraege.Add(vertrag);
            }

            ctx.Vertraege.AddRange(vertraege);
            //await ctx.SaveChangesAsync();

            return vertraege;
        }

        public static async Task<List<Miete>> FillMieten(SaverwalterContext ctx, List<Vertrag> vertraege)
        {
            Console.WriteLine("Füge Mieten hinzu:");

            var mieten = new List<Miete> { };

            foreach (var vertrag in vertraege)
            {
                foreach (var version in vertrag.Versionen)
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    for (DateOnly date = version.Beginn; date <= version.Ende() || date <= today; date.AddMonths(1))
                    {
                        mieten.Add(new Miete(date, date, version.Grundmiete + 300 + (date.Day % 3) * 100)
                        {
                            Vertrag = vertrag
                        });
                    }
                }
            }

            ctx.Mieten.AddRange(mieten);
            //await ctx.SaveChangesAsync();

            return mieten;
        }

        public static async Task<List<Garage>> FillGaragen(SaverwalterContext ctx)
        {
            var garagen = new List<Garage> { };

            // TODO still empty...
            await ctx.SaveChangesAsync();

            return garagen;
        }

        public static async Task<List<Konto>> FillKontos(SaverwalterContext ctx)
        {
            var kontos = new List<Konto> { };

            // TODO still empty...

            ctx.Kontos.AddRange(kontos);
            //await ctx.SaveChangesAsync();

            return kontos;
        }

        public static async Task<List<Mietminderung>> FillMietminderungen(SaverwalterContext ctx)
        {
            var mietminderungen = new List<Mietminderung> { };

            // TODO still empty...

            ctx.Mietminderungen.AddRange(mietminderungen);
            //await ctx.SaveChangesAsync();

            return mietminderungen;
        }

        public static async Task<List<Betriebskostenrechnung>> FillBetriebskostenrechnungen(SaverwalterContext ctx)
        {
            var betriebskostenrechnungen = new List<Betriebskostenrechnung> { };

            // TODO

            ctx.Betriebskostenrechnungen.AddRange(betriebskostenrechnungen);
            //await ctx.SaveChangesAsync();

            return betriebskostenrechnungen;
        }

        public static async Task<List<Mieter>> FillMieterSet(SaverwalterContext ctx)
        {
            var mieter = new List<Mieter> { };

            // TODO

            ctx.MieterSet.AddRange(mieter);
            //await ctx.SaveChangesAsync();

            return mieter;
        }

        public static async Task<List<Umlage>> FillUmlagen(SaverwalterContext ctx)
        {
            var umlagen = new List<Umlage> { };

            // TODO

            ctx.Umlagen.AddRange(umlagen);
            //await ctx.SaveChangesAsync();

            return umlagen;
        }

        public static async Task<List<Zaehler>> FillZaehlerSet(SaverwalterContext ctx)
        {
            var zaehler = new List<Zaehler> { };

            // TODO

            ctx.ZaehlerSet.AddRange(zaehler);
            //await ctx.SaveChangesAsync();

            return zaehler;
        }

        public static async Task<List<Zaehlerstand>> FillZaehlerstaende(SaverwalterContext ctx)
        {
            var zaehlerstaende = new List<Zaehlerstand> { };

            // TODO

            ctx.Zaehlerstaende.AddRange(zaehlerstaende);
            //await ctx.SaveChangesAsync();

            return zaehlerstaende;
        }
    }
}
