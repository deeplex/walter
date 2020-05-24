using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class WohnungDetailViewModel : BindableBase
    {
        public int Id;
        public ObservableProperty<JuristischePersonViewModel> Besitzer
            = new ObservableProperty<JuristischePersonViewModel>();
        public ObservableProperty<int> AdresseId = new ObservableProperty<int>();
        public ObservableProperty<string> Bezeichnung = new ObservableProperty<string>();
        public ObservableProperty<string> Anschrift = new ObservableProperty<string>();
        public ObservableProperty<double> Wohnflaeche = new ObservableProperty<double>();
        public ObservableProperty<double> Nutzflaeche = new ObservableProperty<double>();
        public ObservableProperty<List<WohnungDetailZaehler>> Zaehler
            = new ObservableProperty<List<WohnungDetailZaehler>>();
        public ObservableProperty<List<WohnungDetailVertrag>> Vertraege
            = new ObservableProperty<List<WohnungDetailVertrag>>();
        public ObservableProperty<AdresseViewModel> Adresse
            = new ObservableProperty<AdresseViewModel>();
        public ObservableProperty<string> Notiz
            = new ObservableProperty<string>();

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
            Id = w.WohnungId;
            AdresseId.Value = w.AdresseId;
            Anschrift.Value = AdresseViewModel.Anschrift(w);
            Bezeichnung.Value = w.Bezeichnung;
            Wohnflaeche.Value = w.Wohnflaeche;
            Nutzflaeche.Value = w.Nutzflaeche;
            Notiz.Value = w.Notiz;

            Adresse.Value = w.Adresse is Adresse ?
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

                w.Adresse = AdresseViewModel.GetAdresse(Adresse.Value);
                w.Besitzer = JuristischePersonViewModel.GetJuristischePerson(Besitzer.Value);
                // w.Zaehler = ZaehlerViewModel // TODO
                // w.Zaehlergemeinschaften = Zaehlergemeinschaften TODO
                w.Bezeichnung = Bezeichnung.Value;
                w.Nutzflaeche = Nutzflaeche.Value;
                w.Wohnflaeche = Wohnflaeche.Value;
                w.Notiz = Notiz.Value;

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
