using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class IPersonDetailViewModel : DetailViewModel<IPerson>, IDetailViewModel
    {
        protected bool mInklusiveZusatz;
        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Wohnungen = new();

        public override void SetEntity(IPerson p)
        {
            Entity = p;
            Email = new(this, p.Email);
            Bezeichnung = new(this, p.Bezeichnung);
            Telefon = new(this, p.Telefon);
            Mobil = new(this, p.Mobil);
            Fax = new(this, p.Fax);
            Notiz = new(this, p.Notiz);
            isHandwerker = new(this, p.isHandwerker);
            isMieter = new(this, p.isMieter);
            isVermieter = new(this, p.isVermieter);
        }


        public IPersonDetailViewModel(INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            NotificationService = ns;
        }

        protected new void save()
        {
            Entity.Email = Email.Value;
            Entity.Telefon = Telefon.Value;
            Entity.Mobil = Mobil.Value;
            Entity.Fax = Fax.Value;
            Entity.Notiz = Notiz.Value;
            Entity.isHandwerker = isHandwerker.Value;
            Entity.isMieter = isMieter.Value;
            Entity.isVermieter = isVermieter.Value;

            base.save();
        }

        public Guid PersonId;
        public override string ToString() => Bezeichnung?.Value ?? "Neue Person";

        public SavableProperty<string> Bezeichnung { get; private set; }
        public SavableProperty<string> Notiz { get; private set; }
        public SavableProperty<bool> isVermieter { get; private set; }
        public SavableProperty<bool> isMieter { get; private set; }
        public SavableProperty<bool> isHandwerker { get; private set; }
        public SavableProperty<string> Email { get; private set; }
        public SavableProperty<string> Telefon { get; private set; }
        public SavableProperty<string> Mobil { get; private set; }
        public SavableProperty<string> Fax { get; private set; }

        public int AdresseId => Entity.AdresseId ?? 0;

        public abstract override void checkForChanges();
        protected bool BaseCheckForChanges() =>
            Entity.Bezeichnung != Bezeichnung.Value ||
            Entity.Notiz != Notiz.Value ||
            Entity.isVermieter != isVermieter.Value ||
            Entity.isMieter != isMieter.Value ||
            Entity.isHandwerker != isHandwerker.Value ||
            Entity.Email != Email.Value ||
            Entity.Telefon != Telefon.Value ||
            Entity.Mobil != Mobil.Value ||
            Entity.Fax != Fax.Value;
    }
}
