using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AdresseViewModel : BindableBase
    {
        private Adresse Entity { get; set; }
        private NatuerlichePerson NPerson { get; }
        private JuristischePerson JPerson { get; }
        private Wohnung Wohnung { get; }
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
        private Adresse Update(string value, int prop)
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

            if (JPerson != null)
            {
                JPerson.Adresse = Entity;
                App.Walter.JuristischePersonen.Update(JPerson);
            }
            if (NPerson != null)
            {
                NPerson.Adresse = Entity;
                App.Walter.NatuerlichePersonen.Update(NPerson);
            }
            if (Wohnung != null)
            {
                Wohnung.Adresse = Entity;
                App.Walter.Wohnungen.Update(Wohnung);
            }
            // TODO Garage

            App.SaveWalter();
            return a;
        }

        public int Id;
        public string Strasse
        {
            get => Entity?.Strasse ?? "";
            set
            {
                Update(value, 0);
                RaisePropertyChangedAuto();
            }
            // update(nameof(Entity.Strasse), value);
        }

        public string Hausnummer
        {
            get => Entity?.Hausnummer ?? "";
            set
            {
                Update(value, 1);
                RaisePropertyChangedAuto();
            }
        }

        public string Postleitzahl
        {
            get => Entity?.Postleitzahl ?? "";
            set
            {
                Update(value, 2);
                RaisePropertyChangedAuto();
            }
        }

        public string Stadt
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

        public AdresseViewModel(Wohnung w) : this(w?.Adresse ?? new Adresse { })
        {
            Wohnung = w;
        }

        public AdresseViewModel(NatuerlichePerson p) : this(p?.Adresse ?? new Adresse { })
        {
            NPerson = p;
        }

        public AdresseViewModel(JuristischePerson p) : this(p?.Adresse ?? new Adresse { })
        {
            JPerson = p;
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

        //public static Adresse GetAdresse(AdresseViewModel avm)
        //{
        //    // If one is set => all must be set.
        //    if (avm is null ||
        //        avm.Postleitzahl == null || avm.Postleitzahl == "" ||
        //        avm.Hausnummer == null || avm.Hausnummer == "" ||
        //        avm.Strasse == null || avm.Strasse == "" ||
        //        avm.Stadt == null || avm.Stadt == "")
        //    {
        //        App.Walter.Adressen.Remove(avm.getEntity);
        //        return null;
        //    }

        //    // TODO Remove deprecated Adressen

        //    var adr = App.Walter.Adressen.FirstOrDefault(a =>
        //        a.Postleitzahl == avm.Postleitzahl &&
        //        a.Hausnummer == avm.Hausnummer &&
        //        a.Strasse == avm.Strasse &&
        //        a.Stadt == avm.Stadt);

        //    if (adr != null)
        //    {
        //        App.SaveWalter();
        //        return adr;
        //    }
        //    else
        //    {
        //        adr = new Adresse
        //        {
        //            Postleitzahl = avm.Postleitzahl,
        //            Hausnummer = avm.Hausnummer,
        //            Strasse = avm.Strasse,
        //            Stadt = avm.Stadt
        //        };

        //        App.Walter.Adressen.Add(adr);
        //        App.SaveWalter();
        //        return adr;
        //    }
        //}
    }
}
