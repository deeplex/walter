using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class ZaehlerstandDetailViewModel : BindableBase
    {
        public Zaehlerstand Entity { get; }
        
        public DateTime Datum
        {
            get => Entity.Datum;
            set
            {
                Entity.Datum = value;
                RaisePropertyChangedAuto();
            }
        }

        public double Stand
        {
            get => Entity.Stand;
            set
            {
                Entity.Stand = value;
                RaisePropertyChangedAuto();
            }
        }

        public Zaehler Zaehler
        {
            get => Entity.Zaehler;
            set
            {
                Entity.Zaehler = value;
                RaisePropertyChangedAuto();
            }
        }

        public ZaehlerstandDetailViewModel() : this(new Zaehlerstand()) { }
        public ZaehlerstandDetailViewModel(Zaehlerstand z)
        {
            Entity = z;
        }
    }
}
