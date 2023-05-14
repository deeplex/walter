using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using System.Security.Cryptography;
using System.Text;

namespace Deeplex.Saverwalter.InitiateTestDbs.Templates
{
    internal sealed class FullGenericDatabase
    {
        private static DateOnly globalToday = new DateOnly(2023, 5, 14);

        public static async Task PopulateDatabase(
           SaverwalterContext ctx,
           string databaseUser,
           string databasePass)
        {
            CreateUserAccount(ctx, databaseUser, databasePass);
            var adressen = FillAdressen(ctx);
            var natuerlichePersonen = FillNatuerlichePersonen(ctx, adressen);
            var juristischePersonen = FillJuristischePersonen(ctx, adressen);
            var wohnungen = FillWohnungen(ctx, adressen, natuerlichePersonen, juristischePersonen);
            var vertraege = FillVertraege(ctx, wohnungen, natuerlichePersonen, juristischePersonen);
            var vertragVersionen = FillVertragversionen(ctx, vertraege);
            var mieten = FillMieten(ctx, vertraege);
            var erhaltungsaufwendungen = FillErhaltungsaufwendungen(ctx, wohnungen, juristischePersonen);

            // Still empty
            var mietminderungen = FillMietminderungen(ctx);
            var kontos = FillKontos(ctx);
            var garagen = FillGaragen(ctx);

            // TODO
            var umlagen = FillUmlagen(ctx, adressen);
            var mieterSet = FillMieterSet(ctx);
            var zaehlerSet = FillZaehlerSet(ctx, umlagen);
            var zaehlerstaende = FillZaehlerstaende(ctx, vertraege);
            var betriebskostenrechnungen = FillBetriebskostenrechnungen(ctx);

            Console.WriteLine("Lade erzeugte Daten in Datenbank...");
            await ctx.SaveChangesAsync();
            Console.WriteLine("Fertig!");
        }

        private static void CreateUserAccount(SaverwalterContext ctx, string databaseUser, string databasePass)
        {
            Console.WriteLine($"Erstelle Nutzer mit Nutzernamen {databaseUser} und Passwort {databasePass}");
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
        }

        static List<Adresse> FillAdressen(SaverwalterContext ctx)
        {
            Console.Write("Füge Adressen hinzu: ");

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
            Console.WriteLine($"{adressen.Count} Adressen hinzugefügt");

            return adressen;
        }

        static List<JuristischePerson> FillJuristischePersonen(SaverwalterContext ctx, List<Adresse> adressen)
        {
            Console.Write("Füge juristische Personen hinzu: ");

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
            Console.WriteLine($"{juristischePersonen.Count} juristische Personen hinzugefügt");

            return juristischePersonen;
        }

        static List<NatuerlichePerson> FillNatuerlichePersonen(SaverwalterContext ctx, List<Adresse> adressen)
        {
            Console.Write("Füge natürliche Personen hinzu: ");

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
            Console.WriteLine($"{natuerlichePersonen.Count} natürliche Personen hinzugefügt");

            return natuerlichePersonen;
        }

        static List<Wohnung> FillWohnungen(SaverwalterContext ctx, List<Adresse> adressen, List<NatuerlichePerson> natuerlichePersonen, List<JuristischePerson> juristischePersonen)
        {
            Console.Write("Füge Wohnungen hinzu: ");

            var wohnungen = new List<Wohnung> { };

            var personIds = natuerlichePersonen.Select(e => e.PersonId)
                .Concat(juristischePersonen.Select(e => e.PersonId)).ToList();

            for (var i = 0; i < 100; i++)
            {
                var adresse = adressen[i * 3 % adressen.Count];
                // Should run 200 times
                for (var j = 1; j < (i % 5) + 1; j++)
                {
                    var bezeichnung = $"Wohnung Nr. {j}";
                    wohnungen.Add(new Wohnung(bezeichnung, i * 2, i * 2, 1)
                    {
                        Adresse = adresse,
                        BesitzerId = personIds[i % personIds.Count],
                    });
                }
            }

            ctx.Wohnungen.AddRange(wohnungen);
            Console.WriteLine($"{wohnungen.Count} Wohnungen hinzugefügt");

            return wohnungen;
        }

