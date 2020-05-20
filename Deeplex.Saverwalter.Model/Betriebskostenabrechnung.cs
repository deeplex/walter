using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<Wohnung> Wohnungen { get; set; }
        public List<KalteBetriebskostenpunkt> KalteBetriebskosten { get; set; }
        public List<KalteBetriebskostenRechnung> RechnungenKalt { get; set; }
        public double GesamtBetragKalt { get; set; }
        public double BetragKalt { get; set; }
        public double Gezahlt { get; set; }

        public DateTime Abrechnungsbeginn { get; set; }
        public DateTime Abrechnungsende { get; set; }
        public DateTime Nutzungsbeginn { get; set; }
        public DateTime Nutzungsende { get; set; }

        public List<(DateTime Beginn, DateTime Ende, int Personenzahl)> GesamtPersonenIntervall { get; set; }
        public double GesamtWohnflaeche { get; set; }
        public double GesamtNutzflaeche { get; set; }
        public int GesamtEinheiten { get; set; }

        public int Nutzungszeitspanne { get; set; }
        public int Abrechnungszeitspanne { get; set; }

        public double Zeitanteil { get; set; }
        public double WFZeitanteil { get; set; }
        public double NEZeitanteil { get; set; }
        public List<(DateTime Beginn, DateTime Ende, double Anteil)> PersZeitanteil { get; set; }

        public List<(DateTime Beginn, DateTime Ende, int Personenzahl)> PersonenIntervall { get; set; }

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
                .Include(v => v.Wohnung!)
                    .ThenInclude(w => w.Adresse)
                        .ThenInclude(a => a.KalteBetriebskosten)
                .Include(v => v.Wohnung!)
                    .ThenInclude(w => w.Adresse)
                        .ThenInclude(a => a.KalteBetriebskostenRechnungen)
                .Include(v => v.Wohnung!)
                    .ThenInclude(w => w.Adresse)
                        .ThenInclude(a => a.Wohnungen)
                            .ThenInclude(w2 => w2.Vertraege)
                .Include(v => v.Wohnung!)
                    .ThenInclude(w => w.Besitzer)
                .Include(v => v.Wohnung!)
                .First();

            Ansprechpartner = vertrag.Ansprechpartner;
            Mieter = db.MieterSet
                .Where(m => m.VertragId == vertrag.VertragId)
                .Select(m => m.Kontakt)
                .ToList();
            Wohnung = vertrag.Wohnung!;
            Adresse = Wohnung.Adresse;
            Vermieter = Wohnung.Besitzer;
            Wohnungen = Adresse.Wohnungen;

            Vertragsversionen = Wohnung.Vertraege
                .Where(v => v.VertragId == vertrag.VertragId)
                .OrderBy(v => v.Beginn).ToList();

            Nutzungsbeginn = Max(Vertragsversionen.First().Beginn, Abrechnungsbeginn);
            Nutzungsende = Min(Vertragsversionen.Last().Ende ?? Abrechnungsende, Abrechnungsende);

            var alleVertraegeDieserWohnungen = Wohnungen.SelectMany(w => w.Vertraege.Where(v =>
                v.Beginn <= Abrechnungsende && (v.Ende is null || v.Ende >= Abrechnungsbeginn)));

            GesamtWohnflaeche = Wohnungen.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = Wohnungen.Sum(w => w.Nutzflaeche);
            GesamtEinheiten = Wohnungen.Count();
            GesamtPersonenIntervall =
                VertraegeIntervallPersonenzahl(alleVertraegeDieserWohnungen,
                Abrechnungsbeginn, Abrechnungsende).ToList();
            PersonenIntervall =
                VertraegeIntervallPersonenzahl(Vertragsversionen,
                Nutzungsbeginn, Nutzungsende).ToList();

            Abrechnungszeitspanne = (Abrechnungsende - Abrechnungsbeginn).Days + 1;
            Nutzungszeitspanne = (Nutzungsende - Nutzungsbeginn).Days + 1;

            Zeitanteil = (double)Nutzungszeitspanne / Abrechnungszeitspanne;
            WFZeitanteil = (Wohnung.Wohnflaeche / GesamtWohnflaeche) * Zeitanteil;
            NEZeitanteil = (1.0 / GesamtEinheiten) * Zeitanteil;

            PersZeitanteil = GesamtPersonenIntervall
                .Where(g => g.Beginn < Nutzungsende && g.Ende >= Nutzungsbeginn)
                .Select((w, i) =>
                    (w.Beginn, w.Ende, Anteil:
                    (double)PersonenIntervall.Where(p => p.Beginn <= w.Beginn).First().Personenzahl / w.Personenzahl *
                    (((double)(w.Ende - w.Beginn).Days + 1) / Abrechnungszeitspanne))).ToList();

            KalteBetriebskosten = Adresse.KalteBetriebskosten.OrderBy(k => k.Typ).ToList();
            RechnungenKalt = Adresse.KalteBetriebskostenRechnungen.Where(k => k.Jahr == Jahr).OrderBy(k => k.Typ).ToList();
            GesamtBetragKalt = RechnungenKalt.Sum(r => r.Betrag);
            BetragKalt = RechnungenKalt.Aggregate(0.0, (a, b) =>
                KalteBetriebskosten.First(k => k.Typ == b.Typ).Schluessel switch
                {
                    UmlageSchluessel.NachWohnflaeche => a + b.Betrag * WFZeitanteil,
                    UmlageSchluessel.NachNutzeinheit => a + b.Betrag * NEZeitanteil,
                    UmlageSchluessel.NachPersonenzahl => a + PersZeitanteil
                            .Aggregate(0.0, (c, z) => c += z.Anteil * b.Betrag),
                    UmlageSchluessel.NachVerbrauch => a + 0, // TODO
                    _ => a + 0, // TODO or throw something...
                }
            );

            // TODO Timerange should be more dynamic...
            Gezahlt = db.Mieten
                .Where(m => m.VertragId == vertrag.VertragId)
                .Where(m => m.Datum >= Abrechnungsbeginn && m.Datum < Abrechnungsende)
                .Sum(z => z.WarmMiete ?? 0);

            Result = Gezahlt - BetragKalt;
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
                .GroupBy(t => t.Item1)
                .Select(g => (Beginn: g.Key, Ende, Personenzahl: g.Sum(t => t.Item2)))
                .OrderBy(t => t.Beginn)
                .ToList();
            merged.RemoveAt(merged.Count - 1);

            for (int i = 0, count = merged.Count; i < count; ++i)
            {
                merged[i] = (
                    merged[i].Beginn,
                    i + 1 < merged.Count ? merged[i + 1].Beginn.AddDays(-1) : Ende,
                    i - 1 >= 0 ? merged[i - 1].Personenzahl + merged[i].Personenzahl : merged[i].Personenzahl);
            }

            return merged;
        }
    }
}
