using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragDetailViewModelVersion : BindableBase, IDetailViewModel<Vertrag>
    {
        public override string ToString() => "Vertrag"; // TODO

        public Vertrag Entity { get; set; }
        public int Id => Entity.rowid;
        public int Version => Entity.Version;
        public SavableProperty<double, Vertrag> KaltMiete;
        public SavableProperty<int, Vertrag> Personenzahl;
        public SavableProperty<WohnungListViewModelEntry?, Vertrag> Wohnung;
        public SavableProperty<DateTimeOffset, Vertrag> Beginn;
        public SavableProperty<DateTimeOffset?, Vertrag> Ende;
        public SavableProperty<string, Vertrag> Notiz;
        public SavableProperty<KontaktListViewModelEntry, Vertrag> Ansprechpartner { get; private set; }

        public KontaktListViewModelEntry Vermieter
            => Wohnung.Value?.Entity?.BesitzerId is Guid g && g != Guid.Empty ?
                    new KontaktListViewModelEntry(WalterDbService, g) : null;

        public RelayCommand RemoveDate;
        public RelayCommand Save { get; protected set; }
        public AsyncRelayCommand Delete { get; protected set; }

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }

        public VertragDetailViewModelVersion(INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            RemoveDate = new RelayCommand(_ => Ende = null, _ => Ende != null);
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    WalterDbService.ctx.Vertraege.Remove(Entity);
                    WalterDbService.SaveWalter();
                }
            }, _ => true);
        }

        public void SetEntity(Vertrag v)
        {
            Entity = v;

            KaltMiete = new(this, v.KaltMiete);
            Personenzahl = new(this, v.Personenzahl);
            Wohnung = new(this, new(v.Wohnung, WalterDbService));
            Beginn = new(this, v.Beginn);
            Ende = new(this, v.Ende);
            Notiz = new(this, v.Notiz);

            if (v.AnsprechpartnerId != Guid.Empty && v.AnsprechpartnerId != null)
            {
                Ansprechpartner = new(this, new(WalterDbService, v.AnsprechpartnerId.Value));
            }
            else
            {
                Ansprechpartner = new(this, null);
            }
        }

        public void checkForChanges()
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
