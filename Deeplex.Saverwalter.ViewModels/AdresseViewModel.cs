using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.ComponentModel;
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

            Entity.Strasse = Strasse;
            Entity.Hausnummer = Hausnummer;
            Entity.Postleitzahl = Postleitzahl;
            Entity.Stadt = Stadt;

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
                Avm.ctx.Adressen.Add(adresse);
            }

            reference.Adresse = adresse;

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
        protected Adresse Entity { get; private set; }

        protected AppViewModel Avm;

        private ImmutableList<Adresse> AlleAdressen;
        public void updateAdressen(string strasse = null, string hausnr = null, string plz = null, string stadt = null)
        {
            Staedte.Value = AlleAdressen.Select(a => a.Stadt)
                .Where(s => stadt == null || s.ToLower().Contains(stadt.ToLower()))
                .Distinct().ToImmutableList();
            Postleitzahlen.Value = AlleAdressen
                .Where(a => Stadt == "" || a.Stadt == Stadt)
                .Select(a => a.Postleitzahl)
                .Where(s => plz == null || s.ToLower().Contains(plz.ToLower()))
                .Distinct().ToImmutableList();
            Strassen.Value = AlleAdressen
                .Where(a => Postleitzahl == "" || a.Postleitzahl == Entity.Postleitzahl)
                .Select(a => a.Strasse)
                .Where(s => strasse == null || s.ToLower().Contains(strasse.ToLower()))
                .Distinct().ToImmutableList();
            Hausnummern.Value = AlleAdressen
                .Where(a => Hausnummer == "" || a.Hausnummer == Entity.Hausnummer)
                .Select(a => a.Hausnummer)
                .Where(s => hausnr == null || s.ToLower().Contains(hausnr.ToLower()))
                .Distinct().ToImmutableList();
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
            set
            {
                var old = Entity.Strasse;
                Entity.Strasse = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public virtual string Hausnummer
        {
            get => Entity?.Hausnummer ?? "";
            set
            {
                var old = Entity.Hausnummer;
                Entity.Hausnummer = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public virtual string Postleitzahl
        {
            get => Entity?.Postleitzahl ?? "";
            set
            {
                var old = Entity.Postleitzahl;
                Entity.Postleitzahl = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public virtual string Stadt
        {
            get => Entity?.Stadt ?? "";
            set
            {
                var old = Entity.Stadt;
                Entity.Stadt = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public AdresseViewModel(Adresse a, AppViewModel avm)
        {
            Avm = avm;
            Entity = a;

            AlleAdressen = Avm.ctx.Adressen.ToImmutableList();
            updateAdressen();

            PropertyChanged += OnUpdate;

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

        protected void OnUpdate(object sender, PropertyChangedEventArgs e)
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

            if (IsValid())
            {
                return;
            }

            if (Entity.AdresseId != 0)
            {
                Avm.ctx.Adressen.Update(Entity);
            }
            else
            {
                Avm.ctx.Adressen.Add(Entity);
            }
            Avm.SaveWalter();
        }
    }
}
