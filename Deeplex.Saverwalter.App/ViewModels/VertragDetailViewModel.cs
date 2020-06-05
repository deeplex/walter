using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragDetailViewModel : VertragDetailVersion
    {
        public Guid guid { get; }
        public ImmutableList<VertragDetailKontakt> AlleKontakte { get; }
        public ImmutableList<JuristischePersonViewModel> AlleJuristischePersonen { get; }
        public ImmutableList<VertragDetailWohnung> AlleWohnungen { get; }

        public ObservableProperty<ImmutableList<VertragDetailVersion>> Versionen
            = new ObservableProperty<ImmutableList<VertragDetailVersion>>();
        public ObservableProperty<VertragDetailMiete> AddMieteValue
            = new ObservableProperty<VertragDetailMiete>();
        public ObservableProperty<VertragDetailMietMinderung> AddMietMinderungValue
            = new ObservableProperty<VertragDetailMietMinderung>();
        public ObservableProperty<ImmutableList<VertragDetailKontakt>> Mieter
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public ObservableProperty<ImmutableList<VertragDetailMiete>> Mieten
            = new ObservableProperty<ImmutableList<VertragDetailMiete>>();
        public ObservableProperty<ImmutableList<VertragDetailMietMinderung>> MietMinderungen
            = new ObservableProperty<ImmutableList<VertragDetailMietMinderung>>();
        public DateTimeOffset? AddVersionDatum;
        public ObservableProperty<int> BetriebskostenJahr
            = new ObservableProperty<int>();

        // TODO Define setter
        public DateTimeOffset lastBeginn
        {
            get => Versionen.Value.Last().Beginn;
        }
        public DateTimeOffset? firstEnde
        {
            get => Versionen.Value.First().Ende;
        }

        public VertragDetailViewModel() : this(
            new List<Vertrag>
            {
                new Vertrag
                {
                    Beginn = DateTime.UtcNow.Date,
                }
            })
        {
            IsInEdit.Value = true;
        }

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Where(v => v.VertragId == id)
                  .ToList()
                  .OrderBy(v => v.Version)
                  .Reverse()
                  .ToList())
        { }

        public VertragDetailViewModel(List<Vertrag> v) : base(v.OrderBy(vs => vs.Version).Last())
        {
            guid = v.First().VertragId;

            AlleKontakte = App.Walter.Kontakte.Select(k => new VertragDetailKontakt(k)).ToImmutableList();
            AlleWohnungen = App.Walter.Wohnungen.Include(w => w.Besitzer).Select(w => new VertragDetailWohnung(w)).ToImmutableList();
            AlleJuristischePersonen = App.Walter.JuristischePersonen.Select(j => new JuristischePersonViewModel(j)).ToImmutableList();

            Mieter.Value = App.Walter.MieterSet
                .Where(m => m.VertragId == v.First().VertragId).Include(m => m.Kontakt)
                .Select(m => new VertragDetailKontakt(m.Kontakt)).ToList()
                .OrderBy(m => m.Name.Length).Reverse().ToImmutableList();

            Mieten.Value = App.Walter.Mieten
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new VertragDetailMiete(m)).ToList()
                .OrderBy(m => m.Zahlungsdatum).Reverse().ToImmutableList();

            MietMinderungen.Value = App.Walter.MietMinderungen
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new VertragDetailMietMinderung(m)).ToList()
                .OrderBy(m => m.Beginn).Reverse().ToImmutableList();

            BetriebskostenJahr.Value = DateTime.Now.Year - 1;

            Versionen.Value = v.Select(vs => new VertragDetailVersion(vs)).ToImmutableList();

            AddVersion = new RelayCommand(_ =>
            {
                var last = App.Walter.Vertraege.Find(Versionen.Value.First().Id);
                var entity = new Vertrag(last, AddVersionDatum?.UtcDateTime ?? DateTime.UtcNow.Date)
                {
                    Personenzahl = Personenzahl,
                    //KaltMiete = KaltMiete, TODO
                };
                var nv = new VertragDetailVersion(entity);
                Versionen.Value = Versionen.Value.Insert(0, nv);
                App.Walter.Add(entity);
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

            AddMietMinderungValue.Value = new VertragDetailMietMinderung(v.First().VertragId);
            AddMietMinderung = new RelayCommand(_ =>
            {
                var amv = AddMietMinderungValue.Value;
                MietMinderungen.Value = MietMinderungen.Value
                    .Add(amv)
                    .OrderBy(m => m.Beginn)
                    .Reverse()
                    .ToImmutableList();
                AddMietMinderungValue.Value = new VertragDetailMietMinderung(v.First().VertragId);
            }, _ => true);

            RemoveVersion = new RelayCommand(_ =>
            {
                var vs = App.Walter.Vertraege.Find(Versionen.Value.First().Id);
                App.Walter.Vertraege.Remove(vs);
                Versionen.Value = Versionen.Value.Skip(1).ToImmutableList();
                App.Walter.SaveChanges();
            }, _ => true);

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        public RelayCommand AddMiete { get; }
        public RelayCommand AddMietMinderung { get; }
        public RelayCommand AddVersion { get; }
        public RelayCommand RemoveVersion { get; }
    }

    public class VertragDetailVersion : BindableBase
    {
        private Vertrag Entity { get; }
        public int Id => Entity.rowid;
        public int Version => Entity.Version;
        public double KaltMiete
        {
            get => Entity.KaltMiete;
            set
            {
                Entity.KaltMiete = value;
                RaisePropertyChangedAuto();
            }
        }

        public int Personenzahl
        {
            get => Entity.Personenzahl;
            set
            {
                Entity.Personenzahl = value;
                RaisePropertyChangedAuto();
            }
        }

        private VertragDetailWohnung mWohnung;
        public VertragDetailWohnung Wohnung
        {
            get => mWohnung;
            set
            {
                if (value == null) return;
                mWohnung = value;
                Entity.WohnungId = mWohnung.Id;
                RaisePropertyChanged(nameof(Vermieter));
                RaisePropertyChangedAuto();
            }
        }
        public DateTimeOffset Beginn
        {
            get => Entity.Beginn.AsUtcKind();
            set
            {
                Entity.Beginn = value.UtcDateTime;
                RaisePropertyChangedAuto();
            }
        }
        public DateTimeOffset? Ende
        {
            get => Entity.Ende?.AsUtcKind();
            set
            {
                Entity.Ende = value?.UtcDateTime;
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
        public string Vermieter => Wohnung.Besitzer;
        private VertragDetailKontakt mAnsprechpartner;
        public VertragDetailKontakt Ansprechpartner
        {
            get => mAnsprechpartner;
            set
            {
                mAnsprechpartner = value;
                Entity.AnsprechpartnerId = mAnsprechpartner.Id;
                RaisePropertyChangedAuto();
            }
        }

        public VertragDetailVersion(int id) : this(App.Walter.Vertraege.Find(id)) { }
        public VertragDetailVersion(Vertrag v)
        {
            Entity = v;

            if (v.Wohnung != null)
            {
                Wohnung = new VertragDetailWohnung(v.Wohnung);
            }
            if (v.Ansprechpartner != null)
            {
                Ansprechpartner = new VertragDetailKontakt(v.Ansprechpartner);
            }
            PropertyChanged += OnUpdate;
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Wohnung):
                case nameof(Beginn):
                case nameof(Ende):
                case nameof(Notiz):
                case nameof(Personenzahl):
                case nameof(KaltMiete):
                // case nameof(VersionsNotiz):
                case nameof(Ansprechpartner):
                    break;
                default:
                    return;
            }

            if (Entity.VertragId == null ||
                Entity.Beginn == null ||
                (Entity.Wohnung == null && Entity.WohnungId == 0) ||
                (Entity.Ansprechpartner == null && (Entity.AnsprechpartnerId == 0 || Entity.AnsprechpartnerId == null)))
            {
                return;
            }

            if (Entity.rowid != 0)
            {
                App.Walter.Vertraege.Update(Entity);
            }
            else
            {
                App.Walter.Vertraege.Add(Entity);
            }
            App.Walter.SaveChanges();
        }
    }

    public class VertragDetailMiete : BindableBase
    {
        public void selfDestruct()
        {
            App.Walter.Remove(Entity);
            App.Walter.SaveChanges();
        }
        private Miete Entity { get; }

        public DateTimeOffset Zahlungsdatum
        {
            get => Entity.Zahlungsdatum.AsUtcKind();
            set
            {
                Entity.Zahlungsdatum = value.UtcDateTime;
                RaisePropertyChangedAuto();
            }
        }

        public DateTimeOffset BetreffenderMonat
        {
            get => Entity.BetreffenderMonat.AsUtcKind();
            set
            {
                Entity.BetreffenderMonat = new DateTime(value.Year, value.Month, 1).AsUtcKind();
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
                Zahlungsdatum = DateTime.UtcNow.Date,
            })
        {
        }

        public VertragDetailMiete(Miete m)
        {
            Entity = m;
            PropertyChanged += OnUpdate;
        }

        private bool savable =>
            Entity.Betrag != null &&
            Entity.BetreffenderMonat != null &&
            Entity.Zahlungsdatum != null &&
            Entity.VertragId != null;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Betrag):
                case nameof(Zahlungsdatum):
                case nameof(Notiz):
                case nameof(BetreffenderMonat):
                    break;
                default:
                    return;
            }

            if (!savable) return;

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

    public class VertragDetailMietMinderung : BindableBase
    {
        private MietMinderung Entity { get; }
        public void selfDestruct()
        {
            App.Walter.Remove(Entity);
            App.Walter.SaveChanges();
        }
        public DateTimeOffset Beginn
        {
            get => Entity.Beginn;
            set
            {
                Entity.Beginn = value.UtcDateTime.AsUtcKind();
                RaisePropertyChangedAuto();
            }
        }

        public DateTimeOffset? Ende
        {
            get => Entity.Ende;
            set
            {
                Entity.Ende = value?.UtcDateTime.AsUtcKind();
                RaisePropertyChangedAuto();
            }
        }

        public double Minderung
        {
            get => Entity.Minderung;
            set
            {
                Entity.Minderung = value;
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

        public VertragDetailMietMinderung(Guid vertragId)
            : this(new MietMinderung
            {
                VertragId = vertragId,
                Beginn = DateTime.UtcNow.Date,
            })
        {
        }


        public VertragDetailMietMinderung(MietMinderung m)
        {
            Entity = m;
            PropertyChanged += OnUpdate;
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Beginn):
                case nameof(Ende):
                case nameof(Notiz):
                case nameof(Minderung):
                    break;
                default:
                    return;
            }

            if (Beginn == null || Minderung == 0) return;

            if (Entity.MietMinderungId != 0)
            {
                App.Walter.MietMinderungen.Update(Entity);
            }
            else
            {
                App.Walter.MietMinderungen.Add(Entity);
            }
            App.Walter.SaveChanges();
        }
    }

    public class VertragDetailKontakt
    {
        public override string ToString() => Name;

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

        public int Id { get; }
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

