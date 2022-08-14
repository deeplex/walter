using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Print;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenrechnungPrintViewModel : BindableBase, IPrintViewModel
    {
        public bool HasNotes => Betriebskostenabrechnung.Value.notes.Count() > 0;

        private int mJahr { get; set; }
        public int Jahr
        {
            get => mJahr;
            set
            {
                mJahr = value;
                Betriebskostenabrechnung.Value =
                new Betriebskostenabrechnung(
                    WalterDbService.ctx,
                    Entity.rowid,
                    Jahr,
                    new DateTime(Jahr, 1, 1),
                    new DateTime(Jahr, 12, 31));
            }
        }
        public ObservableProperty<Betriebskostenabrechnung> Betriebskostenabrechnung { get; private set; } = new();
        public IPerson FirstMieter => Betriebskostenabrechnung.Value.Mieter.First();
        public DateTime Today => DateTime.Now;
        public Vertrag Entity { get; private set; }

        public AsyncRelayCommand Print { get; }
        public IWalterDbService WalterDbService { get; }
        public IFileService FileService { get; }

        public BetriebskostenrechnungPrintViewModel(IWalterDbService db, IFileService fs)
        {
            WalterDbService = db;
            FileService = fs;

            Print = new AsyncRelayCommand(async _ =>
            {
                try
                {
                    await Files.PrintBetriebskostenabrechnung(Entity, Jahr, db, fs);
                }
                catch (Exception)
                {
                    // TODO Show that on call
                    // impl.ShowAlert(ex.Message);
                }
            }, _ => true);
        }

        public void SetEntity(Vertrag v, int year)
        {
            Entity = v;
            Jahr = year;
        }
    }
}
