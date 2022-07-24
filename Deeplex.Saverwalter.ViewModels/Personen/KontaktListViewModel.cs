using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class KontaktListViewModel : ListViewModel<KontaktListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Kontakte";

        protected override ImmutableList<KontaktListViewModelEntry> updateList(string value)
            => List.Value.Where(v => applyFilter(value, v.Name, v.Vorname, v.Email, v.Telefon)).ToImmutableList();

        public KontaktListViewModel(IWalterDbService db, INotificationService ns)
        {
            Add = new RelayCommand(_ => ns.Navigation<NatuerlichePerson>(null), _ => true);

            AllRelevant = db.ctx.NatuerlichePersonen
                .Include(k => k.Anhaenge)
                .Include(k => k.Adresse).ThenInclude(a => a.Anhaenge)
                .Select(k => new KontaktListViewModelEntry(k)).ToImmutableList();

            var jp = db.ctx.JuristischePersonen
                .Include(j => j.Anhaenge)
                .Include(j => j.Adresse).ThenInclude(a => a.Anhaenge);
            foreach (var j in jp)
            {
                AllRelevant = AllRelevant.Add(new KontaktListViewModelEntry(j));
            }

            List.Value = AllRelevant;
        }
    }
}
