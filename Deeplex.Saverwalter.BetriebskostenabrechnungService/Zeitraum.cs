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

        public Zeitraum(
            int jahr,
            DateOnly nutzungsbeginn,
            DateOnly nutzungsende,
            DateOnly abrechnungsbeginn,
            DateOnly abrechnungsende)
        {
            Jahr = jahr;

            Nutzungsbeginn = nutzungsbeginn;
            Nutzungsende = nutzungsende;
            Abrechnungsbeginn = abrechnungsbeginn;
            Abrechnungsende = abrechnungsende;

            Nutzungszeitraum = nutzungsende.DayNumber - nutzungsbeginn.DayNumber;
            Abrechnungszeitraum = abrechnungsende.DayNumber - abrechnungsbeginn.DayNumber;
            Zeitanteil = (double)Nutzungszeitraum / Abrechnungszeitraum;
        }
    }
}
