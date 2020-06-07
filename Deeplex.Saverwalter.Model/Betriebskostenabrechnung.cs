using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Windows.Foundation.Diagnostics;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Betriebskostenabrechnung
    {
        public double Result { get; set; }
        public int Jahr { get; set; }
        public List<Vertrag> Vertragsversionen { get; set; }
        public JuristischePerson Vermieter { get; set; }
        public Kontakt Ansprechpartner { get; set; }
        public List<Kontakt> Mieter { get; set; }
        public Wohnung Wohnung { get; set; }
        public Adresse Adresse { get; set; }
        public double Gezahlt { get; set; }
        public double KaltMiete { get; set; }
        public double BetragNebenkosten { get; set; }
        public double BezahltNebenkosten { get; set; }
        public List<MietMinderung> Minderungen { get; set; }
        public double Minderung { get; set; }
        public double NebenkostenMinderung { get; set; }
        public double KaltMinderung { get; set; }

        public DateTime Abrechnungsbeginn { get; set; }
        public DateTime Abrechnungsende { get; set; }
        public DateTime Nutzungsbeginn { get; set; }
        public DateTime Nutzungsende { get; set; }

        public List<Zaehler> Zaehler { get; set; }

        public int Abrechnungszeitspanne;
        public int Nutzungszeitspanne;
        public double Zeitanteil;

        public List<Rechnungsgruppe> Gruppen { get; set; }

        private SaverwalterContext db { get; }

        public Betriebskostenabrechnung(int rowid, int jahr, DateTime abrechnungsbeginn, DateTime abrechnungsende)
        {
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Jahr = jahr;

            db = new SaverwalterContext();

            var vertrag = db.Vertraege
                .Where(v => v.rowid == rowid)
                .Include(v => v.Ansprechpartner)
                    .ThenInclude(k => k.Adresse)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Adresse)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Besitzer)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Betriebskostenrechnungsgruppen)
                        .ThenInclude(b => b.Rechnung)
                            .ThenInclude(r => r.Gruppen)
                                .ThenInclude(g => g.Wohnung)
                                    .ThenInclude(w => w.Adresse)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Betriebskostenrechnungsgruppen)
                        .ThenInclude(b => b.Rechnung)
                            .ThenInclude(r => r.Gruppen)
                                .ThenInclude(g => g.Wohnung)
                                    .ThenInclude(w => w.Vertraege)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Zaehler)
                        .ThenInclude(z => z.Staende)
                .First();

            // If Ansprechpartner or Besitzer is null => throw

            Ansprechpartner = vertrag.Ansprechpartner;
            Mieter = db.MieterSet
                .Where(m => m.VertragId == vertrag.VertragId)
                .Select(m => m.Kontakt)
                .ToList();

            Wohnung = vertrag.Wohnung!;
            Zaehler = Wohnung.Zaehler;
            Adresse = Wohnung.Adresse;
            Vermieter = Wohnung.Besitzer;

            Vertragsversionen = Wohnung.Vertraege
                .Where(v => v.VertragId == vertrag.VertragId)
                .OrderBy(v => v.Beginn).ToList();

            Nutzungsbeginn = Max(Vertragsversionen.First().Beginn, Abrechnungsbeginn);
            Nutzungsende = Min(Vertragsversionen.Last().Ende ?? Abrechnungsende, Abrechnungsende);

            Abrechnungszeitspanne = (Abrechnungsende - Abrechnungsbeginn).Days + 1;
            Nutzungszeitspanne = (Nutzungsende - Nutzungsbeginn).Days + 1;
            Zeitanteil = (double)Nutzungszeitspanne / Abrechnungszeitspanne;

            Gruppen = vertrag.Wohnung.Betriebskostenrechnungsgruppen
                .Where(g => g.Rechnung.BetreffendesJahr == Jahr)
                .GroupBy(p => new SortedSet<int>(p.Rechnung.Gruppen.Select(gr => gr.WohnungId)), new SortedSetIntEqualityComparer())
                .Select(g => new Rechnungsgruppe(this, g.Select(i => i.Rechnung).ToList()))
                .ToList();

            Gezahlt = db.Mieten
                .Where(m => m.VertragId == vertrag.VertragId)
                    .Where(m => m.BetreffenderMonat >= Abrechnungsbeginn && m.BetreffenderMonat < Abrechnungsende)
                    .Sum(z => z.Betrag ?? 0);

            Minderungen = db.MietMinderungen
                .Where(m => m.VertragId == vertrag.VertragId && (m.Ende == null || m.Ende > Abrechnungsbeginn) && m.Beginn <= Abrechnungsende).ToList();
            Minderung = Minderungen.Sum(m => m.Minderung * ((Min(m.Ende ?? Abrechnungsende, Abrechnungsende) - Max(m.Beginn, Abrechnungsbeginn)).Days + 1)) / Abrechnungszeitspanne;

            KaltMiete = Vertragsversionen.Sum(v => (Min(v.Ende ?? Abrechnungsende, Abrechnungsende).Month - Max(v.Beginn, Abrechnungsbeginn).Month + 1) * v.KaltMiete);
            KaltMinderung = KaltMiete * Minderung;

            BetragNebenkosten = Gruppen.Sum(g => g.Betrag);
            NebenkostenMinderung = BetragNebenkosten * Minderung;

            BezahltNebenkosten = Gezahlt - KaltMiete;

            Result = BezahltNebenkosten - BetragNebenkosten + KaltMinderung + NebenkostenMinderung;

            db.Dispose();
        }

        public List<(string Kennnummer, Zaehlertyp Typ, double Delta)> GetVerbrauch(Betriebskostenrechnung r)
        {
            Zaehlerstand interpolateZaehlerstand(DateTime d, Zaehlerstand z1, Zaehlerstand z2)
            {
                var m = (z1.Stand - z2.Stand) / (z1.Datum - z2.Datum).Days;
                var Stand = m * (d - z1.Datum).Days + z1.Stand;

                var zs = new Zaehlerstand
                {
                    Datum = d,
                    Stand = Stand,
                    Zaehler = z1.Zaehler,
                    Abgelesen = false,
                    Notiz = "Erstellt durch Betriebskostenabrechnung am " + DateTime.UtcNow.ToString("dd.mm.yyyy") +
                        ". Berechnet durch Ablesung vom: " + z1.Datum.ToString("dd.mm.yyyy") + " und " + z2.Datum.ToString("dd.mm.yyyy")
                };
                db.Zaehlerstaende.Add(zs);
                db.SaveChanges();
                return zs;
            }

            var Zaehler = r.Typ switch
            {
                Betriebskostentyp.Wasserversorgung =>
                    this.Zaehler.Where(z => z.Typ == Zaehlertyp.Kaltwasser || z.Typ == Zaehlertyp.Warmwasser).ToList(),
                // Heizung wird zu 50% mit Warmwasser verrechnet oder so...
                // Betriebskostentyp.Heizung => b.Zaehler.Where(z => z.Typ == Zaehlertyp.Heizung).ToList
                _ => null
            };

            var Ende = Zaehler.Select(z => z.Staende
                .LastOrDefault(l => l.Datum <= Nutzungsende && (Nutzungsende - l.Datum).Days < 30))
                .Where(zs => zs != null)
                .ToImmutableList();
            var Beginn = Zaehler.Select(z => z.Staende
                .LastOrDefault(l => l.Datum <= Nutzungsbeginn && (Nutzungsbeginn - l.Datum).Days < 30))
                .Where(zs => zs != null)
                .ToImmutableList();

            if (Ende.IsEmpty) throw new Exception("Kein Zähler für Nutzungsbeginn gefunden.");
            if (Beginn.IsEmpty) throw new Exception("Kein Zähler für Nutzungsende gefunden.");
            if (Ende.Count() != Beginn.Count()) throw new Exception("Zählerstände sind nicht korrekt...");

            List<(string Kennnummer, Zaehlertyp Typ, double Delta)> Deltas = new List<(string Kennnummer, Zaehlertyp Typ, double Delta)>();

            for (var i = 0; i < Ende.Count(); ++i)
            {
                Zaehlerstand zBeginn = Beginn[i].Datum.Date == Nutzungsbeginn.Date.AddDays(-1) ? // TODO is same day also okay? ...
                     Beginn[i] : interpolateZaehlerstand(Nutzungsbeginn, Beginn[i], Ende[i]);
                Zaehlerstand zEnde = Ende[i].Datum.Date == Nutzungsende.Date ?
                    Ende[i] : interpolateZaehlerstand(Nutzungsende, Beginn[i], Ende[i]);

                Deltas.Add((zEnde.Zaehler.Kennnummer, zEnde.Zaehler.Typ, zEnde.Stand - zBeginn.Stand));
            }

            return Deltas;
        }

        public sealed class Rechnungsgruppe
        {
            public List<Betriebskostenrechnung> Rechnungen;

            public double GesamtWohnflaeche;
            public double WFZeitanteil;
            public double GesamtNutzflaeche;
            public int GesamtEinheiten;
            public double NEZeitanteil;
            public List<(DateTime Beginn, DateTime Ende, int Personenzahl)> GesamtPersonenIntervall;
            public List<(DateTime Beginn, DateTime Ende, int Personenzahl)> PersonenIntervall;
            public List<(DateTime Beginn, DateTime Ende, double Anteil)> PersZeitanteil;
            public Dictionary<Betriebskostentyp, List<(string Kennnummer, Zaehlertyp Typ, double Delta)>> Verbrauch;
            public double GesamtBetrag;
            public double Betrag;

            public Rechnungsgruppe(Betriebskostenabrechnung b, List<Betriebskostenrechnung> gruppe)
            {
                var gr = gruppe.First().Gruppen;

                Rechnungen = gruppe;
                GesamtWohnflaeche = gr.Sum(w => w.Wohnung.Wohnflaeche);
                GesamtNutzflaeche = gr.Sum(w => w.Wohnung.Nutzflaeche);
                GesamtEinheiten = gr.Sum(w => w.Wohnung.Nutzeinheit);

                var alleVertraegeDieserWohnungen = gr.SelectMany(w => w.Wohnung.Vertraege.Where(v =>
                    v.Beginn <= b.Abrechnungsende && (v.Ende is null || v.Ende >= b.Abrechnungsbeginn)));

                GesamtPersonenIntervall =
                    VertraegeIntervallPersonenzahl(alleVertraegeDieserWohnungen,
                    b.Abrechnungsbeginn, b.Abrechnungsende).ToList();

                PersonenIntervall =
                    VertraegeIntervallPersonenzahl(b.Vertragsversionen,
                    b.Nutzungsbeginn, b.Nutzungsende).ToList();

                WFZeitanteil = b.Wohnung.Wohnflaeche / GesamtWohnflaeche * b.Zeitanteil;
                NEZeitanteil = (double)b.Wohnung.Nutzeinheit / GesamtEinheiten * b.Zeitanteil;

                PersZeitanteil = GesamtPersonenIntervall
                    .Where(g => g.Beginn < b.Nutzungsende && g.Ende >= b.Nutzungsbeginn)
                    .Select((w, i) =>
                        (w.Beginn, w.Ende, Anteil:
                            (double)PersonenIntervall.Where(p => p.Beginn <= w.Beginn).First().Personenzahl / w.Personenzahl *
                            (((double)(w.Ende - w.Beginn).Days + 1) / b.Abrechnungszeitspanne))).ToList();

                Verbrauch = gruppe.Where(g => g.Schluessel == UmlageSchluessel.NachVerbrauch).ToDictionary(r => r.Typ, r => b.GetVerbrauch(r));

                GesamtBetrag = gruppe.Sum(r => r.Betrag);
                Betrag = gruppe.Aggregate(0.0, (a, r) =>
                    r.Schluessel switch
                    {
                        UmlageSchluessel.NachWohnflaeche => a + r.Betrag * WFZeitanteil,
                        UmlageSchluessel.NachNutzeinheit => a + r.Betrag * NEZeitanteil,
                        UmlageSchluessel.NachPersonenzahl => a + PersZeitanteil.Aggregate(0.0, (a2, z) => a2 += z.Anteil * r.Betrag),
                        UmlageSchluessel.NachVerbrauch => a + r.Betrag * 0 / 1, // obvious todo
                        _ => a + 0, // TODO or throw something...
                    }
                );

                for (int i = 0, count = GesamtPersonenIntervall.Count - 1; i < count; ++i)
                {
                    if (GesamtPersonenIntervall[i].Personenzahl == GesamtPersonenIntervall[i + 1].Personenzahl)
                    {
                        var Beginn = GesamtPersonenIntervall[i].Beginn;
                        var Ende = GesamtPersonenIntervall[i + 1].Ende;

                        GesamtPersonenIntervall[i] = (Beginn, Ende, GesamtPersonenIntervall[i].Personenzahl);
                        GesamtPersonenIntervall.RemoveAt(1 + i--);
                        count--;
                    }
                }

                for (int i = 0, count = PersZeitanteil.Count - 1; i < count; ++i)
                {
                    var gpz = GesamtPersonenIntervall.Last(g => g.Beginn.Date <= PersZeitanteil[i].Beginn.Date).Personenzahl;
                    var ngpz = GesamtPersonenIntervall.Last(g => g.Beginn.Date <= PersZeitanteil[i + 1].Beginn.Date).Personenzahl;
                    var pz = PersonenIntervall.Last(p => p.Beginn.Date <= PersZeitanteil[i].Beginn).Personenzahl;
                    var npz = PersonenIntervall.Last(p => p.Beginn.Date <= PersZeitanteil[i + 1].Beginn).Personenzahl;

                    if (gpz == ngpz && pz == npz)
                    {
                        PersZeitanteil[i] = (
                            PersZeitanteil[i].Beginn,
                            PersZeitanteil[i + 1].Ende,
                            PersZeitanteil[i].Anteil + PersZeitanteil[i + 1].Anteil);

                        PersZeitanteil.RemoveAt(1 + i--);
                        count--;
                    }
                }
            }
        }

        private static T Max<T>(T l, T r) where T : IComparable<T>
            => Max(l, r, Comparer<T>.Default);
        private static T Max<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) < 0 ? r : l;

        private static T Min<T>(T l, T r) where T : IComparable<T>
            => Min(l, r, Comparer<T>.Default);
        private static T Min<T>(T l, T r, IComparer<T> c)
            => c.Compare(l, r) > 0 ? r : l;

        private static List<(DateTime Beginn, DateTime Ende, int Personenzahl)>
            VertraegeIntervallPersonenzahl(IEnumerable<Vertrag> vertraege, DateTime Beginn, DateTime Ende)
        {
            var merged = vertraege
                .Where(v => v.Beginn <= Ende && (v.Ende is null || v.Ende >= Beginn))
                .SelectMany(v => new[]
                {
                    (Max(v.Beginn, Beginn), v.Personenzahl),
                    (Min(v.Ende ?? Ende, Ende).AddDays(1), -v.Personenzahl)
                })
                .GroupBy(t => t.Item1.Date)
                .Select(g => (Beginn: g.Key, Ende, Personenzahl: g.Sum(t => t.Item2)))
                .OrderBy(t => t.Beginn)
                .ToList();

            for (int i = 0; i < merged.Count; ++i)
            {
                merged[i] = (
                    merged[i].Beginn,
                    i + 1 < merged.Count ? merged[i + 1].Beginn.AddDays(-1) : Ende,
                    i - 1 >= 0 ? merged[i - 1].Personenzahl + merged[i].Personenzahl : merged[i].Personenzahl);
            }
            merged.RemoveAt(merged.Count - 1);

            return merged;
        }
    }
}
