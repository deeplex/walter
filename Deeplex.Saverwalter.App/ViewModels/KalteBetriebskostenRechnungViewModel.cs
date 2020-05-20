using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KalteBetriebskostenRechnungViewModel : BindableBase
    {
        public string Anschrift;

        public int AdresseId;

        public ObservableProperty<ImmutableSortedDictionary<int, ImmutableList<KalteBetriebskostenRechnungJahr>>> Jahre
            = new ObservableProperty<ImmutableSortedDictionary<int, ImmutableList<KalteBetriebskostenRechnungJahr>>>();

        public bool IsNotEmpty => !Jahre.Value.IsEmpty;

        public int SelectedJahr { get; set; }

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
                RaisePropertyChanged(nameof(IsNotEmpty));
            }, _ => true);

            RemoveJahr = new RelayCommand(_ =>
            {
                Jahre.Value = Jahre.Value.Remove(SelectedJahr);
                RaisePropertyChanged(nameof(IsNotEmpty));
            }, _ => true); // TODO Only if a year is selected

            SaveEdit = new RelayCommand(_ =>
            {
                var alleJahre = Jahre.Value.SelectMany(r => r.Value).ToList();
                foreach (var kbr in a.KalteBetriebskostenRechnungen)
                {
                    var alleRechnungen = alleJahre.FirstOrDefault(r => r.Jahr.Value == kbr.Jahr && r.Typ.Value == kbr.Typ);
                    if (alleRechnungen == null)
                    {
                        App.Walter.KalteBetriebskostenRechnungen.Remove(kbr);
                    }
                    else
                    {
                        kbr.Betrag = alleRechnungen.Betrag;
                        App.Walter.KalteBetriebskostenRechnungen.Update(kbr);
                    }
                }
                foreach (var rechnung in alleJahre)
                {
                    if (!a.KalteBetriebskostenRechnungen.Exists(r => r.Jahr == rechnung.Jahr.Value && r.Typ == rechnung.Typ.Value))
                    {
                        App.Walter.KalteBetriebskostenRechnungen.Add(new KalteBetriebskostenRechnung
                        {
                            Adresse = a,
                            Typ = rechnung.Typ.Value,
                            Betrag = rechnung.Betrag,
                            Jahr = rechnung.Jahr.Value,
                        });
                    }
                }
                App.Walter.SaveChanges();
            }, _ => true);

            AddJahrBox.Value = Jahre.Value.Count() > 0 ? Jahre.Value.First().Key + 1 : DateTime.Today.Year;
        }

        public RelayCommand AddJahr { get; }
        public RelayCommand RemoveJahr { get; }
        public RelayCommand SaveEdit { get; }
    }

    public class KalteBetriebskostenRechnungJahr : BindableBase
    {
        public ObservableProperty<int> Jahr = new ObservableProperty<int>();
        public ObservableProperty<KalteBetriebskosten> Typ = new ObservableProperty<KalteBetriebskosten>();
        public string Beschreibung { get; } = "";
        public bool HatBeschreibung => Beschreibung == null ? false : Beschreibung.Length > 0;

        public string Bezeichnung => Typ.Value.ToDescriptionString();

        public double Betrag;
        public string BetragString
        {
            get => Betrag > 0 ? string.Format("{0:F2}", Betrag) : "";
            set
            {
                if (double.TryParse(value, out double result))
                {
                    SetProperty(ref Betrag, result);
                }
                else
                {
                    SetProperty(ref Betrag, 0.0);
                }
                RaisePropertyChanged(nameof(Betrag));
            }
        }

        public KalteBetriebskostenRechnungJahr(KalteBetriebskosten typ, int jahr)
        {
            Jahr.Value = jahr;
            Typ.Value = typ;
            Betrag = 0.0;
        }

        public KalteBetriebskostenRechnungJahr(KalteBetriebskostenRechnung r)
        {
            Beschreibung = r.Adresse.KalteBetriebskosten.FirstOrDefault(k => k.Typ == r.Typ)?.Beschreibung;
            Jahr.Value = r.Jahr;
            Betrag = r.Betrag;
            Typ.Value = r.Typ;
        }
    }
}
