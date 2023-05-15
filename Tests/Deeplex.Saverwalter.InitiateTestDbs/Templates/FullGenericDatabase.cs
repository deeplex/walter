﻿using Deeplex.Saverwalter.Model;
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
            var wohnungen = FillWohnungen(ctx, adressen);
            var vertraege = FillVertraege(ctx, wohnungen);
            var vertragVersionen = FillVertragversionen(ctx, vertraege);
            var mieten = FillMieten(ctx, vertraege);
            var erhaltungsaufwendungen = FillErhaltungsaufwendungen(ctx, wohnungen);

            // Still empty
            var mietminderungen = FillMietminderungen(ctx);
            var kontos = FillKontos(ctx);
            var garagen = FillGaragen(ctx);

            // TODO
            var umlagen = FillUmlagen(ctx, adressen);
            var mieterSet = FillMieterSet(ctx, vertraege);
            var zaehlerSet = FillZaehlerSet(ctx, umlagen);
            var zaehlerstaende = FillZaehlerstaende(ctx, vertraege);
            var betriebskostenrechnungen = FillBetriebskostenrechnungen(ctx, umlagen);

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
            var postleitzahlList = GenericData.postleitzahlList;
            var stadtList = GenericData.stadtList;

            for (int i = 0; i < 100; ++i)
            {
                var strasse = GetOne(strasseList, i);
                var hausnummer = (Math.Ceiling((double)i * 3 / 2) + 1).ToString();
                var postleitzahl = GetOne(postleitzahlList, i);
                var stadt = GetOne(stadtList, i);

                adressen.Add(new Adresse(strasse, hausnummer, postleitzahl, stadt));
            }

            ctx.Adressen.AddRange(adressen);
            Console.WriteLine($"{adressen.Count} Adressen hinzugefügt");

            return adressen;
        }

        private static T GetOne<T>(List<T> list, int seed)
        {
            return list[seed * 37 % list.Count];
        }

        private static NatuerlichePerson generateNatuerlichePerson(int seed)
        {
            var nachname = GetOne(GenericData.lastNames, seed * 13);

            var anrede = seed % 50 == 0 || seed % 49 == 0 ?
                Anrede.Keine : seed % 2 == 0 ? Anrede.Herr : Anrede.Frau;

            string vorname = "";
            if (anrede == Anrede.Herr || seed % 50 == 0)
            {
                vorname = GetOne(GenericData.FirstNamesMale, seed * 17);
            }
            else if (anrede == Anrede.Frau || seed % 49 == 0)
            {
                vorname = GetOne(GenericData.FirstNamesFemale, seed * 19);
            }

            var person = new NatuerlichePerson(nachname)
            {
                Vorname = vorname,
                Telefon = GetOne(GenericData.telefonnummerList, seed * 7),
                Email = $"{vorname}.{nachname}@{GetOne(GenericData.emailProvider, seed * 3).ToLower()}",
                Fax = GetOne(GenericData.telefonnummerList, seed * 21),
                // TODO Adresse
            };

            return person;
        }

        private static JuristischePerson generateJuristischePerson(int seed)
        {
            var name = GetOne(GenericData.companyNames, seed * 13);

            var person = new JuristischePerson(name)
            {
                Email = name.Replace(" ", "_").ToLower() + (GenericData.emailProvider, seed * 7),
                Telefon = GetOne(GenericData.telefonnummerList, seed * 7),
                // TODO Adresse
            };

            return person;
        }

        private static bool isPrime(int number)
        {
            if (number == 1) return false;
            if (number == 2) return true;

            var limit = Math.Ceiling(Math.Sqrt(number));

            for (int i = 2; i <= limit; ++i)
                if (number % i == 0)
                    return false;
            return true;
        }

        private static List<Wohnung> FillWohnungen(SaverwalterContext ctx, List<Adresse> adressen)
        {
            Console.Write("Füge Wohnungen hinzu: ");

            var wohnungen = new List<Wohnung> { };
            var besitzerIdList = new List<Guid> { };

            for (var i = 0; i < 100; i++)
            {
                Guid besitzerId;
                if (i != 0 && (i % 2 == 0 || isPrime(i)))
                {
                    besitzerId = besitzerIdList.Last();
                }
                else
                {
                    if (isPrime(i))
                    {
                        var besitzer = generateJuristischePerson(i * 3);
                        ctx.JuristischePersonen.Add(besitzer);
                        besitzerIdList.Add(besitzer.PersonId);
                        besitzerId = besitzer.PersonId;
                    }
                    else
                    {
                        var besitzer = generateNatuerlichePerson(i * 3);
                        ctx.NatuerlichePersonen.Add(besitzer);
                        besitzerIdList.Add(besitzer.PersonId);
                        besitzerId = besitzer.PersonId;
                    }
                }

                var adresse = adressen[i * 3 % adressen.Count];
                // Should run 200 times
                for (var j = 1; j < (i % 5) + 1; ++j)
                {
                    var bezeichnung = $"Wohnung Nr. {j}";
                    var flaeche = 35 + (j * 35);
                    wohnungen.Add(new Wohnung(bezeichnung, i * 2, i * 2, 1)
                    {
                        Adresse = adresse,
                        BesitzerId = besitzerId,
                        Wohnflaeche = flaeche,
                        Nutzflaeche = flaeche,
                        Nutzeinheit = 1
                    });
                }
            }

            ctx.Wohnungen.AddRange(wohnungen);
            Console.WriteLine($"{ctx.JuristischePersonen.Count()} juristische Personen hinzugefügt,.");
            Console.WriteLine($"{ctx.NatuerlichePersonen.Count()} natürliche Personen hinzugefügt,.");
            Console.WriteLine($"{wohnungen.Count} Wohnungen hinzugefügt");

            return wohnungen;
        }

        static List<Erhaltungsaufwendung> FillErhaltungsaufwendungen(SaverwalterContext ctx, List<Wohnung> wohnungen)
        {
            Console.Write("Füge Erhaltungsaufwendungen hinzu: ");

            var erhaltungsaufwendungen = new List<Erhaltungsaufwendung> { };

            var ausstellerIdList = new List<Guid> { };

            for (var i = 0; i < 3000; i++)
            {
                Guid ausstellerId;
                if (i < 100)
                {
                    var aussteller = generateJuristischePerson(11);
                    ctx.JuristischePersonen.Add(aussteller);
                    ausstellerIdList.Add(aussteller.PersonId);
                    ausstellerId = aussteller.PersonId;
                }
                else
                {
                    ausstellerId = ausstellerIdList[i % ausstellerIdList.Count];
                }
                var betrag = 100;
                var bezeichnung = $"Rechnungsnr. {i}";
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
            Console.WriteLine($"{ausstellerIdList.Count} juristische Personen hinzugefügt.");
            Console.WriteLine($"{erhaltungsaufwendungen.Count} Erhaltungsaufwendungen hinzugefügt");

            return erhaltungsaufwendungen;
        }

        static List<Vertrag> FillVertraege(
           SaverwalterContext ctx,
           List<Wohnung> wohnungen)
        {
            Console.Write("Füge Verträge hinzu: ");

            var vertraege = new List<Vertrag>();

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
                    AnsprechpartnerId = wohnung.BesitzerId, // TODO add some variation. Maybe a chance to add a new person
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

        private static DateOnly getEarliestDate(List<Wohnung> wohnungen)
        {
            DateOnly earliest = globalToday;

            foreach (var wohnung in wohnungen)
            {
                foreach (var vertrag in wohnung.Vertraege)
                {
                    var beginn = vertrag.Beginn();
                    if (beginn < earliest)
                    {
                        earliest = beginn;
                    }
                }
            }

            return earliest;
        }

        private static List<Betriebskostenrechnung> FillBetriebskostenrechnungen(
            SaverwalterContext ctx,
            List<Umlage> umlagen)
        {
            Console.Write("Füge Betriebskostenrechnung hinzu: ");

            var betriebskostenrechnungen = new List<Betriebskostenrechnung> { };

            foreach (var umlage in umlagen)
            {
                var beginn = getEarliestDate(umlage.Wohnungen.ToList());
                for (var date = beginn; date < globalToday; date = date.AddYears(1))
                {
                    var betrag = date.DayOfYear + date.Year - 700;
                    betriebskostenrechnungen.Add(new Betriebskostenrechnung(betrag, date, date.Year)
                    {
                        Umlage = umlage,
                    });
                }
            }

            ctx.Betriebskostenrechnungen.AddRange(betriebskostenrechnungen);
            Console.WriteLine($"{betriebskostenrechnungen.Count} Betriebskostenrechnungen hinzugefügt");

            return betriebskostenrechnungen;
        }

        private static List<Mieter> FillMieterSet(SaverwalterContext ctx, List<Vertrag> vertraege)
        {
            Console.Write("Füge Mieter hinzu: ");

            var mieter = new List<Mieter> { };

            for (var i = 0; i < vertraege.Count; ++i)
            {
                var anzahl = i % 5 + 1;

                for (var j = 0; j < anzahl; ++j)
                {
                    var person = generateNatuerlichePerson((i + 1) * (j + 1));
                    ctx.NatuerlichePersonen.Add(person);
                    mieter.Add(new Mieter(person.PersonId)
                    {
                        Vertrag = vertraege[i]
                    });
                }
            }

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
