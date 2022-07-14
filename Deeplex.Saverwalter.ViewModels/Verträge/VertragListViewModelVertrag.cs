using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragListViewModelVertrag : VertragListViewModelVertragVersion
    {
        public List<VertragListViewModelVertragVersion> Versionen { get; }
            = new List<VertragListViewModelVertragVersion>();
        public ObservableProperty<VertragListViewModelMiete> AddMieteValue = new();

        public ImmutableList<VertragListViewModelMiete> Mieten { get; set; }
        public string LastMiete
        {
            get
            {
                var mieten = Mieten.OrderBy(m => m.Datum.Value);
                var last = mieten.Any() ? mieten.Last() : null;
                return last != null ? last.Datum.Value.ToString("dd.MM.yyyy") +
                    " - Betrag: " + string.Format("{0:F2}€", last.Betrag) : "";
            }
        }
        public bool HasLastMiete => LastMiete != "";

        public VertragListViewModelVertrag(IGrouping<Guid, Vertrag> v, IWalterDbService db)
            : base(v.OrderBy(vs => vs.Version).Last(), db)
        {
            Versionen = v.OrderBy(vs => vs.Version).Select(vs => new VertragListViewModelVertragVersion(vs, db)).ToList();
            Beginn = Versionen.First().Beginn;

            Mieten = db.ctx.Mieten
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new VertragListViewModelMiete(m))
                .ToImmutableList();

            AddMieteValue.Value = new VertragListViewModelMiete();
            AddMiete = new RelayCommand(_ =>
            {
                Mieten = Mieten.Add(AddMieteValue.Value);
                db.ctx.Mieten.Add(new Miete
                {
                    Zahlungsdatum = AddMieteValue.Value.Datum.Value.UtcDateTime,
                    BetreffenderMonat = AddMieteValue.Value.BetreffenderMonat.Value.UtcDateTime,
                    Betrag = AddMieteValue.Value.Betrag,
                    VertragId = Versionen.Last().VertragId,
                });
                db.SaveWalter();
                AddMieteValue.Value = new VertragListViewModelMiete();
                RaisePropertyChanged(nameof(LastMiete));
                RaisePropertyChanged(nameof(HasLastMiete));
            }, _ => true);
        }

        public RelayCommand AddMiete { get; }
    }
}
