using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class KontaktListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<KontaktListEntry>> Kontakte = new ObservableProperty<ImmutableList<KontaktListEntry>>();
        private KontaktListEntry mSelectedKontakt;
        public KontaktListEntry SelectedKontakt
        {
            get => mSelectedKontakt;
            set
            {
                mSelectedKontakt = value;
                RaisePropertyChangedAuto();
                RaisePropertyChanged(nameof(hasSelectedKontakt));
            }
        }
        public bool hasSelectedKontakt => SelectedKontakt != null;

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<KontaktListEntry> AllRelevant { get; }

        public KontaktListViewModel()
        {
            AllRelevant = App.Walter.NatuerlichePersonen
                .Include(k => k.Adresse)
                .Select(k => new KontaktListEntry(k)).ToImmutableList();

            var jp = App.Walter.JuristischePersonen;
            foreach (var j in jp)
            {
                AllRelevant = AllRelevant.Add(new KontaktListEntry(j));
            }

            Kontakte.Value = AllRelevant;
        }
    }

    public sealed class KontaktListEntry
    {
        public Type Type { get; }
        public int Id { get; }
        public Guid Guid { get; }
        public string Vorname { get; }
        public string Name { get; }
        public string Anschrift { get; }
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }

        public KontaktListEntry(JuristischePerson j) : this(j as IPerson)
        {
            Type = j.GetType();
            Id = j.JuristischePersonId;
            Vorname = "";
            Name = j.Bezeichnung;
        }

        public KontaktListEntry(NatuerlichePerson k) : this(k as IPerson)
        {
            Type = k.GetType();
            Id = k.NatuerlichePersonId;
            Vorname = k.Vorname ?? "";
            Name = k.Nachname;
        }

        private KontaktListEntry(IPerson p)
        {
            Guid = p.PersonId;
            Email = p.Email ?? "";
            Telefon = p.Telefon ?? "";
            Mobil = p.Mobil ?? "";
            Anschrift = p.Adresse is Adresse a ?
                a.Strasse + " " + a.Hausnummer + ", " +
                a.Postleitzahl + " " + a.Stadt : "";
        }
    }
}
