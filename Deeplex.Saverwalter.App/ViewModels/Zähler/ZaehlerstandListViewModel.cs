using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class ZaehlerstandListViewModel
    {
        public ObservableProperty<List<ZaehlerstandListEntry>> Liste = new ObservableProperty<List<ZaehlerstandListEntry>>();

        public ZaehlerstandListViewModel(Zaehler z)
        {
            Liste.Value = z.Staende.Select(s => new ZaehlerstandListEntry(s)).ToList();
        }

        public class ZaehlerstandListEntry
        {
            private Zaehlerstand Entity;
            public int Id => Entity.ZaehlerstandId;
            public double Stand => Entity.Stand;
            public string StandString => Entity.Stand.ToString();
            public DateTimeOffset Datum => Entity.Datum;
            public string DatumString => Datum.ToString("dd.MM.yyyy");

            public ZaehlerstandListEntry(Zaehlerstand z)
            {
                Entity = z;
            }
        }
    }
}
