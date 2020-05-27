using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class WohnungDetailViewModel : ValidatableBase
    {
        public int Id;

        public ImmutableList<JuristischePersonViewModel> AlleJuristischePersonen { get; }
        public ImmutableList<AdresseViewModel> AlleAdressen { get; }

        public ObservableProperty<JuristischePersonViewModel> Besitzer
            = new ObservableProperty<JuristischePersonViewModel>();
        public ObservableProperty<int> AdresseId = new ObservableProperty<int>();
        public ObservableProperty<List<WohnungDetailZaehler>> Zaehler
            = new ObservableProperty<List<WohnungDetailZaehler>>();
        public ObservableProperty<List<WohnungDetailVertrag>> Vertraege
            = new ObservableProperty<List<WohnungDetailVertrag>>();

        public AdresseViewModel Adresse { get; }
        public string Anschrift => AdresseViewModel.Anschrift(App.Walter.Wohnungen.Find(Id));
        private string mBezeichnung;
        public string Bezeichnung
        {
            get => mBezeichnung;
            set
            {
                SetProperty(ref mBezeichnung, value);
                var w = App.Walter.Wohnungen.Find(Id);
                w.Bezeichnung = value;
                App.Walter.SaveChanges();
            }
        }

        private string mNotiz;
        public string Notiz
        {
            get => mNotiz;
            set
            {
                SetProperty(ref mNotiz, value);
                var w = App.Walter.Wohnungen.Find(Id);
                w.Notiz = value;
                App.Walter.SaveChanges();
            }
        }

        private double mWohnflaeche;
        public double Wohnflaeche
        {
            get => mWohnflaeche;
            set
            {
                SetProperty(ref mWohnflaeche, value);
                var w = App.Walter.Wohnungen.Find(Id);
                w.Wohnflaeche = value;
                App.Walter.SaveChanges();
                if (mWohnflaeche <= mNutzflaeche)
                {
                    ClearErrors(nameof(Nutzflaeche));
                }
                else
                {
                    AddError(nameof(Nutzflaeche), "Wohnfläche muss kleiner als Nutzfläche");
                }
            }
        }
        private double mNutzflaeche;
        public double Nutzflaeche
        {
            get => mNutzflaeche;
            set
            {
                SetProperty(ref mNutzflaeche, value);
                var w = App.Walter.Wohnungen.Find(Id);
                w.Nutzflaeche = value;
                App.Walter.SaveChanges();
                if (mWohnflaeche <= mNutzflaeche)
                {
                    ClearErrors(nameof(Nutzflaeche));
                }
                else
                {
                    AddErrorAuto("Wohnfläche muss kleiner als Nutzfläche");
                }
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
            AlleJuristischePersonen = App.Walter.JuristischePersonen
                .Select(j => new JuristischePersonViewModel(j))
                .ToImmutableList();
            AlleAdressen = App.Walter.Adressen
                .Select(a => new AdresseViewModel(a))
                .ToImmutableList();

            Id = w.WohnungId;
            AdresseId.Value = w.AdresseId;
            Bezeichnung = w.Bezeichnung;
            mWohnflaeche = w.Wohnflaeche;
            Nutzflaeche = w.Nutzflaeche;
            Notiz = w.Notiz;

            Adresse = w.Adresse is Adresse ?
                new AdresseViewModel(w.Adresse) :
                new AdresseViewModel();

            Besitzer.Value = w.Besitzer is JuristischePerson ?
                new JuristischePersonViewModel(w.Besitzer) :
                new JuristischePersonViewModel();

            Zaehler.Value = w.Zaehler.Select(z => new WohnungDetailZaehler(z)).ToList();

            BeginEdit = new RelayCommand(_ => IsInEdit.Value = true, _ => !IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => BeginEdit.RaiseCanExecuteChanged(ev);

            Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Wohnung).ToList()
                .Where(v => v.Wohnung.WohnungId == Id)
                .Select(v => new WohnungDetailVertrag(v.VertragId))
                .ToList();

            SaveEdit = new RelayCommand(_ =>
            {
                IsInEdit.Value = false;

                w.Adresse = AdresseViewModel.GetAdresse(Adresse);
                w.Besitzer = JuristischePersonViewModel.GetJuristischePerson(Besitzer.Value);
                // w.Zaehler = ZaehlerViewModel // TODO
                // w.Zaehlergemeinschaften = Zaehlergemeinschaften TODO
                w.Bezeichnung = Bezeichnung;
                w.Notiz = Notiz;

                if (w.WohnungId > 0)
                {
                    App.Walter.Wohnungen.Update(w);
                }
                else
                {
                    App.Walter.Wohnungen.Add(w);
                }

                App.Walter.SaveChanges();
            }, _ => IsInEdit.Value);


            IsInEdit.PropertyChanged += (_, ev) => SaveEdit.RaiseCanExecuteChanged(ev);

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));
        }
        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        public RelayCommand BeginEdit { get; }
        public RelayCommand SaveEdit { get; }
    }

    public class WohnungDetailZaehler
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

    public class WohnungDetailVertrag
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

            Beginn.Value = v.Beginn;
            Ende.Value = v.Ende;

            AuflistungMieter.Value = string.Join(", ", App.Walter.MieterSet
                .Include(m => m.Kontakt)
                .Where(m => m.VertragId == v.VertragId)
                .ToList()
                .Select(m => (m.Kontakt.Vorname is string n ? n + " " : "") + m.Kontakt.Nachname));
        }
    }
}
