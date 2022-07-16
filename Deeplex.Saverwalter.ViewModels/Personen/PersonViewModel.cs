using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class PersonViewModel : BindableBase
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
            Email.Value = p.Email;
            Telefon.Value = p.Telefon;
            Mobil.Value = p.Mobil;
            Fax.Value = p.Fax;
            Notiz.Value = p.Notiz;
            isHandwerker.Value = p.isHandwerker;
            isMieter.Value = p.isMieter;
            isVermieter.Value = p.isVermieter;

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
        public ObservableProperty<string> Notiz = new();
        public ObservableProperty<bool> isVermieter = new();
        public ObservableProperty<bool> isMieter = new();
        public ObservableProperty<bool> isHandwerker = new();
        public ObservableProperty<string> Email = new();
        public ObservableProperty<string> Telefon = new();
        public ObservableProperty<string> Mobil = new();
        public ObservableProperty<string> Fax = new();

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
