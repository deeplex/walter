using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MemberViewModel<T> : BindableBase, IMemberListViewModel
    {
        public override string ToString() => "Personen";

        public T Entity;
        private bool OnlyJuristische { get; set; }

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }

        public RelayCommand Add { get; }

        public KontaktListViewModelEntry Selected { get; set; }
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> List { get; private set; } = new();

        public MemberViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;

            Add = new RelayCommand(_ =>
            {
                if (Entity is JuristischePerson j && OnlyJuristische)
                {
                    j.Mitglieder.Add(Selected.Entity);
                }
                else if (Entity is IPerson n && Selected.Entity is JuristischePerson jj)
                {
                    n.JuristischePersonen.Add(jj);
                }
                else if (Entity is Vertrag v)
                {
                    WalterDbService.ctx.MieterSet.Add(new Mieter
                    {
                        PersonId = Selected.Entity.PersonId,
                        VertragId = v.VertragId,
                    });
                }
                // TODO add the added thing to the corresponding List... 
            }, _ => true);
        }

        // If onlyJuristische is selected, then we want to add a JuristischePerson to a Juristische, or NatuerlichePerson.
        // If it is false we want to add a Mitglied to a JuristischePerson
        public void SetList(T e, bool onlyJuristische = false)
        {
            Entity = e;
            OnlyJuristische = onlyJuristische;

            if (onlyJuristische)
            {
                List.Value = transform(new List<NatuerlichePerson> { }, includeJP());
            }
            else
            {
                List.Value = transform(includeNP(), includeJP());
            }
        }

        private List<NatuerlichePerson> includeNP()
        {
            return WalterDbService.ctx.NatuerlichePersonen
                .Include(k => k.Anhaenge)
                .Include(k => k.Adresse)
                .ThenInclude(a => a.Anhaenge)
                .ToList();
        }
        private List<JuristischePerson> includeJP()
        {
            return WalterDbService.ctx.JuristischePersonen
                .Include(j => j.Anhaenge)
                .Include(j => j.Adresse)
                .ThenInclude(a => a.Anhaenge)
                .ToList();
        }

        private ImmutableList<KontaktListViewModelEntry> transform(List<NatuerlichePerson> np, List<JuristischePerson> jp)
        {
            return
                np.Select(p => new KontaktListViewModelEntry(this, p))
             .Concat(
                jp.Select(p => new KontaktListViewModelEntry(this, p)))
             .ToImmutableList();
        }
    }

    public interface IMemberListViewModel
    {
        RelayCommand Add { get; }
        IWalterDbService WalterDbService { get; }
        INotificationService NotificationService { get; }
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> List { get; }
        public KontaktListViewModelEntry Selected { get; set; }
    }
}
