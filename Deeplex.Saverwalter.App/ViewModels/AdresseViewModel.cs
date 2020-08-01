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

        private void update(string property, string value)
        {
            var type = Entity.GetType();
            var prop = type.GetProperty(property);
            var val = prop.GetValue(Entity, null);
            if (value.Equals(val))
            {
                return;
            }
            var ent = GetAdresse(
                property == "Strasse" ? value : Strasse,
                property == "Hausnummer" ? value : Hausnummer,
                property == "Postleitzahl" ? value : Postleitzahl,
                property == "Stadt" ? value : Stadt);
            if (ent == null)
            {
                Entity = new Adresse
                {
                    Strasse = Strasse,
                    Hausnummer = Hausnummer,
                    Postleitzahl = Postleitzahl,
                    Stadt = Stadt,
                };
                prop.SetValue(Entity, value);
            }
            else
            {
                Entity = ent;
            }

            RaisePropertyChanged(property);
            bool b(string s) => s != null && s != "";
            if (b(Strasse) && b(Hausnummer) && b(Postleitzahl) && b(Stadt))
            {
                if (ent == null)
                {
                    App.Walter.Adressen.Add(Entity);
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
                RemoveAllDeprecated();
                App.SaveWalter();
            }
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

        public static Adresse GetAdresse(string Strasse, string Hausnummer, string Postleitzahl, string Stadt)
        {
            return App.Walter.Adressen.FirstOrDefault(a =>
                a.Strasse == Strasse && a.Hausnummer == Hausnummer &&
                a.Postleitzahl == Postleitzahl && a.Stadt == Stadt);
        }

        public static void RemoveDeprecated(Adresse a)
        {
            if (App.Walter.Wohnungen.Any(w => w.Adresse == a) ||
                App.Walter.NatuerlichePersonen.Any(p => p.Adresse == a) ||
                App.Walter.JuristischePersonen.Any(p => p.Adresse == a))
            {
                return;
            }
            App.Walter.Remove(a);
        }

        // This action does not save the database
        public static void RemoveAllDeprecated()
            => App.Walter.Adressen.ToList().ForEach(a => RemoveDeprecated(a));

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
