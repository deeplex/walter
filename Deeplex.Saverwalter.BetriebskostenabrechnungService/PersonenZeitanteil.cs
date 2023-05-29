namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public class PersonenZeitanteil
    {
        public DateOnly Beginn { get; }
        public DateOnly Ende { get; }
        public double Tage { get; }
        public int Personenzahl { get; }
        public int GesamtPersonenzahl { get; }
        public double Anteil { get; }

        public PersonenZeitanteil(
            DateOnly beginn, DateOnly ende, int personenzahl, int gesamtPersonenzahl, Zeitraum zeitraum)
        {
            Beginn = beginn;
            Ende = ende;
            Tage = ende.DayNumber - beginn.DayNumber + 1;
            Personenzahl = personenzahl;
            GesamtPersonenzahl = gesamtPersonenzahl;

            var personenAnteil = gesamtPersonenzahl == 0
                ? 0
                : (double)personenzahl / gesamtPersonenzahl;

            var zeitanteil = (double)Tage / zeitraum.Abrechnungszeitraum;

            Anteil = personenAnteil * zeitanteil;
        }
    }
}
