﻿using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
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
        public ObservableProperty<bool> Vermieter { get; set; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> Mieter { get; set; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> Handwerker { get; set; } = new ObservableProperty<bool>(true);
        public ImmutableList<KontaktListEntry> AllRelevant { get; }

        public KontaktListViewModel(IAppImplementation impl)
        {
            AllRelevant = impl.ctx.NatuerlichePersonen
                .Include(k => k.Adresse)
                .Select(k => new KontaktListEntry(k)).ToImmutableList();

            var jp = impl.ctx.JuristischePersonen;
            foreach (var j in jp)
            {
                AllRelevant = AllRelevant.Add(new KontaktListEntry(j));
            }

            Kontakte.Value = AllRelevant;
        }
    }

    public sealed class KontaktListEntry
    {
        public override string ToString()
            => Entity.Bezeichnung;

        public Type Type { get; }
        public int Id { get; }
        public Guid Guid { get; }
        public string Vorname { get; }
        public string Name { get; }
        public string Anschrift { get; }
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }
        public IPerson Entity { get; }

        public KontaktListEntry(Guid id, IAppImplementation impl) : this(impl.ctx.FindPerson(id)) { }
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
            Entity = p;
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