        static List<Erhaltungsaufwendung> FillErhaltungsaufwendungen(SaverwalterContext ctx, List<Wohnung> wohnungen, List<JuristischePerson> juristischePersonen)
        {
            Console.Write("Füge Erhaltungsaufwendungen hinzu: ");

            var erhaltungsaufwendungen = new List<Erhaltungsaufwendung> { };

            for (var i = 0; i < 3000; i++)
            {
                var betrag = 100;
                var bezeichnung = $"Rechnungsnr. {i}";
                var ausstellerId = juristischePersonen[i % juristischePersonen.Count].PersonId;
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
            Console.WriteLine($"{erhaltungsaufwendungen.Count} Erhaltungsaufwendungen hinzugefügt");

            return erhaltungsaufwendungen;
        }

        static List<Vertrag> FillVertraege(
           SaverwalterContext ctx,
           List<Wohnung> wohnungen,
           List<NatuerlichePerson> natuerlichePersonen,
           List<JuristischePerson> juristischePersonen)
        {
            Console.Write("Füge Verträge hinzu: ");

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

                vertraege.Add(vertrag);
            }

            ctx.Vertraege.AddRange(vertraege);
            Console.WriteLine($"{vertraege.Count} Verträge hinzugefügt");

            return vertraege;
        }

        static List<VertragVersion> FillVertragversionen(SaverwalterContext ctx, List<Vertrag> vertraege)
        {
            Console.Write("Füge Vertragversionen hinzu: ");

            List<VertragVersion> vertragVersionen = new List<VertragVersion> { };

            for (var i = 0; i < vertraege.Count; ++i)
            {
                var vertrag = vertraege[i];
                var wohnung = vertrag.Wohnung;
                var ende = vertrag.Ende ?? globalToday;

                for (var j = 0; j <= (j + 1) % 3; ++j)
                {
                    var grundmiete = wohnung.Wohnflaeche * (6 + i % 3 + j % 3);
                    var personenzahl = (i + 1) % 3 * (j + 1) % 3;
                    var length = Math.Abs((vertraege.Count - i) * (i / 2) * (j + 1));
                    var beginn = ende.AddMonths(-3).AddDays(-length);
                    vertragVersionen.Add(new VertragVersion(beginn, grundmiete, personenzahl)
                    {
                        Vertrag = vertrag
                    });
                }
            }

            ctx.VertragVersionen.AddRange(vertragVersionen);
            Console.WriteLine($"{vertragVersionen.Count} Vertragversionen hinzugefügt");

            return vertragVersionen;
        }

        static List<Miete> FillMieten(SaverwalterContext ctx, List<Vertrag> vertraege)
        {
            Console.Write("Füge Mieten hinzu: ");

            var mieten = new List<Miete> { };

            foreach (var vertrag in vertraege)
            {
                foreach (var version in vertrag.Versionen)
                {
                    var ende = version.Ende() ?? globalToday;

                    for (DateOnly date = version.Beginn; date <= ende; date = date.AddMonths(1))
                    {
                        mieten.Add(new Miete(date, date, version.Grundmiete + 300 + (date.Day % 3) * 100)
                        {
                            Vertrag = vertrag
                        });
                    }
                }
            }

            ctx.Mieten.AddRange(mieten);
            Console.WriteLine($"{mieten.Count} Mieten hinzugefügt");

            return mieten;
        }

        static List<Garage> FillGaragen(SaverwalterContext ctx)
        {
            Console.Write("Füge Garagen hinzu: ");

            var garagen = new List<Garage> { };

            // TODO still empty...
            ctx.Garagen.AddRange(garagen);
            Console.WriteLine($"{garagen.Count} Garagen hinzugefügt");

            return garagen;
        }

        static List<Konto> FillKontos(SaverwalterContext ctx)
        {
            Console.Write("Füge Kontos hinzu: ");

            var kontos = new List<Konto> { };

            // TODO still empty...

            ctx.Kontos.AddRange(kontos);
            Console.WriteLine($"{kontos.Count} Kontos hinzugefügt");

            return kontos;
        }

