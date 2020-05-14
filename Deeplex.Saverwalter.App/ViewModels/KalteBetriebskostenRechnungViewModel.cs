using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KalteBetriebskostenRechnungViewModel
    {
        public ObservableProperty<string> Anschrift = new ObservableProperty<string>();

        public ObservableProperty<ImmutableDictionary<int, ImmutableList<KalteBetriebskostenRechnungJahr>>> Jahre
            = new ObservableProperty<ImmutableDictionary<int, ImmutableList<KalteBetriebskostenRechnungJahr>>>();

        public KalteBetriebskostenRechnungViewModel(int id) : this(App.Walter.Adressen.Find(id)) { }
        public KalteBetriebskostenRechnungViewModel(Adresse a)
        {
            Anschrift.Value = AdresseViewModel.Anschrift(a);

            Jahre.Value = App.Walter.KalteBetriebskostenRechnungen
                .Where(r => r.Adresse == a)
                .Select(r => new KalteBetriebskostenRechnungJahr(r))
                .ToList()
                .GroupBy(r => r.Jahr.Value)
                .ToImmutableDictionary(g => g.Key, g => g.ToImmutableList());
        }

        public void AddJahr(Adresse a, int jahr)
        {
            Jahre.Value = Jahre.Value.Add(jahr, App.Walter.Adressen.Find(a.AdresseId).KalteBetriebskosten
                .Select(k => new KalteBetriebskostenRechnungJahr(k.Typ, jahr)).ToImmutableList());
        }
    }

    public class KalteBetriebskostenRechnungJahr
    {
        public ObservableProperty<int> Jahr = new ObservableProperty<int>();
        public ObservableProperty<double> Betrag = new ObservableProperty<double>();
        public ObservableProperty<KalteBetriebskosten> Typ = new ObservableProperty<KalteBetriebskosten>();

        public string Bezeichnung => Typ.Value.ToDescriptionString();

        public string BetragString
        {
            get => string.Format("{0:F2}", Betrag.Value);
            set { this.Betrag.Value = double.Parse(value); }
         }

        public KalteBetriebskostenRechnungJahr(KalteBetriebskosten typ, int jahr)
        {
            Jahr.Value = jahr;
            Typ.Value = typ;
            Betrag.Value = 0.0;
        }

        public KalteBetriebskostenRechnungJahr(KalteBetriebskostenRechnung r)
        {
            Jahr.Value = r.Jahr;
            Betrag.Value = r.Betrag;
            Typ.Value = r.Typ;
        }
    }
}
