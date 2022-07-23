using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragDetailViewModelVersion : BindableBase, IDetail
    {
        public override string ToString() => "Vertrag"; // TODO

        public Vertrag Entity { get; }
        public int Id => Entity.rowid;
        public int Version => Entity.Version;
        public SavableProperty<double> KaltMiete;
        public SavableProperty<int> Personenzahl;
        public SavableProperty<WohnungListViewModelEntry?> Wohnung;
        public SavableProperty<DateTimeOffset> Beginn;
        public SavableProperty<DateTimeOffset?> Ende;
        public SavableProperty<string> Notiz;
        public SavableProperty<KontaktListViewModelEntry> Ansprechpartner { get; }

        public KontaktListViewModelEntry Vermieter
            => Wohnung.Value?.Entity?.BesitzerId is Guid g && g != Guid.Empty ?
                    new KontaktListViewModelEntry(g, Db) : null;

        protected IWalterDbService Db;
        protected INotificationService NotificationService;

        public RelayCommand RemoveDate;
        public RelayCommand Save { get; protected set; }
        public AsyncRelayCommand Delete { get; protected set; }

        public VertragDetailViewModelVersion(int id, INotificationService ns, IWalterDbService db) : this(db.ctx.Vertraege.Find(id), ns, db) { }
        public VertragDetailViewModelVersion(Vertrag v, INotificationService ns, IWalterDbService db)
        {
            Entity = v;
            Db = db;
            NotificationService = ns;

            KaltMiete = new(this, v.KaltMiete);
            Personenzahl = new(this, v.Personenzahl);
            Wohnung = new(this, new(v.Wohnung, db));
            Beginn = new(this, v.Beginn);
            Ende = new(this, v.Ende);
            Notiz = new(this, v.Notiz);

            if (v.AnsprechpartnerId != Guid.Empty && v.AnsprechpartnerId != null)
            {
                Ansprechpartner = new(this, new(v.AnsprechpartnerId.Value, db));
            }
            else
            {
                Ansprechpartner = new(this, null);
            }

            RemoveDate = new RelayCommand(_ => Ende = null, _ => Ende != null);
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Db.ctx.Vertraege.Remove(Entity);
                    Db.SaveWalter();
                }
            }, _ => true);
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
                Db.ctx.Vertraege.Update(Entity);
            }
            else
            {
                Db.ctx.Vertraege.Add(Entity);
            }
        }
    }

}