        static List<Mietminderung> FillMietminderungen(SaverwalterContext ctx)
        {
            Console.Write("Füge Mietminderungen hinzu: ");

            var mietminderungen = new List<Mietminderung> { };

            // TODO still empty...

            ctx.Mietminderungen.AddRange(mietminderungen);
            Console.WriteLine($"{mietminderungen.Count} Mietminderungen hinzugefügt");

            return mietminderungen;
        }

        static List<Betriebskostenrechnung> FillBetriebskostenrechnungen(SaverwalterContext ctx)
        {
            Console.Write("Füge Betriebskostenrechnung hinzu: ");

            var betriebskostenrechnungen = new List<Betriebskostenrechnung> { };

            // TODO

            ctx.Betriebskostenrechnungen.AddRange(betriebskostenrechnungen);
            Console.WriteLine($"{betriebskostenrechnungen.Count} Betriebskostenrechnungen hinzugefügt");

            return betriebskostenrechnungen;
        }

        private static List<Mieter> FillMieterSet(SaverwalterContext ctx)
        {
            Console.Write("Füge Mieter hinzu: ");

            var mieter = new List<Mieter> { };

            // TODO

            ctx.MieterSet.AddRange(mieter);
            Console.WriteLine($"{mieter.Count} Mieter hinzugefügt");

            return mieter;
        }

        private static Umlage addUmlage(
            Adresse adresse,
            Betriebskostentyp typ,
            Umlageschluessel schluessel)
        {
            var umlage = new Umlage(typ, schluessel)
            {
                Beschreibung = $"{typ.ToDescriptionString()} wird über die Stadt {adresse.Stadt} abgerechnet.",
                Wohnungen = adresse.Wohnungen,
            };

            return umlage;
        }

        private static List<Umlage> FillUmlagen(SaverwalterContext ctx, List<Adresse> adressen)
        {
            Console.Write("Füge Umlagen hinzu: ");

            var umlagen = new List<Umlage> { };

            for (var i = 0; i < adressen.Count; ++i)
            {
                var adresse = adressen[i];

                umlagen.Add(addUmlage(adresse, Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachWohnflaeche));

                if (i % 10 == 0)
                {
                    umlagen.Add(addUmlage(adresse, Betriebskostentyp.Breitbandkabelanschluss, Umlageschluessel.NachNutzeinheit));
                }

                umlagen.Add(addUmlage(adresse, Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachWohnflaeche));

                umlagen.Add(addUmlage(adresse, Betriebskostentyp.EntwaesserungNiederschlagswasser, Umlageschluessel.NachWohnflaeche));

                // Can also use Personen
                umlagen.Add(addUmlage(adresse, Betriebskostentyp.EntwaesserungSchmutzwasser, Umlageschluessel.NachVerbrauch));

                if (i % 2 == 0)
                {
                    umlagen.Add(addUmlage(adresse, Betriebskostentyp.Gartenpflege, Umlageschluessel.NachWohnflaeche));
                }

                // Can also be made direct
                umlagen.Add(addUmlage(adresse, Betriebskostentyp.Grundsteuer, Umlageschluessel.NachWohnflaeche));

                umlagen.Add(addUmlage(adresse, Betriebskostentyp.Haftpflichtversicherung, Umlageschluessel.NachWohnflaeche));

                if (i % 3 == 0)
                {
                    umlagen.Add(addUmlage(adresse, Betriebskostentyp.Hauswartarbeiten, Umlageschluessel.NachWohnflaeche));
                }

                if (i % 3 == 0 || i % 7 == 0 || i % 11 == 0)
                {
                    umlagen.Add(addUmlage(adresse, Betriebskostentyp.Heizkosten, Umlageschluessel.NachVerbrauch));
                    umlagen.Add(addUmlage(adresse, Betriebskostentyp.WartungThermenSpeicher, Umlageschluessel.NachWohnflaeche));
                }

                umlagen.Add(addUmlage(adresse, Betriebskostentyp.Muellbeseitigung, Umlageschluessel.NachPersonenzahl));

                umlagen.Add(addUmlage(adresse, Betriebskostentyp.Sachversicherung, Umlageschluessel.NachWohnflaeche));
                umlagen.Add(addUmlage(adresse, Betriebskostentyp.SchornsteinfegerarbeitenKalt, Umlageschluessel.NachNutzeinheit));

                umlagen.Add(addUmlage(adresse, Betriebskostentyp.Strassenreinigung, Umlageschluessel.NachWohnflaeche));

                umlagen.Add(addUmlage(adresse, Betriebskostentyp.WasserversorgungKalt, Umlageschluessel.NachVerbrauch));
                // umlagen.Add(addUmlage(adresse, Betriebskostentyp.WasserversorgungWarm, Umlageschluessel.NachVerbrauch))
            }

            ctx.Umlagen.AddRange(umlagen);
            Console.WriteLine($"{umlagen.Count} Umlagen hinzugefügt");

            return umlagen;
        }

