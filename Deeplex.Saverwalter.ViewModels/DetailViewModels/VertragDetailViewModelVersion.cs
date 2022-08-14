using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragDetailViewModelVersion : DetailViewModel<Vertrag>, IDetailViewModel
    {
        public override string ToString() => "Vertrag"; // TODO

        public int Id => Entity.rowid;
        public int Version => Entity.Version;
        public SavableProperty<double> KaltMiete;
        public SavableProperty<int> Personenzahl;
        public SavableProperty<WohnungListViewModelEntry> Wohnung { get; protected set; }
        public SavableProperty<DateTimeOffset> Beginn;
        public SavableProperty<DateTimeOffset?> Ende;
        public SavableProperty<string> Notiz;
        public SavableProperty<KontaktListViewModelEntry> Ansprechpartner { get; protected set; }

        public RelayCommand RemoveDate;

        public VertragDetailViewModelVersion(INotificationService ns, IWalterDbService db): base(ns, db)
        {
            RemoveDate = new RelayCommand(_ => Ende = null, _ => Ende != null);
        }

        public override void SetEntity(Vertrag v)
        {
            Entity = v;

            KaltMiete = new(this, v.KaltMiete);
            Personenzahl = new(this, v.Personenzahl);
            Wohnung = new(this, new(v.Wohnung, WalterDbService));
            Beginn = new(this, v.Beginn);
            Ende = new(this, v.Ende);
            Notiz = new(this, v.Notiz);
            Ansprechpartner = new(this, v.AnsprechpartnerId is Guid g ? new(WalterDbService, g) : null);
        }

        public override void checkForChanges()
        {
            if (!(Ansprechpartner.Value == null && Entity.AnsprechpartnerId == Guid.Empty))
            {
                NotificationService.outOfSync = Ansprechpartner.Value?.Guid != Entity.AnsprechpartnerId;
            }

            NotificationService.outOfSync =
                Wohnung.Value.Entity.WohnungId != Entity.Wohnung.WohnungId ||
                Personenzahl.Value != Entity.Personenzahl ||
                KaltMiete.Value != Entity.KaltMiete ||
                Beginn.Value != Entity.Beginn ||
                Ende.Value != Entity.Ende ||
                Notiz.Value != Entity.Notiz;
        }

        public void versionSave()
        {
            Entity.Wohnung = Wohnung.Value.Entity;
            Entity.Beginn = Beginn.Value.DateTime;
            Entity.Ende = Ende.Value?.DateTime;
            Entity.Notiz = Notiz.Value;
            Entity.Personenzahl = Personenzahl.Value;
            Entity.KaltMiete = KaltMiete.Value;
            Entity.AnsprechpartnerId = Ansprechpartner.Value?.Guid ?? Guid.Empty;

            if (Entity.rowid != 0)
            {
                WalterDbService.ctx.Vertraege.Update(Entity);
            }
            else
            {
                WalterDbService.ctx.Vertraege.Add(Entity);
            }
        }
    }

}
