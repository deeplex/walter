﻿using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using static Deeplex.Saverwalter.App.Views.WohnungDetailPage;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class WohnungDetailViewModel : ValidatableBase
    {
        private Wohnung Entity;
        public int Id;

        public ImmutableList<WohnungDetailVermieter> AlleVermieter =>
            App.Walter.JuristischePersonen.ToImmutableList()
                .Where(j => j.isVermieter == true).Select(j => new WohnungDetailVermieter(j))
                .Concat(App.Walter.NatuerlichePersonen
                    .Where(n => n.isVermieter == true).Select(n => new WohnungDetailVermieter(n)))
                .ToImmutableList();

        private WohnungDetailVermieter mBesitzer;
        public WohnungDetailVermieter Besitzer
        {
            get => mBesitzer;
            set
            {
                if (value?.Id != Guid.Empty) return;
                Entity.BesitzerId = value.Id;
                mBesitzer = value;
                RaisePropertyChangedAuto();
            }
        }

        public ObservableProperty<ImmutableList<ZaehlerViewModel>> Zaehler
            = new ObservableProperty<ImmutableList<ZaehlerViewModel>>();
        public ObservableProperty<List<WohnungDetailVertrag>> Vertraege
            = new ObservableProperty<List<WohnungDetailVertrag>>();

        public class WohnungDetailVermieter
        {
            public Guid Id { get; set; }
            public string Bezeichnung { get; }

            public WohnungDetailVermieter(Guid g) : this(App.Walter.FindPerson(g)) { }

            public WohnungDetailVermieter(IPerson p)
            {
                Id = p.PersonId;
                Bezeichnung = p.Bezeichnung;
            }
        }

        public int AdresseId => Entity.AdresseId;
        public string Anschrift => AdresseViewModel.Anschrift(AdresseId);

        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set
            {
                Entity.Bezeichnung = value;
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

        public double Wohnflaeche
        {
            get => Entity.Wohnflaeche;
            set
            {
                Entity.Wohnflaeche = value;
                RaisePropertyChangedAuto();
            }
        }
        public double Nutzflaeche
        {
            get => Entity.Nutzflaeche;
            set
            {
                Entity.Nutzflaeche = value;
                RaisePropertyChangedAuto();
            }
        }

        public int Nutzeinheit
        {
            get => Entity.Nutzeinheit;
            set
            {
                Entity.Nutzeinheit = value;
                RaisePropertyChangedAuto();
            }
        }

        public WohnungDetailViewModel(int id)
            : this(App.Walter.Wohnungen
                  .Include(w => w.Adresse)
                  .Include(w => w.Zaehler)
                  .First(w => w.WohnungId == id))
        { }

        public WohnungDetailViewModel() : this(new Wohnung())
        {
            IsInEdit.Value = true;
        } // Create new Wohnung

        public ImmutableList<Zaehlertyp> Zaehlertypen =
            Enum.GetValues(typeof(Zaehlertyp))
                .Cast<Zaehlertyp>()
                .ToImmutableList();

        private WohnungDetailViewModel(Wohnung w)
        {
            Entity = w;

            if (w.BesitzerId != Guid.Empty)
            {
                Besitzer = new WohnungDetailVermieter(w.BesitzerId);
            }

            Zaehler.Value = w.Zaehler
                .Select(z => new ZaehlerViewModel(z))
                .Concat(App.Walter.AllgemeinZaehlerGruppen
                    .Where(g => g.WohnungId == w.WohnungId)
                    .Select(g => new ZaehlerViewModel(g.Zaehler)))
                    .ToImmutableList();

            Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Wohnung).ToList()
                .Where(v => v.Wohnung.WohnungId == Id)
                .Select(v => new WohnungDetailVertrag(v.VertragId))
                .ToList();

            AddAllgemeinZaehler = new RelayCommand(AddAllgemeinZaehlerPanel =>
            {
                var Tree = (AddAllgemeinZaehlerPanel as StackPanel).Children[0] as Microsoft.UI.Xaml.Controls.TreeView;
                var Panel = (AddAllgemeinZaehlerPanel as StackPanel).Children[1] as StackPanel;

                var kn = (Panel.Children[0] as TextBox).Text;
                var Typ = (Zaehlertyp)(Panel.Children[1] as ComboBox).SelectedItem;

                var z = new AllgemeinZaehler
                {
                    Kennnummer = kn,
                    Typ = Typ,
                };
                App.Walter.AllgemeinZaehlerSet.Add(z);

                foreach (var item in Tree.SelectedNodes)
                {
                    if (!(item is WohnungDetailAdresseWohnung wohnung))
                    {
                        continue;
                    }
                    App.Walter.AllgemeinZaehlerGruppen.Add(new AllgemeinZaehlerGruppe
                    {
                        WohnungId = wohnung.Id,
                        Zaehler = z,
                    });
                }

                App.Walter.SaveChanges();
                var wdz = new ZaehlerViewModel(z);
                RaisePropertyChanged(nameof(AllgemeinZaehler));
            }, _ => true);

            AddZaehler = new RelayCommand(AddZaehlerPanel =>
            {
                var kn = ((TextBox)((StackPanel)AddZaehlerPanel).Children[0]).Text;
                var Typ = (Zaehlertyp)((ComboBox)((StackPanel)AddZaehlerPanel).Children[1]).SelectedItem;

                var z = new Zaehler
                {
                    Wohnung = w,
                    Kennnummer = kn,
                    Typ = Typ,
                };
                App.Walter.ZaehlerSet.Add(z);
                App.Walter.SaveChanges();
                var wdz = new ZaehlerViewModel(z);
                Zaehler.Value = Zaehler.Value.Add(wdz);
                RaisePropertyChanged(nameof(Zaehler));
            }, _ => true);

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.WohnungAnhaenge, w), _ => true);

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));
            PropertyChanged += OnUpdate;
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public AsyncRelayCommand AttachFile;
        public RelayCommand AddZaehler { get; }
        public RelayCommand AddAllgemeinZaehler { get; }
        public bool IsNotInEdit => !IsInEdit.Value;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Besitzer):
                case nameof(Bezeichnung):
                case nameof(Wohnflaeche):
                case nameof(Nutzflaeche):
                case nameof(Nutzeinheit):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if (Entity.BesitzerId == null ||
                (Entity.Adresse == null && Entity.AdresseId == 0) ||
                Entity.Bezeichnung == null || Entity.Bezeichnung == "")
            {
                return;
            }

            if (Entity.WohnungId != 0)
            {
                App.Walter.Wohnungen.Update(Entity);
            }
            else
            {
                App.Walter.Wohnungen.Add(Entity);
            }
            App.Walter.SaveChanges();
        }
    }

    public sealed class WohnungDetailVertrag
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

            Beginn.Value = v.Beginn.AsUtcKind();
            Ende.Value = v.Ende?.AsUtcKind();

            var bs = App.Walter.MieterSet.Where(m => m.VertragId == v.VertragId).ToList();
            var cs = bs.Select(b =>
            {
                var c = App.Walter.NatuerlichePersonen.Find(b);
                return string.Join(" ", c.Vorname ?? "", c.Nachname);
            });
            AuflistungMieter.Value = string.Join(", ", cs);
        }
    }
}
