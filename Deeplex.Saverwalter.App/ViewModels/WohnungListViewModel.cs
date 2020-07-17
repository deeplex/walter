using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class WohnungListViewModel
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
        }
    }

    public sealed class WohnungListAdresse : BindableBase
    {
        public int Id { get; }
        public Adresse Entity { get; }

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

        public string Strasse
        {
            get => Entity?.Strasse ?? "";
            set => update(nameof(Entity.Strasse), value);
        }

        public string Hausnummer
        {
            get => Entity?.Hausnummer ?? "";
            set => update(nameof(Entity.Hausnummer), value);
        }

        public string Postleitzahl
        {
            get => Entity?.Postleitzahl ?? "";
            set => update(nameof(Entity.Postleitzahl), value);
        }

        public string Stadt
        {
            get => Entity?.Stadt ?? "";
            set => update(nameof(Entity.Stadt), value);
        }

        public string GesamtString
            => "Nutzeinheiten: " + GesamtNutzeinheiten
            + ", Wohnfläche: " + string.Format("{0:N2}", GesamtWohnflaeche)
            + "m², Nutzfläche: " + string.Format("{0:N2}", GesamtNutzflaeche)
            + "m², Bewohnerzahl: " + GesamtBewohner;

        public double GesamtWohnflaeche;
        public int GesamtNutzeinheiten;
        public double GesamtNutzflaeche;
        public int GesamtBewohner;

        public WohnungListAdresse(int id)
        {
            Id = id;
            var Adresse = App.Walter.Adressen.Find(Id);
            Entity = Adresse;
            var wn = Adresse.Wohnungen;
            GesamtNutzeinheiten = wn.Sum(w => w.Nutzeinheit);
            GesamtWohnflaeche = wn.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = wn.Sum(w => w.Nutzflaeche);
            GesamtBewohner = wn
                .Select(w => w.Vertraege.FirstOrDefault(v => v.Ende == null || v.Ende >= DateTime.UtcNow.Date))
                .Sum(v => v?.Personenzahl ?? 0);

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.AdresseAnhaenge, Adresse), _ => true);
        }

        public AsyncRelayCommand AttachFile { get; }
    }

    public sealed class WohnungListWohnung
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
            AdresseId = w.AdresseId;
        }
    }
}
