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
        double Zeitanteil { get; }
        List<Note> notes { get; }
        Vertrag Vertrag { get; }
        List<VertragVersion> Versionen { get; }
        List<Verbrauch> GetVerbrauch(SaverwalterContext ctx, Umlage u, bool ganzeGruppe = false);
        double AllgStromFaktor { get; set; }
        List<Zaehler> Zaehler { get; }
    }

    public sealed class Betriebskostenabrechnung : IBetriebskostenabrechnung
    {
        public List<Note> notes { get; } = new List<Note>();
        public int Jahr { get; set; }
        public DateTime Abrechnungsbeginn { get; set; }
        public DateTime Abrechnungsende { get; set; }
        public List<VertragVersion> Versionen { get; }
        public IPerson Vermieter { get; }
        public IPerson Ansprechpartner { get; }
        public List<IPerson> Mieter { get; }
        public Vertrag Vertrag { get; }
        public Wohnung Wohnung { get; }
        public Adresse Adresse { get; }
        public double Gezahlt { get; }
        public double KaltMiete { get; }
        public double BetragNebenkosten { get; }
        public double BezahltNebenkosten { get; }
        public double Minderung { get; }
        public double NebenkostenMinderung { get; }
        public double KaltMinderung { get; }
        public DateTime Nutzungsbeginn { get; }
        public DateTime Nutzungsende { get; }
        public List<Zaehler> Zaehler { get; }

        public int Abrechnungszeitspanne { get; }
        public int Nutzungszeitspanne { get; }
        public double Zeitanteil { get; }

        public List<Rechnungsgruppe> Gruppen { get; }

        public double Result { get; }

        public double AllgStromFaktor { get; set; }

        public Betriebskostenabrechnung(SaverwalterContext ctx, Vertrag v, int jahr, DateTime abrechnungsbeginn, DateTime abrechnungsende)
        {
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Jahr = jahr;

            Vertrag = v;
            Wohnung = v.Wohnung;
            Adresse = Wohnung.Adresse;
            Zaehler = Wohnung.Zaehler.ToList();
            Versionen = v.Versionen.OrderBy(v => v.Beginn).ToList();

            Nutzungsbeginn = Max(Vertrag.Beginn(), Abrechnungsbeginn);
            Nutzungsende = Min(Vertrag.Ende ?? Abrechnungsende, Abrechnungsende);

            Abrechnungszeitspanne = (Abrechnungsende - Abrechnungsbeginn).Days + 1;
            Nutzungszeitspanne = (Nutzungsende - Nutzungsbeginn).Days + 1;
            Zeitanteil = (double)Nutzungszeitspanne / Abrechnungszeitspanne;

            Vermieter = ctx.FindPerson(Wohnung.BesitzerId);
            Ansprechpartner = ctx.FindPerson(Vertrag.AnsprechpartnerId!.Value) ?? Vermieter;

            Gezahlt = Vertrag.Mieten
                .Where(m => m.BetreffenderMonat >= Abrechnungsbeginn && m.BetreffenderMonat < Abrechnungsende)
                .ToList()
                .Sum(z => z.Betrag ?? 0);

            KaltMiete = GetKaltMiete(v);

            Mieter = ctx.MieterSet
                .Where(m => m.Vertrag.VertragId == Vertrag.VertragId)
                .ToList()
                .Select(m => ctx.FindPerson(m.PersonId))
                .ToList();

            AllgStromFaktor = Vertrag.Wohnung.Umlagen
                .Where(s => s.Typ == Betriebskostentyp.Heizkosten)
                .ToList()
                .SelectMany(u => u.Betriebskostenrechnungen)
                .ToList()
                .Where(u => u.BetreffendesJahr == Jahr)
                .Sum(e => e.Betrag) * 0.05;

            Gruppen = Vertrag.Wohnung.Umlagen
                .GroupBy(p => new SortedSet<int>(p.Wohnungen.Select(gr => gr.WohnungId).ToList()), new SortedSetIntEqualityComparer())
                .ToList()
                .Select(g => new Rechnungsgruppe(ctx, this, g.ToList()))
                .ToList();

            BetragNebenkosten = Gruppen.Sum(g => g.BetragKalt + g.BetragWarm);

            Minderung = GetMinderung(v);
            NebenkostenMinderung = BetragNebenkosten * Minderung;
            KaltMinderung = KaltMiete * Minderung;

            BezahltNebenkosten = Gezahlt - KaltMiete + KaltMinderung;

            Result = BezahltNebenkosten - BetragNebenkosten + NebenkostenMinderung;
        }

        private double GetMinderung(Vertrag v)
        {
            var Minderungen = v.Mietminderungen
                .Where(m =>
                {
                    var beginBeforeEnd = m.Beginn <= Abrechnungsende;
                    var endAfterBegin = m.Ende == null || m.Ende > Abrechnungsbeginn;

                    return beginBeforeEnd && endAfterBegin;
                }).ToList();

            return Minderungen
                .Sum(m =>
                {
                    var endDate = Min(m.Ende ?? Abrechnungsende, Abrechnungsende);
                    var beginDate = Max(m.Beginn, Abrechnungsbeginn);

                    return m.Minderung * ((endDate - beginDate).Days + 1);
                }) / Abrechnungszeitspanne;
        }

        private double GetKaltMiete(Vertrag v)
        {
            if (Jahr < v.Beginn().Year ||
               (Vertrag.Ende is DateTime d && d.Year < Jahr))
            {
                return 0;
            }
            return Versionen
                .Sum(v =>
                {
                    var ende = v.Ende();
                    if (ende is DateTime d && d < Abrechnungsbeginn)
                    {
                        return 0;
                    }
                    else
                    {
                        var last = Min(ende ?? Abrechnungsende, Abrechnungsende).Month;
                        var first = Max(v.Beginn, Abrechnungsbeginn).Month - 1;
                        return (last - first) * v.Grundmiete;
                    }
                });
        }

        public List<Verbrauch> GetVerbrauch(SaverwalterContext db, Umlage u, bool ganzeGruppe = false)
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
                    z.Wohnung?.WohnungId == Wohnung.WohnungId)
                .ToList();

            var ende = (ganzeGruppe ? Abrechnungsende : Nutzungsende).Date;
            var beginn = (ganzeGruppe ? Abrechnungsbeginn : Nutzungsbeginn).Date.AddDays(-1);

            var Ende = fZaehler
                .Select(z => z.Staende
                    .OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum.Date <= ende && (ende - l.Datum.Date).Days < 30))
                .ToList()
                .Where(zs => zs != null)
                .ToImmutableList();

            var Beginn = fZaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum)
                    .ToList()
                    .Where(l => Math.Abs((l.Datum - beginn).Days) < 30)
                    .ToList()
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
