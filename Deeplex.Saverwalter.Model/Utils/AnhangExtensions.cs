using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.Model
{
    public static class AnhangExtensions
    {
        public static string getPath(this Anhang a, string root)
        {
            return System.IO.Path.Combine(root, a.AnhangId + System.IO.Path.GetExtension(a.FileName));
        }

        public static int getNumberOfTypes()
        {
            return 14;
        }

        public static int getReferences(this Anhang a)
        {
            var counts = new List<int>()
            {
                a.Adressen.Count,
                a.Betriebskostenrechnungen.Count,
                a.Erhaltungsaufwendungen.Count,
                a.Garagen.Count,
                a.JuristischePersonen.Count,
                a.Konten.Count,
                a.Mieten.Count,
                a.Mietminderungen.Count,
                a.NatuerlichePersonen.Count,
                a.Vertraege.Count,
                a.VertragsBetriebskostenrechnungen.Count,
                a.Wohnungen.Count,
                a.Zaehler.Count,
                a.Zaehlerstaende.Count,
            };
            // TODO check for length == 14
            return counts.Sum();

        }

        public static Type getType(this IAnhang a)
        {
            var types = new List<Type>()
            {
                typeof(Adresse),
                typeof(Betriebskostenrechnung),
                typeof(Erhaltungsaufwendung),
                typeof(Garage),
                typeof(JuristischePerson),
                typeof(Konto),
                typeof(Miete),
                typeof(MietMinderung),
                typeof(NatuerlichePerson),
                typeof(Vertrag),
                typeof(VertragsBetriebskostenrechnung),
                typeof(Wohnung),
                typeof(Zaehler),
                typeof(Zaehlerstand)
            };
            // TODO test for length == 14

            return types.FirstOrDefault(e => a.GetType() == e);
        }

        public static List<Anhang> getAnhaenge(this IAnhang a)
        {
            if (a is Adresse ad) return ad.Anhaenge;
            else if (a is Betriebskostenrechnung b) return b.Anhaenge;
            else if (a is Erhaltungsaufwendung e) return e.Anhaenge;
            else if (a is Garage g) return g.Anhaenge;
            else if (a is JuristischePerson j) return j.Anhaenge;
            else if (a is Konto k) return k.Anhaenge;
            else return null;
        }
    }
}
