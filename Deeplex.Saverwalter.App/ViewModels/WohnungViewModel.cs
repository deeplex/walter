using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Deeplex.Utils.ObjectModel;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class WohnungViewModel
    {
        public int Id { get; }
        public ObservableProperty<string> Bezeichnung { get; } = new ObservableProperty<string>();
        public ObservableProperty<AdresseViewModel> Adresse { get; } = new ObservableProperty<AdresseViewModel>();
        public ObservableProperty<double> Wohnflaeche { get; } = new ObservableProperty<double>();
        public ObservableProperty<double> Nutzflaeche { get; } = new ObservableProperty<double>();

        public string Anschrift => Adresse.Value.Anschrift;
        
        public WohnungViewModel(Wohnung w)
        {
            Id = w.WohnungId;
            Bezeichnung.Value = w.Bezeichnung;
            Wohnflaeche.Value = w.Wohnflaeche;
            Nutzflaeche.Value = w.Nutzflaeche;
            Adresse.Value = new AdresseViewModel(w.Adresse);
        }
    }
}
