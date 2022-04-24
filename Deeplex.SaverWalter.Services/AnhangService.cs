using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Services
{
    public interface IAnhangService<AnhangListViewModel> where AnhangListViewModel: class
    {
        ObservableProperty<AnhangListViewModel> ListAnhang { get; }
        ObservableProperty<AnhangListViewModel> DetailAnhang { get; }

        void clearAnhang();
        void updateListAnhang(AnhangListViewModel list);
        void updateDetailAnhang(AnhangListViewModel detail);
    }

    public sealed class AnhangService<AnhangListViewModel> : IAnhangService<AnhangListViewModel> where AnhangListViewModel: class
    {
        public ObservableProperty<AnhangListViewModel> DetailAnhang { get; } = new();
        public ObservableProperty<AnhangListViewModel> ListAnhang { get; } = new();

        public void clearAnhang()
        {
            updateListAnhang(null);
            updateDetailAnhang(null);
        }
        public void updateListAnhang(AnhangListViewModel list) => updateAnhang(ListAnhang, list);
        public void updateDetailAnhang(AnhangListViewModel detail) => updateAnhang(DetailAnhang, detail);
        private void updateAnhang(ObservableProperty<AnhangListViewModel> op, AnhangListViewModel a)
        {
            op.Value = a;
        }

    }

}
