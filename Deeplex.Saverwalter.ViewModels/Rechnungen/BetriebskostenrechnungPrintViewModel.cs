using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenrechnungPrintViewModel : IPrintViewModel
    {
        public bool HasNotes => Betriebskostenabrechnung.notes.Count() > 0;

        public ObservableProperty<int> Jahr { get; } = new();
        public Betriebskostenabrechnung Betriebskostenabrechnung { get; private set; }
        public IPerson FirstMieter => Betriebskostenabrechnung.Mieter.First();
        public DateTime Today => DateTime.Now;
        public Vertrag Entity { get; private set; }

        public string Title => Betriebskostenabrechnung.Title();
        public string Mieterliste => Betriebskostenabrechnung.Mieterliste();
        public string Mietobjekt => Betriebskostenabrechnung.Mietobjekt();
        public string Abrechnungszeitraum => Betriebskostenabrechnung.Abrechnungszeitraum();
        public string Nutzungszeitraum => Betriebskostenabrechnung.Nutzungszeitraum();
        public string Gruss => Betriebskostenabrechnung.Gruss();
        public string ResultTxt => Betriebskostenabrechnung.ResultTxt();
        public string Result => Betriebskostenabrechnung.Result.ToString() + "€"; // TODO
        public string RefundDemand => Betriebskostenabrechnung.RefundDemand();
        public string GenerischerText => Betriebskostenabrechnung.GenerischerText();
        public bool dir => Betriebskostenabrechnung.dir();
        public bool nWF => Betriebskostenabrechnung.nWF();
        public bool nNF => Betriebskostenabrechnung.nNF();
        public bool nNE => Betriebskostenabrechnung.nNE();
        public bool nPZ => Betriebskostenabrechnung.nPZ();
        public bool nVb => Betriebskostenabrechnung.nVb();
        public string Anmerkung => Betriebskostenabrechnung.Anmerkung();

        public AsyncRelayCommand Print { get; }
        public IWalterDbService WalterDbService { get; }

        public BetriebskostenrechnungPrintViewModel(Vertrag v, IWalterDbService db, IFileService fs)
        {
            Jahr.Value = DateTime.Now.Year - 1;



            Print = new AsyncRelayCommand(async _ =>
            {
                try
                {
                    await Files.PrintBetriebskostenabrechnung(
                        Entity, Jahr.Value, db, fs);
                }
                catch (Exception ex)
                {
                    // TODO Show that on call
                    // impl.ShowAlert(ex.Message);
                }
            }, _ => true);
        }

        public void SetEntity(Vertrag v)
        {
            Entity = v;
            Betriebskostenabrechnung = new Betriebskostenabrechnung(
                WalterDbService.ctx,
                Entity.rowid,
                Jahr.Value,
                new DateTime(Jahr.Value, 1, 1),
                new DateTime(Jahr.Value, 12, 31));
        }
    }
}
