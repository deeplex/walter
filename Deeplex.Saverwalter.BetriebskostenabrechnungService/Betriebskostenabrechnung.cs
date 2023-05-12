using Deeplex.Saverwalter.Model;
using System.Collections.Immutable;
using static Deeplex.Saverwalter.Model.Utils;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public interface IBetriebskostenabrechnung
    {
        IPerson Ansprechpartner { get; }
        List<Abrechnungseinheit> Abrechnungseinheiten { get; }
        Wohnung Wohnung { get; }
        DateOnly Nutzungsbeginn { get; }
        int Nutzungszeitspanne { get; }
        int Abrechnungszeitspanne { get; }
        DateOnly Nutzungsende { get; }
        int Jahr { get; }
        IPerson Vermieter { get; }
        List<IPerson> Mieter { get; }
        double GezahlteMiete { get; }
        double KaltMietminderung { get; }
        double KaltMiete { get; }
        double Mietminderung { get; }
        double NebenkostenMietminderung { get; }
        double Result { get; }
        Adresse Adresse { get; }
        DateOnly Abrechnungsbeginn { get; }
        DateOnly Abrechnungsende { get; }
        double Zeitanteil { get; }
        List<Note> Notes { get; }
        Vertrag Vertrag { get; }
        List<VertragVersion> Versionen { get; }
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
        public double Mietminderung { get; }
        public double NebenkostenMietminderung { get; }
        public double KaltMietminderung { get; }
        public DateOnly Nutzungsbeginn { get; }
        public DateOnly Nutzungsende { get; }
        public List<Zaehler> Zaehler { get; }

        public int Abrechnungszeitspanne { get; }
        public int Nutzungszeitspanne { get; }
        public double Zeitanteil { get; }

        public List<Abrechnungseinheit> Abrechnungseinheiten { get; }

        public double Result { get; }

        public double AllgStromFaktor { get; set; }

        public Betriebskostenabrechnung(SaverwalterContext ctx, Vertrag vertrag, int jahr, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            Vertrag = vertrag;
            Jahr = jahr;
            
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;
            Abrechnungszeitspanne = abrechnungsende.DayNumber - abrechnungsbeginn.DayNumber + 1;

            Wohnung = vertrag.Wohnung;
            Adresse = Wohnung.Adresse!; // TODO the Adresse here shouldn't be null, this should be catched.
            Zaehler = Wohnung.Zaehler.ToList();
            Versionen = vertrag.Versionen.OrderBy(v => v.Beginn).ToList();

            Nutzungsbeginn = Max(vertrag.Beginn(), abrechnungsbeginn);
            Nutzungsende = Min(vertrag.Ende ?? abrechnungsende, abrechnungsende);
            Nutzungszeitspanne = Nutzungsende.DayNumber - Nutzungsbeginn.DayNumber + 1;
            Zeitanteil = (double)Nutzungszeitspanne / Abrechnungszeitspanne;

            Vermieter = ctx.FindPerson(Wohnung.BesitzerId);
            Ansprechpartner = ctx.FindPerson(vertrag.AnsprechpartnerId!.Value) ?? Vermieter;

            GezahlteMiete = Mietzahlungen(vertrag, abrechnungsbeginn, abrechnungsende);
            KaltMiete = GetKaltMiete(vertrag, Versionen, jahr, abrechnungsbeginn, abrechnungsende);

            Mieter = GetMieter(ctx, vertrag);

            AllgStromFaktor = CalcAllgStromFactor(vertrag, jahr);

            Abrechnungseinheiten = DetermineAbrechnungseinheiten(vertrag);

            BetragNebenkosten = Abrechnungseinheiten.Sum(g => g.BetragKalteNebenkosten + g.BetragWarmeNebenkosten);

            Mietminderung = GetMietminderung(vertrag, abrechnungsbeginn, abrechnungsende);
            NebenkostenMietminderung = BetragNebenkosten * Mietminderung;
            KaltMietminderung = KaltMiete * Mietminderung;

            BezahltNebenkosten = GezahlteMiete - KaltMiete + KaltMietminderung;

            Result = BezahltNebenkosten - BetragNebenkosten + NebenkostenMietminderung;
        }

        private List<Abrechnungseinheit> DetermineAbrechnungseinheiten(Vertrag vertrag)
        {
            // Group up all Wohnungen sharing the same Umlage
            var einheiten = vertrag.Wohnung.Umlagen
                .GroupBy(umlage =>
                    new SortedSet<int>(umlage.Wohnungen.Select(gr => gr.WohnungId).ToList()),
                    new SortedSetIntEqualityComparer())
                .ToList();
            // Then create Rechnungsgruppen for every single one of those groups with respective information to calculate the distribution
            return einheiten
                .Select(umlagen => new Abrechnungseinheit(umlagen.ToList(), Wohnung, Versionen, Jahr, Abrechnungsbeginn, Abrechnungsende, Abrechnungszeitspanne, Nutzungsbeginn, Nutzungsende, Zeitanteil, Notes))
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

        private static double CalcAllgStromFactor(Vertrag vertrag, int Jahr)
        {
            return vertrag.Wohnung.Umlagen
                .Where(s => s.Typ == Betriebskostentyp.Heizkosten)
                .ToList()
                .SelectMany(u => u.Betriebskostenrechnungen)
                .ToList()
                .Where(u => u.BetreffendesJahr == Jahr)
                .Sum(e => e.Betrag) * 0.05;
        }

        private static double GetMietminderung(Vertrag vertrag, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
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
    }
}
