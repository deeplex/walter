using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class PersonViewModel : BindableBase, ISingleItem
    {
        public IPerson Entity { get; set; }

        protected bool mInklusiveZusatz;
        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Wohnungen = new();

        protected IWalterDbService Db;
        protected INotificationService NotificationService;

        public RelayCommand Save { get; protected set; }
        public AsyncRelayCommand Delete { get; protected set; }

        public PersonViewModel(IPerson p, INotificationService ns, IWalterDbService db)
        {
            Email = new(this, p.Email);
            Telefon = new(this, p.Telefon);
            Mobil = new(this, p.Mobil);
            Fax = new(this, p.Fax);
            Notiz = new(this, p.Notiz);
            isHandwerker = new(this, p.isHandwerker);
            isMieter = new(this, p.isMieter);
            isVermieter = new(this, p.isVermieter);

            Db = db;
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
        public SavableProperty<string> Notiz { get; }
        public SavableProperty<bool> isVermieter { get; }
        public SavableProperty<bool> isMieter { get; }
        public SavableProperty<bool> isHandwerker { get; }
        public SavableProperty<string> Email { get; }
        public SavableProperty<string> Telefon { get; }
        public SavableProperty<string> Mobil { get; }
        public SavableProperty<string> Fax { get; }

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
