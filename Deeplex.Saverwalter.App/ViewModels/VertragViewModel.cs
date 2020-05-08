using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Deeplex.Utils.ObjectModel;
using Deeplex.Saverwalter.Model;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragViewModel : VertragsVersionViewModel
    {
        public ObservableProperty<List<VertragsVersionViewModel>> Versionen { get; }
            = new ObservableProperty<List<VertragsVersionViewModel>>();

        public string AuflistungMieter => "Mieter: " +
            string.Join(", ", Mieter.Value.Select(m =>
            m.Vorname.Value + (m.Vorname.Value == "" ? "" : " ") + m.Nachname.Value)); // Such grace...

        public VertragViewModel(IGrouping<Guid, Vertrag> v)
            : base (v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragsVersionViewModel(vs)).ToList();
            Beginn.Value = Versionen.Value.First().Beginn.Value;
        }
    }

    public class VertragsVersionViewModel
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<int> Personenzahl { get; } = new ObservableProperty<int>();
        public ObservableProperty<WohnungViewModel> Wohnung { get; } = new ObservableProperty<WohnungViewModel>();
        public ObservableProperty<List<KontaktViewModel>> Mieter { get; } = new ObservableProperty<List<KontaktViewModel>>();
        public ObservableProperty<DateTime> Beginn { get; } = new ObservableProperty<DateTime>();
        public ObservableProperty<DateTime?> Ende { get; } = new ObservableProperty<DateTime?>();

        public string BeginnString => Beginn.Value.ToShortDateString();
        public string EndeString => Ende.Value is DateTime e ? e.ToShortDateString() : "";

        public bool hasEnde => Ende.Value is DateTime;

        public VertragsVersionViewModel(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Personenzahl.Value = v.Personenzahl;
            Wohnung.Value = new WohnungViewModel(v.Wohnung);
            Mieter.Value = v.Mieter.Select(m => new KontaktViewModel(m.Kontakt)).ToList();
            Beginn.Value = v.Beginn;
            Ende.Value = v.Ende;
        }

        public void BeginEdit()
        {
            throw new NotImplementedException();
        }

        public void CancelEdit()
        {
            throw new NotImplementedException();
        }

        public void EndEdit()
        {
            throw new NotImplementedException();
        }
    }
}
