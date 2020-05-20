using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public ObservableProperty<ImmutableList<VertragDetailKontakt>> Mieter
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();
        public ObservableProperty<ImmutableList<KalteBetriebskostenViewModel>> KalteBetriebskosten { get; }
            = new ObservableProperty<ImmutableList<KalteBetriebskostenViewModel>>();
        public ObservableProperty<ImmutableList<VertragDetailMiete>> Mieten
            = new ObservableProperty<ImmutableList<VertragDetailMiete>>();

        public ObservableProperty<int> BetriebskostenJahr = new ObservableProperty<int>();

        public VertragDetailViewModel() : this(new List<Vertrag> { new Vertrag() })
        {
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
               .Where(m => m.VertragId == id)
               .Include(m => m.Kontakt)
               .Select(m => new VertragDetailKontakt(m.Kontakt))
               .ToList()
               .OrderBy(m => m.Name.Value.Length).Reverse() // From the longest to the smallest because of XAML I guess
               .ToImmutableList();

            Mieten.Value = App.Walter.Mieten
                .Where(m => m.VertragId == id)
                .Select(m => new VertragDetailMiete(m))
                .ToList()
                .OrderBy(m => m.Datum.Value).Reverse()
                .ToImmutableList();
        }

        public VertragDetailViewModel(List<Vertrag> v) : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.Select(vs => new VertragDetailVersion(vs)).ToImmutableList();
            Beginn.Value = Versionen.Value.Last().Beginn.Value;

            KalteBetriebskosten.Value = v.First().Wohnung is Wohnung w2 ? App.Walter.KalteBetriebskosten
                .Where(k => k.Adresse == w2.Adresse)
                .Select(k => new KalteBetriebskostenViewModel(k, v.First()))
                .ToImmutableList() : null;

            BeginEdit = new RelayCommand(_ => IsInEdit.Value = true, _ => !IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => BeginEdit.RaiseCanExecuteChanged(ev);

            AddVersionValue.Value = Versionen.Value.Count() > 0 ?
                new VertragDetailVersion(Versionen.Value.First()) :
                new VertragDetailVersion(this);
            AddVersion = new RelayCommand(_ =>
            {
                Versionen.Value.First().Ende.Value = AddVersionValue.Value.Beginn.Value.AddDays(-1);
                Versionen.Value = Versionen.Value
                    .Insert(0, AddVersionValue.Value);
                AddVersionValue.Value = new VertragDetailVersion(AddVersionValue.Value);
            }, _ => true);

            AddMieteValue.Value = new VertragDetailMiete();
            AddMiete = new RelayCommand(_ =>
            {
                var amv = AddMieteValue.Value;
                amv.VertragsVersion.Value = v.Last().Version;
                Mieten.Value = Mieten.Value
                    .Add(amv)
                    .OrderBy(m => m.Datum.Value)
                    .Reverse()
                    .ToImmutableList();
                AddMieteValue.Value = new VertragDetailMiete();
            }, _ => true);

            RemoveVersion = new RelayCommand(_ =>
            {
                Versionen.Value = Versionen.Value.Skip(1).ToImmutableList();
                AddVersionValue.Value = new VertragDetailVersion(Versionen.Value.First());
            }, _ => true);

            SaveEdit = new RelayCommand(_ =>
            {
                IsInEdit.Value = false;

                Guid guid = v.First().VertragId;

                // Update Mieten
                var mieten = App.Walter.Mieten.Where(m => m.VertragId == guid).ToList();
                for (var i = 0; i < mieten.Count(); ++i)
                {
                    var m = mieten[i];
                    var mv = Mieten.Value.FirstOrDefault(mm => mm.Id == m.MieteId);
                    if (mv != null)
                    {
                        m.KaltMiete = mv.Kalt;
                        m.WarmMiete = mv.Warm;
                        m.Datum = mv.Datum.Value.UtcDateTime;
                        m.Notiz = mv.Notiz.Value;
                        App.Walter.Mieten.Update(m);
                    }
                    else
                    {
                        App.Walter.Mieten.Remove(m);
                    }
                }
                foreach (var m in Mieten.Value)
                {
                    if (!mieten.Exists(mm => mm.MieteId == m.Id))
                    {
                        App.Walter.Mieten.Add(new Miete
                        {
                            Datum = m.Datum.Value.UtcDateTime,
                            Notiz = m.Notiz.Value,
                            KaltMiete = m.Kalt,
                            WarmMiete = m.Warm,
                            VertragId = guid,
                        });
                    }
                }
                // Update Mieter
                var mieterset = App.Walter.MieterSet.Where(m => m.VertragId == guid).ToList();
                for (var i = 0; i < mieterset.Count(); ++i)
                {
                    var m = mieterset[i];
                    var mv = Mieter.Value.FirstOrDefault(mm => mm.Id == m.MieterId);
                    if (mv != null)
                    {
                        m.Kontakt = VertragDetailKontakt.GetKontakt(mv.Id);
                        App.Walter.MieterSet.Update(m);
                    }
                    else
                    {
                        App.Walter.MieterSet.Remove(m);
                    }
                }
                foreach (var m in Mieter.Value)
                {
                    if (!mieterset.Exists(mm => mm.MieterId == m.Id))
                    {
                        App.Walter.MieterSet.Add(new Mieter
                        {
                            Kontakt = VertragDetailKontakt.GetKontakt(m.Id),
                            VertragId = guid,
                        });
                    }
                }

                
                for (var i = 0; i < Versionen.Value.Count; ++i)
                {
                    var nV = Versionen.Value[Versionen.Value.Count - i - 1];
                    if (v.Count - i > 0)
                    {
                        var vi = v[v.Count - i - 1];
                        vi.Beginn = nV.Beginn.Value.UtcDateTime;
                        vi.Personenzahl = nV.Personenzahl.Value;
                        vi.Notiz = nV.Notiz.Value;
                        vi.VersionsNotiz = nV.Notiz.Value;
                        vi.Wohnung = VertragDetailWohnung.GetWohnung(nV.Wohnung.Value.Id);
                        vi.Ende = nV != Versionen.Value.First() ?
                            Versionen.Value[Versionen.Value.Count - i - 2].Beginn.Value.UtcDateTime.AddDays(-1) :
                            Ende.Value?.UtcDateTime;

                        App.Walter.Vertraege.Update(vi);
                    }
                    else
                    {
                        var neueVersion = new Vertrag
                        {
                            Ansprechpartner = VertragDetailKontakt.GetKontakt(nV.Ansprechpartner.Value.Id),
                            Wohnung = VertragDetailWohnung.GetWohnung(nV.Wohnung.Value.Id),
                            Personenzahl = nV.Personenzahl.Value,
                            Beginn = nV.Beginn.Value.UtcDateTime,
                            Ende = nV.Ende.Value?.UtcDateTime,
                            Notiz = nV.Notiz.Value,
                            Version = nV.Version,
                            VertragId = guid,
                        };
                        v.Insert(0, neueVersion);
                        App.Walter.Vertraege.Add(neueVersion);
                    }
                }

                var highest = Versionen.Value.First().Version;
                foreach (var vs in v)
                {
                    if (vs.Version > highest)
                    {
                        App.Walter.Vertraege.Remove(vs);
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
            Wohnung.Value = v.Wohnung is Wohnung w ? new VertragDetailWohnung(w) : null;
           
            Vermieter.Value = new JuristischePersonViewModel(v.Wohnung.Besitzer);
            Ansprechpartner.Value = v.Ansprechpartner is Kontakt va ? new VertragDetailKontakt(va) : null;
            Ende.Value = v.Ende;
            Beginn.Value = v.Beginn == DateTime.MinValue ? DateTime.Today : v.Beginn;
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
            //VertragsVersion.Value = m.Vertrag.Version;
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

