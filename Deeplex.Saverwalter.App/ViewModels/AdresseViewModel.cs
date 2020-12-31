using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AdresseViewModel<T> : AdresseViewModel where T : IAdresse
    {
        private T reference;

        private void updateReference()
        {
            reference.Adresse = Entity;
            App.Walter.Update(reference);
        }

        public override string Hausnummer
        {
            get => Entity.Hausnummer;
            set
            {
                base.Hausnummer = value;
                updateReference();
                RaisePropertyChangedAuto();
                App.SaveWalter();
            }
        }
        public override string Strasse
        {
            get => Entity.Strasse;
            set
            {
                base.Strasse = value;
                updateReference();
                RaisePropertyChangedAuto();
                App.SaveWalter();
            }
        }

        public override string Postleitzahl
        {
            get => Entity.Postleitzahl;
            set
            {
                base.Postleitzahl = value;
                updateReference();
                RaisePropertyChangedAuto();
                App.SaveWalter();
            }
        }

        public override string Stadt
        {
            get => Entity.Stadt;
            set
            {
                base.Stadt = value;
                updateReference();
                RaisePropertyChangedAuto();
                App.SaveWalter();
            }
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
        protected Adresse Update(string value, int prop)
        {
            var a = GetAdresse(Entity, value, prop);
            if (a != null)
            {
                Entity = a;
            }
            else
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
                    default: return null;
                }
                App.Walter.Adressen.Add(a);
                Entity = a;
            }

            App.SaveWalter();
            return a;
        }

        public int Id;
        public virtual string Strasse
        {
            get => Entity?.Strasse ?? "";
            set
            {
                Update(value, 0);
                RaisePropertyChangedAuto();
            }
            // update(nameof(Entity.Strasse), value);
        }

        public virtual string Hausnummer
        {
            get => Entity?.Hausnummer ?? "";
            set
            {
                Update(value, 1);
                RaisePropertyChangedAuto();
            }
        }

        public virtual string Postleitzahl
        {
            get => Entity?.Postleitzahl ?? "";
            set
            {
                Update(value, 2);
                RaisePropertyChangedAuto();
            }
        }

        public virtual string Stadt
        {
            get => Entity?.Stadt ?? "";
            set
            {
                Update(value, 3);
                RaisePropertyChangedAuto();
            }
        }

        public AdresseViewModel(Adresse a)
        {
            Entity = a;

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

        private static Adresse GetAdresse(Adresse a, string value, int prop)
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
    }
}
