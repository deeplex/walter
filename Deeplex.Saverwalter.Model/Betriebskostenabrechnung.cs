using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Deeplex.Saverwalter.Model
{
    public class Betriebskostenabrechnung
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
        public double BezahltNebenkosten { get; set; }

        public DateTime Abrechnungsbeginn { get; set; }
        public DateTime Abrechnungsende { get; set; }
        public DateTime Nutzungsbeginn { get; set; }
        public DateTime Nutzungsende { get; set; }

        public int Abrechnungszeitspanne;
        public int Nutzungszeitspanne;
        public double Zeitanteil;

        public double BetragNebenkosten;

        public List<Rechnungsgruppe> Gruppen { get; set; }

        public Betriebskostenabrechnung(int rowid, int jahr, DateTime abrechnungsbeginn, DateTime abrechnungsende)
        {
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Jahr = jahr;

            using var db = new SaverwalterContext();

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
                .First();

            // If Ansprechpartner or Besitzer is null => throw

            Ansprechpartner = vertrag.Ansprechpartner;
            Mieter = db.MieterSet
                .Where(m => m.VertragId == vertrag.VertragId)
                .Select(m => m.Kontakt)
                .ToList();
            Wohnung = vertrag.Wohnung!;
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

            // TODO Timerange should be more dynamic...
            Gezahlt = db.Mieten
                .Where(m => m.VertragId == vertrag.VertragId)
                    .Where(m => m.Zahlungsdatum >= Abrechnungsbeginn && m.Zahlungsdatum < Abrechnungsende)
                    .Sum(z => z.Betrag ?? 0);

            KaltMiete = Vertragsversionen.Sum(v => (Min(v.Ende?? Abrechnungsende, Abrechnungsende).Month - Max(v.Beginn, Abrechnungsbeginn).Month + 1) * v.KaltMiete);
            BezahltNebenkosten = Gezahlt - KaltMiete;
            BetragNebenkosten = Gruppen.Sum(g => g.Betrag);

            Result = BezahltNebenkosten - BetragNebenkosten;
        }

        public class Rechnungsgruppe
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

                GesamtBetrag = gruppe.Sum(r => r.Betrag);
                Betrag = gruppe.Aggregate(0.0, (a, b) =>
                    b.Schluessel switch
                    {
                        UmlageSchluessel.NachWohnflaeche => a + b.Betrag * WFZeitanteil,
                        UmlageSchluessel.NachNutzeinheit => a + b.Betrag * NEZeitanteil,
                        UmlageSchluessel.NachPersonenzahl => a + PersZeitanteil
                                .Aggregate(0.0, (c, z) => c += z.Anteil * b.Betrag),
                        UmlageSchluessel.NachVerbrauch => a + 0, // TODO
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