        static List<Zaehler> FillZaehlerSet(SaverwalterContext ctx, List<Umlage> umlagen)
        {
            Console.Write("Füge Zähler hinzu: ");

            var zaehler = new List<Zaehler> { };

            foreach (var umlage in umlagen)
            {
                if (umlage.Typ == Betriebskostentyp.WasserversorgungKalt)
                {
                    foreach (var wohnung in umlage.Wohnungen)
                    {
                        var kaltKennung = "Kaltwasserzähler " + wohnung.Bezeichnung;
                        zaehler.Add(new Zaehler(kaltKennung, Zaehlertyp.Kaltwasser)
                        {
                            Wohnung = wohnung
                        });
                        var warmKennung = "Warmwasserzähler " + wohnung.Bezeichnung;
                        zaehler.Add(new Zaehler(warmKennung, Zaehlertyp.Warmwasser)
                        {
                            Wohnung = wohnung
                        });
                    }
                }
                // Vielleicht brauchen die Zähler?
                if (umlage.Typ == Betriebskostentyp.Heizkosten)
                {
                    var kennung = "Allgemein Heizung" + umlage.GetWohnungenBezeichnung();
                    zaehler.Add(new Zaehler(kennung, Zaehlertyp.Gas)
                    {
                    });

                    foreach (var wohnung in umlage.Wohnungen)
                    {
                        var kennungEinzeln = "Heizzähler " + wohnung.Bezeichnung;
                        zaehler.Add(new Zaehler(kennungEinzeln, Zaehlertyp.Gas)
                        {
                            Wohnung = wohnung
                        });
                    }
                }
            }

            ctx.ZaehlerSet.AddRange(zaehler);
            Console.WriteLine($"{zaehler.Count} Zähler hinzugefügt");

            return zaehler;
        }

        static List<Zaehlerstand> FillZaehlerstaende(SaverwalterContext ctx, List<Vertrag> vertraege)
        {
            Console.Write("Füge Zählerstände hinzu: ");

            var zaehlerstaende = new List<Zaehlerstand> { };

            foreach (var vertrag in vertraege)
            {
                var beginn = vertrag.Beginn();
                var ende = vertrag.Ende ?? globalToday;

                // TODO wenn der Vertrag früher endet muss entsprechend noch ein Zählerstand dazwischen.
                for (var date = beginn; date <= ende; date = date.AddYears(1))
                {
                    foreach (var zaehler in vertrag.Wohnung.Zaehler)
                    {
                        var lastStand = zaehler.Staende.ToList().OrderBy(e => e.Datum).LastOrDefault();
                        var max = zaehler.Typ switch
                        {
                            Zaehlertyp.Warmwasser => 50,
                            Zaehlertyp.Kaltwasser => 100,
                            Zaehlertyp.Strom => 1000,
                            Zaehlertyp.Gas => 20000,
                            _ => 10000
                        };
                        var lastStandStand = lastStand?.Stand ?? (double)0;
                        var stand = lastStandStand += date.DayNumber % max;
                        zaehlerstaende.Add(new Zaehlerstand(date, stand)
                        {
                            Zaehler = zaehler
                        });
                    }
                }
            }

            ctx.Zaehlerstaende.AddRange(zaehlerstaende);
            Console.WriteLine($"{zaehlerstaende.Count} Zählerstände hinzugefügt");

            return zaehlerstaende;
        }
    }
}
