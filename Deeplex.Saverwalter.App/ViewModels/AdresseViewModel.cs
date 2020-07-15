using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AdresseViewModel : BindableBase
    {
        private Adresse Entity { get; set; }
        public Adresse getEntity => Entity;

        private ImmutableList<Adresse> AlleAdressen = App.Walter.Adressen.ToImmutableList();

        public ImmutableList<string> Staedte => AlleAdressen
            .Select(a => a.Stadt).Distinct().ToImmutableList();
        public ImmutableList<string> Postleitzahlen => AlleAdressen
            .Where(a => Stadt != "" && a.Stadt == Stadt)
            .Select(a => a.Postleitzahl).Distinct().ToImmutableList();
        public ImmutableList<string> Strassen => AlleAdressen
            .Where(a => Postleitzahl != "" && a.Postleitzahl == Entity.Postleitzahl)
            .Select(a => a.Strasse).Distinct().ToImmutableList();
        public ImmutableList<string> Hausnummern => AlleAdressen
            .Where(a => Hausnummer != "" && a.Hausnummer == Entity.Hausnummer)
            .Select(a => a.Hausnummer).Distinct().ToImmutableList();

        private void update<U>(string property, U value)
        {
            if (Entity == null) return;
            var type = Entity.GetType();
            var prop = type.GetProperty(property);
            var val = prop.GetValue(Entity, null);
            if (!value.Equals(val))
            {
                prop.SetValue(Entity, value);
                RaisePropertyChanged(property);
            };
        }

        public int Id;
        public string Strasse
        {
            get => Entity?.Strasse ?? "";
            set => update(nameof(Entity.Strasse), value);
        }

        public string Hausnummer
        {
            get => Entity?.Hausnummer ?? "";
            set => update(nameof(Entity.Hausnummer), value);
        }

        public string Postleitzahl
        {
            get => Entity?.Postleitzahl ?? "";
            set => update(nameof(Entity.Postleitzahl), value);
        }

        public string Stadt
        {
            get => Entity?.Stadt ?? "";
            set => update(nameof(Entity.Stadt), value);
        }

        public AdresseViewModel(Adresse a)
        {
            Entity = a;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.AdresseAnhaenge, a), _ => true);

            PropertyChanged += OnUpdate;
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Strasse):
                case nameof(Hausnummer):
                case nameof(Postleitzahl):
                case nameof(Stadt):
                    break;
                default:
                    return;
            }

            if (Entity.Strasse == null ||
                Entity.Hausnummer == null ||
                Entity.Postleitzahl == null ||
                Entity.Stadt == null)
            {
                return;
            }

            // TODO this is different for Adressen...
            if (Entity.AdresseId != 0)
            {
                App.Walter.Adressen.Update(Entity);
            }
            else
            {
                App.Walter.Adressen.Add(Entity);
            }

            App.SaveWalter();
        }

        public AsyncRelayCommand AttachFile;

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
                avm.Postleitzahl == null || avm.Postleitzahl == "" ||
                avm.Hausnummer == null || avm.Hausnummer == "" ||
                avm.Strasse == null || avm.Strasse == "" ||
                avm.Stadt == null || avm.Stadt == "")
            {
                App.Walter.Adressen.Remove(avm.getEntity);
                return null;
            }

            // TODO Remove deprecated Adressen

            var adr = App.Walter.Adressen.FirstOrDefault(a =>
                a.Postleitzahl == avm.Postleitzahl &&
                a.Hausnummer == avm.Hausnummer &&
                a.Strasse == avm.Strasse &&
                a.Stadt == avm.Stadt);

            if (adr != null)
            {
                App.SaveWalter();
                return adr;
            }
            else
            {
                adr = new Adresse
                {
                    Postleitzahl = avm.Postleitzahl,
                    Hausnummer = avm.Hausnummer,
                    Strasse = avm.Strasse,
                    Stadt = avm.Stadt
                };

                App.Walter.Adressen.Add(adr);
                App.SaveWalter();
                return adr;
            }
        }
    }
}
