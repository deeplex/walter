using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Betriebskostenabrechnung
    {
        public double Result { get; set; }
        public int Jahr { get; set; }
        public List<Vertrag> Vertragsversionen { get; set; }
        public IPerson Vermieter { get; set; }
        public IPerson Ansprechpartner { get; set; }
        public List<IPerson> Mieter { get; set; }
        // TODO juristische
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

        public SaverwalterContext db { get; }

        public Betriebskostenabrechnung(SaverwalterContext db, int rowid, int jahr, DateTime abrechnungsbeginn, DateTime abrechnungsende)
        {
            this.db = db;

            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Jahr = jahr;

            var vertrag = db.Vertraege
                .Where(v => v.rowid == rowid)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Adresse)
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
                    .ThenInclude(w => w.Betriebskostenrechnungsgruppen)
                        .ThenInclude(b => b.Rechnung)
                            .ThenInclude(r => r.Gruppen)
                                .ThenInclude(g => g.Wohnung)
                                    .ThenInclude(w => w.Zaehler)
                                        .ThenInclude(z => z.Staende)
                .First();

            // If Ansprechpartner or Besitzer is null => throw

            Ansprechpartner = db.FindPerson(vertrag.AnsprechpartnerId!.Value);
            Mieter = db.MieterSet
                .Where(m => m.VertragId == vertrag.VertragId)
                .Select(m => db.FindPerson(m.PersonId))
                .ToList();

            Wohnung = vertrag.Wohnung!;
            Zaehler = Wohnung.Zaehler;
            Adresse = Wohnung.Adresse;
            Vermieter = db.FindPerson(Wohnung.BesitzerId);

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

            BetragNebenkosten = Gruppen.Sum(g => g.BetragKalt + g.BetragWarm);
            NebenkostenMinderung = BetragNebenkosten * Minderung;

            BezahltNebenkosten = Gezahlt - KaltMiete;

            Result = BezahltNebenkosten - BetragNebenkosten + KaltMinderung + NebenkostenMinderung;
        }

        public List<(Betriebskostentyp bTyp, string Kennnummer, Zaehlertyp zTyp, double Delta)>
            GetVerbrauch(Betriebskostenrechnung r, bool ganzeGruppe = false)
        {
            var Zaehler = r.Typ switch
            {
                Betriebskostentyp.WasserversorgungKalt =>
                    db.ZaehlerSet.Where(z => z.Typ == Zaehlertyp.Kaltwasser || z.Typ == Zaehlertyp.Warmwasser).ToList(),
                Betriebskostentyp.Heizkosten => // TODO Man kann auch mit was anderem als Gas heizen...
                    db.ZaehlerSet.Where(z => z.Typ == Zaehlertyp.Gas || z.Typ == Zaehlertyp.Warmwasser).ToList(),
                _ => null
            };

            var fZaehler = Zaehler.Where(z =>
                ganzeGruppe ?
                    r.Gruppen.Select(g => g.Wohnung).Contains(z.Wohnung) :
                    z.WohnungId == Wohnung.WohnungId);

            var ende = (ganzeGruppe ? Abrechnungsende : Nutzungsende).Date;
            var beginn = (ganzeGruppe ? Abrechnungsbeginn : Nutzungsbeginn).Date.AddDays(-1);

            var Ende = fZaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum).LastOrDefault(l => l.Datum.Date <= ende && (ende - l.Datum.Date).Days < 30))
                .Where(zs => zs != null)
                .ToImmutableList();
            var Beginn = fZaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum).LastOrDefault(l => l.Datum.Date <= beginn && (beginn - l.Datum.Date).Days < 30))
                .Where(zs => zs != null)
                .ToImmutableList();

            if (Ende.IsEmpty) throw new Exception("Kein Zähler für Nutzungsbeginn gefunden.");
            if (Beginn.IsEmpty) throw new Exception("Kein Zähler für Nutzungsende gefunden.");
            if (Ende.Count() != Beginn.Count()) throw new Exception("Zählerstände sind nicht korrekt...");

            List<(Betriebskostentyp bTyp, string Kennnummer, Zaehlertyp zTyp, double Delta)> Deltas
                = new List<(Betriebskostentyp bTyp, string Kennnummer, Zaehlertyp zTyp, double Delta)>();

            for (var i = 0; i < Ende.Count(); ++i)
            {
                Deltas.Add((r.Typ, Ende[i].Zaehler.Kennnummer, Ende[i].Zaehler.Typ, Ende[i].Stand - Beginn[i].Stand));
            }

            return Deltas;
        }
    }
}
