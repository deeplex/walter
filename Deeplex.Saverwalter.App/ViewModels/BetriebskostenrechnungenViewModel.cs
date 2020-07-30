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
    public sealed class BetriebskostenrechnungViewModel : BindableBase
    {
        private Betriebskostenrechnung Entity { get; }

        public ImmutableDictionary<BetriebskostenRechungenListWohnungListAdresse, ImmutableList<BetriebskostenRechungenListWohnungListWohnung>> AdresseGroup;

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>();

        public void selfDestruct()
        {
            App.Walter.Betriebskostenrechnungen.Remove(Entity);
            App.SaveWalter();
        }

        public sealed class BetriebskostenrechnungEnum
        {
            public int index { get; }
            public string text { get; }

            public BetriebskostenrechnungEnum(HKVO_P9A2 S)
            {
                index = (int)S;
                text = "Satz " + index.ToString();
            }

            public BetriebskostenrechnungEnum(UmlageSchluessel S)
            {
                index = (int)S;
                text = S.ToDescriptionString();
            }

            public BetriebskostenrechnungEnum(Betriebskostentyp T)
            {
                index = (int)T;
                text = T.ToDescriptionString();
            }
        }

        public List<BetriebskostenrechnungEnum> HKVO_P9_List =
            Enum.GetValues(typeof(HKVO_P9A2))
                .Cast<HKVO_P9A2>().ToList()
                .Select(s => new BetriebskostenrechnungEnum(s))
                .ToList();

        public List<BetriebskostenrechnungEnum> Schluessel_List =
            Enum.GetValues(typeof(UmlageSchluessel))
                .Cast<UmlageSchluessel>().ToList()
                .Select(s => new BetriebskostenrechnungEnum(s))
                .ToList();

        public List<BetriebskostenrechnungEnum> Typen_List =
            Enum.GetValues(typeof(Betriebskostentyp))
            .Cast<Betriebskostentyp>().ToList()
            .Select(s => new BetriebskostenrechnungEnum(s))
            .ToList();

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

        public DateTime Datum
        {
            get => Entity?.Datum ?? DateTime.Now;
            set => update(nameof(Entity.Datum), value.Date.AsUtcKind());
        }

        public Betriebskostentyp Typ
        {
            get => Entity?.Typ ?? Betriebskostentyp.AllgemeinstromHausbeleuchtung;
            set => update(nameof(Entity.Typ), value);
        }

        public UmlageSchluessel Schluessel
        {
            get => Entity?.Schluessel ?? UmlageSchluessel.NachNutzeinheit;
            set => update(nameof(Entity.Schluessel), value);
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

        public BetriebskostenrechnungViewModel(Betriebskostenrechnung r) : this()
        {
            Entity = r;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.BetriebskostenrechnungAnhaenge, r), _ => true);

        }

        public BetriebskostenrechnungViewModel()
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

            PropertyChanged += OnUpdate;
        }

        public AsyncRelayCommand AttachFile;

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
