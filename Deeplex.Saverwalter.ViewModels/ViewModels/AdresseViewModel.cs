using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class AdresseViewModel<T> : AdresseViewModel, IDetailViewModel where T : IAdresse
    {
        private T reference;

        public AdresseViewModel(T entity, IWalterDbService db, INotificationService ns) : base(entity.Adresse ?? new Adresse(), db, ns)
        {
            reference = entity;
        }
    }

    public class AdresseViewModel : DetailViewModel<Adresse>, IDetailViewModel
    {
        public override string ToString() => Anschrift(Entity);

        protected ImmutableList<Adresse> AlleAdressen;

        protected Adresse findAdresse()
            => WalterDbService.ctx.Adressen.FirstOrDefault(a =>
                a.Strasse == Strasse.Value &&
                a.Hausnummer == Hausnummer.Value &&
                a.Postleitzahl == Postleitzahl.Value &&
                a.Stadt == Stadt.Value);

        public void updateAdressen(string strasse = null, string hausnr = null, string plz = null, string stadt = null)
        {
            Strassen.Value = AlleAdressen
                //.Where(a => plz == null || plz == "" || a.Postleitzahl == plz)
                .Select(a => a.Strasse)
                //.Where(s => strasse == null || strasse == "" || s.ToLower().Contains(strasse.ToLower()))
                .Distinct().ToImmutableList();

            Hausnummern.Value = AlleAdressen
                //.Where(a => strasse == null || strasse == "" || strasse == a.Strasse)
                .Select(a => a.Hausnummer)
                //.Where(s => hausnr == null || hausnr == "" || s.ToLower().Contains(hausnr.ToLower()))
                .Distinct().ToImmutableList();

            Postleitzahlen.Value = AlleAdressen
                //.Where(a => strasse == null || strasse == "" || a.Strasse == strasse)
                .Select(a => a.Postleitzahl)
                //.Where(s => plz == null || plz == "" || s.ToLower().Contains(plz.ToLower()))
                .Distinct().ToImmutableList();

            Staedte.Value = AlleAdressen
                //.Where(a => plz == null || plz == "" || plz == a.Postleitzahl)
                .Select(a => a.Stadt)
                //.Where(s => stadt == null || stadt == "" || s.ToLower().Contains(stadt.ToLower()))
                .Distinct().ToImmutableList();

            var updated = false;
            if (Hausnummern.Value.Count == 1 && (hausnr == null || hausnr == ""))
            {
                updated = true;
                Hausnummer.Value = Hausnummern.Value.First();
            }

            if (Postleitzahlen.Value.Count == 1 && (plz == null || plz == ""))
            {
                updated = true;
                Postleitzahl.Value = Postleitzahlen.Value.First();
            }

            if (Staedte.Value.Count == 1 && (stadt == null || stadt == ""))
            {
                updated = true;
                Stadt.Value = Staedte.Value.First();
            }

            if (updated == true && (strasse != Strasse.Value || hausnr != Hausnummer.Value || plz != Postleitzahl.Value || stadt != Stadt.Value))
            {
                updateAdressen(Strasse.Value, Hausnummer.Value, Postleitzahl.Value, Stadt.Value);
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
                var count = WalterDbService.ctx.JuristischePersonen.Count(j => j.Adresse == Entity);
                count += WalterDbService.ctx.NatuerlichePersonen.Count(n => n.Adresse == Entity);
                count += WalterDbService.ctx.Wohnungen.Count(w => w.Adresse == Entity);
                count += WalterDbService.ctx.Garagen.Count(g => g.Adresse == Entity);

                return count;
            }
        }

        public new int Id => Entity.AdresseId;
        public SavableProperty<string> Strasse { get; set; }
        public SavableProperty<string> Hausnummer { get; set; }
        public SavableProperty<string> Postleitzahl { get; set; }
        public SavableProperty<string> Stadt { get; set; }

        public AdresseViewModel(Adresse a, IWalterDbService db, INotificationService ns) : base(ns, db)
        {
            AlleAdressen = WalterDbService.ctx.Adressen.ToImmutableList();
            updateAdressen();

            SetEntity(a);

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public override void SetEntity(Adresse a)
        {
            Strasse = new(this, a.Strasse);
            Hausnummer = new(this, a.Hausnummer);
            Postleitzahl = new(this, a.Postleitzahl);
            Stadt = new(this, a.Stadt);

            Entity = a;
        }

        public static string Anschrift(IAdresse k) => Anschrift(k is IAdresse a ? a.Adresse : null);
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

        public new void save()
        {
            Entity.Strasse = Strasse.Value;
            Entity.Hausnummer = Hausnummer.Value;
            Entity.Postleitzahl = Postleitzahl.Value;
            Entity.Stadt = Stadt.Value;

            base.save();
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Strasse.Value != Entity.Strasse ||
                Hausnummer.Value != Entity.Hausnummer ||
                Postleitzahl.Value != Entity.Postleitzahl ||
                Stadt.Value != Entity.Stadt;
        }
    }
}
