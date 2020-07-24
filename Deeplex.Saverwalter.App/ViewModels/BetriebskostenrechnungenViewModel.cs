using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class BetriebskostenrechnungViewModel : BindableBase
    {
        private Betriebskostenrechnung Entity { get; }

        public ImmutableDictionary<BetriebskostenRechungenListWohnungListAdresse, ImmutableList<BetriebskostenRechungenListWohnungListWohnung>> AdresseGroup;

        public void selfDestruct()
        {
            App.Walter.Betriebskostenrechnungen.Remove(Entity);
            App.SaveWalter();
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

        public BetriebskostenrechnungViewModel(Betriebskostenrechnung r)
            : this()
        {
            Entity = r;
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
        }
    }
}
