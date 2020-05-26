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
        public ImmutableList<VertragDetailKontakt> AlleKontakte { get; }
        public ImmutableList<JuristischePersonViewModel> AlleJuristischePersonen { get; }
        public ImmutableList<VertragDetailWohnung> AlleWohnungen { get; }

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

            AlleKontakte = App.Walter.Kontakte
                .Select(k => new VertragDetailKontakt(k))
                .ToImmutableList();
            AlleJuristischePersonen = App.Walter.JuristischePersonen
                .Select(j => new JuristischePersonViewModel(j))
                .ToImmutableList();
            AlleWohnungen = App.Walter.Wohnungen
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
                .OrderBy(m => m.Zahlungsdatum).Reverse()
                .ToImmutableList();

            BetriebskostenJahr.Value = DateTime.Now.Year - 1;

            Versionen.Value = v.Select(vs => new VertragDetailVersion(vs)).ToImmutableList();
            Beginn.Value = Versionen.Value.Last().Beginn.Value;

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
                    .OrderBy(m => m.Zahlungsdatum)
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

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        public RelayCommand AddMiete { get; }
        public RelayCommand AddVersion { get; }
        public RelayCommand RemoveVersion { get; }
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
        public Miete Entity { get; }

        public DateTimeOffset Zahlungsdatum
        {
            get => Entity.Zahlungsdatum;
            set
            {
                Entity.Zahlungsdatum = value.UtcDateTime;
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

        public double Betrag
        {
            get => Entity.Betrag ?? 0;
            set
            {
                Entity.Betrag = value;
                RaisePropertyChangedAuto();
            }
        }

        public VertragDetailMiete(Guid vertragId)
            : this(new Miete
            {
                VertragId = vertragId,
                Zahlungsdatum = DateTime.UtcNow,
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
                case nameof(Betrag):
                case nameof(Zahlungsdatum):
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
        public override string ToString() => BezeichnungVoll;

        public Wohnung Entity { get; }

        public int Id;
        public int BesitzerId { get; }
        public string Besitzer { get; }
        public string BezeichnungVoll { get; }

        public VertragDetailWohnung(Wohnung w)
        {
            Entity = w;
            Id = w.WohnungId;
            Besitzer = w.Besitzer.Bezeichnung;
            BesitzerId = w.Besitzer.JuristischePersonId;
            BezeichnungVoll = AdresseViewModel.Anschrift(w) + " - " + w.Bezeichnung;
        }

        public static Wohnung GetWohnung(int id) => App.Walter.Wohnungen.Find(id);
    }
}

