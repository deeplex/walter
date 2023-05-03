namespace Deeplex.Saverwalter.Model
{
    public sealed class VerbrauchAnteil
    {
        public string Kennnummer;
        public Zaehlertyp Typ;
        public double Delta;
        public double Anteil;

        public VerbrauchAnteil(string kennnummer, Zaehlertyp typ, double delta, double anteil)
        {
            Kennnummer = kennnummer;
            Typ = typ;
            Delta = delta;
            Anteil = anteil;
        }
    }
}
