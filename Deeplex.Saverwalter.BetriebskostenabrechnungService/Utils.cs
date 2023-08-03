﻿using Deeplex.Saverwalter.Model;
using System.Collections.Immutable;
using static System.Net.Mime.MediaTypeNames;

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

        public static List<Verbrauch> GetVerbrauch(List<Zaehler> zaehler, Betriebskostentyp betriebskostentyp, Zeitraum zeitraum, List<Note> notes)
        {
            // TODO check that all Zaehler have the Same typ
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(zaehler, zeitraum.Nutzungsende);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(zaehler, zeitraum.Nutzungsbeginn);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(betriebskostentyp, ZaehlerAnfangsStaende, ZaehlerEndStaende, notes);

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

        public static List<Verbrauch> GetVerbrauchForZaehlerStaende(Betriebskostentyp betriebskostentyp, ImmutableList<Zaehlerstand> beginne, ImmutableList<Zaehlerstand> enden, List<Note> notes)
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
                    Deltas.Add(new Verbrauch(betriebskostentyp, enden[i].Zaehler.Kennnummer, enden[i].Zaehler.Typ, enden[i].Stand - beginne[i].Stand));
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
                    .Select(umlage =>
                    {
                        var zaehler = umlage.Zaehler.Where(z => z.Wohnung == wohnung).ToList();
                        return GetVerbrauch(zaehler, umlage.Typ, zeitraum, notes);
                    })
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

        private static List<DateOnly> getTimestampsOfPersonenAnzahlChanges(List<Vertrag> vertraege, Zeitraum zeitraum)
        {
            var begins = vertraege
                .SelectMany(e => e.Versionen)
                .Select(e => e.Beginn)
                .ToList();
            var ends = vertraege
                .Where(e => e.Ende != null)
                .Select(e => e.Ende is DateOnly d ? d.AddDays(1) : new DateOnly())
                .ToList();

            var breakpoints = begins
                .Concat(ends)
                .Where(e => e <= zeitraum.Abrechnungsende && e >= zeitraum.Abrechnungsbeginn)
                .Distinct()
                .OrderBy(e => e)
                .ToList();

            if (breakpoints.FirstOrDefault() != zeitraum.Abrechnungsbeginn)
            {
                breakpoints.Insert(0, zeitraum.Abrechnungsbeginn);
            }

            return breakpoints;
        }

        public static VertragVersion? getVersion(Vertrag vertrag, DateOnly timestamp)
        {
            return vertrag.Versionen.SingleOrDefault(version =>
            {
                var startedBefore = version.Beginn <= timestamp;
                var end = version.Ende();
                var endsAfter = end == null || end > timestamp;
                return startedBefore && endsAfter;
            });
        }

        public static int SumPersonenzahlen(List<Vertrag> vertraege, DateOnly timestamp)
        {
            var Personenzahl = 0;

            foreach (var vertrag in vertraege)
            {
                var version = getVersion(vertrag, timestamp);
                if (version is VertragVersion v)
                {
                    Personenzahl += v.Personenzahl;
                }
            }

            return Personenzahl;
        }

        public static List<PersonenZeitanteil> GetPersonenZeitanteil(
            Vertrag vertrag,
            Abrechnungseinheit einheit,
            Zeitraum zeitraum)
        {
            var vertraege = einheit.Wohnungen.SelectMany(e => e.Vertraege).ToList();

            if (!vertraege.Contains(vertrag))
            {
                throw new ArgumentException("Vertrag not in Einheit!");
            }

            var breakPoints = getTimestampsOfPersonenAnzahlChanges(vertraege, zeitraum);

            List<(DateOnly beginn, int personenzahl)> einheitAnteile = new();

            foreach (var change in breakPoints)
            {
                var sum = SumPersonenzahlen(vertraege, change);
                einheitAnteile.Add((change, sum));
            }

            List<PersonenZeitanteil> personenzeitanteile = new();

            // Skip the last
            for (var i = 0; i < einheitAnteile.Count; ++i)
            {
                var current = einheitAnteile[i];
                var ende = i == einheitAnteile.Count - 1
                    ? zeitraum.Abrechnungsende
                    : einheitAnteile[i + 1].beginn.AddDays(-1);

                var personenzahl = getVersion(vertrag, current.beginn)?.Personenzahl ?? 0;

                var personenZeitanteil = new PersonenZeitanteil(
                    current.beginn,
                    ende,
                    personenzahl,
                    current.personenzahl,
                    zeitraum);

                personenzeitanteile.Add(personenZeitanteil);
            }

            return personenzeitanteile;
        }

        public static Dictionary<Betriebskostentyp, List<(Zaehlertyp, double)>> CalculateAbrechnungseinheitVerbrauch(
            List<Umlage> umlagen,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return umlagen
                .Where(umlage => umlage.Schluessel == Umlageschluessel.NachVerbrauch)
                .ToList()
                .Select(umlage =>
                {
                    var zaehler = umlage.Zaehler.Where(z => z.Wohnung != null).ToList();
                    return GetVerbrauch(zaehler, umlage.Typ, zeitraum, notes);
                })
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
            Wohnung wohnung,
            Abrechnungseinheit einheit,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return einheit.Umlagen
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

        public static List<Abrechnungseinheit> GetAbrechnungseinheiten(Vertrag vertrag)
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

        public static double WFZeitanteil(Wohnung wohnung, Abrechnungseinheit einheit, Zeitraum zeitraum)
            => wohnung.Wohnflaeche / einheit.GesamtWohnflaeche * zeitraum.Zeitanteil;

        public static double NFZeitanteil(Wohnung wohnung, Abrechnungseinheit einheit, Zeitraum zeitraum)
            => wohnung.Nutzflaeche / einheit.GesamtNutzflaeche * zeitraum.Zeitanteil;

        public static double NEZeitanteil(Wohnung wohnung, Abrechnungseinheit einheit, Zeitraum zeitraum)
            => wohnung.Nutzeinheit / einheit.GesamtEinheiten * zeitraum.Zeitanteil;

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

        public static double BetragKalteNebenkosten(Vertrag vertrag, Abrechnungseinheit einheit, Zeitraum zeitraum, List<Note> notes)
            => CalculateBetragKalteNebenkosten(
                GetKalteNebenkosten(einheit, zeitraum),
                WFZeitanteil(vertrag.Wohnung, einheit, zeitraum),
                NEZeitanteil(vertrag.Wohnung, einheit, zeitraum),
                GetPersonenZeitanteil(vertrag, einheit, zeitraum),
                VerbrauchAnteil(vertrag.Wohnung, einheit, zeitraum, notes),
                notes);

        public static Dictionary<Betriebskostentyp, double> VerbrauchAnteil(
            Wohnung wohnung,
            Abrechnungseinheit einheit,
            Zeitraum zeitraum,
            List<Note> notes)
            => CalculateVerbrauchAnteil(
                CalculateWohnungVerbrauch(
                    einheit.Umlagen,
                    wohnung,
                    zeitraum,
                    CalculateAbrechnungseinheitVerbrauch(einheit.Umlagen, zeitraum, notes),
                    notes));

        public static Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch(
            Wohnung wohnung,
            Abrechnungseinheit einheit,
            Zeitraum zeitraum,
            List<Note> notes)
            => CalculateWohnungVerbrauch(
                einheit.Umlagen,
                wohnung,
                zeitraum,
                CalculateAbrechnungseinheitVerbrauch(einheit.Umlagen, zeitraum, notes),
                notes);

        public static double GesamtBetragWarmeNebenkosten(Wohnung wohnung, Abrechnungseinheit einheit, Zeitraum zeitraum, List<Note> notes)
            => CalculateHeizkosten(wohnung, einheit, zeitraum, notes)
                .Sum(heizkostenberechnung => heizkostenberechnung.PauschalBetrag);

        public static double BetragWarmeNebenkosten(Wohnung wohnung, Abrechnungseinheit einheit, Zeitraum zeitraum, List<Note> notes)
            => CalculateHeizkosten(wohnung, einheit, zeitraum, notes)
                .Sum(heizkostenberechnung => heizkostenberechnung.Kosten);
    }
}
