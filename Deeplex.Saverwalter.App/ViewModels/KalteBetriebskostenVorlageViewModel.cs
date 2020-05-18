using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
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
            : this(App.Walter.Adressen.Include(a => a.KalteBetriebskosten).First(a => a.AdresseId == id)) { }
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

            SaveEdit = new RelayCommand(_ =>
            {
                foreach (var p in Punkte.Value)
                {
                    var kbkp = a.KalteBetriebskosten.FirstOrDefault(k => k.Typ == p.Typ);
                    if (kbkp != null)
                    {
                        if (!p.Active.Value)
                        {
                            App.Walter.KalteBetriebskosten.Remove(kbkp);
                        }
                        else
                        {
                            kbkp.Beschreibung = p.Beschreibung;
                            kbkp.Schluessel = p.Schluessel;
                            App.Walter.KalteBetriebskosten.Update(kbkp);
                        }
                    }
                    else if (p.Active.Value)
                    {
                        App.Walter.KalteBetriebskosten.Add(new KalteBetriebskostenpunkt
                        {
                            Adresse = a,
                            Schluessel = p.Schluessel,
                            Beschreibung = p.Beschreibung,
                            Typ = p.Typ,
                        });
                    }
                    App.Walter.SaveChanges();
                };
            });
        }

        public RelayCommand SaveEdit { get; }
    }

    public class KalteBetriebskostenVorlageKostenpunkt : BindableBase
    {
        public KalteBetriebskosten Typ { get; }
        public ObservableProperty<bool> Active { get; } = new ObservableProperty<bool>();
        public UmlageSchluessel Schluessel { get; set; }


        private string mBeschreibung;
        public string Beschreibung
        {
            get => mBeschreibung;
            set
            {
                SetProperty(ref mBeschreibung, value);
                RaisePropertyChanged(nameof(BeschreibungKurz));
            }
        }

        public string Bezeichnung => Typ.ToDescriptionString();

        public string BeschreibungKurz => Beschreibung.Length > 30 ?
            Beschreibung.Substring(0, 30) + "…" :
            Beschreibung;

        public KalteBetriebskostenVorlageKostenpunkt(Adresse a, KalteBetriebskosten e)
        {
            Typ = e;

            var pt = a.KalteBetriebskosten.FirstOrDefault(k => k.Typ == e);
            Active.Value = pt is KalteBetriebskostenpunkt p;
            Schluessel = pt?.Schluessel ?? UmlageSchluessel.NachWohnflaeche;
            Beschreibung = pt?.Beschreibung ?? "";
        }
    }

    public class KalteBetriebskostenUmlageSchluessel
    {
        public UmlageSchluessel Schluessel { get; set; }
        public string Bezeichnung { get; set; }
    }
}
