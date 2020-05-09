using System;
using System.Collections.Generic;
using System.Linq;
using Deeplex.Utils.ObjectModel;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.App;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragListViewModel : VertragVersionListViewModel
    {
        public ObservableProperty<List<VertragVersionListViewModel>> Versionen { get; }
            = new ObservableProperty<List<VertragVersionListViewModel>>();

        public VertragListViewModel(IGrouping<Guid, Vertrag> v)
            : base (v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
            BeginnString.Value = Versionen.Value.First().BeginnString.Value;
            Beginn.Value = Versionen.Value.First().Beginn.Value;
        }
    }

    public class VertragVersionListViewModel
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<int> Personenzahl { get; } = new ObservableProperty<int>();
        public ObservableProperty<string> Wohnung { get; } = new ObservableProperty<string>();
        public ObservableProperty<DateTime> Beginn { get; } = new ObservableProperty<DateTime>();
        public ObservableProperty<string> BeginnString { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> EndeString { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> AuflistungMieter { get; } = new ObservableProperty<string>();
        public ObservableProperty<bool> hasEnde = new ObservableProperty<bool>();

        public VertragVersionListViewModel(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Personenzahl.Value = v.Personenzahl;
            Wohnung.Value = v.Wohnung is Wohnung w ? Utils.Anschrift(w) + " - " + w.Bezeichnung : "";
            AuflistungMieter.Value = string.Join(", ", v.Mieter.Select(m =>
                (m.Kontakt.Vorname is string n ? n + " " : "") + m.Kontakt.Nachname)); // Such grace...

            Beginn.Value = v.Beginn;
            BeginnString.Value = v.Beginn.ToShortDateString(); ;
            hasEnde.Value = v.Ende is DateTime;
            EndeString.Value = v.Ende is DateTime e ? e.ToShortDateString() : "";
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
