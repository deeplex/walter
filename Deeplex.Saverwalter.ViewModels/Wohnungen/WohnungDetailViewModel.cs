using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungDetailViewModel : ValidatableBase
    {
        public Wohnung Entity;
        public int Id => Entity.WohnungId;

        public async Task selfDestruct()
        {
            if (await Impl.Confirmation())
            {
                Avm.ctx.Wohnungen.Remove(Entity);
                Avm.SaveWalter();
            }
        }

        public ImmutableList<KontaktListEntry> AlleVermieter;

        private KontaktListEntry mBesitzer;
        public KontaktListEntry Besitzer
        {
            get => mBesitzer;
            set
            {
                Entity.BesitzerId = value == null ? Guid.Empty : value.Guid;
                mBesitzer = value;
                RaisePropertyChangedAuto();
            }
        }

        public int AdresseId => Entity.AdresseId;
        public string Anschrift => AdresseViewModel.Anschrift(AdresseId, Avm);

        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set
            {
                var old = Entity.Bezeichnung;
                Entity.Bezeichnung = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Notiz
        {
            get => Entity.Notiz;
            set
            {
                var old = Entity.Notiz;
                Entity.Notiz = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public double Wohnflaeche
        {
            get => Entity.Wohnflaeche;
            set
            {
                var val = double.IsNaN(value) ? 0 : value;
                var old = Entity.Wohnflaeche;
                Entity.Wohnflaeche = val;
                RaisePropertyChangedAuto(old, val);
            }
        }

        public double Nutzflaeche
        {
            get => Entity.Nutzflaeche;
            set
            {
                var val = double.IsNaN(value) ? 0 : value;
                var old = Entity.Nutzflaeche;
                Entity.Nutzflaeche = val;
                RaisePropertyChangedAuto(old, val);
            }
        }

        public int Nutzeinheit
        {
            get => Entity.Nutzeinheit;
            set
            {
                var val = int.MinValue == value ? 0 : value;
                var old = Entity.Nutzeinheit;
                Entity.Nutzeinheit = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        private IAppImplementation Impl;
        private AppViewModel Avm;

        public WohnungDetailViewModel(IAppImplementation impl, AppViewModel avm) : this(new Wohnung(), impl, avm) { }
        public WohnungDetailViewModel(Wohnung w, IAppImplementation impl, AppViewModel avm)
        {
            Entity = w;
            Avm = avm;
            Impl = impl;

            AlleVermieter = Avm.ctx.JuristischePersonen.ToImmutableList()
                .Where(j => j.isVermieter == true).Select(j => new KontaktListEntry(j))
                .Concat(Avm.ctx.NatuerlichePersonen
                    .Where(n => n.isVermieter == true).Select(n => new KontaktListEntry(n)))
                .ToImmutableList();

            if (w.BesitzerId != Guid.Empty)
            {
                Besitzer = AlleVermieter.SingleOrDefault(e => e.Guid == w.BesitzerId);
            }

            PropertyChanged += OnUpdate;
        }

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

            if ((Entity.Adresse == null && Entity.AdresseId == 0) ||
                Entity.Bezeichnung == null || Entity.Bezeichnung == "")
            {
                return;
            }

            if (Entity.WohnungId != 0)
            {
                Avm.ctx.Wohnungen.Update(Entity);
            }
            else
            {
                Avm.ctx.Wohnungen.Add(Entity);
            }
            Avm.SaveWalter();
        }
    }
}
