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

        public VertragDetailViewModel() : this(new List<Vertrag> { new Vertrag() })
        {
            IsInEdit.Value = true;
        }

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer)
                  .Include(v => v.Garagen)
                  .Where(v => v.VertragId == id).ToList()) { }

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

            BeginEdit = new RelayCommand(_ => IsInEdit.Value = true, _ => !IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => BeginEdit.RaiseCanExecuteChanged(ev);

            SaveEdit = new RelayCommand(_ =>
            {
                IsInEdit.Value = false;

                v.First().Beginn = Beginn.Value.UtcDateTime;
                v.Last().Ende = Ende.Value?.UtcDateTime;
                v.Last().Personenzahl = Personenzahl.Value;

                var MSet = App.Walter.MieterSet;
                //Remove deprecated MieterSets
                MSet.ToList().Where(ms =>
                        ms.VertragId == Id &&
                        !Mieter.Value.Exists(mv => mv.Id == ms.KontaktId))
                    .ToList().ForEach(d => MSet.Remove(d));
                // Add new MieterSets
                v.ForEach(vs => Mieter.Value
                    .Where(mv =>
                        !MSet.ToList().Exists(ms =>
                            ms.VertragId == Id &&
                            ms.MieterId == mv.Id))
                    .ToList().ForEach(d => MSet.Add(new Mieter()
                    {
                        Vertrag = vs,
                        Kontakt = VertragDetailKontakt.GetKontakt(d.Id)
                    })));

                v.ForEach(vs =>
                {
                    // vs.Garagen
                    vs.Ansprechpartner = VertragDetailKontakt.GetKontakt(Ansprechpartner.Value.Id);
                    vs.Wohnung = VertragDetailWohnung.GetWohnung(Wohnung.Value.Id);
                    if (vs.rowid > 0)
                    {
                        App.Walter.Vertraege.Update(vs);
                    }
                    else
                    {
                        App.Walter.Vertraege.Add(vs);
                    }
                });

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

    public class VertragDetailVersion : BindableBase
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<int> Personenzahl = new ObservableProperty<int>();
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

        public string BeginnString => Beginn.Value.DateTime.ToShortDateString();
        public string EndeString => Ende.Value is DateTimeOffset e ? e.DateTime.ToShortDateString() : "";

        public VertragDetailVersion(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
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
            BezeichnungVoll.Value = Utils.Anschrift(w) + " - " + w.Bezeichnung;
        }

        public static Wohnung GetWohnung(int id) => App.Walter.Wohnungen.Find(id);
    }
}

