using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.ComponentModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragDetailVersion : BindableBase
    {
        public Vertrag Entity { get; }
        public int Id => Entity.rowid;
        public int Version => Entity.Version;
        public double KaltMiete
        {
            get => Entity.KaltMiete;
            set
            {
                var val = Double.IsNaN(value) ? 0 : value;
                var old = Entity.KaltMiete;
                Entity.KaltMiete = val;
                RaisePropertyChangedAuto(old, val);
            }
        }

        public int Personenzahl
        {
            get => Entity.Personenzahl;
            set
            {
                var val = int.MinValue == value ? 0 : value;
                var old = Entity.Personenzahl;
                Entity.Personenzahl = val;
                RaisePropertyChangedAuto(old, val);
            }
        }

        private WohnungListEntry mWohnung;
        public WohnungListEntry Wohnung
        {
            get => mWohnung;
            set
            {
                if (value == null) return;

                var old = Entity.Wohnung;
                Entity.Wohnung = value?.Entity;
                mWohnung = value;
                if (RaisePropertyChangedAuto(old, value.Entity))
                {
                    RaisePropertyChanged(nameof(Vermieter));
                }
            }
        }
        public DateTimeOffset Beginn
        {
            get => Entity.Beginn.AsUtcKind();
            set
            {
                var old = Entity.Beginn;
                Entity.Beginn = value.UtcDateTime;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public DateTimeOffset? Ende
        {
            get => Entity.Ende?.AsUtcKind();
            set
            {
                var old = Entity.Ende;
                Entity.Ende = value?.UtcDateTime;
                RaisePropertyChangedAuto(old, value?.UtcDateTime);
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

        public KontaktListEntry Vermieter
            => Wohnung?.Entity?.BesitzerId is Guid g && g != Guid.Empty ?
                    new KontaktListEntry(g) : null;

        private KontaktListEntry mAnsprechpartner;
        public KontaktListEntry Ansprechpartner
        {
            get => mAnsprechpartner;
            set
            {
                if (value == null && mAnsprechpartner == null)
                {
                    return;
                }
                var old = mAnsprechpartner?.Guid;
                mAnsprechpartner = value;
                Entity.AnsprechpartnerId = mAnsprechpartner?.Guid;
                RaisePropertyChangedAuto(old, value?.Guid);
            }
        }

        public VertragDetailVersion(int id) : this(App.Walter.Vertraege.Find(id)) { }
        public VertragDetailVersion(Vertrag v)
        {
            Entity = v;

            if (v.AnsprechpartnerId != Guid.Empty && v.AnsprechpartnerId != null)
            {
                Ansprechpartner = new KontaktListEntry(v.AnsprechpartnerId.Value);
            }

            PropertyChanged += OnUpdate;
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Wohnung):
                case nameof(Beginn):
                case nameof(Ende):
                case nameof(Notiz):
                case nameof(Personenzahl):
                case nameof(KaltMiete):
                // case nameof(VersionsNotiz):
                case nameof(Ansprechpartner):
                    break;
                default:
                    return;
            }

            if (Entity.VertragId == null ||
                Entity.Beginn == null ||
                (Entity.Wohnung == null && Entity.WohnungId == 0) ||
                Entity.AnsprechpartnerId == null)
            {
                return;
            }

            if (Entity.rowid != 0)
            {
                App.Walter.Vertraege.Update(Entity);
            }
            else
            {
                App.Walter.Vertraege.Add(Entity);
            }
            App.SaveWalter();
        }
    }

}
