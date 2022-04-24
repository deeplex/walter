using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.Services
{
    public sealed class AutoSuggestEntry
    {
        public override string ToString() => Bezeichnung;
        public string Bezeichnung;
        public string Icon;
        public object Entity;

        public AutoSuggestEntry(NatuerlichePerson a)
        {
            Entity = a;
            Icon = "ContactInfo";
            Bezeichnung = a.Bezeichnung;
        }
        public AutoSuggestEntry(JuristischePerson a)
        {
            Entity = a;
            Icon = "ContactInfo";
            Bezeichnung = a.Bezeichnung;
        }
        public AutoSuggestEntry(Wohnung a)
        {
            Entity = a;
            Icon = "Street";
            Bezeichnung = a.Adresse.Anschrift + " - " + a.Bezeichnung;
        }
        public AutoSuggestEntry(Zaehler a)
        {
            Entity = a;
            Icon = "Clock";
            Bezeichnung = a.Kennnummer;
        }
        public AutoSuggestEntry(Vertrag a)
        {
            Entity = a;
            Icon = "Library";
            Bezeichnung = a.Wohnung.Adresse.Anschrift + " - " + a.Wohnung.Bezeichnung;
        }
        public AutoSuggestEntry(Betriebskostenrechnung a)
        {
            Entity = a;
            Icon = "List";
            Bezeichnung = a.Typ.ToDescriptionString() + " - " + a.GetWohnungenBezeichnung();
        }
        public AutoSuggestEntry(Erhaltungsaufwendung a)
        {
            Entity = a;
            Icon = "Bullets";
            Bezeichnung = a.Bezeichnung;
        }
    }
}
