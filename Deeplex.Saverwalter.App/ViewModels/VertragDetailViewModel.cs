using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragDetailViewModel : VertragDetailVersion
    {
        public ObservableProperty<ImmutableList<VertragDetailKontakt>> Kontakte
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public ObservableProperty<ImmutableList<JuristischePersonViewModel>> JuristischePersonen
            = new ObservableProperty<ImmutableList<JuristischePersonViewModel>>();
        public ObservableProperty<ImmutableList<VertragDetailWohnung>> AlleWohnungen
           = new ObservableProperty<ImmutableList<VertragDetailWohnung>>();
        public ObservableProperty<List<VertragDetailVersion>> Versionen { get; }
            = new ObservableProperty<List<VertragDetailVersion>>();

        public ObservableProperty<VertragDetailMiete> AddMieteValue
            = new ObservableProperty<VertragDetailMiete>();

        public VertragDetailViewModel() : this(new List<Vertrag> { new Vertrag() })
        {
            IsInEdit.Value = true;
        }

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Include(v => v.Ansprechpartner)
                  .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer)
                  .Include(v => v.Garagen)
                  .Where(v => v.VertragId == id).ToList())
        { }

        public VertragDetailViewModel(List<Vertrag> v) : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragDetailVersion(vs)).ToList();

            Beginn.Value = Versionen.Value.First().Beginn.Value;

            Kontakte.Value = App.Walter.Kontakte
                .Select(k => new VertragDetailKontakt(k))
                .ToImmutableList();
            JuristischePersonen.Value = App.Walter.JuristischePersonen
                .Select(j => new JuristischePersonViewModel(j))
                .ToImmutableList();
            AlleWohnungen.Value = App.Walter.Wohnungen
                .Select(w => new VertragDetailWohnung(w))
                .ToImmutableList();

            Mieten.Value = v
                .SelectMany(vs => vs.Mieten)
                .Select(m => new VertragDetailMiete(m))
                .OrderBy(m => m.Datum.Value).Reverse()
                .ToImmutableList();

            BeginEdit = new RelayCommand(_ => IsInEdit.Value = true, _ => !IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => BeginEdit.RaiseCanExecuteChanged(ev);
            
            AddMieteValue.Value = new VertragDetailMiete();
            AddMiete = new RelayCommand(_ =>
            {
                var amv = AddMieteValue.Value;
                amv.VertragsVersion.Value = v.Last().Version;
                Mieten.Value = Mieten.Value
                    .Add(amv)
                    .OrderBy(m => m.Datum.Value)
                    .Reverse()
                    .ToImmutableList(); ;
                AddMieteValue.Value = new VertragDetailMiete();
            }, _ => true);

            SaveEdit = new RelayCommand(_ =>
            {
                IsInEdit.Value = false;

                var MieterSet = App.Walter.MieterSet;
                //Remove deprecated MieterSets
                MieterSet.ToList().Where(ms =>
                        ms.VertragId == Id &&
                        !Mieter.Value.Exists(mv => mv.Id == ms.KontaktId))
                    .ToList().ForEach(d => MieterSet.Remove(d));
                // Add new MieterSets
                v.ForEach(vs => Mieter.Value
                    .Where(mv =>
                        !MieterSet.ToList().Exists(ms =>
                            ms.VertragId == vs.rowid &&
                            ms.KontaktId == mv.Id))
                    .ToList().ForEach(d => MieterSet.Add(new Mieter()
                    {
                        Vertrag = vs,
                        Kontakt = VertragDetailKontakt.GetKontakt(d.Id)
                    })));

                var VertragMiete = App.Walter.Mieten.Where(m => m.Vertrag.rowid == v.Last().rowid).ToList();
                foreach (var miete in VertragMiete)
                {
                    if (!Mieten.Value.Exists(m => m.Id == miete.MieteId))
                    {
                        App.Walter.Mieten.Remove(miete);
                    }
                }
                foreach (var miete in Mieten.Value)
                {
                    var vm = VertragMiete.FirstOrDefault(m => m.MieteId == miete.Id);
                    if (vm != null)
                    {
                        vm.Vertrag = v.First(vs => miete.VertragsVersion.Value == vs.Version);
                        vm.Datum = miete.Datum.Value.UtcDateTime;
                        vm.KaltMiete = miete.Kalt;
                        vm.WarmMiete = miete.Warm;
                        vm.Notiz = miete.Notiz.Value;
                        App.Walter.Mieten.Update(vm);
                    }
                    else
                    {
                        App.Walter.Mieten.Add(new Miete
                        {
                            Datum = miete.Datum.Value.UtcDateTime,
                            KaltMiete = miete.Kalt,
                            WarmMiete = miete.Warm,
                            Notiz = miete.Notiz.Value,
                            Vertrag = v.First(vs => vs.Version == miete.VertragsVersion.Value),
                        });
                    }
                }

                for (int i = 0; i < v.Count(); ++i)
                {
                    var val = Versionen.Value[i];
                    v[i].Beginn = val.Beginn.Value.UtcDateTime;


                    v[i].Ende = val.Ende.Value?.UtcDateTime;

                    if (Wohnung.Value is VertragDetailWohnung &&
                        AlleWohnungen.Value.Exists(w => w.BezeichnungVoll.Value == Wohnung.Value.BezeichnungVoll.Value))
                    {
                        v[i].Wohnung = VertragDetailWohnung.GetWohnung(Wohnung.Value.Id);
                    }
                    else
                    {
                        v[i].Wohnung = null;
                    }

                    v[i].Notiz = val.Notiz.Value;
                    v[i].Personenzahl = val.Personenzahl.Value;
                    v[i].Ansprechpartner = VertragDetailKontakt.GetKontakt(val.Ansprechpartner.Value.Id);

                    if (v[i].rowid > 0)
                    {
                        App.Walter.Vertraege.Update(v[i]);
                    }
                    else
                    {
                        App.Walter.Vertraege.Add(v[i]);
                    }
                }

                App.Walter.SaveChanges();

            }, _ => IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => SaveEdit.RaiseCanExecuteChanged(ev);

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        public RelayCommand BeginEdit { get; }
        public RelayCommand AddMiete { get; }
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
        public ObservableProperty<ImmutableList<VertragDetailKontakt>> Mieter
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public ObservableProperty<ImmutableList<KalteBetriebskostenViewModel>> KalteBetriebskosten { get; }
            = new ObservableProperty<ImmutableList<KalteBetriebskostenViewModel>>();
        public ObservableProperty<ImmutableList<VertragDetailMiete>> Mieten
            = new ObservableProperty<ImmutableList<VertragDetailMiete>>();
        public ObservableProperty<string> Notiz { get; }
            = new ObservableProperty<string>();

        public VertragDetailVersion(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Notiz.Value = v.Notiz;
            Personenzahl.Value = v.Personenzahl;
            Wohnung.Value = v.Wohnung is Wohnung w ? new VertragDetailWohnung(w) : null;
            Mieter.Value = v.Mieter.Select(m => new VertragDetailKontakt(m.Kontakt))
                .OrderBy(m => m.Name.Value.Length).Reverse() // From the longest to the smallest because of XAML I guess
                .ToImmutableList();
            Vermieter.Value = v.Wohnung is Wohnung wv || v.Garagen.Count() > 0 ?
                new JuristischePersonViewModel(
                    v.Wohnung is Wohnung vw ?
                    vw.Besitzer :
                    v.Garagen.First().Garage.Besitzer) :
                null;
            Ansprechpartner.Value = v.Ansprechpartner is Kontakt va ? new VertragDetailKontakt(va) : null;
            Ende.Value = v.Ende;
            Beginn.Value = v.Beginn == DateTime.MinValue ? DateTime.Today : v.Beginn;

            KalteBetriebskosten.Value = v.Wohnung is Wohnung w2 ? App.Walter.KalteBetriebskosten
                .Where(k => k.Adresse == w2.Adresse)
                .Select(k => new KalteBetriebskostenViewModel(k, v))
                .ToImmutableList() : null;
            Mieten.Value = v.Mieten.Select(vm => new VertragDetailMiete(vm)).ToImmutableList();
        }
    }

    public class VertragDetailMiete : BindableBase
    {
        public int Id;
        public ObservableProperty<int> VertragsVersion
            = new ObservableProperty<int>();
        public ObservableProperty<DateTimeOffset> Datum = new ObservableProperty<DateTimeOffset>();
        public ObservableProperty<string> Notiz { get; } = new ObservableProperty<string>();
        public double Kalt;
        public string KaltString
        {
            get => Kalt > 0 ? string.Format("{0:F2}", Kalt) : "";
            set
            {
                SetProperty(ref Kalt, double.TryParse(value, out double result) ? result : 0);
                RaisePropertyChanged(nameof(Kalt));
            }
        }
        public double Warm;
        public string WarmString
        {
            get => Warm > 0 ? string.Format("{0:F2}", Warm) : "";
            set
            {
                SetProperty(ref Warm, double.TryParse(value, out double result) ? result : 0);
                RaisePropertyChanged(nameof(Warm));
            }
        }

        public VertragDetailMiete()
        {
            Datum.Value = DateTime.UtcNow;
            Kalt = 0;
            Warm = 0;
            Notiz.Value = "";
        }

        public VertragDetailMiete(Miete m)
        {
            Id = m.MieteId;
            VertragsVersion.Value = m.Vertrag.Version;
            Datum.Value = m.Datum;
            Kalt = m.KaltMiete ?? 0;
            Warm = m.WarmMiete ?? 0;
            Notiz.Value = m.Notiz ?? "";
        }
    }

    public class VertragDetailKontakt
    {
        public int Id;
        public ObservableProperty<string> Name = new ObservableProperty<string>();

        public VertragDetailKontakt(int id) : this(App.Walter.Kontakte.Find(id)) { }

        public VertragDetailKontakt(Kontakt k)
        {
            Id = k.KontaktId;
            Name.Value = k.Vorname + " " + k.Nachname;
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

