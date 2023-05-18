namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public class PersonenZeitanteil
    {
        public DateOnly Beginn { get; }
        public DateOnly Ende { get; }
        public double Anteil { get; }
        public int Personenzahl { get; }

        public PersonenZeitanteil(
            PersonenZeitIntervall interval,
            List<PersonenZeitIntervall> gesamtPersonenZeitIntervallList,
            int abrechnungszeitspanne)
        {
            Beginn = interval.Beginn;
            Ende = interval.Ende;
            Personenzahl = interval.Personenzahl;

            if (Personenzahl == 0)
            {
                Anteil = 0;
            }
            else
            {
                var gesamtPersonenZahl = gesamtPersonenZeitIntervallList
                    .FirstOrDefault(pzi => Beginn <= pzi.Beginn)?.Personenzahl ?? 0;
                var personenAnteil = (double)Personenzahl / gesamtPersonenZahl;
                var zeitSpanne = Ende.DayNumber - Beginn.DayNumber + 1;
                var zeitAnteil = (double)zeitSpanne / abrechnungszeitspanne;

                Anteil = personenAnteil * zeitAnteil;
            }
        }
    }
}
