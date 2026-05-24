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

using System.Security.Cryptography;
using System.Text;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;

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
            GenericData.FillUmlagetypen(ctx);

            CreateUserAccount(ctx, databaseUser, databasePass);
            var adressen = FillAdressen(ctx);
            var wohnungen = FillWohnungen(ctx, adressen);
            var vertraege = FillVertraege(ctx, wohnungen);
            var vertragVersionen = FillVertragversionen(ctx, vertraege);
            var mieten = FillMieten(ctx, vertraege);
            var erhaltungsaufwendungen = FillErhaltungsaufwendungen(ctx, wohnungen);

            // Still empty
            var mietminderungen = FillMietminderungen(ctx);
            var bankkontos = FillBankkontos(ctx);
            var garagen = FillGaragen(ctx);

            // TODO
            var umlagen = FillUmlagen(ctx, adressen);
            var (zaehlerSet, hauszaehlerHeizung) = FillZaehlerSet(ctx, umlagen);
            var zaehlerstaende = FillZaehlerstaende(ctx, zaehlerSet, vertraege);
            FillHKVO(ctx, umlagen, hauszaehlerHeizung);
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

        private static Kontakt generateNatuerlichePerson(int seed)
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

            var person = new Kontakt(nachname, Rechtsform.natuerlich)
            {
                Vorname = vorname,
                Telefon = GetOne(GenericData.telefonnummerList, seed * 7),
                Email = $"{vorname}.{nachname}@{GetOne(GenericData.emailProvider, seed * 3).ToLower()}",
                Fax = GetOne(GenericData.telefonnummerList, seed * 21),
                // TODO Adresse
            };

            return person;
        }

        private static Kontakt generateJuristischePerson(int seed)
        {
            var name = GetOne(GenericData.companyNames, seed * 13);

            // TODO only gmbh
            var person = new Kontakt(name, Rechtsform.gmbh)
            {
                Email = name.Replace(" ", "_").ToLower() + (GenericData.emailProvider, seed * 7),
                Telefon = GetOne(GenericData.telefonnummerList, seed * 7),
                // TODO Adresse
            };

            return person;
        }

        private static List<Wohnung> FillWohnungen(SaverwalterContext ctx, List<Adresse> adressen)
        {
            Console.Write("Füge Wohnungen hinzu: ");

            var wohnungen = new List<Wohnung> { };
            // TODO: only one besitzer?
            var besitzer = generateNatuerlichePerson(3);
            ctx.Kontakte.Add(besitzer);

            for (var i = 0; i < 100; i++)
            {

                var adresse = adressen[i * 3 % adressen.Count];
                // Should run 200 times
                for (var j = 1; j < (i % 5) + 1; ++j)
                {
                    var bezeichnung = $"Wohnung Nr. {j}";
                    var flaeche = 35 + (j * 35);
                    var wIdx = wohnungen.Count;
                    var wohnung = new Wohnung(bezeichnung)
                    {
                        Adresse = adresse,
                        Besitzer = besitzer,
                        MietErtragskonto = new Buchungskonto($"W{wIdx:D5}-M", $"Mieterlöse {bezeichnung}", BuchungskontoTyp.Ertrag),
                        AufwandsKonto = new Buchungskonto($"W{wIdx:D5}-E", $"Erhaltungsaufwand {bezeichnung}", BuchungskontoTyp.Aufwand),
                    };
                    wohnung.Versionen.Add(new WohnungVersion(new DateOnly(2000, 1, 1), flaeche, flaeche, flaeche, 1) { Wohnung = wohnung });
                    wohnungen.Add(wohnung);
                }
            }

            ctx.Wohnungen.AddRange(wohnungen);
            Console.WriteLine($"{ctx.Kontakte.Count()} Kontakte hinzugefügt,.");
            Console.WriteLine($"{wohnungen.Count} Wohnungen hinzugefügt");

            return wohnungen;
        }

        static List<Erhaltungsaufwendung> FillErhaltungsaufwendungen(SaverwalterContext ctx, List<Wohnung> wohnungen)
        {
            Console.Write("Füge Erhaltungsaufwendungen hinzu: ");

            var erhaltungsaufwendungen = new List<Erhaltungsaufwendung> { };
            // TODO only one aussteller...
            var aussteller = generateJuristischePerson(11);

            for (var i = 0; i < 3000; i++)
            {
                var betrag = 100;
                var bezeichnung = $"Rechnungsnr. {i}";
                var jahr = 2020 + i % 5;
                var monat = 1 + i % 12;
                var tag = 1 + i % DateTime.DaysInMonth(jahr, monat);
                var datum = new DateOnly(jahr, monat, tag);
                var wohnung = wohnungen[i % wohnungen.Count];

                erhaltungsaufwendungen.Add(new Erhaltungsaufwendung(betrag, bezeichnung, datum)
                {
                    Aussteller = aussteller,
                    Wohnung = wohnung
                });
            }

            ctx.Erhaltungsaufwendungen.AddRange(erhaltungsaufwendungen);
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
                DateOnly ende = new DateOnly(2022, i % 12 + 1, 1);
                if (i % 3 == 0)
                {
                    var jahr = 2020 - (i % 5);
                    var monat = 1 + i % 12;
                    var tag = 1 + i % DateTime.DaysInMonth(jahr, monat);
                    ende = new DateOnly(jahr, monat, tag);
                }

                var vIdx = vertraege.Count;
                var vertrag = new Vertrag()
                {
                    Ansprechpartner = wohnung.Besitzer, // TODO add some variation. Maybe a chance to add a new person
                    Ende = ende,
                    Wohnung = wohnung,
                    MietBuchungskonto = new Buchungskonto($"V{vIdx:D5}-MB", "Mietforderungen", BuchungskontoTyp.Aktiv),
                    NkBuchungskonto = new Buchungskonto($"V{vIdx:D5}-NK", "NK-Vorauszahlungen", BuchungskontoTyp.Passiv),
                    KautionsKonto = new Buchungskonto($"V{vIdx:D5}-KA", "Kaution", BuchungskontoTyp.Aktiv),
                    BkAbrechnungsKonto = new Buchungskonto($"V{vIdx:D5}-BK", "BK-Abrechnung", BuchungskontoTyp.Aktiv),
                    ZahlungsKonto = new Buchungskonto($"V{vIdx:D5}-ZK", "Zahlung", BuchungskontoTyp.Aktiv),
                    MietminderungsKonto = new Buchungskonto($"V{vIdx:D5}-MM", "Mietminderung", BuchungskontoTyp.Aufwand),
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
                    var grundmiete = wohnung.VersionAt(ende).Wohnflaeche * (6 - i % 3 + j % 3);
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
                        mieten.Add(new Miete(date, date, (double)(version.Grundmiete + 200 + (date.Day % 3) * 50))
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

        static List<Bankkonto> FillBankkontos(SaverwalterContext ctx)
        {
            Console.Write("Füge Bankkontos hinzu: ");

            var bankkontos = new List<Bankkonto>();

            // TODO still empty...

            ctx.Bankkontos.AddRange(bankkontos);
            Console.WriteLine($"{bankkontos.Count} Bankkontos hinzugefügt");

            return bankkontos;
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
                    var abrechnungsEnde = new DateOnly(date.Year, 12, 31);
                    decimal betrag = 100 + beginn.DayOfYear;
                    if (umlage.Typ == GetTyp(ctx, "Heizkosten"))
                    {
                        betrag = umlage.Wohnungen.Sum(e => e.VersionAt(abrechnungsEnde).Wohnflaeche) * 12 + beginn.DayOfYear;
                    }
                    else if (umlage.Typ == GetTyp(ctx, "Grundsteuer"))
                    {
                        betrag = umlage.Wohnungen.Sum(e => e.VersionAt(abrechnungsEnde).Wohnflaeche) * 5;
                    }
                    else if (umlage.Typ == GetTyp(ctx, "Entwässerung/Schmutzwasser")
                        || umlage.Typ == GetTyp(ctx, "Wasserversorgung"))
                    {
                        betrag = umlage.Wohnungen.Sum(e => e.VersionAt(abrechnungsEnde).Wohnflaeche) * 5 + beginn.DayOfYear;
                    }
                    var satz = new Buchungssatz(
                        date,
                        $"BK-Eingang {umlage.Typ.Bezeichnung} {date.Year}");
                    satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
                    {
                        Buchungssatz = satz,
                        Buchungskonto = umlage.NkVerrechnungsKonto
                    });

                    betriebskostenrechnungen.Add(new Betriebskostenrechnung(betrag, date, date.Year)
                    {
                        Umlage = umlage,
                        Buchungssatz = satz
                    });
                }
            }

            ctx.Betriebskostenrechnungen.AddRange(betriebskostenrechnungen);
            Console.WriteLine($"{betriebskostenrechnungen.Count} Betriebskostenrechnungen hinzugefügt");

            return betriebskostenrechnungen;
        }

        private static Umlage addUmlage(
            Adresse adresse,
            Umlagetyp typ,
            Umlageschluessel schluessel,
            int idx)
        {
            var umlage = new Umlage
            {
                Typ = typ,
                Beschreibung = $"{typ.Bezeichnung} wird über die Stadt {adresse.Stadt} abgerechnet.",
                Wohnungen = adresse.Wohnungen,
                NkVerrechnungsKonto = new Buchungskonto($"U{idx:D5}-NR", $"NK-Verrechnung {typ.Bezeichnung}", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto($"U{idx:D5}-ZK", $"NK-Zahlung {typ.Bezeichnung}", BuchungskontoTyp.Aktiv),
            };
            umlage.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), schluessel) { Umlage = umlage });

            return umlage;
        }

        private static Umlagetyp GetTyp(SaverwalterContext ctx, string bezeichnung)
        {
            return ctx.Umlagetypen.First(typ => typ.Bezeichnung == bezeichnung);
        }

        private static List<Umlage> FillUmlagen(SaverwalterContext ctx, List<Adresse> adressen)
        {
            Console.Write("Füge Umlagen hinzu: ");

            var umlagen = new List<Umlage> { };

            for (var i = 0; i < adressen.Count; ++i)
            {
                var adresse = adressen[i];

                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Allgemeinstrom/Hausbeleuchtung"), Umlageschluessel.NachWohnflaeche, umlagen.Count));

                if (i % 10 == 0)
                {
                    umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Breitbandkabelanschluss"), Umlageschluessel.NachNutzeinheit, umlagen.Count));
                }

                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Dachrinnenreinigung"), Umlageschluessel.NachWohnflaeche, umlagen.Count));

                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Müllbeseitigung"), Umlageschluessel.NachWohnflaeche, umlagen.Count));

                // Niederschlagswasser wird nach Fläche abgerechnet (kein Zähler)
                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Entwässerung/Niederschlagswasser"), Umlageschluessel.NachWohnflaeche, umlagen.Count));

                if (i % 2 == 0)
                {
                    // Schmutzwasser nach Verbrauch (teilt sich Kaltwasserzähler mit Wasserversorgung)
                    umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Entwässerung/Schmutzwasser"), Umlageschluessel.NachVerbrauch, umlagen.Count));
                }

                // Can also be made direct
                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Gartenpflege"), Umlageschluessel.NachWohnflaeche, umlagen.Count));

                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Grundsteuer"), Umlageschluessel.NachWohnflaeche, umlagen.Count));

                if (i % 3 == 0)
                {
                    umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Haftpflichtversicherung"), Umlageschluessel.NachWohnflaeche, umlagen.Count));
                }

                if (i % 3 == 0 || i % 7 == 0 || i % 11 == 0)
                {
                    umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Heizkosten"), Umlageschluessel.NachVerbrauch, umlagen.Count));
                    umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Betriebsstrom (Heizung)"), Umlageschluessel.NachWohnflaeche, umlagen.Count));
                    umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Wartung Therme/Speicher"), Umlageschluessel.NachWohnflaeche, umlagen.Count));
                }

                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Müllbeseitigung"), Umlageschluessel.NachPersonenzahl, umlagen.Count));

                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Sachversicherung"), Umlageschluessel.NachWohnflaeche, umlagen.Count));
                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Schornsteinfegerarbeiten"), Umlageschluessel.NachNutzeinheit, umlagen.Count));

                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Strassenreinigung"), Umlageschluessel.NachWohnflaeche, umlagen.Count));

                umlagen.Add(addUmlage(adresse, GetTyp(ctx, "Wasserversorgung"), Umlageschluessel.NachVerbrauch, umlagen.Count));
                // umlagen.Add(addUmlage(adresse, Betriebskostentyp.WasserversorgungWarm, Umlageschluessel.NachVerbrauch))
            }

            ctx.Umlagen.AddRange(umlagen);
            Console.WriteLine($"{umlagen.Count} Umlagen hinzugefügt");

            return umlagen;
        }

        static (List<Zaehler>, Dictionary<Adresse, Zaehler>) FillZaehlerSet(SaverwalterContext ctx, List<Umlage> umlagen)
        {
            Console.Write("Füge Zähler hinzu: ");

            var zaehlerListe = new List<Zaehler>();
            var wohnungszaehler = new Dictionary<(Wohnung, Zaehlertyp), Zaehler>();
            var hauszaehlerHeizung = new Dictionary<Adresse, Zaehler>();

            Zaehler GetOrAddWohnungszaehler(Wohnung wohnung, Zaehlertyp typ, string kennung)
            {
                var key = (wohnung, typ);
                if (!wohnungszaehler.TryGetValue(key, out var existing))
                {
                    existing = new Zaehler(kennung, typ) { Wohnung = wohnung };
                    zaehlerListe.Add(existing);
                    wohnungszaehler[key] = existing;
                }
                return existing;
            }

            foreach (var umlage in umlagen)
            {
                var bezeichnung = umlage.Typ.Bezeichnung;

                if (bezeichnung == "Wasserversorgung" || bezeichnung == "Entwässerung/Schmutzwasser")
                {
                    foreach (var wohnung in umlage.Wohnungen)
                    {
                        var prefix = $"{wohnung.Adresse?.Strasse ?? "?"} – {wohnung.Bezeichnung}";
                        var kw = GetOrAddWohnungszaehler(wohnung, Zaehlertyp.Kaltwasser, $"KW {prefix}");
                        var ww = GetOrAddWohnungszaehler(wohnung, Zaehlertyp.Warmwasser, $"WW {prefix}");
                        if (!umlage.Zaehler.Contains(kw)) umlage.Zaehler.Add(kw);
                        if (!umlage.Zaehler.Contains(ww)) umlage.Zaehler.Add(ww);
                    }
                }

                if (bezeichnung == "Heizkosten")
                {
                    var adresse = umlage.Wohnungen.FirstOrDefault()?.Adresse;
                    if (adresse != null && !hauszaehlerHeizung.ContainsKey(adresse))
                    {
                        var hauszaehler = new Zaehler($"Heizung Haus {adresse.Strasse}", Zaehlertyp.Gas) { Adresse = adresse };
                        zaehlerListe.Add(hauszaehler);
                        hauszaehlerHeizung[adresse] = hauszaehler;
                    }

                    foreach (var wohnung in umlage.Wohnungen)
                    {
                        var prefix = $"{wohnung.Adresse?.Strasse ?? "?"} – {wohnung.Bezeichnung}";
                        var hz = GetOrAddWohnungszaehler(wohnung, Zaehlertyp.Gas, $"HZ {prefix}");
                        var ww = GetOrAddWohnungszaehler(wohnung, Zaehlertyp.Warmwasser, $"WW {prefix}");
                        if (!umlage.Zaehler.Contains(hz)) umlage.Zaehler.Add(hz);
                        if (!umlage.Zaehler.Contains(ww)) umlage.Zaehler.Add(ww);
                    }
                }
            }

            ctx.ZaehlerSet.AddRange(zaehlerListe);
            Console.WriteLine($"{zaehlerListe.Count} Zähler hinzugefügt");
            return (zaehlerListe, hauszaehlerHeizung);
        }

        static void FillHKVO(
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
                    AllgemeinWaerme = hauszaehler,
                };
                heizUmlage.HeizkostenHKVOs.Add(hkvo);
                hkvoList.Add(hkvo);
            }

            ctx.HKVO.AddRange(hkvoList);
            Console.WriteLine($"{hkvoList.Count} HKVO-Einträge hinzugefügt");
        }

        static List<Zaehlerstand> FillZaehlerstaende(
            SaverwalterContext ctx,
            List<Zaehler> zaehlerSet,
            List<Vertrag> vertraege)
        {
            Console.Write("Füge Zählerstände hinzu: ");

            var zaehlerstaende = new List<Zaehlerstand>();

            // Build lookup: Wohnung → earliest contract start, so we know when to begin readings
            var fruehesterBeginnByWohnung = vertraege
                .Where(v => v.Wohnung != null)
                .GroupBy(v => v.Wohnung)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(v => v.Beginn())
                          .Where(b => b != default)
                          .DefaultIfEmpty(globalToday.AddYears(-3))
                          .Min());

            foreach (var zaehler in zaehlerSet)
            {
                DateOnly rangeStart;
                if (zaehler.Wohnung != null)
                {
                    if (!fruehesterBeginnByWohnung.TryGetValue(zaehler.Wohnung, out rangeStart)
                        || rangeStart == default)
                    {
                        continue; // Wohnung hat keine Verträge – kein Zählerstand nötig
                    }
                }
                else
                {
                    // Hauszähler (z.B. Gas-Gesamtzähler Heizung) – gesamten Zeitraum abdecken
                    rangeStart = globalToday.AddYears(-6);
                }

                var wohnungsAnzahl = zaehler.Adresse?.Wohnungen.Count ?? 1;
                var tagesVerbrauch = zaehler.Typ switch
                {
                    Zaehlertyp.Warmwasser => 0.05m,                      // ~18 m³/Jahr pro Wohnung
                    Zaehlertyp.Kaltwasser => 0.18m,                      // ~65 m³/Jahr pro Wohnung
                    Zaehlertyp.Gas when zaehler.Wohnung == null => 25m * wohnungsAnzahl, // Hauszähler: Summe aller Wohnungen
                    Zaehlertyp.Gas => 25m,                               // ~9000 kWh/Jahr pro Wohnung
                    Zaehlertyp.Strom => 1.2m,                            // ~440 kWh/Jahr
                    _ => 0.5m
                };

                var stand = zaehler.Typ switch
                {
                    Zaehlertyp.Warmwasser => 100m + (rangeStart.DayOfYear % 200),
                    Zaehlertyp.Kaltwasser => 200m + (rangeStart.DayOfYear % 500),
                    Zaehlertyp.Gas => 5000m + (rangeStart.DayOfYear % 10000),
                    Zaehlertyp.Strom => 1000m + (rangeStart.DayOfYear % 3000),
                    _ => 100m
                };

                // One reading per Dec 31 plus one at contract start
                var stichtage = new SortedSet<DateOnly> { rangeStart };
                for (var jahr = rangeStart.Year; jahr <= globalToday.Year; jahr++)
                {
                    var jahresende = new DateOnly(jahr, 12, 31);
                    if (jahresende >= rangeStart && jahresende <= globalToday)
                    {
                        stichtage.Add(jahresende);
                    }
                }

                DateOnly? letzterTag = null;
                foreach (var stichtag in stichtage)
                {
                    var tage = letzterTag.HasValue
                        ? Math.Max(1, stichtag.DayNumber - letzterTag.Value.DayNumber)
                        : 1;
                    stand = Math.Round(stand + tagesVerbrauch * tage, 2);
                    zaehlerstaende.Add(new Zaehlerstand(stichtag, stand) { Zaehler = zaehler });
                    letzterTag = stichtag;
                }
            }

            ctx.Zaehlerstaende.AddRange(zaehlerstaende);
            Console.WriteLine($"{zaehlerstaende.Count} Zählerstände hinzugefügt");
            return zaehlerstaende;
        }
    }
}
