using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KontaktListViewModel : BindableBase
    {
        public int Id { get; }
        public ObservableProperty<string> Vorname { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Nachname { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Anschrift { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Email { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Telefon { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Mobil { get; } = new ObservableProperty<string>();

        public KontaktListViewModel(Kontakt k)
        {
            Id = k.KontaktId;
            Vorname.Value = k.Vorname ?? "";
            Nachname.Value = k.Nachname ?? "";
            Email.Value = k.Email ?? "";
            Telefon.Value = k.Telefon ?? "";
            Mobil.Value = k.Mobil ?? "";
            Anschrift.Value = k.Adresse is Adresse a ?
                a.Strasse + " " + a.Hausnummer + ", " +
                a.Postleitzahl + " " + a.Stadt : "";
        }
    }
}
