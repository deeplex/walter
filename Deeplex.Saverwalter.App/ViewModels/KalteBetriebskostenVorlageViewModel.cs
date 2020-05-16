using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KalteBetriebskostenVorlageViewModel
    {
        public ObservableProperty<List<KalteBetriebskostenVorlageKostenpunkt>> Punkte { get; set; }
            = new ObservableProperty<List<KalteBetriebskostenVorlageKostenpunkt>>();

        public List<KalteBetriebskostenUmlageSchluessel> AlleSchluessel { get; set; }
        public string Anschrift;

        public KalteBetriebskostenVorlageViewModel(int id)
            : this(App.Walter.Adressen.Find(id)) { }
        public KalteBetriebskostenVorlageViewModel(Adresse a)
        {           
            AlleSchluessel = Enum.GetValues(typeof(UmlageSchluessel)).Cast<UmlageSchluessel>()
                .Select(k => new KalteBetriebskostenUmlageSchluessel
                {
                    Schluessel = k,
                    Bezeichnung = k.ToDescriptionString(),
                }).ToList();

            Anschrift = AdresseViewModel.Anschrift(a);
            var enums = Enum.GetValues(typeof(KalteBetriebskosten)).Cast<KalteBetriebskosten>().ToList();
            Punkte.Value = enums.Select(e => new KalteBetriebskostenVorlageKostenpunkt(a, e)).ToList();
        }
    }

    public class KalteBetriebskostenVorlageKostenpunkt
    {
        public string Bezeichnung { get; }
        public ObservableProperty<bool> Active { get; } = new ObservableProperty<bool>();
        public UmlageSchluessel Schluessel { get; set; }
        public ObservableProperty<string> Beschreibung { get; } = new ObservableProperty<string>();

        public string BeschreibungKurz => Beschreibung.Value.Length > 20 ?
            Beschreibung.Value.Substring(0, 30) + "…" :
            Beschreibung.Value;

        public KalteBetriebskostenVorlageKostenpunkt(Adresse a, KalteBetriebskosten e)
        {
            Bezeichnung = e.ToDescriptionString();


            var pt = a.KalteBetriebskosten.FirstOrDefault(k => k.Typ == e);
            Active.Value = pt is KalteBetriebskostenpunkt p;
            Schluessel = pt?.Schluessel ?? UmlageSchluessel.NachWohnflaeche;
            Beschreibung.Value = pt?.Beschreibung ?? "";
        }
    }

    public class KalteBetriebskostenUmlageSchluessel
    {
        public UmlageSchluessel Schluessel { get; set; }
        public string Bezeichnung { get; set; }
    }
}
