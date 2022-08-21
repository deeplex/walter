using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragListViewModelEntry : BindableBase
    {
        public Vertrag Entity { get; }
        public IWalterDbService WalterDbService { get; }

        public DateTimeOffset? Ende => Entity.Ende?.AsUtcKind();
        public DateTimeOffset Beginn => Entity.Beginn().AsUtcKind();

        public string Anschrift => AdresseViewModel.Anschrift(Entity.Wohnung);
        public string AnschriftMitWohnung => Anschrift + ", " + Wohnung.Bezeichnung;
        public Wohnung Wohnung => Entity.Wohnung;
        public ImmutableList<Guid> Mieter => WalterDbService.ctx.MieterSet
            .Where(w => w.Vertrag.VertragId == Entity.VertragId)
            .Select(m => m.PersonId).ToImmutableList();

        public string AuflistungMieter => string.Join(", ", WalterDbService.ctx.MieterSet
            .Where(m => m.Vertrag.VertragId == Entity.VertragId).ToList()
            .Select(a => WalterDbService.ctx.FindPerson(a.PersonId).Bezeichnung));
        public string Besitzer => WalterDbService.ctx.FindPerson(Entity.Wohnung.BesitzerId)?.Bezeichnung;

        public MieteListViewModel Mieten { get; set; }
        public string LastMiete
        {
            get
            {
                // TODO Vertrag can have mieten
                var mieten = Mieten.List.Value.OrderBy(m => m.Zahlungsdatum.Value);
                var last = mieten.Any() ? mieten.Last() : null;
                return last != null ? last.Zahlungsdatum.Value.ToString("dd.MM.yyyy") +
                    " - Betrag: " + string.Format("{0:F2}€", last.Betrag) : "";
            }
        }
        public bool HasLastMiete => LastMiete != "";

        public VertragListViewModelEntry(Vertrag v, IWalterDbService db, INotificationService ns)
        {
            Entity = v;

            WalterDbService = db;

            Mieten = new MieteListViewModel(v, ns, db);

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
