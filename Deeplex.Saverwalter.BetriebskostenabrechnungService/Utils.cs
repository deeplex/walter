using Deeplex.Saverwalter.Model;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public static class Utils
    {
        public static T Max<T>(T l, T r) where T : IComparable<T>
            => Max(l, r, Comparer<T>.Default);
        public static T Max<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) < 0 ? r : l;

        public static T Min<T>(T l, T r) where T : IComparable<T>
            => Min(l, r, Comparer<T>.Default);
        public static T Min<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) > 0 ? r : l;

        public static string GetBriefAnrede(this IPerson person)
        {
            var text = "";
            if (person is NatuerlichePerson natuerlichePerson)
            {
                text += natuerlichePerson.Anrede == Anrede.Herr ? "Herrn " :
                    natuerlichePerson.Anrede == Anrede.Frau ? "Frau " :
                    "";
            }
            text += person.Bezeichnung;

            return text;
        }

        public static string Title(this Betriebskostenabrechnung abrechnung)
            => "Betriebskostenabrechnung " + abrechnung.Zeitraum.Jahr.ToString();

        public static string Mieterliste(this Betriebskostenabrechnung abrechnung)
            => "Mieter: " + string.Join(", ", abrechnung.Mieter.Select(person => person.Bezeichnung));

        public static string Mietobjekt(this Betriebskostenabrechnung abrechnung)
            => "Mietobjekt: " + abrechnung.Vertrag.Wohnung.Adresse!.Strasse + " " + abrechnung.Vertrag.Wohnung.Adresse.Hausnummer + ", " + abrechnung.Vertrag.Wohnung.Bezeichnung;

        public static string Abrechnungszeitraum(this Betriebskostenabrechnung abrechnung)
            => abrechnung.Zeitraum.Abrechnungsbeginn.ToString("dd.MM.yyyy") + " - " + abrechnung.Zeitraum.Abrechnungsende.ToString("dd.MM.yyyy");

        public static string Nutzungszeitraum(this Betriebskostenabrechnung abrechnung)
            => abrechnung.Zeitraum.Nutzungsbeginn.ToString("dd.MM.yyyy") + " - " + abrechnung.Zeitraum.Nutzungsende.ToString("dd.MM.yyyy");

        public static string Gruss(this Betriebskostenabrechnung abrechnung)
        {
            var gruss = abrechnung.Mieter.Aggregate("", (text, mieter) =>
            {
                if (mieter is NatuerlichePerson natuerlichePerson)
                {
                    return text + (natuerlichePerson.Anrede == Anrede.Herr ? "sehr geehrter Herr " :
                        natuerlichePerson.Anrede == Anrede.Frau ? "sehr geehrte Frau " :
                        natuerlichePerson.Vorname) + natuerlichePerson.Nachname + ", ";
                }
                else
                {
                    return "Sehr geehrte Damen und Herren, ";
                }
            });

            return gruss.Remove(1).ToUpper() + gruss[1..];
        }

        public static string ResultTxt(this Betriebskostenabrechnung abrechnung)
            => "wir haben die Kosten, die im Abrechnungszeitraum angefallen sind, berechnet. Die Abrechnung schließt mit " + (abrechnung.Result > 0 ? "einem Guthaben" : "einer Nachforderung") + " in Höhe von: ";

        public static string RefundDemand(this Betriebskostenabrechnung abrechnung)
            => abrechnung.Result > 0 ?
            "Dieser Betrag wird über die von Ihnen angegebene Bankverbindung erstattet." :
            "Bitte überweisen Sie diesen Betrag auf das Ihnen bekannte Konto.";

        public static string GenerischerText(this Betriebskostenabrechnung abrechnung)
        {
            // TODO Text auf Anwesenheit von Heizung oder so testen und anpassen.

            var text = "Die Abrechnung betrifft zunächst die mietvertraglich vereinbarten Nebenkosten (die kalten Betriebskosten). ";

            if (abrechnung.Abrechnungseinheiten.Any(abrechnungseinheit =>
                abrechnung.GesamtBetragWarmeNebenkosten(abrechnungseinheit) != 0 &&
                abrechnung.BetragWarmeNebenkosten(abrechnungseinheit) != 0))
            {
                text += "Die Kosten für die Heizung und für die Erwärmung von Wasser über die Heizanlage Ihres Wohnhauses (warme Betriebskosten) werden gesondert berechnet, nach Verbrauch und Wohn -/ Nutzfläche auf die einzelnen Wohnungen umgelegt („Ihre Heizungsrechnung“) und mit dem Ergebnis aus der Aufrechnung Ihrer Nebenkosten und der Summe der von Ihnen geleisteten Vorauszahlungen verrechnet.";
            }

            text += "Bei bestehenden Mietrückständen ist das Ergebnis der Abrechnung zusätzlich mit den Mietrückständen verrechnet. Gegebenenfalls bestehende Mietminderungen / Ratenzahlungsvereinbarungen sind hier nicht berücksichtigt, haben aber weiterhin für den vereinbarten Zeitraum Bestand. Aufgelöste oder gekündigte Mietverhältnisse werden durch dieses Schreiben nicht neu begründet. Die Aufstellung, Verteilung und Erläuterung der Gesamtkosten, die Berechnung der Kostenanteile, die Verrechnung der geleisteten Vorauszahlungen und gegebenenfalls die Neuberechnung der monatlichen Vorauszahlungen entnehmen Sie bitte den folgenden Seiten.";

            return text;
        }

        public static bool DirekteZuordnung(this Betriebskostenabrechnung abrechnung)
            => abrechnung.Abrechnungseinheiten.Any(rechnungsgruppe => rechnungsgruppe.Umlagen.Any(umlage => umlage.Wohnungen.Count == 1));

        private static bool UmlageSchluesselExistsInabrechnung(this Betriebskostenabrechnung betriebskostenrechnung, Umlageschluessel umlageSchluessel) =>
            betriebskostenrechnung.Abrechnungseinheiten
                .Any(rechnungsgruppe => rechnungsgruppe.Umlagen
                    .Where(umlage => umlage.Wohnungen.Count > 1)
                    .Any(umlage => umlage.Schluessel == umlageSchluessel));

        public static bool NachWohnflaeche(this Betriebskostenabrechnung abrechnung)
            => abrechnung.UmlageSchluesselExistsInabrechnung(Umlageschluessel.NachWohnflaeche);
        public static bool NachNutzflaeche(this Betriebskostenabrechnung abrechnung)
            => abrechnung.UmlageSchluesselExistsInabrechnung(Umlageschluessel.NachNutzflaeche);
        public static bool NachNutzeinheiten(this Betriebskostenabrechnung abrechnung)
            => abrechnung.UmlageSchluesselExistsInabrechnung(Umlageschluessel.NachNutzeinheit);
        public static bool NachPersonenzahl(this Betriebskostenabrechnung abrechnung)
            => abrechnung.UmlageSchluesselExistsInabrechnung(Umlageschluessel.NachPersonenzahl);
        public static bool NachVerbrauch(this Betriebskostenabrechnung abrechnung)
            => abrechnung.UmlageSchluesselExistsInabrechnung(Umlageschluessel.NachVerbrauch);

        public static string Anmerkung(this Betriebskostenabrechnung _)
            => "Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger) Kostenanteil";

        public static List<VertragVersion> getAllVertragVersionen(List<Wohnung> wohnungen, Zeitraum zeitraum)
        {
            return wohnungen
                .SelectMany(w => w.Vertraege.SelectMany(e => e.Versionen))
                .ToList()
                .Where(v => v.Beginn <= zeitraum.Abrechnungsende && (v.Ende() is null || v.Ende() >= zeitraum.Abrechnungsbeginn))
                .ToList();
        }

        public static double checkVerbrauch(Dictionary<Betriebskostentyp, double> verbrauchAnteil, Betriebskostentyp typ, List<Note> notes)
        {
            if (verbrauchAnteil.ContainsKey(typ))
            {
                return verbrauchAnteil[typ];
            }
            else
            {
                notes.Add(new Note("Konnte keinen Anteil für " + typ.ToDescriptionString() + " feststellen.", Severity.Error));
                return 0;
            }
        }

        public static List<Verbrauch> GetVerbrauchAbrechnungseinheit(Umlage umlage, Zeitraum zeitraum, List<Note> notes)
        {
            var Zaehler = umlage.Zaehler.Where(z => z.Wohnung != null).ToList();
            // TODO check that all Zaehler have the Same typ
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(Zaehler, zeitraum.Nutzungsende);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(Zaehler, zeitraum.Nutzungsbeginn);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(umlage, ZaehlerAnfangsStaende, ZaehlerEndStaende, notes);

            return Deltas;
        }

        private static List<Verbrauch> GetVerbrauchWohnung(Umlage umlage, Wohnung wohnung, Zeitraum zeitraum, List<Note> notes)
        {
            var Zaehler = umlage.Zaehler.Where(z => z.Wohnung == wohnung).ToList();
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(Zaehler, zeitraum.Nutzungsende);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(Zaehler, zeitraum.Nutzungsbeginn);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(umlage, ZaehlerAnfangsStaende, ZaehlerEndStaende, notes);

            return Deltas;
        }

        public static ImmutableList<Zaehlerstand> GetZaehlerEndStaendeFuerBerechnung(List<Zaehler> zaehler, DateOnly nutzungsende)
        {
            return zaehler
                .Select(z => z.Staende
                    .OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= nutzungsende && (nutzungsende.DayNumber - l.Datum.DayNumber) < 30))
                .ToList()
                .Where(zaehlerstand => zaehlerstand is not null)
                .Select(e => e!) // still necessary to make sure not null?
                .ToImmutableList() ?? new List<Zaehlerstand>() { }.ToImmutableList();
        }

        public static ImmutableList<Zaehlerstand> GetZaehlerAnfangsStaendeFuerBerechnung(List<Zaehler> zaehler, DateOnly nutzungsbeginn)
        {
            return zaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum)
                    .ToList()
                    .Where(l => Math.Abs(l.Datum.DayNumber - nutzungsbeginn.AddDays(-1).DayNumber) < 30)
                    .ToList()
                .FirstOrDefault())
                .Where(zs => zs != null)
                .Select(e => e!) // still necessary to make sure not null?
                .ToImmutableList();
        }

        public static List<Verbrauch> GetVerbrauchForZaehlerStaende(Umlage umlage, ImmutableList<Zaehlerstand> beginne, ImmutableList<Zaehlerstand> enden, List<Note> notes)
        {
            List<Verbrauch> Deltas = new();

            if (enden.IsEmpty)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (beginne.IsEmpty)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (enden.Count != beginne.Count)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else
            {
                for (var i = 0; i < enden.Count; ++i)
                {
                    Deltas.Add(new Verbrauch(umlage.Typ, enden[i].Zaehler.Kennnummer, enden[i].Zaehler.Typ, enden[i].Stand - beginne[i].Stand));
                }
            }

            return Deltas;
        }

        public static Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> CalculateWohnungVerbrauch(
            List<Umlage> Umlagen,
            Wohnung wohnung,
            Zeitraum zeitraum,
            Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch,
            List<Note> notes)
        {
            var VerbrauchList = Umlagen
                    .Where(g => g.Schluessel == Umlageschluessel.NachVerbrauch)
                    .ToList()
                    .Select(umlage => GetVerbrauchWohnung(umlage, wohnung, zeitraum, notes))
                    .ToList();

            if (VerbrauchList.Any(w => w.Count == 0))
            {
                // TODO this can be made even more explicit.
                notes.Add(new Note("Für eine Rechnung konnte keine Zuordnung erstellt werden.", Severity.Error));
                VerbrauchList = VerbrauchList.Where(w => w.Count > 0).ToList();
            }

            return VerbrauchList
                .Where(verbrauchList => verbrauchList.Count > 0 && GesamtVerbrauch.ContainsKey(verbrauchList.First().Betriebskostentyp))
                .ToList()
                .ToDictionary(
                    verbrauchList => verbrauchList.First().Betriebskostentyp,
                    verbrauchList => verbrauchList.Select(verbrauch => new VerbrauchAnteil(
                        verbrauch.Kennnummer,
                        verbrauch.Zaehlertyp,
                        verbrauch.Delta,
                        verbrauch.Delta / GesamtVerbrauch[verbrauchList.First().Betriebskostentyp]
                            .First(typDeltaGroup => typDeltaGroup.Typ == verbrauch.Zaehlertyp).Delta))
                .ToList());
        }

        public static List<PersonenZeitIntervall> VertraegeIntervallPersonenzahl(
            List<VertragVersion> vertraege,
            Zeitraum zeitraum)
        {
            var merged = vertraege
                .Where(vertragVersion =>
                    vertragVersion.Beginn <= zeitraum.Abrechnungsende
                    && (vertragVersion.Ende() is null || vertragVersion.Ende() >= zeitraum.Abrechnungsbeginn))
                .ToList()
                .SelectMany(v => new[]
                {
                    (Max(v.Beginn, zeitraum.Abrechnungsbeginn), v.Personenzahl),
                    (Min(v.Ende() ?? zeitraum.Abrechnungsende, zeitraum.Abrechnungsende).AddDays(1), -v.Personenzahl)
                })
                .ToList()
                .GroupBy(t => t.Item1)
                .ToList()
                .Select(g => new PersonenZeitIntervall(g.Key, zeitraum.Abrechnungsende, g.Sum(t => t.Item2)))
                .ToList()
                .OrderBy(t => t.Beginn)
                .ToList();

            for (int i = 0; i < merged.Count; ++i)
            {
                merged[i] = new PersonenZeitIntervall(
                    merged[i].Beginn,
                    i + 1 < merged.Count ? merged[i + 1].Beginn.AddDays(-1) : zeitraum.Abrechnungsende,
                    i - 1 >= 0 ? merged[i - 1].Personenzahl + merged[i].Personenzahl : merged[i].Personenzahl);
            }
            if (merged.Count > 0)
            {
                merged.RemoveAt(merged.Count - 1);
            }

            // TODO refactor function to switch from tuple to class - or replace this function by constructor
            var ret = merged.Select(personenZeitIntervall => new PersonenZeitIntervall(
                personenZeitIntervall.Beginn,
                personenZeitIntervall.Ende,
                personenZeitIntervall.Personenzahl))
                .ToList();

            return ret;
        }

        public static List<PersonenZeitanteil> GetPersonenZeitanteil(
            List<PersonenZeitIntervall> intervalle,
            List<PersonenZeitIntervall> gesamtIntervalle,
            Zeitraum zeitraum)
        {
            return intervalle
                .Where(g => g.Beginn < zeitraum.Nutzungsende && g.Ende >= zeitraum.Nutzungsbeginn)
                .ToList()
                .Select((w, i) => new PersonenZeitanteil(w, gesamtIntervalle, zeitraum))
                .ToList();
        }

        public static Dictionary<Betriebskostentyp, List<(Zaehlertyp, double)>> CalculateAbrechnungseinheitVerbrauch(
            List<Umlage> umlagen,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return umlagen
                .Where(umlage => umlage.Schluessel == Umlageschluessel.NachVerbrauch)
                .ToList()
                .Select(umlage => GetVerbrauchAbrechnungseinheit(umlage, zeitraum, notes))
                .ToList()
                .Where(verbrauchList => verbrauchList.Count > 0)
                .ToList()
                .ToDictionary(
                    verbrauchList => verbrauchList.First().Betriebskostentyp,
                    verbrauchList => verbrauchList.GroupBy(gg => gg.Zaehlertyp)
                .Select(typVerbrauchGrouping => (typVerbrauchGrouping.Key, typVerbrauchGrouping
                    .Sum(verbrauch => verbrauch.Delta)))
                .ToList());
        }

        public static Dictionary<Betriebskostentyp, double> CalculateVerbrauchAnteil(Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> verbrauch)
        {
            return verbrauch
                .Select(typVerbrauchAnteilPair => (
                    typVerbrauchAnteilPair.Key,
                    typVerbrauchAnteilPair.Value.Sum(verbrauchAnteil => verbrauchAnteil.Delta),
                    typVerbrauchAnteilPair.Value.Sum(verbrauchAnteil => verbrauchAnteil.Delta / verbrauchAnteil.Anteil)))
                .ToList()
                .ToDictionary(typ => typ.Key, typ => typ.Item2 / typ.Item3);
        }

        public static List<Heizkostenberechnung> CalculateHeizkosten(
            List<Umlage> umlagen,
            Wohnung wohnung,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return umlagen
                .Where(r => (int)r.Typ % 2 == 1)
                .ToList()
                .SelectMany(umlage => umlage.Betriebskostenrechnungen)
                .ToList()
                .Where(rechnung => rechnung.BetreffendesJahr == zeitraum.Jahr)
                .ToList()
                .Select(rechnung => new Heizkostenberechnung(rechnung, wohnung, zeitraum, notes))
                .ToList();
        }

        public static List<Betriebskostenrechnung> GetKalteNebenkosten(Abrechnungseinheit einheit, Zeitraum zeitraum)
        {
            return einheit.Umlagen
                .Where(umlage => (int)umlage.Typ % 2 == 0)
                .ToList()
                .SelectMany(umlage => umlage.Betriebskostenrechnungen)
                .ToList()
                .Where(rechnung => rechnung.BetreffendesJahr == zeitraum.Jahr)
                .ToList();
        }

        public static double CalculateBetragKalteNebenkosten(
            List<Betriebskostenrechnung> kalteUmlagen,
            double wfZeitanteil,
            double neZeitanteil,
            List<PersonenZeitanteil>
            personenZeitanteil,
            Dictionary<Betriebskostentyp, double> verbrauchAnteil,
            List<Note> notes)
        {
            return kalteUmlagen.Sum(rechnung => rechnung.Umlage.Schluessel switch
            {
                Umlageschluessel.NachWohnflaeche => rechnung.Betrag * wfZeitanteil,
                Umlageschluessel.NachNutzeinheit => rechnung.Betrag * neZeitanteil,
                Umlageschluessel.NachPersonenzahl => personenZeitanteil.Sum(z => z.Anteil * rechnung.Betrag),
                Umlageschluessel.NachVerbrauch => rechnung.Betrag * checkVerbrauch(verbrauchAnteil, rechnung.Umlage.Typ, notes),
                _ => 0
            });
        }

        public static List<Abrechnungseinheit> DetermineAbrechnungseinheiten(Vertrag vertrag)
        {
            // Group up all Wohnungen sharing the same Umlage
            var einheiten = vertrag.Wohnung.Umlagen
                .GroupBy(umlage =>
                    new SortedSet<int>(umlage.Wohnungen.Select(gr => gr.WohnungId).ToList()),
                    new SortedSetIntEqualityComparer())
                .ToList();
            // Then create Rechnungsgruppen for every single one of those groups with respective information to calculate the distribution
            return einheiten
                .Select(umlagen => new Abrechnungseinheit(umlagen.ToList()))
                .ToList();
        }

        public static double Mietzahlungen(Vertrag vertrag, Zeitraum zeitraum)
        {
            return vertrag.Mieten
                .Where(m => m.BetreffenderMonat >= zeitraum.Abrechnungsbeginn &&
                            m.BetreffenderMonat < zeitraum.Abrechnungsende)
                .ToList()
                .Sum(z => z.Betrag);
        }

        public static List<IPerson> GetMieter(SaverwalterContext ctx, Vertrag vertrag)
        {
            return ctx.MieterSet
                .Where(m => m.Vertrag.VertragId == vertrag.VertragId)
                .ToList()
                .Select(m => ctx.FindPerson(m.PersonId))
                .ToList();
        }

        public static double CalcAllgStromFactor(Vertrag vertrag, int Jahr)
        {
            return vertrag.Wohnung.Umlagen
                .Where(s => s.Typ == Betriebskostentyp.Heizkosten)
                .ToList()
                .SelectMany(u => u.Betriebskostenrechnungen)
                .ToList()
                .Where(u => u.BetreffendesJahr == Jahr)
                .Sum(e => e.Betrag) * 0.05;
        }

        public static double GetMietminderung(Vertrag vertrag, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            var Minderungen = vertrag.Mietminderungen
                .Where(m =>
                {
                    var beginBeforeEnd = m.Beginn <= abrechnungsende;
                    var endAfterBegin = m.Ende == null || m.Ende > abrechnungsbeginn;

                    return beginBeforeEnd && endAfterBegin;
                }).ToList();

            return Minderungen
                .Sum(m =>
                {
                    var endDate = Min(m.Ende ?? abrechnungsende, abrechnungsende);
                    var beginDate = Max(m.Beginn, abrechnungsbeginn);

                    return m.Minderung * (endDate.DayNumber - beginDate.DayNumber + 1);
                }) / (abrechnungsende.DayNumber - abrechnungsbeginn.DayNumber + 1);
        }

        public static double GetKaltMiete(Vertrag vertrag, Zeitraum zeitraum)
        {
            if (zeitraum.Jahr < vertrag.Beginn().Year ||
               (vertrag.Ende is DateOnly d && d.Year < zeitraum.Jahr))
            {
                return 0;
            }
            return vertrag.Versionen
                .Sum(version =>
                {
                    var versionEnde = version.Ende();
                    if (versionEnde is DateOnly d && d < zeitraum.Abrechnungsbeginn)
                    {
                        return 0;
                    }
                    else
                    {
                        var last = Min(versionEnde ?? zeitraum.Abrechnungsende, zeitraum.Abrechnungsende).Month;
                        var first = Max(version.Beginn, zeitraum.Abrechnungsbeginn).Month - 1;
                        return (last - first) * version.Grundmiete;
                    }
                });
        }
    }
}
