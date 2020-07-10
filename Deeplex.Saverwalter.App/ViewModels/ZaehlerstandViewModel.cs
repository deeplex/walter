using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class ZaehlerstandViewModel : BindableBase
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

        public ZaehlerstandViewModel(Zaehlerstand z)
        {
            SelfDestruct = new RelayCommand(_ =>
            {
                App.Walter.Remove(Entity);
                App.Walter.SaveChanges();
            }, _ => true);

            Entity = z;
            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.ZaehlerstandAnhaenge, z), _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand SelfDestruct { get; }
    }
}
