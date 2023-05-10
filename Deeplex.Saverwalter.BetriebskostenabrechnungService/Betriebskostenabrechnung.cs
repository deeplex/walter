using Deeplex.Saverwalter.Model;
using System.Collections.Immutable;
using static Deeplex.Saverwalter.Model.Utils;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public interface IBetriebskostenabrechnung
    {
        IPerson Ansprechpartner { get; }
        List<Rechnungsgruppe> Gruppen { get; }
        Wohnung Wohnung => Vertrag.Wohnung;
        DateOnly Nutzungsbeginn { get; }
        int Nutzungszeitspanne { get; }
        int Abrechnungszeitspanne { get; }
        DateOnly Nutzungsende { get; }
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
        DateOnly Abrechnungsbeginn { get; }
        DateOnly Abrechnungsende { get; }
        double Zeitanteil { get; }
        List<Note> Notes { get; }
        Vertrag Vertrag { get; }
        List<VertragVersion> Versionen { get; }
        List<Verbrauch> GetVerbrauchWohnung(SaverwalterContext ctx, Umlage u);
        List<Verbrauch> GetVerbrauchGanzeGruppe(SaverwalterContext ctx, Umlage u);
        double AllgStromFaktor { get; set; }
        List<Zaehler> Zaehler { get; }
    }

    public sealed class Betriebskostenabrechnung : IBetriebskostenabrechnung
    {
        public List<Note> Notes { get; } = new List<Note>();
        public int Jahr { get; set; }
        public DateOnly Abrechnungsbeginn { get; set; }
        public DateOnly Abrechnungsende { get; set; }
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
        public DateOnly Nutzungsbeginn { get; }
        public DateOnly Nutzungsende { get; }
        public List<Zaehler> Zaehler { get; }

        public int Abrechnungszeitspanne { get; }
        public int Nutzungszeitspanne { get; }
        public double Zeitanteil { get; }

        public List<Rechnungsgruppe> Gruppen { get; }

        public double Result { get; }

        public double AllgStromFaktor { get; set; }

        public Betriebskostenabrechnung(SaverwalterContext ctx, Vertrag v, int jahr, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Jahr = jahr;

            Vertrag = v;
            Wohnung = v.Wohnung;
            Adresse = Wohnung.Adresse!; // TODO the Adresse here shouldn't be null, this should be catched.
            Zaehler = Wohnung.Zaehler.ToList();
            Versionen = v.Versionen.OrderBy(v => v.Beginn).ToList();

            Nutzungsbeginn = Max(Vertrag.Beginn(), Abrechnungsbeginn);
            Nutzungsende = Min(Vertrag.Ende ?? Abrechnungsende, Abrechnungsende);

            Abrechnungszeitspanne = Abrechnungsende.DayNumber - Abrechnungsbeginn.DayNumber + 1;
            Nutzungszeitspanne = Nutzungsende.DayNumber - Nutzungsbeginn.DayNumber + 1;
            Zeitanteil = (double)Nutzungszeitspanne / Abrechnungszeitspanne;

            Vermieter = ctx.FindPerson(Wohnung.BesitzerId);
            Ansprechpartner = ctx.FindPerson(Vertrag.AnsprechpartnerId!.Value) ?? Vermieter;

            Gezahlt = Vertrag.Mieten
                .Where(m => m.BetreffenderMonat >= Abrechnungsbeginn && m.BetreffenderMonat < Abrechnungsende)
                .ToList()
                .Sum(z => z.Betrag);

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

                    return m.Minderung * (endDate.DayNumber - beginDate.DayNumber + 1);
                }) / Abrechnungszeitspanne;
        }

        private double GetKaltMiete(Vertrag v)
        {
            if (Jahr < v.Beginn().Year ||
               (Vertrag.Ende is DateOnly d && d.Year < Jahr))
            {
                return 0;
            }
            return Versionen
                .Sum(v =>
                {
                    var ende = v.Ende();
                    if (ende is DateOnly d && d < Abrechnungsbeginn)
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

        private static List<Zaehler> GetAllZaehlerForVerbrauch(SaverwalterContext ctx, Betriebskostentyp typ)
        {
            return typ switch
            {
                Betriebskostentyp.EntwaesserungSchmutzwasser =>
                    ctx.ZaehlerSet.Where(z => z.Typ == Zaehlertyp.Kaltwasser || z.Typ == Zaehlertyp.Warmwasser).ToList(),
                Betriebskostentyp.WasserversorgungKalt =>
                    ctx.ZaehlerSet.Where(z => z.Typ == Zaehlertyp.Kaltwasser || z.Typ == Zaehlertyp.Warmwasser).ToList(),
                Betriebskostentyp.Heizkosten => // TODO Man kann auch mit was anderem als Gas heizen...
                    ctx.ZaehlerSet.Where(z => z.Typ == Zaehlertyp.Gas || z.Typ == Zaehlertyp.Warmwasser).ToList(),
                _ => null
            } ?? new List<Zaehler> { };
        }

        private ImmutableList<Zaehlerstand> GetZaehlerEndStaendeFuerBerechnung(List<Zaehler> zaehler)
        {
            return zaehler
                .Select(z => z.Staende
                    .OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= Nutzungsende && (Nutzungsende.DayNumber - l.Datum.DayNumber) < 30))
                .ToList()
                .Where(zaehlerstand => zaehlerstand is not null)
                .Select(e => e!) // still necessary to make sure not null?
                .ToImmutableList() ?? new List<Zaehlerstand>() { }.ToImmutableList();
        }

        private ImmutableList<Zaehlerstand> GetZaehlerAnfangsStaendeFuerBerechnung(List<Zaehler> zaehler)
        {
            return zaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum)
                    .ToList()
                    .Where(l => Math.Abs(l.Datum.DayNumber - Nutzungsbeginn.AddDays(-1).DayNumber) < 30)
                    .ToList()
                .FirstOrDefault())
                .Where(zs => zs != null)
                .Select(e => e!) // still necessary to make sure not null?
                .ToImmutableList();
        }

        private List<Verbrauch> GetVerbrauchForZaehlerStaende(Umlage umlage, ImmutableList<Zaehlerstand> Beginne, ImmutableList<Zaehlerstand> Enden)
        {
            List<Verbrauch> Deltas = new();

            if (Enden.IsEmpty)
            {
                Notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (Beginne.IsEmpty)
            {
                Notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (Enden.Count != Beginne.Count)
            {
                Notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else
            {
                for (var i = 0; i < Enden.Count; ++i)
                {
                    Deltas.Add(new Verbrauch(umlage.Typ, Enden[i].Zaehler.Kennnummer, Enden[i].Zaehler.Typ, Enden[i].Stand - Beginne[i].Stand));
                }
            }

            return Deltas;
        }

        public List<Verbrauch> GetVerbrauchGanzeGruppe(SaverwalterContext ctx, Umlage umlage)
        {
            var Zaehler = GetAllZaehlerForVerbrauch(ctx, umlage.Typ);
            var ZaehlerMitVerbrauchForGanzeGruppe = Zaehler.Where(z => umlage.Wohnungen.Contains(z.Wohnung!)).ToList();
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(ZaehlerMitVerbrauchForGanzeGruppe);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(ZaehlerMitVerbrauchForGanzeGruppe);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(umlage, ZaehlerAnfangsStaende, ZaehlerEndStaende);

            return Deltas;
        }

        public List<Verbrauch> GetVerbrauchWohnung(SaverwalterContext ctx, Umlage umlage)
        {
            var AlleZaehler = GetAllZaehlerForVerbrauch(ctx, umlage.Typ);
            var ZaehlerMitVerbrauchForThisWohnung = AlleZaehler.Where(z => z.Wohnung?.WohnungId == Wohnung.WohnungId).ToList();
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(ZaehlerMitVerbrauchForThisWohnung);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(ZaehlerMitVerbrauchForThisWohnung);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(umlage, ZaehlerAnfangsStaende, ZaehlerEndStaende);

            return Deltas;
        }
    }
}
