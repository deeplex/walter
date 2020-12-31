using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AdresseViewModel<T> : AdresseViewModel where T : IAdresse
    {
        private T reference;

        private void Update(string value, int prop)
        {
            var a = GetAdresse(Entity, value, prop);

            if (a == null)
            {
                a = new Adresse()
                {
                    Hausnummer = Entity.Hausnummer,
                    Postleitzahl = Entity.Postleitzahl,
                    Strasse = Entity.Strasse,
                    Stadt = Entity.Stadt
                };
                switch (prop)
                {
                    case (0): a.Strasse = value; break;
                    case (1): a.Hausnummer = value; break;
                    case (2): a.Postleitzahl = value; break;
                    case (3): a.Stadt = value; break;
                }
                App.Walter.Adressen.Add(a);
            }

            reference.Adresse = a;
            App.Walter.Update(reference);

            App.SaveWalter();
        }

        public override string Hausnummer
        {
            get => Entity.Hausnummer;
            set => Update(value, 0);
        }
        public override string Strasse
        {
            get => Entity.Strasse;
            set => Update(value, 1);
        }

        public override string Postleitzahl
        {
            get => Entity.Postleitzahl;
            set => Update(value, 2);
        }

        public override string Stadt
        {
            get => Entity.Stadt;
            set => Update(value, 3);
        }

        public AdresseViewModel(T value) : base(value.Adresse)
        {
            reference = value;
        }
    }

    public class AdresseViewModel : BindableBase
    {
        protected Adresse Entity { get; set; }
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

        public int GetReferences
        {
            get
            {
                var count = App.Walter.JuristischePersonen.Count(j => j.Adresse == Entity);
                count += App.Walter.NatuerlichePersonen.Count(n => n.Adresse == Entity);
                count += App.Walter.Wohnungen.Count(w => w.Adresse == Entity);
                count += App.Walter.Garagen.Count(g => g.Adresse == Entity);

                return count;
            }
        }

        // Update changes one value of the address.
        // If this new address does not exist, it will be created.
        // An Address without any references will not be deleted automatically.
        /*
         * prop - 0: Strasse, 1: Hausnr., 2: PLZ, 3: Stadt
         */

        public int Id;
        public virtual string Strasse
        {
            get => Entity?.Strasse ?? "";
            set
            {
                Entity.Strasse = value;
                RaisePropertyChangedAuto();
            }
            // update(nameof(Entity.Strasse), value);
        }

        public virtual string Hausnummer
        {
            get => Entity?.Hausnummer ?? "";
            set
            {
                Entity.Hausnummer = value;
                RaisePropertyChangedAuto();
            }
        }

        public virtual string Postleitzahl
        {
            get => Entity?.Postleitzahl ?? "";
            set
            {
                Entity.Postleitzahl = value;
                RaisePropertyChangedAuto();
            }
        }

        public virtual string Stadt
        {
            get => Entity?.Stadt ?? "";
            set
            {
                Entity.Stadt = value;
                RaisePropertyChangedAuto();
            }
        }

        public AdresseViewModel(Adresse a)
        {
            Entity = a;

            PropertyChanged += OnUpdate;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.AdresseAnhaenge, a), _ => true);
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

        protected static Adresse GetAdresse(Adresse a, string value, int prop)
        {
            switch (prop)
            {
                case (0): return GetAdresse(value, a.Hausnummer, a.Postleitzahl, a.Stadt);
                case (1): return GetAdresse(a.Strasse, value, a.Postleitzahl, a.Stadt);
                case (2): return GetAdresse(a.Strasse, a.Hausnummer, value, a.Stadt);
                case (3): return GetAdresse(a.Strasse, a.Hausnummer, a.Postleitzahl, value);
                default: return null;
            }
        }
        public static Adresse GetAdresse(string Strasse, string Hausnummer, string Postleitzahl, string Stadt)
        {
            return App.Walter.Adressen.FirstOrDefault(a =>
                a.Strasse == Strasse && a.Hausnummer == Hausnummer &&
                a.Postleitzahl == Postleitzahl && a.Stadt == Stadt);
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

            if (Entity.Strasse == null || Entity.Strasse == "" ||
                Entity.Hausnummer == null || Entity.Postleitzahl == "" ||
                Entity.Postleitzahl == null || Entity.Postleitzahl == "" ||
                Entity.Stadt == null || Entity.Stadt == "")
            {
                return;
            }

            if (getEntity.AdresseId != 0)
            {
                App.Walter.Adressen.Update(Entity);
            }
            else
            {
                App.Walter.Adressen.Add(Entity);
            }
            App.SaveWalter();
        }
    }
}
