using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class BetriebskostenrechnungDetailViewModel : BindableBase
    {
        private Betriebskostenrechnung Entity { get; }
        public int Id { get; }

        public ImmutableDictionary<BetriebskostenRechungenListWohnungListAdresse, ImmutableList<BetriebskostenRechungenListWohnungListWohnung>> AdresseGroup;

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>();

        public void selfDestruct()
        {
            App.Walter.Betriebskostenrechnungen.Remove(Entity);
            App.SaveWalter();
        }

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;

        public List<UmlageSchluesselUtil> Schluessel_List = Enums.UmlageSchluessel;

        public List<BetriebskostentypUtil> Typen_List = Enums.Betriebskostentyp;

        public List<BetriebskostenrechnungAllgemeinZaehler> AllgemeinZaehler_List =
            App.Walter.AllgemeinZaehlerSet
        .Select(a => new BetriebskostenrechnungAllgemeinZaehler(a))
        .ToList();

        public sealed class BetriebskostenrechnungAllgemeinZaehler
        {
            public int Id { get; }
            public string Kennnummer { get; }

            public BetriebskostenrechnungAllgemeinZaehler(AllgemeinZaehler a)
            {
                Id = a.AllgemeinZaehlerId;
                Kennnummer = a.Kennnummer;
            }
        }

        public int AllgemeinZaehler
        {
            get => Entity?.Allgemeinzaehler != null ?
                AllgemeinZaehler_List.FindIndex(a => a.Id == Entity.Allgemeinzaehler.AllgemeinZaehlerId) :
                0;
            set => update(nameof(Entity.Allgemeinzaehler),
                App.Walter.AllgemeinZaehlerSet.Find(AllgemeinZaehler_List[value].Id));
        }

        private void update<U>(string property, U value)
        {
            if (Entity == null) return;
            var type = Entity.GetType();
            var prop = type.GetProperty(property);
            var val = prop.GetValue(Entity, null);
            if (!value.Equals(val))
            {
                prop.SetValue(Entity, value);
                RaisePropertyChanged(property);
            };
        }

        public bool isHeizung => Entity?.Typ == Betriebskostentyp.Heizkosten;

        public double Betrag
        {
            get => Entity?.Betrag ?? 0.0;
            set => update(nameof(Entity.Betrag), value);
        }

        public DateTimeOffset? Datum
        {
            get => Entity?.Datum ?? DateTime.Now.Date.AsUtcKind();
            set => update(nameof(Entity.Datum), value.Value.Date.AsUtcKind());
        }

        public int Typ
        {
            get => Typen_List.FindIndex(i => i.index == (Entity != null ? (int)Entity?.Typ : 0));
            set => update(nameof(Entity.Typ), (Betriebskostentyp)Typen_List[value].index);
        }

        public int Schluessel
        {
            get => Entity != null ? (int)Entity.Schluessel : 0;
            set => update(nameof(Entity.Schluessel), (UmlageSchluessel)value);
        }

        public string Beschreibung
        {
            get => Entity?.Beschreibung ?? "";
            set => update(nameof(Entity.Beschreibung), value);
        }

        public int BetreffendesJahr
        {
            get => Entity?.BetreffendesJahr ?? DateTime.Now.Year;
            set => update(nameof(Entity.BetreffendesJahr), value);
        }

        public double HKVO_P7
        {
            get => (Entity?.HKVO_P7 ?? 0.0) * 100;
            set => update(nameof(Entity.HKVO_P7), value / 100);
        }

        public double HKVO_P8
        {
            get => (Entity?.HKVO_P8 ?? 0.0) * 100;
            set => update(nameof(Entity.HKVO_P8), value / 100);
        }

        public int HKVO_P9
        {
            get => Entity?.Typ == Betriebskostentyp.Heizkosten ?
                HKVO_P9_List.FindIndex(i => i.index == (int)Entity.HKVO_P9) : 0;
            set => update(nameof(Entity.HKVO_P9), (HKVO_P9A2)HKVO_P9_List[value].index);
        }

        public List<Tuple<int, string>> Wohnungen { get; }


        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r) : this()
        {
            Entity = r;
            Id = r.BetriebskostenrechnungId;

            
            Wohnungen = r.Gruppen.Select(g => new Tuple<int, string>(g.WohnungId, AdresseViewModel.Anschrift(g.Wohnung.Adresse) + " " + g.Wohnung.Bezeichnung)).ToList();

            AttachFile = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.BetriebskostenrechnungAnhaenge, r), _ => true);

        }

        public BetriebskostenrechnungDetailViewModel()
        {
            AdresseGroup = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Select(w => new BetriebskostenRechungenListWohnungListWohnung(w))
                .GroupBy(w => w.AdresseId)
                .ToImmutableDictionary(
                    g => new BetriebskostenRechungenListWohnungListAdresse(g.Key),
                    g => g.ToImmutableList());

            AttachFile = new AsyncRelayCommand(async _ =>
                /* TODO */await Task.FromResult<object>(null), _ => false);

            dispose = new RelayCommand(_ => this.selfDestruct());

            PropertyChanged += OnUpdate;
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand dispose;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Betrag):
                case nameof(Datum):
                case nameof(Typ):
                case nameof(Schluessel):
                case nameof(Beschreibung):
                case nameof(BetreffendesJahr):
                case nameof(HKVO_P7):
                case nameof(HKVO_P8):
                    break;
                default:
                    return;
            }

            if (Entity.Datum == null)
            {
                return;
            }

            if (Entity.BetriebskostenrechnungId != 0)
            {
                App.Walter.Betriebskostenrechnungen.Update(Entity);
            }
            else
            {
                App.Walter.Betriebskostenrechnungen.Add(Entity);
            }
            App.SaveWalter();
        }
    }
}
