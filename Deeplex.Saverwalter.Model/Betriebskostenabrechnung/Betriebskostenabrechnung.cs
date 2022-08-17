using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;

namespace Deeplex.Saverwalter.Model
{
    public interface IBetriebskostenabrechnung
    {
        IPerson Ansprechpartner { get; }
        List<Rechnungsgruppe> Gruppen { get; }
        Wohnung Wohnung => Vertrag.Wohnung;
        DateTime Nutzungsbeginn { get; }
        int Nutzungszeitspanne { get; }
        int Abrechnungszeitspanne { get; }
        DateTime Nutzungsende { get; }
        int Jahr { get; }
        IPerson Vermieter { get; }
        List<IPerson> Mieter { get; }
        double Gezahlt { get; }
        double KaltMinderung { get; }
        double KaltMiete { get; }
        double Minderung { get; }
        double NebenkostenMinderung { get; }
        double Result { get; }
        Adresse Adresse { get; }
        DateTime Abrechnungsbeginn { get; }
        DateTime Abrechnungsende { get; }
        SaverwalterContext db { get; }
        double Zeitanteil { get; }
        List<Note> notes { get; }
        Vertrag Vertrag { get; }
        List<VertragVersion> Versionen { get; }
        List<Verbrauch> GetVerbrauch(Umlage u, bool ganzeGruppe = false);
        double AllgStromFaktor { get; set; }
    }

    public sealed class Betriebskostenabrechnung : IBetriebskostenabrechnung
    {
        public List<Note> notes { get; } = new List<Note>();

        public SaverwalterContext db { get; }
        public int Jahr { get; set; }
        public DateTime Abrechnungsbeginn { get; set; }
        public DateTime Abrechnungsende { get; set; }

        public List<VertragVersion> Versionen => Versionen.OrderBy(v => v.Beginn).ToList();

        public IPerson Vermieter => db.FindPerson(Wohnung.BesitzerId);
        public IPerson Ansprechpartner => db.FindPerson(Vertrag.AnsprechpartnerId!.Value) ?? Vermieter;
        public List<IPerson> Mieter => db.MieterSet
            .Where(m => m.Vertrag.VertragId == Vertrag.VertragId)
            .Select(m => db.FindPerson(m.PersonId))
            .ToList();
        // TODO juristische

        public Vertrag Vertrag { get; }

        public Wohnung Wohnung => Vertrag.Wohnung;
        public Adresse Adresse => Wohnung.Adresse;

        public double Gezahlt => db.Mieten
            .Where(m => m.Vertrag.VertragId == Vertrag.VertragId)
            .Where(m => m.BetreffenderMonat >= Abrechnungsbeginn && m.BetreffenderMonat < Abrechnungsende)
            .Sum(z => z.Betrag ?? 0);

        public double KaltMiete
        {
            get
            {
                if (Jahr < Versionen.First().Beginn.Year ||
                    (Versionen.Last().Ende is DateTime d && d.Year < Jahr))
                {
                    return 0;
                }
                return Versionen
                    .Sum(v => v.Ende != null &&
                        v.Ende < Abrechnungsbeginn ?
                        0 :
                        (Min(v.Ende ?? Abrechnungsende, Abrechnungsende).Month - Max(v.Beginn, Abrechnungsbeginn).Month + 1) * v.Grundmiete);
            }
        }
        public double BetragNebenkosten => Gruppen.Sum(g => g.BetragKalt + g.BetragWarm);
        public double BezahltNebenkosten => Gezahlt - KaltMiete + KaltMinderung;

        public List<Mietminderung> Minderungen => db.MietMinderungen
                .Where(m => m.Vertrag.VertragId == Vertrag.VertragId &&
                    (m.Ende == null || m.Ende > Abrechnungsbeginn) &&
                    m.Beginn <= Abrechnungsende).ToList();

