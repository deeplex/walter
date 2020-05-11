using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class AdresseViewModel
    {
        public int Id;
        public ObservableProperty<string> Strasse { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Hausnummer { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Postleitzahl { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Stadt { get; } = new ObservableProperty<string>();

        public AdresseViewModel() { }

        public AdresseViewModel(Adresse a)
        {
            Id = a.AdresseId;
            Strasse.Value = a.Strasse;
            Hausnummer.Value = a.Hausnummer;
            Postleitzahl.Value = a.Postleitzahl;
            Stadt.Value = a.Stadt;
        }

        public static Adresse GetAdresse(AdresseViewModel avm)
        {
            // TODO Remove unreferenced Adresses - Add Wohnungen and Kontakte List to Walter.

            // TODO Give some indication to the user?
            // If one is set => all must be set.
            if (avm is null ||
                avm.Postleitzahl.Value == null || avm.Postleitzahl.Value == "" ||
                avm.Hausnummer.Value == null || avm.Hausnummer.Value == "" ||
                avm.Strasse.Value == null || avm.Strasse.Value == "" ||
                avm.Stadt.Value == null || avm.Stadt.Value == "")
            {
                return null;
            }

            var adr = App.Walter.Adressen.FirstOrDefault(a2 =>
                a2.Postleitzahl == avm.Postleitzahl.Value &&
                a2.Hausnummer == avm.Hausnummer.Value &&
                a2.Strasse == avm.Strasse.Value &&
                a2.Stadt == avm.Stadt.Value);

            if (adr is Adresse)
            {
                return adr;
            }
            else
            {
                adr = new Adresse
                {
                    Postleitzahl = avm.Postleitzahl.Value,
                    Hausnummer = avm.Hausnummer.Value,
                    Strasse = avm.Strasse.Value,
                    Stadt = avm.Stadt.Value
                };
                App.Walter.Adressen.Add(adr);
                return adr;
            }
        }
    }
}
