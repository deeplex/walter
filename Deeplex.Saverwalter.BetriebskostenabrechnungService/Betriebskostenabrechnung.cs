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
        public double GezahlteMiete { get; }
        public double KaltMiete { get; }
        public double BetragNebenkosten { get; }
        public double BezahltNebenkosten { get; }
        public double Mietminderung { get; }
        public double NebenkostenMietminderung { get; }
        public double KaltMietminderung { get; }
        public Zeitraum Zeitraum { get; }
        public List<Abrechnungseinheit> Abrechnungseinheiten { get; }

        public double Result { get; }

        public double AllgStromFaktor { get; set; }

        public Betriebskostenabrechnung(SaverwalterContext ctx, Vertrag vertrag, int jahr, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            Vertrag = vertrag;
            Zeitraum = new Zeitraum(jahr, vertrag);

            var wohnung = Vertrag.Wohnung;
            var versionen = vertrag.Versionen.OrderBy(v => v.Beginn).ToList();

            Vermieter = ctx.FindPerson(wohnung.BesitzerId);
            Ansprechpartner = ctx.FindPerson(vertrag.AnsprechpartnerId!.Value) ?? Vermieter;
            GezahlteMiete = Mietzahlungen(vertrag, Zeitraum);
            KaltMiete = GetKaltMiete(vertrag, Zeitraum);
            Mieter = GetMieter(ctx, vertrag);
            AllgStromFaktor = CalcAllgStromFactor(vertrag, jahr);
            Abrechnungseinheiten = GetAbrechnungseinheiten(vertrag);
            BetragNebenkosten = Abrechnungseinheiten.Sum((einheit) => {
                var kalteNebenkosten = BetragKalteNebenkosten(vertrag, einheit, Zeitraum, Notes);
                var warmeNebenkosten = BetragWarmeNebenkosten(wohnung, einheit, Zeitraum, Notes);

                return kalteNebenkosten + warmeNebenkosten;
            });

            Mietminderung = GetMietminderung(vertrag, abrechnungsbeginn, abrechnungsende);
            NebenkostenMietminderung = BetragNebenkosten * Mietminderung;
            KaltMietminderung = KaltMiete * Mietminderung;
            BezahltNebenkosten = GezahlteMiete - KaltMiete + KaltMietminderung;
            Result = BezahltNebenkosten - BetragNebenkosten + NebenkostenMietminderung;
        }
    }
}
