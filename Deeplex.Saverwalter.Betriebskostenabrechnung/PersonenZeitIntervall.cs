namespace Deeplex.Saverwalter.Model
{
    public sealed class PersonenZeitIntervall
    {
        public DateTime Beginn { get; }
        public DateTime Ende { get; }
        public int Tage { get; }
        public int GesamtTage { get; }
        public int Personenzahl { get; }

        public PersonenZeitIntervall(DateTime beginn, DateTime ende, int personenzahl, Rechnungsgruppe parent)
        {
            Beginn = beginn;
            Ende = ende;
            Personenzahl = personenzahl;
            Tage = (Ende - Beginn).Days + 1;
            GesamtTage = (new DateTime(Ende.Year, 12, 31) - new DateTime(Ende.Year, 1, 1)).Days + 1;
        }
    }

}
