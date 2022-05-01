namespace Deeplex.Saverwalter.Model
{
    public sealed class Verbrauch
    {
        public Betriebskostentyp Betriebskostentyp;
        public Zaehlertyp Zaehlertyp;
        public string Kennnummer;
        public double Delta;

        public Verbrauch(Betriebskostentyp bTyp, string kennnummer, Zaehlertyp zTyp, double delta)
        {
            Betriebskostentyp = bTyp;
            Zaehlertyp = zTyp;
            Kennnummer = kennnummer;
            Delta = delta;
        }
    }
}
