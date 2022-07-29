using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenrechnungPrintViewModel : IPrint
    {
        public bool HasNotes => Betriebskostenabrechnung.notes.Count() > 0;

        public ObservableProperty<int> Jahr { get; } = new();
        public Betriebskostenabrechnung Betriebskostenabrechnung { get; }
        public IPerson FirstMieter => Betriebskostenabrechnung.Mieter.First();
        public DateTime Today => DateTime.Now;
        public Vertrag Entity { get; }

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
        public BetriebskostenrechnungPrintViewModel(Vertrag v, IWalterDbService db, IFileService fs)
        {
            Entity = v;
            Jahr.Value = DateTime.Now.Year - 1;
            Betriebskostenabrechnung = new Betriebskostenabrechnung(
                db.ctx,
                v.rowid,
                Jahr.Value,
                new DateTime(Jahr.Value, 1, 1),
                new DateTime(Jahr.Value, 12, 31));


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
    }
}
