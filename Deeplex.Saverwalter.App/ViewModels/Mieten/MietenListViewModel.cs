using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class MietenListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<MietenListEntry>> Liste
            = new ObservableProperty<ImmutableList<MietenListEntry>>();
        public Guid VertragId;

        public MietenListViewModel(Guid VertragGuid)
        {
            VertragId = VertragGuid;
            var self = this;
            Liste.Value = App.Walter.Mieten
                .Where(m => m.VertragId == VertragGuid)
                .Select(m => new MietenListEntry(m, self))
                .ToImmutableList();
        }

        public void AddToList(Miete z)
        {
            Liste.Value = Liste.Value.Add(new MietenListEntry(z, this));
        }
    }

    public sealed class MietenListEntry
    {
        public Miete Entity { get; }
        public string BetragString => Entity.Betrag.ToString() + "€";
        public string DatumString => Entity.Zahlungsdatum.ToString("dd.MM.yyyy");
        public string MonatString => Entity.BetreffenderMonat.Month.ToString();
        public string Notiz => Entity.Notiz;

        public MietenListEntry(Miete m, MietenListViewModel vm)
        {
            Entity = m;

            SelfDestruct = new RelayCommand(_ =>
            {
                vm.Liste.Value = vm.Liste.Value.Remove(this);
                App.Walter.Mieten.Remove(Entity);
                App.SaveWalter();
            }, _ => true);
        }
        public RelayCommand SelfDestruct;
    }
}
