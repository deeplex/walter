using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ZaehlerstandViewModel : BindableBase
    {
        public ZaehlerstandViewModel self => this;

        public Zaehlerstand Entity { get; }
        public int Id => Entity.ZaehlerstandId;

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

        public ZaehlerstandViewModel(Zaehlerstand z, ZaehlerViewModel parent)
        {
            Entity = z;
            // TODO
            //AttachFile = new AsyncRelayCommand(async _ =>
            //    await Utils.Files.SaveFilesToWalter(parent.
            //
            //
            //
            //
            //    ZaehlerstandAnhaenge, z, parent.Impl), _ => true);

            SelfDestruct = new RelayCommand(_ =>
            {
                //parent.impl.Remove(Entity); TODO 
                parent.Zaehlerstaende.Value = parent.Zaehlerstaende.Value.Remove(this);

                //Avm.SaveWalter(); TODO
            }, _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand SelfDestruct { get; }
    }
}
