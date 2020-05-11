using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KalteBetriebskostenViewModel
    {
        public int Id { get; }
        public ObservableProperty<string> Bezeichnung { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Schluessel { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Anteil { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Beschreibung { get; } = new ObservableProperty<string>();

        public KalteBetriebskostenViewModel(KalteBetriebskostenpunkt k, Vertrag v) : this(k)
        {
            var w = v.Wohnung;
            var p = App.Walter.Vertraege
                .Where(a => a.Wohnung.AdresseId == w.AdresseId)
                .Where(a => a.Ende == null || a.Ende < DateTime.Today)
                .Sum(a => a.Personenzahl);

            switch (k.Schluessel)
            {
                case UmlageSchluessel.NachWohnflaeche:
                    Anteil.Value = Percent(w.Wohnflaeche / w.Adresse.Wohnungen.Sum(a => a.Wohnflaeche));
                    break;
                case UmlageSchluessel.NachNutzeinheit:
                    Anteil.Value = Percent(1.0 / w.Adresse.Wohnungen.Count());
                    break;
                case UmlageSchluessel.NachPersonenzahl:
                    Anteil.Value = Percent((double)v.Personenzahl / p);
                    break;
                case UmlageSchluessel.NachVerbrauch:
                    Anteil.Value = "n/a";
                    break;
            }
        }

        public KalteBetriebskostenViewModel(KalteBetriebskostenpunkt k, Wohnung w) : this(k)
        {
            switch (k.Schluessel)
            {
                case UmlageSchluessel.NachWohnflaeche:
                    Anteil.Value = Percent(w.Wohnflaeche / w.Adresse.Wohnungen.Sum(a => a.Wohnflaeche));
                    break;
                case UmlageSchluessel.NachNutzeinheit:
                    Anteil.Value = Percent(1.0 / w.Adresse.Wohnungen.Count());
                    break;
                case UmlageSchluessel.NachPersonenzahl:
                case UmlageSchluessel.NachVerbrauch:
                    Anteil.Value = "n/a";
                    break;
            }
        }

        private KalteBetriebskostenViewModel(KalteBetriebskostenpunkt k)
        {
            Id = k.KalteBetriebskostenpunktId;
            Bezeichnung.Value = k.Bezeichnung.ToDescriptionString();
            Schluessel.Value = k.Schluessel.ToDescriptionString();
            Beschreibung.Value = k.Beschreibung;
        }

        private static string Percent(double d) => string.Format("{0:N2}%", d * 100);
    }
}