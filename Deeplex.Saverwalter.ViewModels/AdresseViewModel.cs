using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
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
            var adresse = Db.ctx.Adressen.FirstOrDefault(a =>
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
                Db.ctx.Adressen.Add(adresse);
            }

            reference.Adresse = adresse;
            Entity = adresse;

            //Check if reference is valid.
            if (Db.ctx.Entry(reference).State != Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                Db.ctx.Update(reference);
            }
            Db.SaveWalter();
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

        public AdresseViewModel(T value, IWalterDbService db) : base(value.Adresse ?? new Adresse(), db)
        {
            reference = value;
        }
    }

    public class AdresseViewModel : BindableBase
    {
        protected Adresse Entity { get; set; }

        protected IWalterDbService Db;

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

        public ObservableProperty<ImmutableList<string>> Staedte = new();
        public ObservableProperty<ImmutableList<string>> Postleitzahlen = new();
        public ObservableProperty<ImmutableList<string>> Strassen = new();
        public ObservableProperty<ImmutableList<string>> Hausnummern = new();

        public int GetReferences
        {
            get
            {
                var count = Db.ctx.JuristischePersonen.Count(j => j.Adresse == Entity);
                count += Db.ctx.NatuerlichePersonen.Count(n => n.Adresse == Entity);
                count += Db.ctx.Wohnungen.Count(w => w.Adresse == Entity);
                count += Db.ctx.Garagen.Count(g => g.Adresse == Entity);

                return count;
            }
        }

        public int Id;

        public virtual string Strasse
        {
            get => Entity?.Strasse ?? "";
            set
            {
                if (value == Strasse) return;
                Entity.Strasse = value;
                RaisePropertyChangedAuto(Entity.Strasse, value);
                update();
            }
        }

        public virtual string Hausnummer
        {
            get => Entity?.Hausnummer ?? "";
            set
            {
                if (value == Hausnummer) return;
                Entity.Hausnummer = value;
                RaisePropertyChangedAuto(Entity.Hausnummer, value);
                update();
            }
        }

        public virtual string Postleitzahl
        {
            get => Entity?.Postleitzahl ?? "";
            set
            {
                if (value == Postleitzahl) return;
                Entity.Postleitzahl = value;
                RaisePropertyChangedAuto(Entity.Postleitzahl, value);
                update();
            }
        }

        public virtual string Stadt
        {
            get => Entity?.Stadt ?? "";
            set
            {
                if (value == Stadt) return;
                Entity.Stadt = value;
                RaisePropertyChangedAuto(Entity.Stadt, value);
                update();
            }
        }

        public AdresseViewModel(Adresse a, IWalterDbService db)
        {
            Db = db;
            Entity = a;

            AlleAdressen = Db.ctx.Adressen.ToImmutableList();
            updateAdressen();

            Dispose = new RelayCommand(_ =>
            {
                Db.ctx.Adressen.Remove(Entity);
                Db.SaveWalter();
            });
        }

        public RelayCommand Dispose;

        public static string Anschrift(int id, IWalterDbService Avm) => Anschrift(Avm.ctx.Adressen.Find(id));
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

        // Only used for update. No adding here.
        private void update()
        {
            if (!IsValid() || Entity.AdresseId == 0)
            {
                return;
            }

            Db.ctx.Adressen.Update(Entity);
            Db.SaveWalter();
        }
    }
}
