using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class AdresseViewModel
    {
        public int Id;
        public ObservableProperty<string> Strasse { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Hausnummer { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Postleitzahl { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Stadt { get; } = new ObservableProperty<string>();

        public AdresseViewModel(Adresse a)
        {
            Id = a.AdresseId;
            Strasse.Value = a.Strasse;
            Hausnummer.Value = a.Hausnummer;
            Postleitzahl.Value = a.Postleitzahl;
            Stadt.Value = a.Stadt;
        }
    }
}
