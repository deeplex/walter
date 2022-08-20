using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragVersionListViewModelEntry : DetailViewModel<VertragVersion>, IDetailViewModel
    {
        public override string ToString() => "Vertragsversion";

        public int Id => Entity.VertragVersionId;

        public SavableProperty<DateTimeOffset> Beginn { get; set; }
        public SavableProperty<string> Notiz { get; set; }
        public SavableProperty<double> Grundmiete { get; set; }
        public SavableProperty<int> Personenzahl { get; set; }

        public VertragVersionListViewModelEntry(VertragVersion v, IWalterDbService db, INotificationService ns) : base(ns, db)
        {
            WalterDbService = db;
            NotificationService = ns;

            SetEntity(v);

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public new void save()
        {
            Personenzahl.Value = Entity.Personenzahl;
            Grundmiete.Value = Entity.Grundmiete;
            Beginn.Value = Entity.Beginn.AsUtcKind();
            Notiz.Value = Entity.Notiz;

            base.save();
        }

        public override void SetEntity(VertragVersion v)
        {
            Entity = v;

            Grundmiete = new(this, v.Grundmiete);
            Personenzahl = new(this, v.Personenzahl);
            Beginn = new(this, v.Beginn);
            Notiz = new(this, v.Notiz);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Personenzahl.Value != Entity.Personenzahl ||
                Grundmiete.Value != Entity.Grundmiete ||
                Beginn.Value != Entity.Beginn.AsUtcKind() ||
                Notiz.Value != Entity.Notiz;
        }
    }
}
