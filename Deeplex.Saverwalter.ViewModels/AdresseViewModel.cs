using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class AdresseViewModel<T> : AdresseViewModel where T : IAdresse

    {
        private T reference;

        private void Update(string Strasse, string Hausnummer, string Postleitzahl, string Stadt)
        {
            if (Strasse == Entity.Strasse &&
                Hausnummer == Entity.Hausnummer &&
                Postleitzahl == Entity.Postleitzahl &&
                Stadt == Entity.Stadt)
            {
                return;
            }

            if (!IsValid()) return;
            var adresse = Avm.ctx.Adressen.FirstOrDefault(a =>
                a.Strasse == Strasse && a.Hausnummer == Hausnummer &&
                a.Postleitzahl == Postleitzahl && a.Stadt == Stadt);
            if (adresse == null)
            {
                adresse = new Adresse()
                {
                    Strasse = Strasse,
                    Hausnummer = Hausnummer,
                    Postleitzahl = Postleitzahl,
                    Stadt = Stadt,
                };
                AlleAdressen = AlleAdressen.Add(adresse);
                Avm.ctx.Adressen.Add(adresse);
            }

            reference.Adresse = adresse;
            Entity = adresse;

            if (base.Strasse != Strasse)
            {
                base.Strasse = Strasse;
            }
            if (base.Hausnummer != Hausnummer)
            {
                base.Hausnummer = Hausnummer;
            }
            if (base.Postleitzahl != Postleitzahl)
            {
                base.Postleitzahl = Postleitzahl;
            }
            if (base.Stadt != Stadt)
            {
                base.Stadt = Stadt;
            }

            //Check if reference is valid.
            if (Avm.ctx.Entry(reference).State != Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                Avm.ctx.Update(reference);
            }
            Avm.SaveWalter();
        }

        public override string Hausnummer
        {
            get => Entity.Hausnummer;
            set => Update(Entity.Strasse, value, Entity.Postleitzahl, Entity.Stadt);
        }
        public override string Strasse
        {
            get => Entity.Strasse;
            set => Update(value, Entity.Hausnummer, Entity.Postleitzahl, Entity.Stadt);
        }

        public override string Postleitzahl
        {
            get => Entity.Postleitzahl;
            set => Update(Entity.Strasse, Entity.Hausnummer, value, Entity.Stadt);
        }

        public override string Stadt
        {
            get => Entity.Stadt;
            set => Update(Entity.Strasse, Entity.Hausnummer, Entity.Postleitzahl, value);
        }

        public AdresseViewModel(T value, AppViewModel Avm) : base(value.Adresse ?? new Adresse(), Avm)
        {
            reference = value;
        }
    }

    public class AdresseViewModel : BindableBase
    {
        protected Adresse Entity { get; set; }

        protected AppViewModel Avm;

        protected ImmutableList<Adresse> AlleAdressen;
        public void updateAdressen(string strasse = null, string hausnr = null, string plz = null, string stadt = null)
        {
            Strassen.Value = AlleAdressen
                .Where(a => plz == null || plz == "" || a.Postleitzahl == plz)
                .Select(a => a.Strasse)
                .Where(s => strasse == null || strasse == "" || s.ToLower().Contains(strasse.ToLower()))
                .Distinct().ToImmutableList();

            Hausnummern.Value = AlleAdressen
                .Where(a => strasse == null || strasse == "" || strasse == a.Strasse)
                .Select(a => a.Hausnummer)
                .Where(s => hausnr == null || hausnr == "" || s.ToLower().Contains(hausnr.ToLower()))
                .Distinct().ToImmutableList();

            Postleitzahlen.Value = AlleAdressen
                .Where(a => strasse == null || strasse == "" || a.Strasse == strasse)
                .Select(a => a.Postleitzahl)
                .Where(s => plz == null || plz == "" || s.ToLower().Contains(plz.ToLower()))
                .Distinct().ToImmutableList();

            Staedte.Value = AlleAdressen
                .Where(a => plz == null || plz == "" || plz == a.Postleitzahl)
                .Select(a => a.Stadt)
                .Where(s => stadt == null || stadt == "" || s.ToLower().Contains(stadt.ToLower()))
                .Distinct().ToImmutableList();

            var updated = false;
            if (Hausnummern.Value.Count == 1 && (hausnr == null || hausnr == ""))
            {
                updated = true;
                Hausnummer = Hausnummern.Value.First();
            }

            if (Postleitzahlen.Value.Count == 1 && (plz == null || plz == ""))
            {
                updated = true;
                Postleitzahl = Postleitzahlen.Value.First();
            }

            if (Staedte.Value.Count == 1 && (stadt == null || stadt == ""))
            {
                updated = true;
                Stadt = Staedte.Value.First();
            }

            if (updated == true && (strasse != Strasse || hausnr != Hausnummer || plz != Postleitzahl || stadt != Stadt))
            {
                updateAdressen(Strasse, Hausnummer, Postleitzahl, Stadt);
            }
        }

        public ObservableProperty<ImmutableList<string>> Staedte
            = new ObservableProperty<ImmutableList<string>>();
        public ObservableProperty<ImmutableList<string>> Postleitzahlen
            = new ObservableProperty<ImmutableList<string>>();
        public ObservableProperty<ImmutableList<string>> Strassen
            = new ObservableProperty<ImmutableList<string>>();
        public ObservableProperty<ImmutableList<string>> Hausnummern
            = new ObservableProperty<ImmutableList<string>>();

        public int GetReferences
        {
            get
            {
                var count = Avm.ctx.JuristischePersonen.Count(j => j.Adresse == Entity);
                count += Avm.ctx.NatuerlichePersonen.Count(n => n.Adresse == Entity);
                count += Avm.ctx.Wohnungen.Count(w => w.Adresse == Entity);
                count += Avm.ctx.Garagen.Count(g => g.Adresse == Entity);

                return count;
            }
        }

        public int Id;

        public virtual string Strasse
        {
            get => Entity?.Strasse ?? "";
            set => RaisePropertyChangedAuto(Entity.Strasse, value);
        }

        public virtual string Hausnummer
        {
            get => Entity?.Hausnummer ?? "";
            set => RaisePropertyChangedAuto(Entity.Hausnummer, value);
        }

        public virtual string Postleitzahl
        {
            get => Entity?.Postleitzahl ?? "";
            set => RaisePropertyChangedAuto(Entity.Postleitzahl, value);
        }

        public virtual string Stadt
        {
            get => Entity?.Stadt ?? "";
            set => RaisePropertyChangedAuto(Entity.Stadt, value);
        }

        public AdresseViewModel(Adresse a, AppViewModel avm)
        {
            Avm = avm;
            Entity = a;

            AlleAdressen = Avm.ctx.Adressen.ToImmutableList();
            updateAdressen();

            Dispose = new RelayCommand(_ =>
            {
                Avm.ctx.Adressen.Remove(Entity);
                Avm.SaveWalter();
            });
        }

        public RelayCommand Dispose;

        public static string Anschrift(int id, AppViewModel Avm) => Anschrift(Avm.ctx.Adressen.Find(id));
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

        protected bool IsValid()
        {
            return !(Entity.Strasse == null || Entity.Strasse == "" ||
                Entity.Hausnummer == null || Entity.Postleitzahl == "" ||
                Entity.Postleitzahl == null || Entity.Postleitzahl == "" ||
                Entity.Stadt == null || Entity.Stadt == "");
        }
    }
}
