using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
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
        public ObservableProperty<List<WohnungDetailVertrag>> Vertraege
            = new ObservableProperty<List<WohnungDetailVertrag>>();

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

            Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Mieter)
                .ThenInclude(m => m.Kontakt)
                .Include(v => v.Wohnung).ToList()
                .Where(v => v.Wohnung.WohnungId == Id)
                .Select(v => new WohnungDetailVertrag(v.VertragId))
                .ToList();
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

    public class WohnungDetailVertrag
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<DateTime> Beginn { get; } = new ObservableProperty<DateTime>();
        public ObservableProperty<string> AuflistungMieter { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> BeginnString { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> EndeString { get; } = new ObservableProperty<string>();
        public ObservableProperty<List<VertragVersionListViewModel>> Versionen { get; }
            = new ObservableProperty<List<VertragVersionListViewModel>>();

        public WohnungDetailVertrag(Guid id)
            : this(App.Walter.Vertraege.Where(v => v.VertragId == id)) { }

        private WohnungDetailVertrag(IEnumerable<Vertrag> v)
            : this(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
            BeginnString.Value = Versionen.Value.First().BeginnString.Value;
            Beginn.Value = Versionen.Value.First().Beginn.Value;
        }

        private WohnungDetailVertrag(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;

            Beginn.Value = v.Beginn;
            BeginnString.Value = v.Beginn.ToShortDateString(); ;
            EndeString.Value = v.Ende is DateTime e ? e.ToShortDateString() : "";

            AuflistungMieter.Value = string.Join(", ", v.Mieter.Select(m =>
                (m.Kontakt.Vorname is string n ? n + " " : "") + m.Kontakt.Nachname));
        }
    }
}
