using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.Utils;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class Betriebskostenabrechnung
    {
        public List<Note> Notes { get; } = new List<Note>();
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
        public List<Zaehler> Zaehler { get; }
        public Zeitraum Zeitraum { get; }
        public List<Abrechnungseinheit> Abrechnungseinheiten { get; }

        public double Result { get; }

        public double AllgStromFaktor { get; set; }

        public Betriebskostenabrechnung(SaverwalterContext ctx, Vertrag vertrag, int jahr, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            Vertrag = vertrag;

            var nutzungsbeginn = Max(vertrag.Beginn(), abrechnungsbeginn);
            var nutzungsende = Min(vertrag.Ende ?? abrechnungsende, abrechnungsende);

            Zeitraum = new Zeitraum(jahr, nutzungsbeginn, nutzungsende, abrechnungsbeginn, abrechnungsende);

            Wohnung = vertrag.Wohnung;
            Adresse = Wohnung.Adresse!; // TODO the Adresse here shouldn't be null, this should be catched.
            Zaehler = Wohnung.Zaehler.ToList();
            var versionen = vertrag.Versionen.OrderBy(v => v.Beginn).ToList();

            Vermieter = ctx.FindPerson(Wohnung.BesitzerId);
            Ansprechpartner = ctx.FindPerson(vertrag.AnsprechpartnerId!.Value) ?? Vermieter;
            GezahlteMiete = Mietzahlungen(vertrag, abrechnungsbeginn, abrechnungsende);
            KaltMiete = GetKaltMiete(vertrag, versionen, jahr, abrechnungsbeginn, abrechnungsende);
            Mieter = GetMieter(ctx, vertrag);
            AllgStromFaktor = CalcAllgStromFactor(vertrag, jahr);
            Abrechnungseinheiten = DetermineAbrechnungseinheiten(vertrag);
            BetragNebenkosten = Abrechnungseinheiten.Sum(
                einheit => BetragKalteNebenkosten(einheit) + BetragWarmeNebenkosten(einheit));

            Mietminderung = GetMietminderung(vertrag, abrechnungsbeginn, abrechnungsende);
            NebenkostenMietminderung = BetragNebenkosten * Mietminderung;
            KaltMietminderung = KaltMiete * Mietminderung;
            BezahltNebenkosten = GezahlteMiete - KaltMiete + KaltMietminderung;
            Result = BezahltNebenkosten - BetragNebenkosten + NebenkostenMietminderung;
        }

        public List<PersonenZeitanteil> PersonenZeitanteil(Abrechnungseinheit einheit)
            => GetPersonenZeitanteil(
                PersonenIntervall(),
                GesamtPersonenIntervall(einheit),
                Zeitraum);

        public Dictionary<Betriebskostentyp, double> VerbrauchAnteil(Abrechnungseinheit einheit)
            => CalculateVerbrauchAnteil(Verbrauch(einheit));


        public double GesamtBetragKalteNebenkosten(Abrechnungseinheit einheit)
            => GetKalteNebenkosten(einheit, Zeitraum).Sum(r => r.Betrag);

        public List<PersonenZeitIntervall> GesamtPersonenIntervall(Abrechnungseinheit einheit)
            => VertraegeIntervallPersonenzahl(
                getAllVertragVersionen(einheit.Wohnungen, Zeitraum),
                Zeitraum);

        public Dictionary<Betriebskostentyp, List<(Zaehlertyp Typ, double Delta)>> GesamtVerbrauch(Abrechnungseinheit einheit)
             => CalculateAbrechnungseinheitVerbrauch(einheit.Umlagen, Zeitraum, Notes);

        public List<PersonenZeitIntervall> PersonenIntervall()
            => VertraegeIntervallPersonenzahl(Vertrag.Versionen, Zeitraum);

        public Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch(Abrechnungseinheit einheit)
            => CalculateWohnungVerbrauch(
                einheit.Umlagen,
                Wohnung,
                Zeitraum,
                GesamtVerbrauch(einheit),
                Notes);

        public List<Heizkostenberechnung> Heizkosten(Abrechnungseinheit einheit)
            => CalculateHeizkosten(einheit.Umlagen, Wohnung, Zeitraum, Notes);

        public double GesamtBetragWarmeNebenkosten(Abrechnungseinheit einheit)
            => Heizkosten(einheit).Sum(heizkostenberechnung => heizkostenberechnung.PauschalBetrag);

        public double BetragWarmeNebenkosten(Abrechnungseinheit einheit)
            => Heizkosten(einheit)
                .Sum(heizkostenberechnung => heizkostenberechnung.Kosten);


        public double BetragKalteNebenkosten(Abrechnungseinheit einheit)
            => CalculateBetragKalteNebenkosten(
                GetKalteNebenkosten(einheit, Zeitraum),
                WFZeitanteil(einheit),
                NEZeitanteil(einheit),
                PersonenZeitanteil(einheit),
                VerbrauchAnteil(einheit),
                Notes);

        public double WFZeitanteil(Abrechnungseinheit einheit)
            => Vertrag.Wohnung.Wohnflaeche / einheit.GesamtWohnflaeche * Zeitraum.Zeitanteil;

        public double NFZeitanteil(Abrechnungseinheit einheit)
            => Vertrag.Wohnung.Nutzflaeche / einheit.GesamtNutzflaeche * Zeitraum.Zeitanteil;

        public double NEZeitanteil(Abrechnungseinheit einheit)
            => Vertrag.Wohnung.Nutzeinheit / einheit.GesamtEinheiten * Zeitraum.Zeitanteil;
    }
}
