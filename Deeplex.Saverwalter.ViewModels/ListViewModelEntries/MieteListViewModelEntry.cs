using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MieteListViewModelEntry : DetailViewModel<Miete>, IDetailViewModel
    {
        public override string ToString() => Betrag + "€";

        public SavableProperty<double> Betrag { get; private set; }
        public SavableProperty<DateTimeOffset> Zahlungsdatum { get; private set; }
        public SavableProperty<DateTimeOffset> BetreffenderMonat { get; private set; }
        public SavableProperty<string> Notiz { get; private set; }

        public MieteListViewModelEntry(Miete m, MieteListViewModel vm) : base(vm.NotificationService, vm.WalterDbService)
        {
            SetEntity(m);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await delete())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                };
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public new void save()
        {
            Entity.Betrag = Betrag.Value;
            Entity.BetreffenderMonat = BetreffenderMonat.Value.UtcDateTime;
            Entity.Zahlungsdatum = Zahlungsdatum.Value.UtcDateTime;
            Entity.Notiz = Notiz.Value;

            base.save();
        }

        public override void SetEntity(Miete m)
        {
            Entity = m;

            Betrag = new(this, m.Betrag ?? 0);
            Zahlungsdatum = new(this, m.Zahlungsdatum);
            BetreffenderMonat = new(this, m.BetreffenderMonat);
            Notiz = new(this, m.Notiz);
        }



        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Betrag != Betrag.Value ||
                Entity.BetreffenderMonat.AsUtcKind() != BetreffenderMonat.Value ||
                Entity.Zahlungsdatum.AsUtcKind() != Zahlungsdatum.Value ||
                Entity.Notiz != Notiz.Value;
        }
    }
}
