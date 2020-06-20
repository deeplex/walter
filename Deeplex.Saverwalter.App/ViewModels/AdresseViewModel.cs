using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AdresseViewModel
    {
        private Adresse Entity;

        public Adresse getEntity => Entity;

        public int Id;
        public ObservableProperty<string> Strasse { get; set; } = new ObservableProperty<string>();
        public ObservableProperty<string> Hausnummer { get; set; } = new ObservableProperty<string>();
        public ObservableProperty<string> Postleitzahl { get; set; } = new ObservableProperty<string>();
        public ObservableProperty<string> Stadt { get; set; } = new ObservableProperty<string>();

        public AdresseViewModel() { }

        public AdresseViewModel(Adresse a)
        {
            Entity = a;
            Id = a.AdresseId;
            Strasse.Value = a.Strasse;
            Hausnummer.Value = a.Hausnummer;
            Postleitzahl.Value = a.Postleitzahl;
            Stadt.Value = a.Stadt;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.AdresseAnhaenge, a), _ => true);
        }

        public AsyncRelayCommand AttachFile;

        public static int GetAdresseIdByAnschrift(string s)
            => App.Walter.Adressen.ToList().First(a => Anschrift(a) == s).AdresseId;

        public static string Anschrift(int id) => Anschrift(App.Walter.Adressen.Find(id));
        public static string Anschrift(IPerson k) => Anschrift(k is IPerson a ? a.Adresse : null);
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
                App.Walter.Adressen.Remove(avm.getEntity);
                return null;
            }

            // TODO Remove deprecated Adressen

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
