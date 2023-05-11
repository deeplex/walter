namespace Deeplex.Saverwalter.Model
{
    public sealed class PersonenZeitIntervall
    {
        public DateOnly Beginn { get; }
        public DateOnly Ende { get; }
        public int Tage { get; }
        public int GesamtTage { get; }
        public int Personenzahl { get; }

        public PersonenZeitIntervall(DateOnly beginn, DateOnly ende, int personenzahl)
        {
            Beginn = beginn;
            Ende = ende;
            Personenzahl = personenzahl;
            Tage = Ende.DayNumber - Beginn.DayNumber + 1;
            GesamtTage = new DateOnly(Ende.Year, 12, 31).DayNumber - new DateOnly(Ende.Year, 1, 1).DayNumber + 1;
        }
    }

}
