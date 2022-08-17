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

        // Used to determine if delete button is shown. Set in injected MemberViewModel
        public bool Deletable { get; set; }

        protected override void updateList()
        {
            List.Value = AllRelevant.Where(v => applyFilter(v.ToString(), v.Email, v.Telefon)).ToImmutableList();
        }

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

        public override void SetList()
        {
            AllRelevant = transform(includeNP(), includeJP());
            updateList();
        }

        public void SetList(Vertrag v)
        {
            if (v == null) return;

            var mieter = WalterDbService.ctx.MieterSet.Where(m => m.Vertrag.VertragId == v.VertragId).Select(m => m.PersonId).ToList();
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

            AllRelevant = transform(np, jp);
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(JuristischePerson jp)
        {
            if (jp == null) return;

            AllRelevant = transform(
                includeNP()
                    .Where(e => jp.NatuerlicheMitglieder.Exists(f => f.PersonId == e.PersonId))
                    .ToList(),
                includeJP()
                    .Where(e => jp.JuristischeMitglieder.Exists(f => f.PersonId == e.PersonId))
                    .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(IPerson p)
        {
            if (p == null) return;

            AllRelevant = transform(
                new List<NatuerlichePerson> { },
                includeJP()
                    .Where(e => p.JuristischePersonen.Exists(f => f.PersonId == e.PersonId))
                    .ToList());
            List.Value = AllRelevant.ToImmutableList();
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
                np.Select(p => new KontaktListViewModelEntry(p))
             .Concat(
                jp.Select(p => new KontaktListViewModelEntry(p)))
             .ToImmutableList();
        }

    }
}
