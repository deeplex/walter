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
    public sealed class VertragDetailViewModel : VertragDetailVersion
    {
        public Guid guid { get; }
        public ObservableProperty<ImmutableList<VertragDetailKontakt>> AlleMieter
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public ImmutableList<VertragDetailWohnung> AlleWohnungen
            => App.Walter.Wohnungen.Select(w => new VertragDetailWohnung(w)).ToImmutableList();

        public ImmutableList<VertragDetailKontakt> AlleKontakte =>
                App.Walter.JuristischePersonen.ToImmutableList().Select(j => new VertragDetailKontakt(j))
                    .Concat(App.Walter.NatuerlichePersonen.Select(n => new VertragDetailKontakt(n)))
                    .ToImmutableList();

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

        public VertragDetailViewModel() : this(new List<Vertrag>
            { new Vertrag { Beginn = DateTime.UtcNow.Date, } })
        { }

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Where(v => v.VertragId == id)
                  .Include(v => v.Wohnung)
                  .ToList()
                  .OrderBy(v => v.Version)
                  .Reverse()
                  .ToList())
        { }

        public VertragDetailViewModel(List<Vertrag> v) : base(v.OrderBy(vs => vs.Version).Last())
        {
            guid = v.First().VertragId;

            Mieter.Value = App.Walter.MieterSet
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new VertragDetailKontakt(m.PersonId))
                .ToImmutableList();

            AlleMieter.Value = App.Walter.JuristischePersonen.ToImmutableList()
                    .Where(j => j.isMieter == true).Select(j => new VertragDetailKontakt(j))
                    .Concat(App.Walter.NatuerlichePersonen
                        .Where(n => n.isMieter == true).Select(n => new VertragDetailKontakt(n)))
                    .Where(p => !Mieter.Value.Exists(e => p.PersonId == e.PersonId))
                    .ToImmutableList();

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
                App.SaveWalter();
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
                App.SaveWalter();
            }, _ => true);


            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.VertragAnhaenge, guid), _ => true);
        }

        public AsyncRelayCommand AttachFile { get; }
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
                RaisePropertyChangedAuto();
                RaisePropertyChanged(nameof(Vermieter));
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
                Entity.AnsprechpartnerId = mAnsprechpartner.PersonId;
                RaisePropertyChangedAuto();
            }
        }

        public VertragDetailVersion(int id) : this(App.Walter.Vertraege.Find(id)) { }
        public VertragDetailVersion(Vertrag v)
        {
            Entity = v;

            if (v.AnsprechpartnerId != Guid.Empty && v.AnsprechpartnerId != null)
            {
                Ansprechpartner = new VertragDetailKontakt(v.AnsprechpartnerId.Value);
            }

            if (v.Wohnung != null)
            {
                Wohnung = new VertragDetailWohnung(v.Wohnung);
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
                Entity.AnsprechpartnerId == null)
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
            App.SaveWalter();
        }
    }

    public sealed class VertragDetailMiete : BindableBase
    {
        public void selfDestruct()
        {
            App.Walter.Remove(Entity);
            App.SaveWalter();
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
            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.MieteAnhaenge, m), _ => true);
        }

        public AsyncRelayCommand AttachFile;

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
            App.SaveWalter();
        }
    }

    public sealed class VertragDetailMietMinderung : BindableBase
    {
        private MietMinderung Entity { get; }
        public void selfDestruct()
        {
            App.Walter.Remove(Entity);
            App.SaveWalter();
        }

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

        public DateTimeOffset Beginn
        {
            get => Entity.Beginn;
            set => update(nameof(Entity.Beginn), value.UtcDateTime.AsUtcKind());

        }

        public DateTimeOffset? Ende
        {
            get => Entity.Ende;
            set => update(nameof(Entity.Ende), value?.UtcDateTime.AsUtcKind());
        }

        public double Minderung
        {
            get => Entity.Minderung;
            set => update(nameof(Entity.Minderung), value);

        }

        public string Notiz
        {
            get => Entity.Notiz;
            set => update(nameof(Entity.Notiz), value);
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
            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.MietMinderungAnhaenge, m), _ => true);
            PropertyChanged += OnUpdate;
        }

        public AsyncRelayCommand AttachFile;

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
            App.SaveWalter();
        }
    }

    public sealed class VertragDetailKontakt
    {
        public override string ToString() => Bezeichnung;

        public Guid PersonId;
        public bool isMieter;
        public string Bezeichnung;

        public VertragDetailKontakt(Guid i) : this(App.Walter.FindPerson(i)) { }

        public VertragDetailKontakt(IPerson p)
        {
            PersonId = p.PersonId;
            isMieter = p.isMieter;
            Bezeichnung = p.Bezeichnung;
        }
    }

    public sealed class VertragDetailWohnung
    {
        public override string ToString() => BezeichnungVoll;
        public Wohnung Entity { get; }

        public int Id { get; }
        public string Besitzer { get; }
        public string BezeichnungVoll { get; }

        public VertragDetailWohnung(Wohnung w)
        {
            Entity = w;
            Id = w.WohnungId;
            Besitzer = App.Walter.FindPerson(w.BesitzerId)?.Bezeichnung;
            BezeichnungVoll = AdresseViewModel.Anschrift(w) + ", " + w.Bezeichnung;
        }

        public static Wohnung GetWohnung(int id) => App.Walter.Wohnungen.Find(id);
    }
}

