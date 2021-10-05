using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels.Utils;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenrechnungPrintViewModel
    {
        public ObservableProperty<int> Jahr = new ObservableProperty<int>();
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

        public AsyncRelayCommand Print;
        public BetriebskostenrechnungPrintViewModel(Vertrag v, AppViewModel avm, IAppImplementation impl)
        {
            Entity = v;
            Jahr.Value = DateTime.Now.Year - 1;
            Betriebskostenabrechnung = new Betriebskostenabrechnung(
                avm.ctx,
                v.rowid,
                Jahr.Value,
                new DateTime(Jahr.Value, 1, 1),
                new DateTime(Jahr.Value, 12, 31));


            Print = new AsyncRelayCommand(async _ =>
            {
                try
                {
                    await Files.PrintBetriebskostenabrechnung(
                        Entity, Jahr.Value, avm, impl);
                }
                catch (Exception ex)
                {
                    // TODO Better error handling
                    impl.ShowAlert(ex.Message);
                }
            }, _ => true);
        }
    }
}
