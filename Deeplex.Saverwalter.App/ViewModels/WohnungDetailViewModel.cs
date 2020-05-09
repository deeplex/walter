using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class WohnungDetailViewModel : BindableBase
    {
        public int Id;
        public ObservableProperty<string> Bezeichnung = new ObservableProperty<string>();
        public ObservableProperty<string> Anschrift = new ObservableProperty<string>();
        public ObservableProperty<double> Wohnflaeche = new ObservableProperty<double>();
        public ObservableProperty<double> Nutzflaeche = new ObservableProperty<double>();

        public WohnungDetailViewModel(int id)
            : this(App.Walter.Wohnungen.Find(id)) { }

        private WohnungDetailViewModel(Wohnung w)
        {
            Id = w.WohnungId;
            Anschrift.Value = Utils.Anschrift(w);
            Bezeichnung.Value = w.Bezeichnung;
            Wohnflaeche.Value = w.Wohnflaeche;
            Nutzflaeche.Value = w.Nutzflaeche;
        }
    }
}
