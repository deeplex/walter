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
                var old = Entity.Datum;
                Entity.Datum = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public double Stand
        {
            get => Entity.Stand;
            set
            {
                var old = Entity.Stand;
                Entity.Stand = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public Zaehler Zaehler
        {
            get => Entity.Zaehler;
            set
            {
                var old = Entity.Zaehler;
                Entity.Zaehler = value;
                RaisePropertyChangedAuto(old, value);
            }
        }
        public ZaehlerstandDetailViewModel() : this(new Zaehlerstand()) { }
        public ZaehlerstandDetailViewModel(Zaehlerstand z)
        {
            Entity = z;
        }
    }
}
