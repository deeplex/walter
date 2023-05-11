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
        double GezahlteMiete { get; }
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
        public double GezahlteMiete { get; }
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

        public Betriebskostenabrechnung(SaverwalterContext ctx, Vertrag vertrag, int jahr, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Jahr = jahr;

            Vertrag = vertrag;
            Wohnung = vertrag.Wohnung;
            Adresse = Wohnung.Adresse!; // TODO the Adresse here shouldn't be null, this should be catched.
            Zaehler = Wohnung.Zaehler.ToList();
            Versionen = vertrag.Versionen.OrderBy(v => v.Beginn).ToList();

            Nutzungsbeginn = Max(vertrag.Beginn(), abrechnungsbeginn);
            Nutzungsende = Min(vertrag.Ende ?? abrechnungsende, abrechnungsende);

            Abrechnungszeitspanne = abrechnungsende.DayNumber - abrechnungsbeginn.DayNumber + 1;
            Nutzungszeitspanne = Nutzungsende.DayNumber - Nutzungsbeginn.DayNumber + 1;
            Zeitanteil = (double)Nutzungszeitspanne / Abrechnungszeitspanne;

            Vermieter = ctx.FindPerson(Wohnung.BesitzerId);
            Ansprechpartner = ctx.FindPerson(vertrag.AnsprechpartnerId!.Value) ?? Vermieter;

            GezahlteMiete = Mietzahlungen(vertrag, abrechnungsbeginn, abrechnungsende);
            KaltMiete = GetKaltMiete(vertrag, Versionen, jahr, abrechnungsbeginn, abrechnungsende);

            Mieter = GetMieter(ctx, vertrag);

            AllgStromFaktor = CalcAllgStromFactor(vertrag, jahr);

            Gruppen = GroupUpRechnungsgruppen(ctx, vertrag, this); // TODO: This this has a smell...

            BetragNebenkosten = Gruppen.Sum(g => g.BetragKalt + g.BetragWarm);

            Minderung = GetMinderung(vertrag, abrechnungsbeginn, abrechnungsende);
            NebenkostenMinderung = BetragNebenkosten * Minderung;
            KaltMinderung = KaltMiete * Minderung;

            BezahltNebenkosten = GezahlteMiete - KaltMiete + KaltMinderung;

            Result = BezahltNebenkosten - BetragNebenkosten + NebenkostenMinderung;
        }

        public List<Verbrauch> GetVerbrauchGanzeGruppe(SaverwalterContext ctx, Umlage umlage)
        {
            var Zaehler = GetAllZaehlerForVerbrauch(ctx, umlage.Typ);
            var ZaehlerMitVerbrauchForGanzeGruppe = Zaehler.Where(z => umlage.Wohnungen.Contains(z.Wohnung!)).ToList();
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(ZaehlerMitVerbrauchForGanzeGruppe, Nutzungsende);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(ZaehlerMitVerbrauchForGanzeGruppe, Nutzungsbeginn);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(umlage, ZaehlerAnfangsStaende, ZaehlerEndStaende, Notes);

            return Deltas;
        }

        public List<Verbrauch> GetVerbrauchWohnung(SaverwalterContext ctx, Umlage umlage)
        {
            var AlleZaehler = GetAllZaehlerForVerbrauch(ctx, umlage.Typ);
            var ZaehlerMitVerbrauchForThisWohnung = AlleZaehler.Where(z => z.Wohnung?.WohnungId == Wohnung.WohnungId).ToList();
            var ZaehlerEndStaende = GetZaehlerEndStaendeFuerBerechnung(ZaehlerMitVerbrauchForThisWohnung, Nutzungsende);
            var ZaehlerAnfangsStaende = GetZaehlerAnfangsStaendeFuerBerechnung(ZaehlerMitVerbrauchForThisWohnung, Nutzungsbeginn);
            List<Verbrauch> Deltas = GetVerbrauchForZaehlerStaende(umlage, ZaehlerAnfangsStaende, ZaehlerEndStaende, Notes);

            return Deltas;
        }

        private static List<Rechnungsgruppe> GroupUpRechnungsgruppen(SaverwalterContext ctx, Vertrag vertrag, Betriebskostenabrechnung b)
        {
            return vertrag.Wohnung.Umlagen
                .GroupBy(p => new SortedSet<int>(p.Wohnungen.Select(gr => gr.WohnungId).ToList()), new SortedSetIntEqualityComparer())
                .ToList()
                .Select(g => new Rechnungsgruppe(ctx, b, g.ToList()))
                .ToList();
        }

        private static double Mietzahlungen(Vertrag vertrag, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            return vertrag.Mieten
                .Where(m => m.BetreffenderMonat >= abrechnungsbeginn && m.BetreffenderMonat < abrechnungsende)
                .ToList()
                .Sum(z => z.Betrag);
        }

        private static List<IPerson> GetMieter(SaverwalterContext ctx, Vertrag vertrag)
        {
            return ctx.MieterSet
                .Where(m => m.Vertrag.VertragId == vertrag.VertragId)
                .ToList()
                .Select(m => ctx.FindPerson(m.PersonId))
                .ToList();
        }

        private static double CalcAllgStromFactor(Vertrag v, int Jahr)
        {
            return v.Wohnung.Umlagen
                .Where(s => s.Typ == Betriebskostentyp.Heizkosten)
                .ToList()
                .SelectMany(u => u.Betriebskostenrechnungen)
                .ToList()
                .Where(u => u.BetreffendesJahr == Jahr)
                .Sum(e => e.Betrag) * 0.05;
        }

        private static double GetMinderung(Vertrag vertrag, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
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

        private static double GetKaltMiete(Vertrag vertrag, List<VertragVersion> versionen, int jahr, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            if (jahr < vertrag.Beginn().Year ||
               (vertrag.Ende is DateOnly d && d.Year < jahr))
            {
                return 0;
            }
            return versionen
                .Sum(version =>
                {
                    var versionEnde = version.Ende();
                    if (versionEnde is DateOnly d && d < abrechnungsbeginn)
                    {
                        return 0;
                    }
                    else
                    {
                        var last = Min(versionEnde ?? abrechnungsende, abrechnungsende).Month;
                        var first = Max(version.Beginn, abrechnungsbeginn).Month - 1;
                        return (last - first) * version.Grundmiete;
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

        private static ImmutableList<Zaehlerstand> GetZaehlerEndStaendeFuerBerechnung(List<Zaehler> zaehler, DateOnly nutzungsende)
        {
            return zaehler
                .Select(z => z.Staende
                    .OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= nutzungsende && (nutzungsende.DayNumber - l.Datum.DayNumber) < 30))
                .ToList()
                .Where(zaehlerstand => zaehlerstand is not null)
                .Select(e => e!) // still necessary to make sure not null?
                .ToImmutableList() ?? new List<Zaehlerstand>() { }.ToImmutableList();
        }

        private static ImmutableList<Zaehlerstand> GetZaehlerAnfangsStaendeFuerBerechnung(List<Zaehler> zaehler, DateOnly nutzungsbeginn)
        {
            return zaehler
                .Select(z => z.Staende.OrderBy(s => s.Datum)
                    .ToList()
                    .Where(l => Math.Abs(l.Datum.DayNumber - nutzungsbeginn.AddDays(-1).DayNumber) < 30)
                    .ToList()
                .FirstOrDefault())
                .Where(zs => zs != null)
                .Select(e => e!) // still necessary to make sure not null?
                .ToImmutableList();
        }

        private static List<Verbrauch> GetVerbrauchForZaehlerStaende(Umlage umlage, ImmutableList<Zaehlerstand> beginne, ImmutableList<Zaehlerstand> enden, List<Note> notes)
        {
            List<Verbrauch> Deltas = new();

            if (enden.IsEmpty)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (beginne.IsEmpty)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else if (enden.Count != beginne.Count)
            {
                notes.Add(new Note("Kein Zähler für Nutzungsbeginn gefunden.", Severity.Error));
            }
            else
            {
                for (var i = 0; i < enden.Count; ++i)
                {
                    Deltas.Add(new Verbrauch(umlage.Typ, enden[i].Zaehler.Kennnummer, enden[i].Zaehler.Typ, enden[i].Stand - beginne[i].Stand));
                }
            }

            return Deltas;
        }
    }
}
