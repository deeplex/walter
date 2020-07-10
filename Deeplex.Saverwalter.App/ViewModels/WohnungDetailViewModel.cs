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

        public ObservableProperty<ImmutableList<WohnungDetailZaehler>> Zaehler
            = new ObservableProperty<ImmutableList<WohnungDetailZaehler>>();
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

            var self = this;
            Zaehler.Value = w.Zaehler
                .Select(z => new WohnungDetailZaehler(z, self))
                .Concat(App.Walter.AllgemeinZaehlerGruppen
                    .Where(g => g.WohnungId == w.WohnungId)
                    .Select(g => new WohnungDetailZaehler(g.Zaehler, self)))
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
                var wdz = new WohnungDetailZaehler(z, self);
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
                var wdz = new WohnungDetailZaehler(z, self);
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

    public sealed class WohnungDetailZaehler : BindableBase
    {
        public int Id;
        private object Entity
        {
            get
            {
                if (Zaehler == null)
                {
                    return AllgemeinZaehler;
                }
                else
                {
                    return Zaehler;
                }
            }
        }

        private Zaehler Zaehler;
        private AllgemeinZaehler AllgemeinZaehler;

        public string AllgemeinString => AllgemeinZaehler != null ? " Allgemein" : "";

        public ObservableProperty<string> Kennnummer = new ObservableProperty<string>();
        public ObservableProperty<string> Typ = new ObservableProperty<string>();
        public ObservableProperty<ImmutableList<WohnungDetailZaehlerStand>> Zaehlerstaende
            = new ObservableProperty<ImmutableList<WohnungDetailZaehlerStand>>();
        public DateTimeOffset AddZaehlerstandDatum => DateTime.UtcNow.Date.AsUtcKind();
        // TODO interpolate between last and prelast to determine stand
        public double AddZaehlerstandStand => Zaehlerstaende.Value.FirstOrDefault()?.Stand ?? 0;
        public void LoadList()
        {
            var self = this;

            Zaehlerstaende.Value = App.Walter.Zaehlerstaende
                .Where(zs => zs.Zaehler == Entity)
                .Select(zs => new WohnungDetailZaehlerStand(zs, self))
                .ToList()
                .OrderBy(zs => zs.Datum).Reverse()
                .ToImmutableList();
        }

        public ObservableProperty<bool> IsInEdit;

        public WohnungDetailZaehler(Zaehler z, WohnungDetailViewModel p)
        {
            Id = z.ZaehlerId;
            IsInEdit = p.IsInEdit;
            Zaehler = z;
            Kennnummer.Value = z.Kennnummer;
            Typ.Value = z.Typ.ToString(); // May be a descript thingy later on?...

            var self = this; // Protect against memory leaks? Ask the debugger im new.

            Zaehlerstaende.Value = App.Walter.Zaehlerstaende
                .Where(zs => zs.Zaehler == Entity)
                .Select(zs => new WohnungDetailZaehlerStand(zs, self))
                .ToList()
                .OrderBy(zs => zs.Datum).Reverse()
                .ToImmutableList();

            AddZaehlerstand = new RelayCommand(AddZaehlerstandPanel =>
            {
                var dtp = ((CalendarDatePicker)((StackPanel)AddZaehlerstandPanel).Children[0]).Date;
                var datum = (dtp.HasValue ? dtp.Value.UtcDateTime : DateTime.UtcNow.Date).AsUtcKind();
                var stand = Convert.ToDouble(((NumberBox)((StackPanel)AddZaehlerstandPanel).Children[1]).Text);

                var zs = new Zaehlerstand
                {
                    Zaehler = z,
                    Datum = datum,
                    Stand = stand,
                };
                App.Walter.Zaehlerstaende.Add(zs);
                App.Walter.SaveChanges();
                var wdzs = new WohnungDetailZaehlerStand(zs, self);
                Zaehlerstaende.Value = Zaehlerstaende.Value
                    .Add(wdzs)
                    .OrderBy(nzs => nzs.Datum).Reverse()
                    .ToImmutableList();
                RaisePropertyChanged(nameof(Zaehlerstaende));
            }, _ => true);


            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.ZaehlerAnhaenge, z), _ => true);
        }

        public WohnungDetailZaehler(AllgemeinZaehler z, WohnungDetailViewModel p)
        {
            Id = z.AllgemeinZaehlerId;
            IsInEdit = p.IsInEdit;
            AllgemeinZaehler = z;
            Kennnummer.Value = z.Kennnummer;
            Typ.Value = z.Typ.ToString(); // May be a descript thingy later on?...

            var self = this; // Protect against memory leaks? Ask the debugger im new.

            Zaehlerstaende.Value = App.Walter.Zaehlerstaende
                .Where(zs => zs.AllgemeinZaehler == Entity)
                .Select(zs => new WohnungDetailZaehlerStand(zs, self))
                .ToList()
                .OrderBy(zs => zs.Datum).Reverse()
                .ToImmutableList();

            AddZaehlerstand = new RelayCommand(AddZaehlerstandPanel =>
            {
                var dtp = ((CalendarDatePicker)((StackPanel)AddZaehlerstandPanel).Children[0]).Date;
                var datum = (dtp.HasValue ? dtp.Value.UtcDateTime : DateTime.UtcNow.Date).AsUtcKind();
                var stand = Convert.ToDouble(((NumberBox)((StackPanel)AddZaehlerstandPanel).Children[1]).Text);

                var zs = new Zaehlerstand
                {
                    AllgemeinZaehler = z,
                    Datum = datum,
                    Stand = stand,
                };
                App.Walter.Zaehlerstaende.Add(zs);
                App.Walter.SaveChanges();
                var wdzs = new WohnungDetailZaehlerStand(zs, self);
                Zaehlerstaende.Value = Zaehlerstaende.Value
                    .Add(wdzs)
                    .OrderBy(nzs => nzs.Datum).Reverse()
                    .ToImmutableList();
                RaisePropertyChanged(nameof(Zaehlerstaende));
            }, _ => true);


            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.AllgemeinZaehlerAnhaenge, z), _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand AddZaehlerstand { get; }
    }

    public sealed class WohnungDetailZaehlerStand : BindableBase
    {
        public int Id => Entity.ZaehlerstandId;
        public Zaehlerstand Entity;
        public double Stand
        {
            get => Entity.Stand;
            set
            {
                Entity.Stand = value;
                RaisePropertyChangedAuto();
            }
        }
        public DateTimeOffset Datum
        {
            get => Entity.Datum.AsUtcKind();
            set
            {
                Entity.Datum = value.UtcDateTime.AsUtcKind();
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

        public ObservableProperty<bool> IsInEdit;

        public WohnungDetailZaehlerStand(Zaehlerstand z, WohnungDetailZaehler p) : this(z)
        {
            IsInEdit = p.IsInEdit;

            SelfDestruct = new RelayCommand(_ =>
            {
                App.Walter.Remove(Entity);
                App.Walter.SaveChanges();
                p.LoadList();
            }, _ => p.IsInEdit.Value);

        }

        private WohnungDetailZaehlerStand(Zaehlerstand z)
        {
            Entity = z;
            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.ZaehlerstandAnhaenge, z), _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand SelfDestruct { get; }
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
