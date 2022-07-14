using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenListViewModelEntry
    {
        public Erhaltungsaufwendung Entity { get; }
        public ObservableProperty<bool> Enabled = new(true);
        public string Aussteller => Db.ctx.FindPerson(Entity.AusstellerId).Bezeichnung;
        public int Id => Entity.ErhaltungsaufwendungId;
        public WohnungListViewModelEntry Wohnung;
        public string Bezeichnung => Entity.Bezeichnung;
        public double Betrag => Entity.Betrag;
        public DateTime Datum => Entity.Datum;
        private IWalterDbService Db;

        public ErhaltungsaufwendungenListViewModelEntry(Erhaltungsaufwendung e, IWalterDbService db)
        {
            Entity = e;
            Db = db;
            Wohnung = new WohnungListViewModelEntry(Entity.Wohnung, Db);
        }
    }
}