        public double Minderung => Minderungen.Sum(m =>
            m.Minderung * ((Min(m.Ende ?? Abrechnungsende, Abrechnungsende) - Max(m.Beginn, Abrechnungsbeginn)).Days + 1)) / Abrechnungszeitspanne;

        public double NebenkostenMinderung => BetragNebenkosten * Minderung;
        public double KaltMinderung => KaltMiete * Minderung;

        public DateTime Nutzungsbeginn => Max(Versionen.First().Beginn, Abrechnungsbeginn);
        public DateTime Nutzungsende => Min(Versionen.Last().Ende ?? Abrechnungsende, Abrechnungsende);

        public List<Zaehler> Zaehler => Wohnung.Zaehler;

        public int Abrechnungszeitspanne => (Abrechnungsende - Abrechnungsbeginn).Days + 1;
        public int Nutzungszeitspanne => (Nutzungsende - Nutzungsbeginn).Days + 1;
        public double Zeitanteil => (double)Nutzungszeitspanne / Abrechnungszeitspanne;

        public List<Rechnungsgruppe> Gruppen { get; }

        public double Result => BezahltNebenkosten - BetragNebenkosten + NebenkostenMinderung;

        public double AllgStromFaktor { get; set; }

        public Betriebskostenabrechnung(SaverwalterContext _db, Vertrag v, int jahr, DateTime abrechnungsbeginn, DateTime abrechnungsende)
        {
            db = _db;
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Jahr = jahr;

            // TODO
            db.Vertraege
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Adresse)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Umlagen)
                        .ThenInclude(w => w.Wohnungen)
                            .ThenInclude(w => w.Adresse)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Umlagen)
                        .ThenInclude(g => g.Wohnungen)
                            .ThenInclude(w => w.Vertraege)
                .Include(v => v.Wohnung)
                    .ThenInclude(b => b.Umlagen)
                        .ThenInclude(g => g.Wohnungen)
                            .ThenInclude(w => w.Zaehler)
                                .ThenInclude(z => z.Staende)
                .Include(v => v.Wohnung)
                    .ThenInclude(u => u.Umlagen)
                        .ThenInclude(w => w.Betriebskostenrechnungen)
                .First();

            Vertrag = v;

            AllgStromFaktor = Vertrag.Wohnung.Umlagen
                .Where(s => s.Typ == Betriebskostentyp.Heizkosten)
                .SelectMany(u => u.Betriebskostenrechnungen)
                .Where(u => u.BetreffendesJahr == Jahr)
                .Sum(e => e.Betrag) * 0.05;

            Gruppen = Vertrag.Wohnung.Umlagen
                .GroupBy(p => new SortedSet<int>(p.Wohnungen.Select(gr => gr.WohnungId)), new SortedSetIntEqualityComparer())
                .Select(g => new Rechnungsgruppe(this, g.ToList()))
                .ToList();

            // If Ansprechpartner or Besitzer is null => throw
        }

        public List<Verbrauch> GetVerbrauch(Umlage u, bool ganzeGruppe = false)
        {
            var Zaehler = u.Typ switch
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
                    u.Wohnungen.Contains(z.Wohnung!) :
                    z.WohnungId == Wohnung.WohnungId);

            var ende = (ganzeGruppe ? Abrechnungsende : Nutzungsende).Date;
            var beginn = (ganzeGruppe ? Abrechnungsbeginn : Nutzungsbeginn).Date.AddDays(-1);

            var Ende = fZaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum).LastOrDefault(l => l.Datum.Date <= ende && (ende - l.Datum.Date).Days < 30))
                .Where(zs => zs != null)
                .ToImmutableList();
            var Beginn = fZaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum)
                .Where(l => Math.Abs((l.Datum - beginn).Days) < 30)
                .FirstOrDefault())
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
                    Deltas.Add(new Verbrauch(u.Typ, Ende[i].Zaehler.Kennnummer, Ende[i].Zaehler.Typ, Ende[i].Stand - Beginn[i].Stand));
                }
            }

            return Deltas;
        }
    }
}
