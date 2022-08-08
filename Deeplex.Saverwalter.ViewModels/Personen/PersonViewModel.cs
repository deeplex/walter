using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class PersonViewModel : BindableBase, IDetailViewModel<IPerson>
    {
        public IPerson Entity { get; set; }

        protected bool mInklusiveZusatz;
        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Wohnungen = new();

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }

        public RelayCommand Save { get; protected set; }
        public AsyncRelayCommand Delete { get; protected set; }

        public void SetEntity(IPerson p)
        {
            Email = new(this, p.Email);
            Telefon = new(this, p.Telefon);
            Mobil = new(this, p.Mobil);
            Fax = new(this, p.Fax);
            Notiz = new(this, p.Notiz);
            isHandwerker = new(this, p.isHandwerker);
            isMieter = new(this, p.isMieter);
            isVermieter = new(this, p.isVermieter);
        }


        public PersonViewModel(INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            NotificationService = ns;
        }

        protected void save()
        {
            Entity.Email = Email.Value;
            Entity.Telefon = Telefon.Value;
            Entity.Mobil = Mobil.Value;
            Entity.Fax = Fax.Value;
            Entity.Notiz = Notiz.Value;
            Entity.isHandwerker = isHandwerker.Value;
            Entity.isMieter = isMieter.Value;
            Entity.isVermieter = isVermieter.Value;
        }

        public Guid PersonId;
        public SavableProperty<string, IPerson> Notiz { get; private set; }
        public SavableProperty<bool, IPerson> isVermieter { get; private set; }
        public SavableProperty<bool, IPerson> isMieter { get; private set; }
        public SavableProperty<bool, IPerson> isHandwerker { get; private set; }
        public SavableProperty<string, IPerson> Email { get; private set; }
        public SavableProperty<string, IPerson> Telefon { get; private set; }
        public SavableProperty<string, IPerson> Mobil { get; private set; }
        public SavableProperty<string, IPerson> Fax { get; private set; }

        public int AdresseId => Entity.AdresseId ?? 0;

        public abstract void checkForChanges();
        protected bool BaseCheckForChanges() =>
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
