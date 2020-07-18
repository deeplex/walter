using Deeplex.Saverwalter.Model;
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
        public Wohnung GetEntity => Entity;
        public int Id => Entity.WohnungId;

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

        public int AdresseId => Entity.AdresseId;
        public string Anschrift => AdresseViewModel.Anschrift(AdresseId);

        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set => update(nameof(Entity.Bezeichnung), value);
        }

        public string Notiz
        {
            get => Entity.Notiz;
            set => update(nameof(Entity.Notiz), value);
        }

        public double Wohnflaeche
        {
            get => Entity.Wohnflaeche;
            set => update(nameof(Entity.Wohnflaeche), value);

        }
        public double Nutzflaeche
        {
            get => Entity.Nutzflaeche;
            set => update(nameof(Entity.Nutzflaeche), value);
        }

        public int Nutzeinheit
        {
            get => Entity.Nutzeinheit;
            set => update(nameof(Entity.Nutzeinheit), value);
        }

        public WohnungDetailViewModel(int id)
            : this(App.Walter.Wohnungen
                  .Include(w => w.Adresse)
                  .Include(w => w.Zaehler)
                  .ThenInclude(z => z.Staende)
                  .Include(w => w.AllgemeinZaehlerGruppen)
                  .ThenInclude(g => g.Zaehler)
                  .ThenInclude(z => z.Staende)
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
                .Concat(w.AllgemeinZaehlerGruppen
                    .Select(g => new ZaehlerViewModel(g.Zaehler)))
                    .ToImmutableList();

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

                App.SaveWalter();
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
                App.SaveWalter();
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
            App.SaveWalter();
        }
    }
}
