namespace Deeplex.Saverwalter.Model
{
    public static class AnhangExtensions
    {
        public static string getPath(this Anhang a, string root)
        {
            return System.IO.Path.Combine(root, a.AnhangId + System.IO.Path.GetExtension(a.FileName));
        }

        public static int getReferences(this Anhang a)
        {
            return a.Adressen.Count +
                a.Betriebskostenrechnungen.Count +
                a.Erhaltungsaufwendungen.Count +
                a.Garagen.Count +
                a.JuristischePersonen.Count +
                a.Konten.Count +
                a.Mieten.Count +
                a.Mietminderungen.Count +
                a.NatuerlichePersonen.Count +
                a.Vertraege.Count +
                a.VertragsBetriebskostenrechnungen.Count +
                a.Wohnungen.Count +
                a.Zaehler.Count +
                a.Zaehlerstaende.Count;
        }
    }
}
