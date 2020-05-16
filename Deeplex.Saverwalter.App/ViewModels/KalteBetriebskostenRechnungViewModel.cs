using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KalteBetriebskostenRechnungViewModel
    {
        public string Anschrift;

        public int AdresseId;

        public ObservableProperty<ImmutableSortedDictionary<int, ImmutableList<KalteBetriebskostenRechnungJahr>>> Jahre
            = new ObservableProperty<ImmutableSortedDictionary<int, ImmutableList<KalteBetriebskostenRechnungJahr>>>();

        public ObservableProperty<int> AddJahrBox = new ObservableProperty<int>();

        public KalteBetriebskostenRechnungViewModel(int id)
            : this(App.Walter.Adressen.Find(id)) { }
        public KalteBetriebskostenRechnungViewModel(Adresse a)
        {
            Anschrift = AdresseViewModel.Anschrift(a);
            AdresseId = a.AdresseId;

            Jahre.Value = App.Walter.KalteBetriebskostenRechnungen
                .Where(r => r.Adresse == a)
                .Include(r => r.Adresse).ThenInclude(a2 => a2.KalteBetriebskosten)
                .Select(r => new KalteBetriebskostenRechnungJahr(r))
                .ToList()
                .GroupBy(r => r.Jahr.Value)
                .ToImmutableSortedDictionary(g => g.Key, g => g.ToImmutableList(),
                    Comparer<int>.Create((x, y) => y.CompareTo(x)));

            AddJahr = new RelayCommand(_ =>
            {
                Jahre.Value = Jahre.Value.Add(AddJahrBox.Value, a.KalteBetriebskosten
                    .Select(k => new KalteBetriebskostenRechnungJahr(k.Typ, AddJahrBox.Value))
                    .ToImmutableList()).ToImmutableSortedDictionary(Comparer<int>.Create((x, y) => y.CompareTo(x)));
                AddJahrBox.Value = Jahre.Value.First().Key + 1;
            },
                _ => true);

            AddJahrBox.Value = Jahre.Value.Count() > 0 ? Jahre.Value.First().Key + 1 : DateTime.Today.Year;
        }

        public RelayCommand AddJahr { get; }
    }

    public class KalteBetriebskostenRechnungJahr
    {
        public ObservableProperty<int> Jahr = new ObservableProperty<int>();
        public ObservableProperty<KalteBetriebskosten> Typ = new ObservableProperty<KalteBetriebskosten>();
        public string Beschreibung { get; } = "";
        public bool HatBeschreibung => Beschreibung.Length > 0;

        public string Bezeichnung => Typ.Value.ToDescriptionString();

        public ObservableProperty<double> Betrag = new ObservableProperty<double>();
        public string BetragString
        {
            get => string.Format("{0:F2}", Betrag.Value);
            set
            {
                if (double.TryParse(value, out double result))
                {
                    Betrag.Value = result;
                }
                else
                {
                    Betrag.Value = 0.0;
                }
            }
        }

        public KalteBetriebskostenRechnungJahr(KalteBetriebskosten typ, int jahr)
        {
            Jahr.Value = jahr;
            Typ.Value = typ;
            Betrag.Value = 0.0;
        }

        public KalteBetriebskostenRechnungJahr(KalteBetriebskostenRechnung r)
        {
            Beschreibung = r.Adresse.KalteBetriebskosten.First(k => k.Typ == r.Typ).Beschreibung;
            Jahr.Value = r.Jahr;
            Betrag.Value = r.Betrag;
            Typ.Value = r.Typ;
        }
    }
}
