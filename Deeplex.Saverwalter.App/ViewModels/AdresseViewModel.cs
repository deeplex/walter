using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class AdresseViewModel
    {
        public int Id;
        public ObservableProperty<string> Strasse { get; set; } = new ObservableProperty<string>();
        public ObservableProperty<string> Hausnummer { get; set; } = new ObservableProperty<string>();
        public ObservableProperty<string> Postleitzahl { get; set; } = new ObservableProperty<string>();
        public ObservableProperty<string> Stadt { get; set; } = new ObservableProperty<string>();

        public AdresseViewModel() { }

        public AdresseViewModel(Adresse a)
        {
            Id = a.AdresseId;
            Strasse.Value = a.Strasse;
            Hausnummer.Value = a.Hausnummer;
            Postleitzahl.Value = a.Postleitzahl;
            Stadt.Value = a.Stadt;
        }

        public static int GetAdresseIdByAnschrift(string s)
            => App.Walter.Adressen.ToList().First(a => Anschrift(a) == s).AdresseId;

        public static string Anschrift(int id) => Anschrift(App.Walter.Adressen.Find(id));
        public static string Anschrift(Kontakt k) => Anschrift(k is Kontakt a ? a.Adresse : null);
        public static string Anschrift(Wohnung w) => Anschrift(w is Wohnung a ? a.Adresse : null);
        public static string Anschrift(Adresse a)
        {
            if (a == null ||
                a.Postleitzahl == null || a.Postleitzahl == "" ||
                a.Hausnummer == null || a.Hausnummer == "" ||
                a.Strasse == null || a.Strasse == "" ||
                a.Stadt == null || a.Stadt == "")
            {
                return "";
            }
            return a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;
        }

        public static Adresse GetAdresse(AdresseViewModel avm)
        {
            // If one is set => all must be set.
            if (avm is null ||
                avm.Postleitzahl.Value == null || avm.Postleitzahl.Value == "" ||
                avm.Hausnummer.Value == null || avm.Hausnummer.Value == "" ||
                avm.Strasse.Value == null || avm.Strasse.Value == "" ||
                avm.Stadt.Value == null || avm.Stadt.Value == "")
            {
                App.Walter.Remove(GetAdresse(avm));
                return null;
            }

            // Remove deprecated Adressen
            foreach (var adresse in App.Walter.Adressen)
            {
                if (adresse.Wohnungen.Count == 0 && adresse.Kontakte.Count == 0 && adresse.Garagen.Count == 0)
                {
                    App.Walter.Remove(adresse);
                }
            }

            var adr = App.Walter.Adressen.FirstOrDefault(a =>
                a.Postleitzahl == avm.Postleitzahl.Value &&
                a.Hausnummer == avm.Hausnummer.Value &&
                a.Strasse == avm.Strasse.Value &&
                a.Stadt == avm.Stadt.Value);

            if (adr != null)
            {
                App.Walter.SaveChanges();
                return adr;
            }
            else
            {
                adr = new Adresse
                {
                    Postleitzahl = avm.Postleitzahl.Value,
                    Hausnummer = avm.Hausnummer.Value,
                    Strasse = avm.Strasse.Value,
                    Stadt = avm.Stadt.Value
                };
                
                App.Walter.Adressen.Add(adr);
                App.Walter.SaveChanges();
                return adr;
            }
        }
    }
}
