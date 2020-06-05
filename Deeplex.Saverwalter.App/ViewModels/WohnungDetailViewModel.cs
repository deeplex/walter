using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class WohnungDetailViewModel : ValidatableBase
    {
        private Wohnung Entity;
        public int Id;
        public ImmutableList<JuristischePersonViewModel> AlleJuristischePersonen { get; }

        private ImmutableList<Adresse> AlleAdressen => App.Walter.Adressen.ToImmutableList();

        public ObservableProperty<string> Stadt = new ObservableProperty<string>();
        public ObservableProperty<string> Postleitzahl = new ObservableProperty<string>();
        public ObservableProperty<string> Strasse = new ObservableProperty<string>();
        public ObservableProperty<string> Hausnummer = new ObservableProperty<string>();

        public ImmutableList<string> Staedte => AlleAdressen.Select(a => a.Stadt).Distinct().ToImmutableList();
        public ImmutableList<string> Postleitzahlen => AlleAdressen
            .Where(a => Stadt.Value != "" && a.Stadt == Stadt.Value)
            .Select(a => a.Postleitzahl).Distinct().ToImmutableList();
        public ImmutableList<string> Strassen => AlleAdressen
            .Where(a => Postleitzahl.Value != "" && a.Postleitzahl == Entity.Adresse.Postleitzahl)
            .Select(a => a.Strasse).Distinct().ToImmutableList();
        public ImmutableList<string> Hausnummern => AlleAdressen
            .Where(a => Hausnummer.Value != "" && a.Hausnummer == Entity.Adresse.Hausnummer)
            .Select(a => a.Hausnummer).Distinct().ToImmutableList();

        public ObservableProperty<List<WohnungDetailZaehler>> Zaehler
            = new ObservableProperty<List<WohnungDetailZaehler>>();
        public ObservableProperty<List<WohnungDetailVertrag>> Vertraege
            = new ObservableProperty<List<WohnungDetailVertrag>>();

        private JuristischePersonViewModel mBesitzer;
        public JuristischePersonViewModel Besitzer
        {
            get => mBesitzer;
            set
            {
                if (value != null)
                {
                    Entity.BesitzerId = value.Id;
                    mBesitzer = value;
                    RaisePropertyChangedAuto();
                }
            }
        }

        private AdresseViewModel mAdresse;
        public AdresseViewModel Adresse
        {
            get => mAdresse;
            set
            {
                Entity.Adresse = AdresseViewModel.GetAdresse(value);
                mAdresse = value;
                RaisePropertyChangedAuto();
            }
        }
        public string Anschrift => AdresseViewModel.Anschrift(Entity);
        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set
            {
                Entity.Bezeichnung = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Notiz
        {
            get => Entity.Notiz;
            set
            {
                Entity.Notiz = value;
                RaisePropertyChangedAuto();
            }
        }

        public double Wohnflaeche
        {
            get => Entity.Wohnflaeche;
            set
            {
                Entity.Wohnflaeche = value;
                RaisePropertyChangedAuto();
            }
        }
        public double Nutzflaeche
        {
            get => Entity.Nutzflaeche;
            set
            {
                Entity.Nutzflaeche = value;
                RaisePropertyChangedAuto();
            }
        }

        public int Nutzeinheit
        {
            get => Entity.Nutzeinheit;
            set
            {
                Entity.Nutzeinheit = value;
                RaisePropertyChangedAuto();
            }
        }

        public WohnungDetailViewModel(int id)
            : this(App.Walter.Wohnungen
                  .Include(w => w.Adresse)
                  .Include(w => w.Zaehler)
                  .Include(w => w.Besitzer)
                  .First(w => w.WohnungId == id))
        { }

        public WohnungDetailViewModel() : this(new Wohnung())
        {
            IsInEdit.Value = true;
        } // Create new Wohnung

        private WohnungDetailViewModel(Wohnung w)
        {
            Entity = w;

            Stadt.Value = w.Adresse?.Stadt ?? "";
            Postleitzahl.Value = w.Adresse?.Postleitzahl ?? "";
            Strasse.Value = w.Adresse?.Strasse ?? "";
            Hausnummer.Value = w.Adresse?.Strasse ?? "";

            AlleJuristischePersonen = App.Walter.JuristischePersonen
                .Select(j => new JuristischePersonViewModel(j))
                .ToImmutableList();

            if (w.Adresse != null)
            {
                Adresse = new AdresseViewModel(w.Adresse);
            }

            Zaehler.Value = w.Zaehler.Select(z => new WohnungDetailZaehler(z)).ToList();

            Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Wohnung).ToList()
                .Where(v => v.Wohnung.WohnungId == Id)
                .Select(v => new WohnungDetailVertrag(v.VertragId))
                .ToList();

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));

            PropertyChanged += OnUpdate;
        }
        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Besitzer):
                case nameof(Bezeichnung):
                case nameof(Wohnflaeche):
                case nameof(Nutzflaeche):
                case nameof(Nutzeinheit):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if ((Entity.Besitzer == null && Entity.BesitzerId == 0) ||
                (Entity.Adresse == null && Entity.AdresseId == 0) ||
                Entity.Bezeichnung == null || Entity.Bezeichnung == "")
            {
                return;
            }

            if (Entity.WohnungId != 0)
            {
                App.Walter.Wohnungen.Update(Entity);
            }
            else
            {
                App.Walter.Wohnungen.Add(Entity);
            }
            App.Walter.SaveChanges();
        }
    }

    public sealed class WohnungDetailZaehler
    {
        public int Id;
        public ObservableProperty<string> Kennnummer = new ObservableProperty<string>();
        public ObservableProperty<string> Typ = new ObservableProperty<string>();

        public WohnungDetailZaehler(Zaehler z)
        {
            Id = z.ZaehlerId;
            Kennnummer.Value = z.Kennnummer;
            Typ.Value = z.Typ.ToString(); // May be a descript thingy later on?...
        }
    }

    public sealed class WohnungDetailVertrag
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<string> AuflistungMieter { get; } = new ObservableProperty<string>();
        public ObservableProperty<DateTimeOffset> Beginn { get; } = new ObservableProperty<DateTimeOffset>();
        public ObservableProperty<DateTimeOffset?> Ende { get; } = new ObservableProperty<DateTimeOffset?>();
        public ObservableProperty<List<WohnungDetailVertrag>> Versionen { get; }
            = new ObservableProperty<List<WohnungDetailVertrag>>();

        public WohnungDetailVertrag(Guid id)
            : this(App.Walter.Vertraege.Where(v => v.VertragId == id)) { }

        private WohnungDetailVertrag(IEnumerable<Vertrag> v)
            : this(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new WohnungDetailVertrag(vs)).ToList();
            Beginn.Value = Versionen.Value.First().Beginn.Value;
            Ende.Value = Versionen.Value.Last().Ende.Value;
        }

        private WohnungDetailVertrag(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;

            Beginn.Value = v.Beginn.AsUtcKind();
            Ende.Value = v.Ende?.AsUtcKind();

            AuflistungMieter.Value = string.Join(", ", App.Walter.MieterSet
                .Include(m => m.Kontakt)
                .Where(m => m.VertragId == v.VertragId)
                .ToList()
                .Select(m => (m.Kontakt.Vorname is string n ? n + " " : "") + m.Kontakt.Nachname));
        }
    }
}
