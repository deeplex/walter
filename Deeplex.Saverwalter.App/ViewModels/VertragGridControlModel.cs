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
    public class VertragGridControlModel : VertragVersionGridControlModel
    {
        public ObservableProperty<List<VertragVersionListViewModel>> Versionen { get; }
           = new ObservableProperty<List<VertragVersionListViewModel>>();

        public VertragGridControlModel(IGrouping<Guid, Vertrag> v)
            : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
            BeginnString.Value = Versionen.Value.First().BeginnString.Value;
            Beginn.Value = Versionen.Value.First().Beginn.Value;
        }
    }

    public class VertragVersionGridControlModel
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<string> Anschrift { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Wohnung { get; } = new ObservableProperty<string>();
        public ObservableProperty<DateTime> Beginn { get; } = new ObservableProperty<DateTime>();
        public ObservableProperty<string> AuflistungMieter { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> BeginnString { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> EndeString { get; } = new ObservableProperty<string>();

        public VertragVersionGridControlModel(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Anschrift.Value = v.Wohnung is Wohnung w ? Utils.Anschrift(w) : "";
            Wohnung.Value = v.Wohnung is Wohnung ww ? ww.Bezeichnung : "";

            Beginn.Value = v.Beginn;
            BeginnString.Value = v.Beginn.ToShortDateString(); ;
            EndeString.Value = v.Ende is DateTime e ? e.ToShortDateString() : "";

            AuflistungMieter.Value = string.Join(", ", v.Mieter.Select(m =>
                (m.Kontakt.Vorname is string n ? n + " " : "") + m.Kontakt.Nachname));
        }
    }
}
