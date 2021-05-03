using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class MietMinderungDetailViewModel : BindableBase
    {
        public MietMinderung Entity { get; }

        public double Minderung
        {
            get => Entity.Minderung;
            set
            {
                var old = Entity.Minderung;
                Entity.Minderung = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public DateTimeOffset Ende
        {
            get => Entity.Ende is DateTime d ? d.AsMin() : DateTime.Now.AsUtcKind();
            set
            {
                var old = Ende;
                Entity.Ende = value.DateTime.AsMin();
                RaisePropertyChangedAuto(old, value);
            }
        }

        public DateTimeOffset Beginn
        {
            get => Entity.Beginn.AsMin();
            set
            {
                var old = Entity.Beginn.AsMin();
                Entity.Beginn = value.DateTime.AsMin();
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

        public Guid VertragId
        {
            get => Entity.VertragId;
            set
            {
                var old = Entity.VertragId;
                Entity.VertragId = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public MietMinderungDetailViewModel() : this(new MietMinderung()) { }
        public MietMinderungDetailViewModel(MietMinderung m)
        {
            Entity = m;
        }
    }
}
