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

        public ObservableProperty<List<VertragVersionListViewModel>> Versionen { get; }
            = new ObservableProperty<List<VertragVersionListViewModel>>();

        public ObservableProperty<ImmutableList<KalteBetriebskostenViewModel>> KalteBetriebskosten { get; }
            = new ObservableProperty<ImmutableList<KalteBetriebskostenViewModel>>();

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer)
                  .Include(v => v.Garagen)
                  .Where(v => v.VertragId == id).ToList())
        { }

        public VertragDetailViewModel(List<Vertrag> v)
            : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
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

            KalteBetriebskosten.Value = App.Walter.KalteBetriebskosten
                .Select(k => new KalteBetriebskostenViewModel(k, v.Last()))
                .ToImmutableList();

            BeginEdit = new RelayCommand(_ => IsInEdit.Value = true, _ => !IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => BeginEdit.RaiseCanExecuteChanged(ev);

            SaveEdit = new RelayCommand(_ =>
            {
                IsInEdit.Value = false;

                v.First().Beginn = Beginn.Value;
                v.Last().Ende = Ende.Value;
                v.Last().Personenzahl = Personenzahl.Value;

                var MSet = App.Walter.MieterSet.ToList();
                // Remove deprecated MieterSets
                MSet.Where(ms =>
                        ms.VertragId == Id &&
                        !Mieter.Value.Exists(mv => mv.Id == ms.KontaktId))
                    .ToList().ForEach(d => MSet.Remove(d));
                // Add new MieterSets
                v.ForEach(vs => Mieter.Value
                    .Where(mv =>
                        !MSet.Exists(ms =>
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
                    vs.Ansprechpartner =
                        VertragDetailKontakt.GetKontakt(Ansprechpartner.Value.Id);
                    vs.Wohnung = VertragDetailWohnung.GetWohnung(vs.WohnungId.Value);
                });

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
        public ObservableProperty<int> Personenzahl { get; } = new ObservableProperty<int>();
        public ObservableProperty<VertragDetailWohnung> Wohnung { get; }
            = new ObservableProperty<VertragDetailWohnung>();
        public ObservableProperty<DateTime> Beginn { get; } = new ObservableProperty<DateTime>();
        public ObservableProperty<DateTime?> Ende { get; } = new ObservableProperty<DateTime?>();
        public ObservableProperty<JuristischePersonViewModel> Vermieter
            = new ObservableProperty<JuristischePersonViewModel>();
        public ObservableProperty<VertragDetailKontakt> Ansprechpartner
            = new ObservableProperty<VertragDetailKontakt>();
        public ObservableProperty<ImmutableList<VertragDetailKontakt>> Mieter
            = new ObservableProperty<ImmutableList<VertragDetailKontakt>>();

        public string BeginnString => Beginn.Value.ToShortDateString();
        public string EndeString => Ende.Value is DateTime e ? e.ToShortDateString() : "";
        public bool HasEnde => Ende.Value is DateTime;

        public VertragDetailVersion(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Personenzahl.Value = v.Personenzahl;
            Wohnung.Value = v.Wohnung is Wohnung w ? new VertragDetailWohnung(w) : null;
            Mieter.Value = v.Mieter.Select(m => new VertragDetailKontakt(m.Kontakt))
                .OrderBy(m => m.Name.Value.Length).Reverse() // From the longest to the smallest because of XAML I guess
                .ToImmutableList();
            Vermieter.Value = new JuristischePersonViewModel(
                v.Wohnung is Wohnung ? v.Wohnung.Besitzer : v.Garagen.First().Garage.Besitzer);
            Ansprechpartner.Value = new VertragDetailKontakt(v.Ansprechpartner);

            Ende.Value = v.Ende;
            Beginn.Value = v.Beginn;
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

