using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class AutoSuggestListViewModelEntry
    {
        public override string ToString() => Bezeichnung;
        public string Bezeichnung;
        public string Icon;
        public object Entity;

        public AutoSuggestListViewModelEntry(NatuerlichePerson a)
        {
            Entity = a;
            Icon = "ContactInfo";
            Bezeichnung = a.Bezeichnung;
        }
        public AutoSuggestListViewModelEntry(JuristischePerson a)
        {
            Entity = a;
            Icon = "ContactInfo";
            Bezeichnung = a.Bezeichnung;
        }
        public AutoSuggestListViewModelEntry(Wohnung a)
        {
            Entity = a;
            Icon = "Street";
            Bezeichnung = a.Adresse.Anschrift + " - " + a.Bezeichnung;
        }
        public AutoSuggestListViewModelEntry(Zaehler a)
        {
            Entity = a;
            Icon = "Clock";
            Bezeichnung = a.Kennnummer;
        }
        public AutoSuggestListViewModelEntry(Vertrag a)
        {
            Entity = a;
            Icon = "Library";
            Bezeichnung = a.Wohnung.Adresse.Anschrift + " - " + a.Wohnung.Bezeichnung;
        }
        public AutoSuggestListViewModelEntry(Betriebskostenrechnung a)
        {
            Entity = a;
            Icon = "List";
            Bezeichnung = a.Typ.ToDescriptionString() + " - " + a.GetWohnungenBezeichnung();
        }
        public AutoSuggestListViewModelEntry(Erhaltungsaufwendung a)
        {
            Entity = a;
            Icon = "Bullets";
            Bezeichnung = a.Bezeichnung;
        }
    }
}
