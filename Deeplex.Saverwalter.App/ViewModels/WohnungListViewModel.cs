using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class WohnungListViewModel
    {
        public ImmutableDictionary<WohnungListAdresse, ImmutableList<WohnungListWohnung>> AdresseGroup;

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>();

        public WohnungListViewModel()
        {
            AdresseGroup = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .Include(w => w.Vertraege)
                .ToList()
                .Select(w => new WohnungListWohnung(w))
                .GroupBy(w => w.AdresseId)
                .ToImmutableDictionary(g => new WohnungListAdresse(g.Key), g => g.ToImmutableList());

            SaveEdit = new RelayCommand(_ =>
            {
                foreach (var address in AdresseGroup)
                {
                    IsInEdit.Value = false;
                    var a = App.Walter.Adressen.Find(address.Key.Id);
                    a.Postleitzahl = address.Key.Postleitzahl.Value;
                    a.Hausnummer = address.Key.Hausnummer.Value;
                    a.Strasse = address.Key.Strasse.Value;
                    a.Stadt = address.Key.Stadt.Value;
                    App.Walter.Adressen.Update(a);
                    App.Walter.SaveChanges();
                }
            });
        }

        public RelayCommand SaveEdit { get; }
    }

    public class WohnungListAdresse
    {
        public int Id { get; }
        public ObservableProperty<string> Postleitzahl { get; }
            = new ObservableProperty<string>();
        public ObservableProperty<string> Hausnummer { get; }
            = new ObservableProperty<string>();
        public ObservableProperty<string> Strasse { get; }
            = new ObservableProperty<string>();
        public ObservableProperty<string> Stadt { get; }
            = new ObservableProperty<string>();

        public string GesamtString 
            => "Nutzeinheiten: " + GesamtNutzeinheiten
            + ", Wohnfläche: " + string.Format("{0:N2}", GesamtWohnflaeche)
            + "m², Nutzfläche: " + string.Format("{0:N2}", GesamtNutzflaeche)
            + "m², Bewohnerzahl: " + GesamtBewohner;

        private double GesamtWohnflaeche;
        private int GesamtNutzeinheiten;
        private double GesamtNutzflaeche;
        private int GesamtBewohner;

        public WohnungListAdresse(int id)
        {
            Id = id;
            var Adresse = App.Walter.Adressen.Find(Id);
            var wn = Adresse.Wohnungen;
            GesamtNutzeinheiten = wn.Sum(w => w.Nutzeinheit);
            GesamtWohnflaeche = wn.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = wn.Sum(w => w.Nutzflaeche);
            GesamtBewohner = wn
                .Select(w => w.Vertraege.FirstOrDefault(v => v.Ende == null || v.Ende >= DateTime.Today))
                .Sum(v => v?.Personenzahl ?? 0);

            Postleitzahl.Value = Adresse.Postleitzahl;
            Hausnummer.Value = Adresse.Hausnummer;
            Strasse.Value = Adresse.Strasse;
            Stadt.Value = Adresse.Stadt;
        }
    }

    public class WohnungListWohnung
    {
        public int Id { get; }
        public int AdresseId { get; }
        public string Bezeichnung { get; }
        public ObservableProperty<string> Anschrift
            = new ObservableProperty<string>();

        public WohnungListWohnung(Wohnung w)
        {
            Id = w.WohnungId;
            Bezeichnung = w.Bezeichnung;
            Anschrift.Value = AdresseViewModel.Anschrift(w);
            AdresseId = AdresseViewModel.GetAdresseIdByAnschrift(Anschrift.Value);
        }
    }
}
