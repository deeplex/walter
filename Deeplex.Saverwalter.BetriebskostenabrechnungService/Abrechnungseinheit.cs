using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public class Abrechnungseinheit
    {
        public List<Umlage> Umlagen { get; }
        public List<Wohnung> Wohnungen { get; }
        public string Bezeichnung { get; }

        public double GesamtWohnflaeche { get; }
        public double GesamtNutzflaeche { get; }
        public int GesamtEinheiten { get; }

        public Abrechnungseinheit(List<Umlage> umlagen)
        {
            Umlagen = umlagen;
            Wohnungen = umlagen.First().Wohnungen.ToList();
            Bezeichnung = umlagen.First().GetWohnungenBezeichnung();

            GesamtWohnflaeche = Wohnungen.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = Wohnungen.Sum(w => w.Nutzflaeche);
            GesamtEinheiten = Wohnungen.Sum(w => w.Nutzeinheit);
        }
    }
}
