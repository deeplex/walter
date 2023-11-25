using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public static class Utils
    {
        public static void Add(this List<Note> notes, string message, Severity severity)
        {
            notes.Add(new Note(message, severity));
        }

        public static T Max<T>(T l, T r) where T : IComparable<T>
            => Max(l, r, Comparer<T>.Default);
        public static T Max<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) < 0 ? r : l;

        public static T Min<T>(T l, T r) where T : IComparable<T>
            => Min(l, r, Comparer<T>.Default);
        public static T Min<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) > 0 ? r : l;

        public static double checkVerbrauch(Dictionary<Umlagetyp, double> verbrauchAnteil, Umlagetyp typ, List<Note> notes)
        {
            if (verbrauchAnteil.ContainsKey(typ))
            {
                return verbrauchAnteil[typ];
            }
            else
            {
                notes.Add("Konnte keinen Anteil für " + typ.Bezeichnung + " feststellen.", Severity.Error);
                return 0;
            }
        }

        public static double Mietzahlungen(Vertrag vertrag, Zeitraum zeitraum)
        {
            return vertrag.Mieten
                .Where(m => m.BetreffenderMonat >= zeitraum.Abrechnungsbeginn &&
                            m.BetreffenderMonat < zeitraum.Abrechnungsende)
                .ToList()
                .Sum(z => z.Betrag);
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
