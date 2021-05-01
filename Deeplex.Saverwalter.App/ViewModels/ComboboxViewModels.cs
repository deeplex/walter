using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class WohnungCombobox
    {
        public override string ToString() => Anschrift;

        internal Wohnung Entity;
        internal int Id => Entity.WohnungId;
        private string Anschrift { get; }

        public WohnungCombobox(Wohnung w)
        {
            Entity = w;
            Anschrift = AdresseViewModel.Anschrift(w) + ", " + w.Bezeichnung;
        }
    }
}
