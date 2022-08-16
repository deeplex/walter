using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragListViewModelEntry : VertragListViewModelEntryVersion
    {
        public List<VertragListViewModelEntryVersion> Versionen { get; } = new();

        public MieteListViewModel Mieten { get; set; }
        public string LastMiete
        {
            get
            {
                var mieten = Mieten.List.Value.OrderBy(m => m.Zahlungsdatum.Value);
                var last = mieten.Any() ? mieten.Last() : null;
                return last != null ? last.Zahlungsdatum.Value.ToString("dd.MM.yyyy") +
                    " - Betrag: " + string.Format("{0:F2}€", last.Betrag) : "";
            }
        }
        public bool HasLastMiete => LastMiete != "";

        public VertragListViewModelEntry(IGrouping<Guid, Vertrag> v, IWalterDbService db, INotificationService ns)
            : base(v.OrderBy(vs => vs.Version).Last(), db)
        {
            Versionen = v.OrderBy(vs => vs.Version).Select(vs => new VertragListViewModelEntryVersion(vs, db)).ToList();
            Beginn = Versionen.First().Beginn;

            Mieten = new MieteListViewModel(v.Key, ns, db);

            AddMiete = new RelayCommand(_ =>
            {
                Mieten.Add.Execute(null);
                db.ctx.Mieten.Add(Mieten.List.Value.Last().Entity);
                db.SaveWalter();
                RaisePropertyChanged(nameof(LastMiete));
                RaisePropertyChanged(nameof(HasLastMiete));
            }, _ => true);
        }

        public RelayCommand AddMiete { get; }
    }
}
