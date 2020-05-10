using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class WohnungDetailViewModel : BindableBase
    {
        public int Id;
        public ObservableProperty<int> AdresseId = new ObservableProperty<int>();
        public ObservableProperty<string> Bezeichnung = new ObservableProperty<string>();
        public ObservableProperty<string> Anschrift = new ObservableProperty<string>();
        public ObservableProperty<double> Wohnflaeche = new ObservableProperty<double>();
        public ObservableProperty<double> Nutzflaeche = new ObservableProperty<double>();

        public ObservableProperty<List<WohnungDetailZaehler>> Zaehler
            = new ObservableProperty<List<WohnungDetailZaehler>>();

        public WohnungDetailViewModel(int id)
            : this(App.Walter.Wohnungen.Include(w => w.Zaehler).First(w => w.WohnungId == id)) { }

        private WohnungDetailViewModel(Wohnung w)
        {
            Id = w.WohnungId;
            AdresseId.Value = w.AdresseId;
            Anschrift.Value = Utils.Anschrift(w);
            Bezeichnung.Value = w.Bezeichnung;
            Wohnflaeche.Value = w.Wohnflaeche;
            Nutzflaeche.Value = w.Nutzflaeche;

            Zaehler.Value = w.Zaehler.Select(z => new WohnungDetailZaehler(z)).ToList();
        }
    }

    public class WohnungDetailZaehler : BindableBase
    {
        public int Id;
        public ObservableProperty<string> Kennnummer = new ObservableProperty<string>();
        public ObservableProperty<string> Typ = new ObservableProperty<string>();

        public WohnungDetailZaehler(Zaehler z)
        {
            Id = z.ZaehlerId;
            Kennnummer.Value = "312k2112nj1fnj21"; // TODO Add Zaehler.Kennummer to Model.cs
            Typ.Value = z.Typ.ToString(); // May be a descript thingy later on?...
        }
    }
}
