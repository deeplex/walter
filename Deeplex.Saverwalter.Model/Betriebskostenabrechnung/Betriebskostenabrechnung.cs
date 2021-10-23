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
        public List<Note> notes = new List<Note>();

        public SaverwalterContext db { get; }
        public int Jahr { get; set; }
        public DateTime Abrechnungsbeginn { get; set; }
        public DateTime Abrechnungsende { get; set; }

        public List<Vertrag> Vertragsversionen => Wohnung.Vertraege
            .Where(v => v.VertragId == Vertrag.VertragId)
            .OrderBy(v => v.Beginn).ToList();

        public IPerson Vermieter => db.FindPerson(Wohnung.BesitzerId);
        public IPerson Ansprechpartner => db.FindPerson(Vertrag.AnsprechpartnerId!.Value);
        public List<IPerson> Mieter => db.MieterSet
            .Where(m => m.VertragId == Vertrag.VertragId)
            .Select(m => db.FindPerson(m.PersonId))
            .ToList();
        // TODO juristische

        public Vertrag Vertrag { get; }

        public Wohnung Wohnung => Vertrag.Wohnung!;
        public Adresse Adresse => Wohnung.Adresse;

        public double Gezahlt => db.Mieten
            .Where(m => m.VertragId == Vertrag.VertragId)
            .Where(m => m.BetreffenderMonat >= Abrechnungsbeginn && m.BetreffenderMonat < Abrechnungsende)
            .Sum(z => z.Betrag ?? 0);

        public double KaltMiete => Vertragsversionen.Sum(v => (Min(v.Ende ?? Abrechnungsende, Abrechnungsende).Month - Max(v.Beginn, Abrechnungsbeginn).Month + 1) * v.KaltMiete);
        public double BetragNebenkosten => Gruppen.Sum(g => g.BetragKalt + g.BetragWarm);
        public double BezahltNebenkosten => Gezahlt - KaltMiete;

        public List<MietMinderung> Minderungen => db.MietMinderungen
                .Where(m => m.VertragId == Vertrag.VertragId &&
                    (m.Ende == null || m.Ende > Abrechnungsbeginn) &&
                    m.Beginn <= Abrechnungsende).ToList();

        public double Minderung => Minderungen.Sum(m =>
            m.Minderung * ((Min(m.Ende ?? Abrechnungsende, Abrechnungsende) - Max(m.Beginn, Abrechnungsbeginn)).Days + 1)) / Abrechnungszeitspanne;

        public double NebenkostenMinderung => BetragNebenkosten * Minderung;
        public double KaltMinderung => KaltMiete * Minderung;

        public DateTime Nutzungsbeginn => Max(Vertragsversionen.First().Beginn, Abrechnungsbeginn);
        public DateTime Nutzungsende => Min(Vertragsversionen.Last().Ende ?? Abrechnungsende, Abrechnungsende);

        public List<Zaehler> Zaehler => Wohnung.Zaehler;

        public int Abrechnungszeitspanne => (Abrechnungsende - Abrechnungsbeginn).Days + 1;
        public int Nutzungszeitspanne => (Nutzungsende - Nutzungsbeginn).Days + 1;
        public double Zeitanteil => (double)Nutzungszeitspanne / Abrechnungszeitspanne;

        public List<Rechnungsgruppe> Gruppen => Vertrag.Wohnung.Betriebskostenrechnungsgruppen
            .Where(g => g.Rechnung.BetreffendesJahr == Jahr)
            .GroupBy(p => new SortedSet<int>(p.Rechnung.Gruppen.Select(gr => gr.WohnungId)), new SortedSetIntEqualityComparer())
            .Select(g => new Rechnungsgruppe(this, g.Select(i => i.Rechnung).ToList()))
            .ToList();

        public double Result => BezahltNebenkosten - BetragNebenkosten + KaltMinderung + NebenkostenMinderung;

        public bool AllgStromVerrechnetMitHeizkosten = false;

        public Betriebskostenabrechnung(SaverwalterContext _db, int rowid, int jahr, DateTime abrechnungsbeginn, DateTime abrechnungsende)
        {
            db = _db;
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Jahr = jahr;

            Vertrag = db.Vertraege
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
        }

        public List<Verbrauch>
            GetVerbrauch(Betriebskostenrechnung r, bool ganzeGruppe = false)
        {
            var Zaehler = r.Typ switch
            {
                Betriebskostentyp.EntwaesserungSchmutzwasser =>
                    db.ZaehlerSet.Where(z => z.Typ == Zaehlertyp.Kaltwasser || z.Typ == Zaehlertyp.Warmwasser).ToList(),
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

            List<Verbrauch> Deltas = new List<Verbrauch>();

            if (Ende.IsEmpty)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (Beginn.IsEmpty)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (Ende.Count() != Beginn.Count())
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else
            {
                for (var i = 0; i < Ende.Count(); ++i)
                {
                    Deltas.Add(new Verbrauch(r.Typ, Ende[i].Zaehler.Kennnummer, Ende[i].Zaehler.Typ, Ende[i].Stand - Beginn[i].Stand));
                }
            }

            return Deltas;
        }
    }

    public sealed class Verbrauch
    {
        public Betriebskostentyp Betriebskostentyp;
        public Zaehlertyp Zaehlertyp;
        public string Kennnummer;
        public double Delta;

        public Verbrauch(Betriebskostentyp bTyp, string kennnummer, Zaehlertyp zTyp, double delta)
        {
            Betriebskostentyp = bTyp;
            Zaehlertyp = zTyp;
            Kennnummer = kennnummer;
            Delta = delta;
        }
    }
}
