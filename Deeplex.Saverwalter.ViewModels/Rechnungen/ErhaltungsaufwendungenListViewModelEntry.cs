using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenListViewModelEntry
    {
        public readonly Erhaltungsaufwendung Entity;
        public ObservableProperty<bool> Enabled = new ObservableProperty<bool>(true);
        public string Aussteller => Avm.ctx.FindPerson(Entity.AusstellerId).Bezeichnung;
        public int Id => Entity.ErhaltungsaufwendungId;
        public WohnungListViewModelEntry Wohnung;
        public string Bezeichnung => Entity.Bezeichnung;
        public double Betrag => Entity.Betrag;
        public DateTime Datum => Entity.Datum;
        private AppViewModel Avm;

        public ErhaltungsaufwendungenListViewModelEntry(Erhaltungsaufwendung e, AppViewModel avm)
        {
            Entity = e;
            Avm = avm;
            Wohnung = new WohnungListViewModelEntry(Entity.Wohnung, Avm);
        }
    }
}
