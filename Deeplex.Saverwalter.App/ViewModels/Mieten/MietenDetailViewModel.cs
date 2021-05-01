﻿using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class MietenDetailViewModel : BindableBase
    {
        public Miete Entity { get; }

        public double Betrag
        {
            get => Entity.Betrag ?? 0;
            set
            {
                var old = Entity.Betrag;
                Entity.Betrag = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public DateTimeOffset BetreffenderMonat
        {
            get => Entity.BetreffenderMonat.AsMin();
            set
            {
                var old = Entity.BetreffenderMonat.AsMin();
                Entity.BetreffenderMonat = value.DateTime.AsUtcKind();
                RaisePropertyChangedAuto(old, value);
            }
        }

        public DateTimeOffset Zahlungsdatum
        {
            get => Entity.Zahlungsdatum.AsMin();
            set
            {
                var old = Entity.Zahlungsdatum.AsMin();
                Entity.Zahlungsdatum = value.DateTime.AsMin();
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

        public MietenDetailViewModel() : this(new Miete()) { }
        public MietenDetailViewModel(Miete m)
        {
            Entity = m;
        }
    }
}