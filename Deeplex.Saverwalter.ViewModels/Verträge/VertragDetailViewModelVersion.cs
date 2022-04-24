using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.ComponentModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragDetailViewModelVersion : BindableBase
    {
        public Vertrag Entity { get; }
        public int Id => Entity.rowid;
        public int Version => Entity.Version;
        public double KaltMiete
        {
            get => Entity.KaltMiete;
            set
            {
                var val = double.IsNaN(value) ? 0 : value;
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

        private WohnungListViewModelEntry mWohnung;
        public WohnungListViewModelEntry Wohnung
        {
            get => mWohnung;
            set
            {
                if (value == null) return;

                var old = Entity.Wohnung;
                Entity.Wohnung = value.Entity;
                if (Ansprechpartner == null)
                {
                    Ansprechpartner = new KontaktListViewModelEntry(value.Entity.BesitzerId, Db);
                }
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

        public KontaktListViewModelEntry Vermieter
            => Wohnung?.Entity?.BesitzerId is Guid g && g != Guid.Empty ?
                    new KontaktListViewModelEntry(g, Db) : null;

        private KontaktListViewModelEntry mAnsprechpartner;
        public KontaktListViewModelEntry Ansprechpartner
        {
            get => mAnsprechpartner;
            set
            {
                if (value == null && mAnsprechpartner == null)
                {
                    return;
                }
                var old = mAnsprechpartner?.Entity.PersonId;
                mAnsprechpartner = value;
                Entity.AnsprechpartnerId = mAnsprechpartner?.Entity.PersonId;
                RaisePropertyChangedAuto(old, value?.Entity.PersonId);
            }
        }

        protected IWalterDbService Db;
        protected IAppImplementation Impl;

        public RelayCommand RemoveDate;

        public VertragDetailViewModelVersion(int id, IAppImplementation impl, IWalterDbService db) : this(db.ctx.Vertraege.Find(id), impl, db) { }
        public VertragDetailViewModelVersion(Vertrag v, IAppImplementation impl, IWalterDbService db)
        {
            Entity = v;
            Db = db;
            Impl = impl;

            if (v.AnsprechpartnerId != Guid.Empty && v.AnsprechpartnerId != null)
            {
                Ansprechpartner = new KontaktListViewModelEntry(v.AnsprechpartnerId.Value, db);
            }

            RemoveDate = new RelayCommand(_ => Ende = null, _ => Ende != null);
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
                Db.ctx.Vertraege.Update(Entity);
            }
            else
            {
                Db.ctx.Vertraege.Add(Entity);
            }
            Db.SaveWalter();
        }
    }

}
