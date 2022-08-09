using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class KontaktListViewModel : ListViewModel<KontaktListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Kontakte";

        protected override ImmutableList<KontaktListViewModelEntry> updateList(string value)
            => AllRelevant.Where(v => applyFilter(value, v.ToString(), v.Email, v.Telefon)).ToImmutableList();

        public IWalterDbService WalterDbService { get; }
        public INotificationService NotificationService { get; }

        public KontaktListViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;

            Navigate = new RelayCommand(el =>
            {
                if (el is NatuerlichePerson n)
                {
                    ns.Navigation(n);
                }
                else if (el is JuristischePerson j)
                {
                    ns.Navigation(j);
                }
                else if (el == null)
                {
                    ns.Navigation((NatuerlichePerson)el);
                }
            }, _ => true);
        }

        public void SetList()
        {
            AllRelevant = transform(WalterDbService, includeNP(WalterDbService), includeJP(WalterDbService));
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Vertrag v)
        {
            var mieter = WalterDbService.ctx.MieterSet.Where(m => m.VertragId == v.VertragId).Select(m => m.PersonId).ToList();
            var np = new List<NatuerlichePerson> { };
            var jp = new List<JuristischePerson> { };

            mieter.ForEach(e =>
            {
                var a = WalterDbService.ctx.FindPerson(e);
                if (a is NatuerlichePerson n)
                {
                    np.Add(n);
                }
                else if (a is JuristischePerson j)
                {
                    jp.Add(j);
                }
            });

            AllRelevant = transform(WalterDbService, np, jp);
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(JuristischePerson jp)
        {
            AllRelevant = transform(WalterDbService, jp.NatuerlicheMitglieder, jp.JuristischeMitglieder);
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(IPerson jp)
        {
            AllRelevant = transform(WalterDbService, new List<NatuerlichePerson> {}, jp.JuristischePersonen);
            List.Value = AllRelevant.ToImmutableList();
        }

        private List<NatuerlichePerson> includeNP(IWalterDbService db)
        {
            return db.ctx.NatuerlichePersonen
                .Include(k => k.Anhaenge)
                .Include(k => k.Adresse)
                .ThenInclude(a => a.Anhaenge)
                .ToList();
        }
        private List<JuristischePerson> includeJP(IWalterDbService db)
        {
            return db.ctx.JuristischePersonen
                .Include(j => j.Anhaenge)
                .Include(j => j.Adresse)
                .ThenInclude(a => a.Anhaenge)
                .ToList();
        }

        private ImmutableList<KontaktListViewModelEntry> transform(IWalterDbService db, List<NatuerlichePerson> np, List<JuristischePerson> jp)
        {
            return
                np.Select(p => new KontaktListViewModelEntry(p))
             .Concat(
                jp.Select(p => new KontaktListViewModelEntry(p)))
             .ToImmutableList();
        }

    }
}
