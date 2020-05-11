using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KontaktDetailViewModel : BindableBase
    {
        public int Id { get; }
        public ObservableProperty<string> Vorname { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Nachname { get; } = new ObservableProperty<string>();
        public ObservableProperty<int> AdresseId { get; } = new ObservableProperty<int>();
        public ObservableProperty<string> Email { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Telefon { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Mobil { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Fax { get; } = new ObservableProperty<string>();
        public ObservableProperty<List<KontaktDetailVertrag>> Vertraege
            = new ObservableProperty<List<KontaktDetailVertrag>>();
        public ObservableProperty<KontaktDetailAdresse> Adresse { get; }
            = new ObservableProperty<KontaktDetailAdresse>();

        public string Name => Vorname.Value + " " + Nachname.Value;

        public KontaktDetailViewModel(int id)
            : this(App.Walter.Kontakte.Find(id))
        {
        }

        private KontaktDetailViewModel(Kontakt k)
        {
            Id = k.KontaktId;
            Vorname.Value = k.Vorname ?? "";
            Nachname.Value = k.Nachname ?? "";
            AdresseId.Value = k.AdresseId ?? 0;
            Email.Value = k.Email ?? "";
            Fax.Value = k.Fax ?? "";
            Telefon.Value = k.Telefon ?? "";
            Mobil.Value = k.Mobil ?? "";

            Adresse.Value = new KontaktDetailAdresse(k.Adresse);

            Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Mieter)
                .ThenInclude(m => m.Kontakt)
                .Include(v => v.Wohnung).ToList()
                .Where(v => v.Mieter.Exists(m => m.KontaktId == Id))
                .Select(v => new KontaktDetailVertrag(v.VertragId))
                .ToList();

            BeginEdit = new RelayCommand(_ => IsInEdit.Value = true, _ => !IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => BeginEdit.RaiseCanExecuteChanged(ev);

            SaveEdit = new RelayCommand(_ =>
            {
                IsInEdit.Value = false;

                k.Vorname = Vorname.Value;
                k.Nachname = Nachname.Value;
                k.Email = Email.Value;
                k.Telefon = Telefon.Value;
                k.Mobil = Mobil.Value;
                k.Fax = Fax.Value;

                App.Walter.Kontakte.Update(k);
                App.Walter.SaveChanges();

            }, _ => IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => SaveEdit.RaiseCanExecuteChanged(ev);

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        public RelayCommand BeginEdit { get; }
        public RelayCommand SaveEdit { get; }

        public class KontaktDetailAdresse
        {
            public int Id;
            public ObservableProperty<string> Strasse { get; } = new ObservableProperty<string>();
            public ObservableProperty<string> Hausnummer { get; } = new ObservableProperty<string>();
            public ObservableProperty<string> Postleitzahl { get; } = new ObservableProperty<string>();
            public ObservableProperty<string> Stadt { get; } = new ObservableProperty<string>();

            public KontaktDetailAdresse(Adresse a)
            {
                Id = a.AdresseId;
                Strasse.Value = a.Strasse;
                Hausnummer.Value = a.Hausnummer;
                Postleitzahl.Value = a.Postleitzahl;
                Stadt.Value = a.Stadt;
            }
        }

        public class KontaktDetailVertrag
        {
            public int Id { get; }
            public int Version { get; }
            public ObservableProperty<string> Anschrift { get; } = new ObservableProperty<string>();
            public ObservableProperty<string> Wohnung { get; } = new ObservableProperty<string>();
            public ObservableProperty<DateTime> Beginn { get; } = new ObservableProperty<DateTime>();
            public ObservableProperty<string> AuflistungMieter { get; } = new ObservableProperty<string>();
            public ObservableProperty<string> BeginnString { get; } = new ObservableProperty<string>();
            public ObservableProperty<string> EndeString { get; } = new ObservableProperty<string>();
            public ObservableProperty<List<VertragVersionListViewModel>> Versionen { get; }
                = new ObservableProperty<List<VertragVersionListViewModel>>();

            public KontaktDetailVertrag(Guid id)
                : this(App.Walter.Vertraege.Where(v => v.VertragId == id)) { }

            private KontaktDetailVertrag(IEnumerable<Vertrag> v)
                : this(v.OrderBy(vs => vs.Version).Last())
            {
                Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
                BeginnString.Value = Versionen.Value.First().BeginnString.Value;
                Beginn.Value = Versionen.Value.First().Beginn.Value;
            }

            private KontaktDetailVertrag(Vertrag v)
            {
                Id = v.rowid;
                Version = v.Version;
                Anschrift.Value = v.Wohnung is Wohnung w ? Utils.Anschrift(w) : "";
                Wohnung.Value = v.Wohnung is Wohnung ww ? ww.Bezeichnung : "";

                Beginn.Value = v.Beginn;
                BeginnString.Value = v.Beginn.ToShortDateString(); ;
                EndeString.Value = v.Ende is DateTime e ? e.ToShortDateString() : "";

                AuflistungMieter.Value = string.Join(", ", v.Mieter.Select(m => m.Kontakt.Nachname));
            }
        }

    }
}
