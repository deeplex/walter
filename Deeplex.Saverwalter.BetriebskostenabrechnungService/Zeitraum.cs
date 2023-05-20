using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public class Zeitraum
    {
        public DateOnly Nutzungsbeginn { get; }
        public DateOnly Nutzungsende { get; }
        public DateOnly Abrechnungsbeginn { get; }
        public DateOnly Abrechnungsende { get; }
        public int Abrechnungszeitraum { get; }
        public int Nutzungszeitraum { get; }
        public double Zeitanteil { get; }
        public int Jahr { get; }

        public Zeitraum(int jahr, Vertrag vertrag)
        {
            Jahr = jahr;
            Abrechnungsbeginn = new DateOnly(jahr, 1, 1);
            Abrechnungsende = new DateOnly(jahr, 12, 31);
            var beginn = vertrag.Beginn();
            Nutzungsbeginn = Utils.Max(vertrag.Beginn(), Abrechnungsbeginn);
            Nutzungsende = Utils.Min(vertrag.Ende ?? Abrechnungsende, Abrechnungsende);

            Nutzungszeitraum = Nutzungsende.DayNumber - Nutzungsbeginn.DayNumber;
            Abrechnungszeitraum = Abrechnungsende.DayNumber - Abrechnungsbeginn.DayNumber;
            Zeitanteil = (double)Nutzungszeitraum / Abrechnungszeitraum;
        }
    }
}
