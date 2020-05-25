using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragDetailViewModel : VertragDetailVersion
    {
        public ObservableProperty<ImmutableList<VertragDetailKontakt>> AlleKontakte
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public ObservableProperty<ImmutableList<JuristischePersonViewModel>> AlleJuristischePersonen
            = new ObservableProperty<ImmutableList<JuristischePersonViewModel>>();
        public ObservableProperty<ImmutableList<VertragDetailWohnung>> AlleWohnungen
           = new ObservableProperty<ImmutableList<VertragDetailWohnung>>();
        public ObservableProperty<ImmutableList<VertragDetailVersion>> Versionen { get; }
            = new ObservableProperty<ImmutableList<VertragDetailVersion>>();
        public ObservableProperty<VertragDetailVersion> AddVersionValue
            = new ObservableProperty<VertragDetailVersion>();
        public ObservableProperty<VertragDetailMiete> AddMieteValue
            = new ObservableProperty<VertragDetailMiete>();

        public Guid guid { get; }

        public ObservableProperty<ImmutableList<VertragDetailKontakt>> Mieter
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public ObservableProperty<ImmutableList<VertragDetailMiete>> Mieten
            = new ObservableProperty<ImmutableList<VertragDetailMiete>>();

        public ObservableProperty<int> BetriebskostenJahr = new ObservableProperty<int>();

        public bool isNew = false;

        public VertragDetailViewModel() : this(new List<Vertrag> { new Vertrag() })
        {
            isNew = true;
            App.Walter.Vertraege
                  .Include(v => v.Ansprechpartner)
                  .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer);
            IsInEdit.Value = true;
        }

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Include(v => v.Ansprechpartner)
                  .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer)
                  .Where(v => v.VertragId == id)
                  .ToList()
                  .OrderBy(v => v.Version)
                  .Reverse()
                  .ToList())
        {
        }

        public VertragDetailViewModel(List<Vertrag> v) : base(v.OrderBy(vs => vs.Version).Last())
        {
            guid = v.First().VertragId;

            AlleKontakte.Value = App.Walter.Kontakte
                .Select(k => new VertragDetailKontakt(k))
                .ToImmutableList();
            AlleJuristischePersonen.Value = App.Walter.JuristischePersonen
                .Select(j => new JuristischePersonViewModel(j))
                .ToImmutableList();
            AlleWohnungen.Value = App.Walter.Wohnungen
                .Select(w => new VertragDetailWohnung(w))
                .ToImmutableList();

            Mieter.Value = App.Walter.MieterSet
               .Where(m => m.VertragId == v.First().VertragId)
               .Include(m => m.Kontakt)
               .Select(m => new VertragDetailKontakt(m.Kontakt))
               .ToList()
               .OrderBy(m => m.Name.Length).Reverse() // From the longest to the smallest because of XAML I guess
               .ToImmutableList();

            Mieten.Value = App.Walter.Mieten
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new VertragDetailMiete(m))
                .ToList()
                .OrderBy(m => m.Datum).Reverse()
                .ToImmutableList();

            BetriebskostenJahr.Value = DateTime.Now.Year - 1;

            Versionen.Value = v.Select(vs => new VertragDetailVersion(vs)).ToImmutableList();
            Beginn.Value = Versionen.Value.Last().Beginn.Value;

            BeginEdit = new RelayCommand(_ => IsInEdit.Value = true, _ => !IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => BeginEdit.RaiseCanExecuteChanged(ev);

            AddVersionValue.Value = Versionen.Value.Count() > 0 ?
                new VertragDetailVersion(Versionen.Value.First()) :
                new VertragDetailVersion(this);
            AddVersion = new RelayCommand(_ =>
            {
                Versionen.Value.First().Ende.Value = AddVersionValue.Value.Beginn.Value.AddDays(-1);
                App.Walter.Vertraege.Update(App.Walter.Vertraege.Find(Versionen.Value.First().Id));
                Versionen.Value = Versionen.Value.Insert(0, AddVersionValue.Value);
                App.Walter.Vertraege.Add(new Vertrag
                {
                    Version = AddVersionValue.Value.Version,
                    Ansprechpartner = App.Walter.Kontakte.Find(AddVersionValue.Value.Ansprechpartner.Value.Id),
                    Personenzahl = AddVersionValue.Value.Personenzahl.Value,
                    Beginn = AddVersionValue.Value.Beginn.Value.UtcDateTime,
                    Ende = AddVersionValue.Value.Ende.Value?.UtcDateTime,
                    Notiz = AddVersionValue.Value.Notiz.Value,
                    // VersionsNotiz
                    WohnungId = AddVersionValue.Value.Wohnung.Value.Id,
                    VertragId = v.First().VertragId,
                });
                AddVersionValue.Value = new VertragDetailVersion(AddVersionValue.Value);
                App.Walter.SaveChanges();
            }, _ => true);

            AddMieteValue.Value = new VertragDetailMiete(v.First().VertragId);
            AddMiete = new RelayCommand(_ =>
            {
                var amv = AddMieteValue.Value;
                Mieten.Value = Mieten.Value
                    .Add(amv)
                    .OrderBy(m => m.Datum)
                    .Reverse()
                    .ToImmutableList();
                AddMieteValue.Value = new VertragDetailMiete(v.First().VertragId);
            }, _ => true);

            RemoveVersion = new RelayCommand(_ =>
            {
                var vs = App.Walter.Vertraege.Find(Versionen.Value.First().Id);
                App.Walter.Vertraege.Remove(vs);
                Versionen.Value = Versionen.Value.Skip(1).ToImmutableList();
                AddVersionValue.Value = new VertragDetailVersion(Versionen.Value.First());
                App.Walter.SaveChanges();
            }, _ => true);

            SaveEdit = new RelayCommand(_ =>
            {
                IsInEdit.Value = false;

            }, _ => IsInEdit.Value);

            IsInEdit.PropertyChanged += (_, ev) => SaveEdit.RaiseCanExecuteChanged(ev);

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        public RelayCommand BeginEdit { get; }
        public RelayCommand AddMiete { get; }
        public RelayCommand AddVersion { get; }
        public RelayCommand RemoveVersion { get; }
        public RelayCommand SaveEdit { get; }
    }

    public class VertragDetailVersion : BindableBase
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<int> Personenzahl { get; } = new ObservableProperty<int>();
        public ObservableProperty<VertragDetailWohnung> Wohnung
            = new ObservableProperty<VertragDetailWohnung>();
        public ObservableProperty<DateTimeOffset> Beginn { get; } = new ObservableProperty<DateTimeOffset>();
        public ObservableProperty<DateTimeOffset?> Ende { get; } = new ObservableProperty<DateTimeOffset?>();
        public ObservableProperty<JuristischePersonViewModel> Vermieter
            = new ObservableProperty<JuristischePersonViewModel>();
        public ObservableProperty<VertragDetailKontakt> Ansprechpartner
            = new ObservableProperty<VertragDetailKontakt>();
        public ObservableProperty<string> Notiz { get; }
            = new ObservableProperty<string>();

        public VertragDetailVersion(VertragDetailVersion v)
        {
            Version = v.Version + 1;
            Personenzahl.Value = v.Personenzahl.Value;
            Wohnung.Value = v.Wohnung.Value;
            Beginn.Value = v.Beginn.Value.AddDays(1);
            Ende.Value = null;
            Vermieter.Value = v.Vermieter.Value;
            Ansprechpartner.Value = v.Ansprechpartner.Value;
            Notiz.Value = "";
        }

        public VertragDetailVersion(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Notiz.Value = v.Notiz;
            Personenzahl.Value = v.Personenzahl;
            var w = v.Wohnung is Wohnung;
            Wohnung.Value = w ? new VertragDetailWohnung(v.Wohnung) : null;
            Vermieter.Value = w ? new JuristischePersonViewModel(v.Wohnung.Besitzer) : null;
            Ansprechpartner.Value = v.Ansprechpartner is Kontakt va ? new VertragDetailKontakt(va) : null;
            Ende.Value = v.Ende;
            Beginn.Value = v.Beginn == DateTime.MinValue ? DateTime.Today : v.Beginn;
        }
    }

    public class VertragDetailMiete : BindableBase
    {
        private Miete Entity { get; }

        public DateTimeOffset Datum
        {
            get => Entity.Datum;
            set
            {
                Entity.Datum = value.UtcDateTime;
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

        public double Kalt 
        { 
            get => Entity.KaltMiete ?? 0; 
            set
            {
                Entity.KaltMiete = value;
                RaisePropertyChangedAuto();
            }
        }

        public double Warm 
        {
            get => Entity.WarmMiete ?? 0;
            set 
            { 
                Entity.WarmMiete = value; 
                RaisePropertyChangedAuto();
            }
        }

        public VertragDetailMiete(Guid vertragId)
            : this(new Miete
            {
                VertragId = vertragId,
                Datum = DateTime.UtcNow,
            })
        {
        }

        public VertragDetailMiete(Miete m)
        {
            Entity = m;

            PropertyChanged += OnUpdate;
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Warm):
                case nameof(Kalt):
                case nameof(Datum):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if (Entity.MieteId != 0)
            {
                App.Walter.Mieten.Update(Entity);
            }
            else
            {
                App.Walter.Mieten.Add(Entity);
            }
            App.Walter.SaveChanges();
        }
    }

    public class VertragDetailKontakt
    {
        public int Id;
        public string Name;

        public VertragDetailKontakt(int id) : this(App.Walter.Kontakte.Find(id)) { }

        public VertragDetailKontakt(Kontakt k)
        {
            Id = k.KontaktId;
            Name = k.Vorname + " " + k.Nachname;
        }

        public static Kontakt GetKontakt(int id) => App.Walter.Kontakte.Find(id);
    }

    public class VertragDetailWohnung
    {
        public int Id;
        public ObservableProperty<int> BesitzerId = new ObservableProperty<int>();
        public ObservableProperty<string> BezeichnungVoll = new ObservableProperty<string>();

        public VertragDetailWohnung(Wohnung w)
        {
            Id = w.WohnungId;
            BesitzerId.Value = w.Besitzer.JuristischePersonId;
            BezeichnungVoll.Value = AdresseViewModel.Anschrift(w) + " - " + w.Bezeichnung;
        }

        public static Wohnung GetWohnung(int id) => App.Walter.Wohnungen.Find(id);
    }
}

