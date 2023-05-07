namespace Deeplex.Saverwalter.Model
{
    public static class WohnungExtensions
    {
        public static string GetWohnungenBezeichnung(this Umlage u)
            => u.Wohnungen.ToList().GetWohnungenBezeichnung();

        public static string GetWohnungenBezeichnung(this List<Wohnung> Wohnungen)
            => string.Join(" — ", Wohnungen
                .Where(w => w.Adresse != null)
                .GroupBy(w => w.Adresse!)
                .ToDictionary(g => g.Key, g => g.ToList())
                .Select(adr =>
                {
                    var a = adr.Key;
                    var ret = a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;
                    if (adr.Value.Count() != a.Wohnungen.Count)
                    {
                        ret += ": " + string.Join(", ", adr.Value.Select(w => w.Bezeichnung));
                    }
                    else
                    {
                        ret += " (gesamt)";
                    }
                    return ret;
                }));
    }
}
